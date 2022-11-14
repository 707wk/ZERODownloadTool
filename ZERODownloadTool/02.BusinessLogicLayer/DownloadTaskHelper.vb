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

    Public Shared MaxThreadCount As Integer = 5

    Private Shared TaskStatePool As New Dictionary(Of String, TaskState)

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

                        Dim tmpHttpClientInstance = New HttpClient(tmpHttpClientHandler)

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

#Region "手动开始全部"
    Public Shared Sub ManualStartAll()

        LocalLiteDBHelper.SetAllMangaChapterInfoState(TaskState.Waiting)

        AutoStartALL()

    End Sub
#End Region

#Region "自动开始"
    Public Shared Sub AutoStartALL()

        Dim TaskCount = LocalLiteDBHelper.GetDownloadingMangaChapterCount()
        If TaskCount > 5 Then
            Exit Sub
        End If

        Dim tmpList = LocalLiteDBHelper.GetWaitingMangaChapterInfo.Take(5 - TaskCount)

        For Each item In tmpList
            Dim tmpTask = Task.Run(Sub()
                                       StartDownload(item)
                                   End Sub)

            TaskStatePool.Add(item.Id, TaskState.Downloading)
        Next

    End Sub
#End Region

    Private Shared Async Sub StartDownload(value As MangaChapterInfo)
        value.State = MangaChapterInfo.TaskState.Downloading
        LocalLiteDBHelper.Update(value)

        ' 标记主界面下载列表更新
        MainWindow.NeedUpdateDownloadingMangaChapterlist = True

        Try

            If value.Images.Count = 0 Then
                ' 获取图片地址
                GetMangaChapterImages(value)
            End If

            ' 标记主界面下载列表更新
            MainWindow.NeedUpdateDownloadingMangaChapterlist = True

            Dim tmpImageList = (From item In value.Images
                                Where Not item.Value
                                Select item.Key).ToList

            For Each item In tmpImageList

                ' 标记主界面下载列表更新
                MainWindow.NeedUpdateDownloadingMangaChapterlist = True

                If TaskStatePool(value.Id) <> TaskState.Downloading Then
                    Exit For
                End If

                ' 下载图片
                ' 创建文件夹
                IO.Directory.CreateDirectory(value.SaveFolderPath)
                Dim fileName = IO.Path.GetFileName(item)

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

            Dim tmpTaskState = TaskStatePool(value.Id)
            TaskStatePool.Remove(value.Id)

            If tmpTaskState <> TaskState.StopDownload Then
                ' 下载完成则标记为结束
                value.State = MangaChapterInfo.TaskState.Completed
                LocalLiteDBHelper.Update(value)
                AutoStartALL()

                ' 标记主界面已完成列表更新
                MainWindow.NeedUpdateCompletedMangaChapterlist = True
            Else
                ' 手动暂停下载
                value.State = MangaChapterInfo.TaskState.StopDownload
                LocalLiteDBHelper.Update(value)
            End If

        Catch ex As Exception
            ' 发生异常时停止本章节下载, 启动下载其他章节
            TaskStatePool.Remove(value.Id)
            value.ErrorMsg = ex.Message
            value.State = MangaChapterInfo.TaskState.StopDownload
            LocalLiteDBHelper.Update(value)

            AutoStartALL()
        End Try

        ' 标记主界面下载列表更新
        MainWindow.NeedUpdateDownloadingMangaChapterlist = True

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

        For Each item In TaskStatePool.Keys.ToList
            TaskStatePool(item) = TaskState.StopDownload
        Next

        LocalLiteDBHelper.SetAllMangaChapterInfoState(TaskState.StopDownload)

        ' 标记主界面下载列表更新
        MainWindow.NeedUpdateDownloadingMangaChapterlist = True

    End Sub
#End Region

    Public Shared Sub RetrySingleDownload(value As MangaChapterInfo)

        value.ErrorMsg = String.Empty
        value.State = TaskState.Waiting
        LocalLiteDBHelper.Update(value)

    End Sub

    Public Shared Sub StopSingleDownload(value As MangaChapterInfo)

        If TaskStatePool.ContainsKey(value.Id) Then
            TaskStatePool(value.Id) = TaskState.StopDownload
        Else
            value.State = TaskState.StopDownload
            LocalLiteDBHelper.Update(value)
        End If

    End Sub

    Public Shared Sub StartSingleDownload(value As MangaChapterInfo)

        value.ErrorMsg = String.Empty
        value.State = TaskState.Waiting
        LocalLiteDBHelper.Update(value)

        AutoStartALL()
    End Sub

    Public Shared Sub RemoveSingle(value As MangaChapterInfo)

        If TaskStatePool.ContainsKey(value.Id) Then
            TaskStatePool(value.Id) = TaskState.StopDownload
        End If

        LocalLiteDBHelper.Delete(value.Id)

    End Sub

End Class
