Imports System.ComponentModel

Public Class MangaChapterInfo
    Implements INotifyPropertyChanged

    ''' <summary>
    ''' 任务状态
    ''' </summary>
    Public Enum TaskState
        ''' <summary>
        ''' 等待下载
        ''' </summary>
        Waiting
        ''' <summary>
        ''' 下载中
        ''' </summary>
        Downloading
        ''' <summary>
        '''  ''' 暂停下载
        ''' </summary>
        StopDownload
        ''' <summary>
        ''' 下载完成
        ''' </summary>
        Completed
    End Enum

    Public Property Id As String = Wangk.Hash.IDHelper.NewID

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
    <LiteDB.BsonIgnore>
    Public ReadOnly Property Count As Integer
        Get
            Return If(Images.Count > 0, Images.Count, 1)
        End Get
    End Property

    ''' <summary>
    ''' 是否选中
    ''' </summary>
    <LiteDB.BsonIgnore>
    Public Property IsChecked As Boolean

    ''' <summary>
    ''' 创建时间
    ''' </summary>
    Public Property CreateTime As DateTime = Now

    ''' <summary>
    ''' 完成时间
    ''' </summary>
    Public Property CompletedTime As DateTime

    Private _CompletedCount As Integer
    ''' <summary>
    ''' 已下载图片数
    ''' </summary>
    Public Property CompletedCount As Integer
        Get
            Return _CompletedCount
        End Get
        Set
            _CompletedCount = Value

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(CompletedCount)))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Count)))
        End Set
    End Property

    Private _State As Integer = TaskState.Waiting
    ''' <summary>
    ''' 任务状态
    ''' </summary>
    Public Property State As Integer
        Get
            Return _State
        End Get
        Set
            _State = Value

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(StateStr)))
        End Set
    End Property

    ''' <summary>
    ''' 任务状态字符
    ''' </summary>
    <LiteDB.BsonIgnore>
    Public ReadOnly Property StateStr As String
        Get
            Return {"等待下载", "下载中", "暂停下载", "下载完成"}(State)
        End Get
    End Property

    Private _ErrorMsg As String
    ''' <summary>
    ''' 错误消息
    ''' </summary>
    Public Property ErrorMsg As String
        Get
            Return _ErrorMsg
        End Get
        Set
            _ErrorMsg = Value

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(ErrorMsg)))
        End Set
    End Property

    <LiteDB.BsonIgnore>
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

End Class
