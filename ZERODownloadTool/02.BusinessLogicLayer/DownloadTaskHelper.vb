Imports ZERODownloadTool.MangaChapterInfo
''' <summary>
''' 下载任务辅助模块
''' </summary>
Public Class DownloadTaskHelper

    Public Shared MaxThreadCount As Integer = 5

    Private Shared TaskStatePool As New Dictionary(Of String, TaskState)

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
