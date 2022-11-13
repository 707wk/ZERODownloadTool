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
    ''' 章节图片地址集合, 图片地址-是否已下载
    ''' </summary>
    Public Property Images As Dictionary(Of String, Boolean) = New Dictionary(Of String, Boolean)

    ''' <summary>
    ''' 总图片数
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return If(Images.Count > 0, Images.Count, 1)
        End Get
    End Property

    ''' <summary>
    ''' 是否选中
    ''' </summary>
    Public Property IsChecked As Boolean

    ''' <summary>
    ''' 创建时间
    ''' </summary>
    Public Property CreateTime As DateTime = Now

    ''' <summary>
    ''' 是否已完成
    ''' </summary>
    Public Property Completed As Boolean

    ''' <summary>
    ''' 完成时间
    ''' </summary>
    Public Property CompletedTime As DateTime

    ''' <summary>
    ''' 已下载图片数
    ''' </summary>
    Public Property CompletedCount As Integer

End Class
