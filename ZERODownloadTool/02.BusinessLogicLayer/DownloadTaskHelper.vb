Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Security.Policy
Imports System.Web
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Imports LiteDB
Imports ZERODownloadTool.MangaChapterInfo
''' <summary>
''' 下载任务辅助模块
''' </summary>
Public Class DownloadTaskHelper

    Public Shared Property MainWindowInstance As MainWindow

    Public Shared MaxThreadCount As Integer = 10

    Public Shared TaskStatePool As New HashSet(Of String)

    Public Shared WaitingMangaChapterList As New ObservableCollection(Of MangaChapterInfo)

    Public Shared CompletedMangaChapterList As New ObservableCollection(Of MangaChapterInfo)

#Region "HttpClientInstance"
    Private Shared ReadOnly LockObject As New Object
    Private Shared _HttpClientInstance As HttpClient
    Private Shared ReadOnly Property HttpClientInstance() As HttpClient
        Get
            ' 第一次判断
            If _HttpClientInstance Is Nothing Then

                ' 排他锁
                SyncLock LockObject

                    ' 第二次判断
                    If _HttpClientInstance Is Nothing Then

                        Dim tmpHttpClientHandler = New HttpClientHandler With {
                            .AllowAutoRedirect = True,
                            .UseCookies = True,
                            .CookieContainer = New CookieContainer
                        }

                        Dim tmpHttpClientInstance = New HttpClient(tmpHttpClientHandler) With {
                            .Timeout = New TimeSpan(0, 0, 10)
                        }

                        Login(tmpHttpClientInstance)

                        _HttpClientInstance = tmpHttpClientInstance

                    End If

                End SyncLock

            End If

            Return _HttpClientInstance
        End Get
    End Property
#End Region

#Region "用户登录"
    ''' <summary>
    ''' 用户登录
    ''' </summary>
    Public Shared Sub Login(tmpHttpClient As HttpClient)

        Dim tmpResponse = tmpHttpClient.GetAsync(AppSettingHelper.HostName).GetAwaiter.GetResult
        tmpResponse.EnsureSuccessStatusCode()
        Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult

        Dim doc As New HtmlAgilityPack.HtmlDocument
        doc.LoadHtml(contentStr)
        Dim formhashNodes = doc.DocumentNode.SelectNodes("//input[@name='formhash']")
        Dim formhashStr = formhashNodes.First.Attributes("value").Value

        Dim tmp = New FormUrlEncodedContent({
            New KeyValuePair(Of String, String)("fastloginfield", "username"),
            New KeyValuePair(Of String, String)("username", AppSettingHelper.UserName),
            New KeyValuePair(Of String, String)("password", AppSettingHelper.UserPassword),
            New KeyValuePair(Of String, String)("formhash", formhashStr),
            New KeyValuePair(Of String, String)("quickforward", "yes"),
            New KeyValuePair(Of String, String)("handlekey", "ls")
                                            })

        Dim tmpResponse2 = tmpHttpClient.PostAsync($"{AppSettingHelper.HostName}/member.php?mod=logging&action=login&loginsubmit=yes&infloat=yes&lssubmit=yes&inajax=1", tmp).GetAwaiter.GetResult
        tmpResponse2.EnsureSuccessStatusCode()
        contentStr = tmpResponse2.Content.ReadAsStringAsync().GetAwaiter.GetResult

        ' 判断登录是否成功
        If Not contentStr.Contains(AppSettingHelper.HostName) Then
            Throw New Exception(contentStr)
        End If

    End Sub
#End Region

#Region "自动开始"
    Private Shared ReadOnly LockObject2 As New Object
    Public Shared Sub AutoStartALL()

        SyncLock LockObject2

            Dim TaskCount = WaitingMangaChapterList.Where(Function(o)
                                                              Return o.State = MangaChapterInfo.TaskState.Downloading
                                                          End Function).Count
            If TaskCount > MaxThreadCount Then
                Exit Sub
            End If

            Dim tmpList = WaitingMangaChapterList.Where(Function(o)
                                                            Return o.State <> MangaChapterInfo.TaskState.Downloading AndAlso o.State <> MangaChapterInfo.TaskState.Completed
                                                        End Function).Take(MaxThreadCount - TaskCount)

            For Each item In tmpList

                If Not TaskStatePool.Contains(item.Id) Then
                    TaskStatePool.Add(item.Id)
                Else
                    item.ErrorMsg = "任务重复添加"
                    Continue For
                End If

                Dim tmpTask = Task.Run(Sub()
                                           StartDownload(item)
                                       End Sub)

            Next

        End SyncLock

    End Sub
