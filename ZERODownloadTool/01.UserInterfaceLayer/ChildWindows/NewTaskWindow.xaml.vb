Imports System.Collections.ObjectModel
Imports System.Timers
Imports System.Web
Imports Wangk.ResourceWPF

Public Class NewTaskWindow

    Private _TaskValues As New List(Of MangaChapterInfo)
    Public ReadOnly Property TaskValues As List(Of MangaChapterInfo)
        Get
            Return _TaskValues
        End Get
    End Property

    Private ReadOnly FindMangaTimer As New Timers.Timer With {
        .Interval = 300,
        .AutoReset = False
    }

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        WindowHelper.InitChildWindowStyle(Me)

        AddHandler FindMangaTimer.Elapsed, AddressOf FindMangaTimer_Elapsed

        Dim iData = Clipboard.GetDataObject()
        If iData.GetDataPresent(DataFormats.Text) AndAlso iData.GetData(DataFormats.Text).ToString.StartsWith("http") Then
            MangaPageUrl.Text = iData.GetData(DataFormats.Text)
        End If

    End Sub

    Private Sub MangaPageUrl_TextChanged(sender As Object, e As TextChangedEventArgs)

        FindMangaTimer.Stop()

        If String.IsNullOrWhiteSpace(MangaPageUrl.Text) Then
            Exit Sub
        End If

        FindMangaTimer.Start()

    End Sub

    Private Sub FindMangaTimer_Elapsed(sender As Object, e As ElapsedEventArgs)

        Dispatcher.Invoke(Threading.DispatcherPriority.Normal,
                          Sub()
                              GetMangaInfo()
                          End Sub)

    End Sub

    Private Sub GetMangaInfo()

        Try

            Dim tmpMangaPageUrl As New Uri(MangaPageUrl.Text)

            Dim webClient As New HtmlAgilityPack.HtmlWeb()
            Dim doc As HtmlAgilityPack.HtmlDocument = webClient.Load(tmpMangaPageUrl)

#Region "获取漫画名"
            Dim titleNodes As HtmlAgilityPack.HtmlNodeCollection = doc.DocumentNode.SelectNodes("//h3[@class='uk-heading-line mt10 m10']")

            If titleNodes Is Nothing Then
                Throw New Exception("未找到标题信息")
            End If

            Dim title As String = HttpUtility.HtmlDecode(titleNodes(0).InnerText)

            For Each invalidPathChar In System.IO.Path.GetInvalidFileNameChars
                title = title.Replace(invalidPathChar, "_")
            Next

            For Each invalidPathChar In System.IO.Path.GetInvalidPathChars
                title = title.Replace(invalidPathChar, "_")
            Next

            MangaName.Text = title
#End Region

#Region "获取章节信息"
            Dim ChapterNodes = doc.DocumentNode.SelectNodes("//a[@class='uk-button uk-button-default']")

            If ChapterNodes Is Nothing Then
                Throw New Exception("未找到章节信息")
            End If

            Dim tmpList As New ObservableCollection(Of MangaChapterInfo)

            For Each item In ChapterNodes
                tmpList.Add(New MangaChapterInfo With {
                .ChapterName = $"第 {item.InnerText} 话",
                .PageUrl = $"{tmpMangaPageUrl.Scheme}://{tmpMangaPageUrl.Host}{HttpUtility.HtmlDecode(item.Attributes("href").Value).Substring(1)}"
                        })
            Next

            MangaChapterList.ItemsSource = tmpList

#End Region

            AppSettingHelper.HostName = $"{tmpMangaPageUrl.Scheme}://{tmpMangaPageUrl.Host}"

        Catch ex As Exception
            Wangk.ResourceWPF.Toast.ShowError(Me, ex.Message)
        End Try

    End Sub

    Private Sub SelectAll(sender As Object, e As RoutedEventArgs)
        Dim tmpCheckBox As CheckBox = sender

        Dim tmpList As ObservableCollection(Of MangaChapterInfo) = MangaChapterList.ItemsSource

        If tmpList Is Nothing Then
            Exit Sub
        End If

        For Each item In tmpList
            item.IsChecked = tmpCheckBox.IsChecked
        Next

        MangaChapterList.Items.Refresh()

    End Sub

    Private Sub AddTask(sender As Object, e As RoutedEventArgs)

        Dim tmpList As ObservableCollection(Of MangaChapterInfo) = MangaChapterList.ItemsSource
        _TaskValues = (From item In tmpList
                       Where item.IsChecked
                       Select item).ToList

        For Each item In _TaskValues
            item.MangaName = MangaName.Text
            item.SaveFolderPath = IO.Path.Combine(LocalLiteDBHelper.GetOption(Of String)("SaveFolderPath"), item.MangaName, item.ChapterName)
            Console.WriteLine(item.SaveFolderPath)
        Next

        Me.DialogResult = True
    End Sub

    Private Sub Cancel(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        RemoveHandler FindMangaTimer.Elapsed, AddressOf FindMangaTimer_Elapsed
    End Sub

End Class
