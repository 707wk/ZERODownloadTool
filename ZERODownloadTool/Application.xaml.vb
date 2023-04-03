Imports System.Globalization
Imports System.Threading
Imports System.Windows.Threading
Imports Microsoft.AppCenter
Imports Microsoft.AppCenter.Analytics

Class Application

    ''' <summary>
    ''' 单例进程
    ''' </summary>
    Private SingleInstanceMutex As Mutex

    Private Sub Application_DispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException

        Application_Exit(Nothing, Nothing)

        AppSettingHelper.Logger.Error(e.Exception)

        MsgBox($"应用程序中发生了未处理的异常 :
{e.Exception.Message}

点击""确定"", 应用程序将立即关闭, 具体异常信息可在 \Logs\Error 文件夹内查看",
               MsgBoxStyle.Critical)

    End Sub

    Private Sub Application_Exit(sender As Object, e As ExitEventArgs) Handles Me.[Exit]

        ' 不是第一个实例则不做退出处理
        If SingleInstanceMutex Is Nothing Then
            Exit Sub
        End If

        AppSettingHelper.ClearTempFiles()

        LocalLiteDBHelper.Close()

    End Sub

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        'Wangk.ResourceWPF.ConsoleDebug.Open()

        ' 单例模式
        Dim createSuccess As Boolean
        SingleInstanceMutex = New Mutex(True, $"SingleInstance {AppSettingHelper.GUID}", createSuccess)
        ' 有多个实例则退出程序
        If Not createSuccess Then

            SingleInstanceMutex = Nothing

            Application.Current.Shutdown()
        End If

        Dim countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName
        AppCenter.SetCountryCode(countryCode)

        '使用调试器时不记录数据
        Analytics.SetEnabledAsync(Not Debugger.IsAttached)

        AppCenter.Start(AppSettingHelper.AppKey,
                        GetType(Analytics))

        AppSettingHelper.Init()

    End Sub

End Class
