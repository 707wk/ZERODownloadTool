Public Class SettingWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        UIHelper.InitChildWindowStyle(Me)

        UserName.Text = AppSettingHelper.UserName
        UserPassword.Password = AppSettingHelper.UserPassword

        SaveFolderPath.Text = AppSettingHelper.SaveFolderPath

    End Sub

    Private Sub SelectSaveFolder(sender As Object, e As RoutedEventArgs)

        Dim tmpDialog As New Wangk.ResourceWPF.FolderBrowserDialog
        If tmpDialog.ShowDialog() <> Forms.DialogResult.OK Then
            Exit Sub
        End If

        SaveFolderPath.Text = tmpDialog.DirectoryPath

    End Sub

    Private Sub Save(sender As Object, e As RoutedEventArgs)

        AppSettingHelper.UserName = UserName.Text
        AppSettingHelper.UserPassword = UserPassword.Password

        AppSettingHelper.SaveFolderPath = SaveFolderPath.Text

        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub Cancel(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class