#End Region

    Private Shared Async Sub StartDownload(value As MangaChapterInfo)

        value.State = MangaChapterInfo.TaskState.Downloading
        value.ErrorMsg = String.Empty

        Try

            If value.Images.Count = 0 Then
                ' 获取图片地址
                GetMangaChapterImages(value)
            End If

            Dim tmpImageList = (From item In value.Images
                                Where Not item.Value
                                Select item.Key).ToList

            For Each item In tmpImageList

                If Not TaskStatePool.Contains(value.Id) Then
                    Exit For
                End If

                ' 创建文件夹
                IO.Directory.CreateDirectory(value.SaveFolderPath)
                Dim fileName = IO.Path.GetFileName(item)

                ' 下载图片
                Using tmpReadFileStream = Await HttpClientInstance.GetStreamAsync(item)
                    Using tmpSaveFileStream = New FileStream(IO.Path.Combine(value.SaveFolderPath, fileName), FileMode.Create)

                        Dim bArr(1024) As Byte
                        Dim readByteSize As Integer
                        Dim downloadByteSize As Long

                        readByteSize = Await tmpReadFileStream.ReadAsync(bArr, 0, bArr.Count)

                        While readByteSize > 0

                            downloadByteSize += readByteSize

                            tmpSaveFileStream.Write(bArr, 0, readByteSize)
                            readByteSize = Await tmpReadFileStream.ReadAsync(bArr, 0, bArr.Count)

                        End While

                    End Using
                End Using

                value.Images(item) = True
                value.CompletedCount += 1

                LocalLiteDBHelper.Update(value)
            Next

            If value.Count = value.CompletedCount Then
                ' 标记为结束
                value.State = MangaChapterInfo.TaskState.Completed

                MainWindowInstance?.UpdateCompletedMangaChapterlist(value)

            Else

                value.State = MangaChapterInfo.TaskState.Waiting
                value.ErrorMsg = "暂停下载"
            End If

        Catch ex As Exception
            ' 异常则显示异常信息
            value.ErrorMsg = ex.Message

            Threading.Thread.Sleep(2000)

            value.State = MangaChapterInfo.TaskState.Waiting

        End Try

        If TaskStatePool.Contains(value.Id) Then
            LocalLiteDBHelper.Update(value)

            TaskStatePool.Remove(value.Id)

            AutoStartALL()
        End If

    End Sub

#Region "获取章节图片地址"
    ''' <summary>
    ''' 获取章节图片地址
    ''' </summary>
    Private Shared Sub GetMangaChapterImages(value As MangaChapterInfo)

        Dim tmpResponse = HttpClientInstance.GetAsync(value.PageUrl).GetAwaiter.GetResult
        tmpResponse.EnsureSuccessStatusCode()
        Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult

        Dim doc As New HtmlAgilityPack.HtmlDocument
        doc.LoadHtml(contentStr)
        Dim formhashNodes = doc.DocumentNode.SelectNodes("//div[@class='uk-text-center mb0']/img")

        value.Images.Clear()

        For Each item In formhashNodes
            value.Images.Add(item.Attributes("src").Value, False)
        Next

    End Sub
#End Region

#Region "手动停止"
    Public Shared Sub ManualStopAll()

        TaskStatePool.Clear()

    End Sub
#End Region

    Public Shared Sub RemoveSingle(value As MangaChapterInfo)

        If TaskStatePool.Contains(value.Id) Then
            TaskStatePool.Remove(value.Id)
        End If

        LocalLiteDBHelper.Delete(value.Id)

    End Sub

End Class
