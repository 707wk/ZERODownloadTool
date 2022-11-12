Class MainWindow
    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        Title = $"{My.Application.Info.Title} V{AppSettingHelper.Instance.ProductVersion} {AppSettingHelper.Instance.DeviceID}"

    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub OpenSettingWindow(sender As Object, e As RoutedEventArgs)

        Dim tmpWindow As New SettingWindow With {
            .Owner = Me
        }
        tmpWindow.ShowDialog()

    End Sub

End Class
