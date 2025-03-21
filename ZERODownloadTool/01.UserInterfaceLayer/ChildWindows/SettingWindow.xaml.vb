Imports Microsoft.WindowsAPICodePack.Dialogs
Imports Wangk.ResourceWPF

Public Class SettingWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        WindowHelper.InitChildWindowStyle(Me)

        UserName.Text = AppSettingHelper.UserName
        UserPassword.Password = AppSettingHelper.UserPassword

        SaveFolderPath.Text = AppSettingHelper.SaveFolderPath

        For i001 = 1 To 50
            DownloadComicChapterCount.Items.Add(i001)
        Next
        DownloadComicChapterCount.SelectedIndex = AppSettingHelper.DownloadComicChapterCount - 1

    End Sub

    Private Sub SelectSaveFolder(sender As Object, e As RoutedEventArgs)

        Dim tmpDialog As New CommonOpenFileDialog() With {
            .IsFolderPicker = True
        }
        If tmpDialog.ShowDialog() <> CommonFileDialogResult.Ok Then
            Exit Sub
        End If

        SaveFolderPath.Text = tmpDialog.FileName

    End Sub

    Private Sub Save(sender As Object, e As RoutedEventArgs)

        AppSettingHelper.UserName = UserName.Text
        AppSettingHelper.UserPassword = UserPassword.Password

        AppSettingHelper.SaveFolderPath = SaveFolderPath.Text

        AppSettingHelper.DownloadComicChapterCount = DownloadComicChapterCount.SelectedIndex + 1

        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub Cancel(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class
