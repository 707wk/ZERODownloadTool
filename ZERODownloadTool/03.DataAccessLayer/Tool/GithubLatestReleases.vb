Imports System.Net

Public Class GithubLatestReleases

    Private ReadOnly appCastUrlStr As String

    Private ReadOnly currentVersion As String

    Private ReadOnly MainUI As Window

    Public Sub New(repositoryName As String, MainWindow As Window)

        If String.IsNullOrWhiteSpace(repositoryName) Then Throw New Exception($"{Wangk.Hash.CodeHelper.GetLocation}: repositoryName 不能为空")
        appCastUrlStr = $"https://api.github.com/repos/{repositoryName}/releases/latest"

        If MainWindow Is Nothing Then Throw New Exception($"{Wangk.Hash.CodeHelper.GetLocation}: MainWindow 不能为空")
        MainUI = MainWindow

        Dim assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location
        currentVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion

        CheckUpdateAsync()

    End Sub

    ''' <summary>
    ''' 检查更新
    ''' </summary>
    Private Sub CheckUpdateAsync()

        ' win7系统忽略SSL验证
        ServicePointManager.ServerCertificateValidationCallback = Function(sender, certificate, chain, sslPolicyErrors)
                                                                      Return True
                                                                  End Function

        Task.Run(Sub()

                     Console.WriteLine($"{Now:G} : 检查更新")

                     Try

                         Dim latestReleases = WebAPIHelper.GetDataWithJWT(Of GithubReleases)(appCastUrlStr, "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")

                         ' 比较版本号大小
                         If Wangk.ResourceWPF.StringHelper.StrCmpLogical(currentVersion, latestReleases.Tag_name) >= 0 Then
                             Console.WriteLine("不需升级")

                         Else

                             If CType(MainUI.Dispatcher.Invoke(Function()

                                                                   Return MsgBox($"有新版本发布
当前版本 : {currentVersion}
最新版本 : {latestReleases.Tag_name}
发布日期 : {latestReleases.Published_at:d}
更新说明 :
{latestReleases.Body}
是否更新 ?", MsgBoxStyle.YesNo Or MsgBoxStyle.Information, "升级提醒")

                                                               End Function), MsgBoxResult) = MsgBoxResult.Yes Then

                                 Process.Start("DownloadUpdate.exe", $"""{latestReleases.Assets.First().Browser_download_url}"" ""{System.Reflection.Assembly.GetExecutingAssembly().Location}""")

                                 End
                             Else
                                 '退出检测
                                 Console.WriteLine("退出升级检测")
                                 Exit Sub
                             End If

                         End If

                     Catch ex As Exception
                         Console.WriteLine(ex)
                     End Try

                 End Sub)

    End Sub

    Public Class GithubReleases
        Public Property Tag_name As String
        Public Property Published_at As DateTime
        Public Property Assets As List(Of Asset)
        Public Property Body As String
    End Class

    Public Class Asset
        Public Property Browser_download_url As String
    End Class

End Class
