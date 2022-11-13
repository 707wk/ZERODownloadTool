Public Class SettingWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        UIHelper.InitChildWindowStyle(Me)

        UserName.Text = LocalLiteDBHelper.GetOption(Of String)(NameOf(UserName))
        UserPassword.Password = LocalLiteDBHelper.GetOption(Of String)(NameOf(UserPassword))

        SaveFolderPath.Text = LocalLiteDBHelper.GetOption(Of String)(NameOf(SaveFolderPath))

    End Sub

    Private Sub SelectSaveFolder(sender As Object, e As RoutedEventArgs)

        Dim tmpDialog As New Wangk.ResourceWPF.FolderBrowserDialog
        If tmpDialog.ShowDialog() <> Forms.DialogResult.OK Then
            Exit Sub
        End If

        SaveFolderPath.Text = tmpDialog.DirectoryPath

    End Sub

    Private Sub Save(sender As Object, e As RoutedEventArgs)

        LocalLiteDBHelper.UpdateOrAddOption(NameOf(UserName), UserName.Text)
        LocalLiteDBHelper.UpdateOrAddOption(NameOf(UserPassword), UserPassword.Password)

        LocalLiteDBHelper.UpdateOrAddOption(NameOf(SaveFolderPath), SaveFolderPath.Text)

        Me.Close()
    End Sub

    Private Sub Cancel(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class
