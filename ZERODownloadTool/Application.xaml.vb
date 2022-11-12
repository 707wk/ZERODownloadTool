Imports System.Windows.Threading

Class Application

    ' 应用程序级事件(例如 Startup、Exit 和 DispatcherUnhandledException)
    ' 可以在此文件中进行处理。
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException

        Application_Exit(Nothing, Nothing)

        AppSettingHelper.Instance.Logger.Error(e.Exception)

        MsgBox($"应用程序中发生了未处理的异常 :
{e.Exception.Message}

点击""确定"", 应用程序将立即关闭, 具体异常信息可在 \Logs\Error 文件夹内查看",
               MsgBoxStyle.Critical)

    End Sub

    Private Sub Application_Exit(sender As Object, e As ExitEventArgs) Handles Me.[Exit]

        'AppSettingHelper.SaveToLocaltion()
        AppSettingHelper.Instance.ClearTempFiles()

    End Sub

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup

        ' 单例模式
        Dim tmpProcess = Process.GetCurrentProcess()
        Dim processCount = Process.GetProcessesByName(tmpProcess.ProcessName).Count()
        ' 有多个实例则退出程序
        If processCount > 1 Then
            Application.Current.Shutdown()
        End If

    End Sub

End Class
