Imports System.Collections.Specialized
Imports System.Net
Imports System.Net.Http
Imports System.Security.Policy
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Imports LiteDB
Imports ZERODownloadTool.MangaChapterInfo
''' <summary>
''' 下载任务辅助模块
''' </summary>
Public Class DownloadTaskHelper

    Public Shared MaxThreadCount As Integer = 5

    Private Shared TaskStatePool As New Dictionary(Of String, TaskState)

#Region "HttpClientHandler"
    Private Shared ReadOnly LockObject As New Object
    Private Shared _HttpClientHandlerInstance As HttpClientHandler
    Private Shared ReadOnly Property HttpClientHandlerInstance() As HttpClientHandler
        Get
            ' 第一次判断
            If _HttpClientHandlerInstance Is Nothing Then

                ' 排他锁
                SyncLock LockObject

                    ' 第二次判断
                    If _HttpClientHandlerInstance Is Nothing Then

                        _HttpClientHandlerInstance = New HttpClientHandler With {
                            .AllowAutoRedirect = True,
                            .UseCookies = True,
                            .CookieContainer = New CookieContainer
                        }

                    End If

                End SyncLock

            End If

            Return _HttpClientHandlerInstance
        End Get
    End Property
#End Region

#Region "HttpClientInstance"
    Private Shared ReadOnly LockObject2 As New Object
    Private Shared _HttpClientInstance As HttpClient
    Private Shared ReadOnly Property HttpClientInstance() As HttpClient
        Get
            ' 第一次判断
            If _HttpClientInstance Is Nothing Then

                ' 排他锁
                SyncLock LockObject2

                    ' 第二次判断
                    If _HttpClientInstance Is Nothing Then

                        _HttpClientInstance = New HttpClient(HttpClientHandlerInstance)

                    End If

                End SyncLock

            End If

            Return _HttpClientInstance
        End Get
    End Property
#End Region

    Public Shared Sub Login()
        Console.WriteLine(HttpClientHandlerInstance.CookieContainer.Count)

        Dim tmpResponse = HttpClientInstance.GetAsync(AppSettingHelper.HostName).GetAwaiter.GetResult
        tmpResponse.EnsureSuccessStatusCode()
        Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult

        Console.WriteLine(HttpClientHandlerInstance.CookieContainer.Count)

        Dim doc As New HtmlAgilityPack.HtmlDocument
        doc.LoadHtml(contentStr)
        Dim formhashNodes = doc.DocumentNode.SelectNodes("//input[@name='formhash']")
        Dim formhashStr = formhashNodes.First.Attributes("value").Value

        'Dim tmpMultipartFormDataContent = New MultipartFormDataContent
        'tmpMultipartFormDataContent.Headers.Add("ContentType", "application/x-www-form-urlencoded")
        'tmpMultipartFormDataContent.Add(New StringContent("username"), "fastloginfield")
        'tmpMultipartFormDataContent.Add(New StringContent(AppSettingHelper.UserName), "username")
        'tmpMultipartFormDataContent.Add(New StringContent(AppSettingHelper.UserPassword), "password")
        'tmpMultipartFormDataContent.Add(New StringContent(formhashStr), "formhash")
        'tmpMultipartFormDataContent.Add(New StringContent("yes"), "quickforward")
        'tmpMultipartFormDataContent.Add(New StringContent("ls"), "handlekey")

        'Dim tmpDictionary As New Dictionary(Of String, String) From {
        '    {"fastloginfield", "username"},
        '    {"username", AppSettingHelper.UserName},
        '    {"password", AppSettingHelper.UserPassword},
        '    {"formhash", formhashStr},
        '    {"quickforward", "yes"},
        '    {"handlekey", "ls"}
        '}
        'Dim req = New HttpRequestMessage(HttpMethod.Post,
        '                                 $"{AppSettingHelper.HostName}/member.php?mod=logging&action=login&loginsubmit=yes&infloat=yes&lssubmit=yes&inajax=1") With {
        '                                 .Content = New FormUrlEncodedContent(tmpDictionary)
        '}

        Dim tmp = New FormUrlEncodedContent({
            New KeyValuePair(Of String, String)("fastloginfield", "username"),
            New KeyValuePair(Of String, String)("username", AppSettingHelper.UserName),
            New KeyValuePair(Of String, String)("password", AppSettingHelper.UserPassword),
            New KeyValuePair(Of String, String)("formhash", formhashStr),
            New KeyValuePair(Of String, String)("quickforward", "yes"),
            New KeyValuePair(Of String, String)("handlekey", "ls")
                                            })

        Dim tmpResponse2 = HttpClientInstance.PostAsync($"{AppSettingHelper.HostName}/member.php?mod=logging&action=login&loginsubmit=yes&infloat=yes&lssubmit=yes&inajax=1", tmp).GetAwaiter.GetResult
        tmpResponse2.EnsureSuccessStatusCode()
        contentStr = tmpResponse2.Content.ReadAsStringAsync().GetAwaiter.GetResult

        Console.WriteLine(HttpClientHandlerInstance.CookieContainer.Count)
    End Sub

    Public Shared Sub AllStart()

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

    Private Shared Sub StartDownload(value As MangaChapterInfo)
        value.State = MangaChapterInfo.TaskState.Downloading
        LocalLiteDBHelper.Update(value)

        If value.Images.Count = 0 Then
            ' 获取图片地址
        End If

        Dim tmpImageList = From item In value.Images
                           Where Not item.Value
                           Select item.Key

        For Each item In tmpImageList

            If TaskStatePool(value.Id) <> TaskState.Downloading Then
                Exit For
            End If

            ' 下载图片


            value.Images(item) = True
            value.CompletedCount += 1

            LocalLiteDBHelper.Update(value)
        Next

        TaskStatePool.Remove(value.Id)

        value.State = MangaChapterInfo.TaskState.Completed
        LocalLiteDBHelper.Update(value)

        AllStart()
    End Sub

    Public Shared Sub AllStop()

        For Each item In TaskStatePool.Keys
            TaskStatePool(item) = TaskState.StopDownload
        Next

    End Sub

End Class
