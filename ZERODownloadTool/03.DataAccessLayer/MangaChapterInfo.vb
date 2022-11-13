Public Class MangaChapterInfo

    Public Property Id As String = ServiceBaseLib.GUIDHelper.NewID

    ''' <summary>
    ''' 漫画名称
    ''' </summary>
    Public Property MangaName As String

    ''' <summary>
    ''' 章节名称
    ''' </summary>
    Public Property ChapterName As String

    ''' <summary>
    ''' 页面地址
    ''' </summary>
    Public Property PageUrl As String

    ''' <summary>
    ''' 保存的文件夹路径
    ''' </summary>
    Public Property SaveFolderPath As String

    ''' <summary>
    ''' 章节图片地址集合
    ''' </summary>
    Public Property Images As List(Of String) = New List(Of String)

    ''' <summary>
    ''' 是否选中
    ''' </summary>
    Public Property IsChecked As Boolean

End Class
