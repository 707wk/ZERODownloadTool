Class MainWindow
    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        Title = $"{My.Application.Info.Title} V{AppSettingHelper.Instance.ProductVersion}"

    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        Dim tmpAppCenterSparkle As New AppCenterSparkle(AppSettingHelper.AppKey, Me)
        tmpAppCenterSparkle.CheckUpdateAsync()

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub NewTask(sender As Object, e As RoutedEventArgs)

        Dim tmpWindow As New NewTaskWindow With {
           .Owner = Me
       }
        tmpWindow.ShowDialog()

    End Sub

    Private Sub OpenSettingWindow(sender As Object, e As RoutedEventArgs)

        Dim tmpWindow As New SettingWindow With {
            .Owner = Me
        }
        tmpWindow.ShowDialog()

    End Sub

End Class
