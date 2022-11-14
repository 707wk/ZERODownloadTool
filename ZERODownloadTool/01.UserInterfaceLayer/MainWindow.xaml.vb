Imports System.IO
Imports System.Timers
Imports Newtonsoft.Json.Linq
Imports ZERODownloadTool.MangaChapterInfo

Class MainWindow

    Private ReadOnly UpdateUITimer As New Timers.Timer With {
        .Interval = 1000
    }

    Public Shared NeedUpdateDownloadingMangaChapterlist As Boolean

    Public Shared NeedUpdateCompletedMangaChapterlist As Boolean

    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        Title = $"{My.Application.Info.Title} V{AppSettingHelper.Instance.ProductVersion}"

    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        Dim tmpAppCenterSparkle As New AppCenterSparkle(AppSettingHelper.AppKey, Me)
        tmpAppCenterSparkle.CheckUpdateAsync()

        LocalLiteDBHelper.InitMangaChapterInfoState()
        'UpdateDownloadingMangaChapterlist()
        'UpdateCompletedMangaChapterlist()
        NeedUpdateDownloadingMangaChapterlist = True
        NeedUpdateCompletedMangaChapterlist = True
        'DownloadTaskHelper.Login()

        AddHandler UpdateUITimer.Elapsed, AddressOf UpdateUITimer_Elapsed
        UpdateUITimer.Start()

        If String.IsNullOrWhiteSpace(AppSettingHelper.UserName) Then
            Dim tmpWindow As New SettingWindow With {
                .Owner = Me
            }
            tmpWindow.ShowDialog()
        End If

    End Sub

    Private Sub UpdateUITimer_Elapsed(sender As Object, e As ElapsedEventArgs)

        Dispatcher.Invoke(Threading.DispatcherPriority.Normal,
                          Sub()
                              If NeedUpdateDownloadingMangaChapterlist Then
                                  NeedUpdateDownloadingMangaChapterlist = False
                                  UpdateDownloadingMangaChapterlist()
                              End If

                              If NeedUpdateCompletedMangaChapterlist Then
                                  NeedUpdateCompletedMangaChapterlist = False
                                  UpdateCompletedMangaChapterlist()
                              End If

                          End Sub)

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        DownloadTaskHelper.ManualStopAll()
    End Sub

    Private Sub NewTask(sender As Object, e As RoutedEventArgs)

        Dim tmpWindow As New NewTaskWindow With {
            .Owner = Me
        }
        If Not tmpWindow.ShowDialog() Then
            Exit Sub
        End If

        For Each item In tmpWindow.TaskValues
            LocalLiteDBHelper.Add(item)
        Next

        UpdateDownloadingMangaChapterlist()

        DownloadTaskHelper.AutoStartALL()

        Wangk.ResourceWPF.Toast.ShowSuccess(Me, "添加成功")

    End Sub

    Private Sub AllTaskStart(sender As Object, e As RoutedEventArgs)
        DownloadTaskHelper.ManualStartAll()
    End Sub

    Private Sub AllTaskStop(sender As Object, e As RoutedEventArgs)
        DownloadTaskHelper.ManualStopAll()
        LocalLiteDBHelper.InitMangaChapterInfoState()
    End Sub

    Private Sub ClearCompleted(sender As Object, e As RoutedEventArgs)
        LocalLiteDBHelper.ClearCompletedMangaChapterInfo()
        UpdateCompletedMangaChapterlist()

        Wangk.ResourceWPF.Toast.ShowSuccess(Me, "清空完毕")
    End Sub

    Private Sub OpenSettingWindow(sender As Object, e As RoutedEventArgs)

        Dim tmpWindow As New SettingWindow With {
            .Owner = Me
        }
        tmpWindow.ShowDialog()

    End Sub

    Private Sub UpdateDownloadingMangaChapterlist()
        DownloadingMangaChapterlist.ItemsSource = LocalLiteDBHelper.GetDownloadingMangaChapterInfo
        DownloadingMangaChapterlist.Items.Refresh()
    End Sub

    Private Sub UpdateCompletedMangaChapterlist()
        CompletedMangaChapterlist.ItemsSource = LocalLiteDBHelper.GetCompletedMangaChapterInfo
        CompletedMangaChapterlist.Items.Refresh()
    End Sub

    Private Sub ListBox_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)

        Dim source As DependencyObject = e.OriginalSource
        While source IsNot Nothing AndAlso
            TypeOf source IsNot ListBoxItem

            source = VisualTreeHelper.GetParent(source)
        End While

        If source Is Nothing Then
            e.Handled = True
            Exit Sub
        End If

        Dim tmpItem As ListBoxItem = source

        If tmpItem IsNot Nothing Then
            tmpItem.IsSelected = True
            'tmpItem.Focus()
            'e.Handled = True
        End If

    End Sub

    Private Sub DownloadingMangaChapterlist_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
        Dim eventArg = New MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        eventArg.RoutedEvent = UIElement.MouseWheelEvent
        eventArg.Source = sender
        DownloadingMangaChapterlist.RaiseEvent(eventArg)
    End Sub

    Private Sub RetryDownload(sender As Object, e As RoutedEventArgs)

        Dim selectedNode As MangaChapterInfo = DownloadingMangaChapterlist.SelectedItem
        DownloadTaskHelper.RetrySingleDownload(selectedNode)

    End Sub

    Private Sub StopDownload(sender As Object, e As RoutedEventArgs)

        Dim selectedNode As MangaChapterInfo = DownloadingMangaChapterlist.SelectedItem
        DownloadTaskHelper.StopSingleDownload(selectedNode)

    End Sub

    Private Sub StartDownload(sender As Object, e As RoutedEventArgs)

        Dim selectedNode As MangaChapterInfo = DownloadingMangaChapterlist.SelectedItem
        DownloadTaskHelper.StartSingleDownload(selectedNode)

    End Sub

    Private Sub OpenDownloadFolder(sender As Object, e As RoutedEventArgs)

        Dim selectedNode As MangaChapterInfo = DownloadingMangaChapterlist.SelectedItem

        If Not IO.Directory.Exists(selectedNode.SaveFolderPath) Then
            Wangk.ResourceWPF.Toast.ShowWarning(Me, $"{selectedNode.SaveFolderPath} 路径不存在")
            Exit Sub
        End If

        FileHelper.Open(selectedNode.SaveFolderPath)

    End Sub

    Private Sub Remove(sender As Object, e As RoutedEventArgs)

        Dim selectedNode As MangaChapterInfo = DownloadingMangaChapterlist.SelectedItem
        DownloadTaskHelper.RemoveSingle(selectedNode)

        UpdateDownloadingMangaChapterlist()

    End Sub

    Private Sub CompletedMangaChapterlist_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
        Dim eventArg = New MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        eventArg.RoutedEvent = UIElement.MouseWheelEvent
        eventArg.Source = sender
        CompletedMangaChapterlist.RaiseEvent(eventArg)
    End Sub

    Private Sub OpenCompletedFolder(sender As Object, e As RoutedEventArgs)

        Dim selectedNode As MangaChapterInfo = CompletedMangaChapterlist.SelectedItem
        If Not IO.Directory.Exists(selectedNode.SaveFolderPath) Then
            Wangk.ResourceWPF.Toast.ShowWarning(Me, $"{selectedNode.SaveFolderPath} 路径不存在")
            Exit Sub
        End If

        FileHelper.Open(selectedNode.SaveFolderPath)

    End Sub

    Private Sub RemoveCompleted(sender As Object, e As RoutedEventArgs)

        Dim selectedNode As MangaChapterInfo = CompletedMangaChapterlist.SelectedItem
        DownloadTaskHelper.RemoveSingle(selectedNode)

        UpdateCompletedMangaChapterlist()

    End Sub

End Class
