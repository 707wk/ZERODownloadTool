Imports System.Collections.ObjectModel
Imports System.Timers
Imports System.Web

Public Class NewTaskWindow

    Private ReadOnly FindMangaTimer As New Timers.Timer With {
        .Interval = 300,
        .AutoReset = False
    }

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        UIHelper.InitChildWindowStyle(Me)

        AddHandler FindMangaTimer.Elapsed, AddressOf FindMangaTimer_Elapsed

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

        Dim webClient As HtmlAgilityPack.HtmlWeb = New HtmlAgilityPack.HtmlWeb()
        Dim doc As HtmlAgilityPack.HtmlDocument = webClient.Load(MangaPageUrl.Text)

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
                .ChapterName = item.InnerText
                        })
        Next

        MangaChapterList.ItemsSource = tmpList

#End Region

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

End Class
