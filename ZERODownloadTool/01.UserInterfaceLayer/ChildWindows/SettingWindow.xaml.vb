Public Class SettingWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        UIHelper.InitChildWindowStyle(Me)

    End Sub

    Private Sub SelectSaveFolder(sender As Object, e As RoutedEventArgs)

        Dim tmpDialog As New Wangk.ResourceWPF.FolderBrowserDialog
        If tmpDialog.ShowDialog() <> Forms.DialogResult.OK Then
            Exit Sub
        End If

    End Sub
End Class
