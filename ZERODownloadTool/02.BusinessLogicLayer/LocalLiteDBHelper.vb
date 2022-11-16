Imports System.Linq.Expressions
Imports LiteDB
Imports Newtonsoft.Json

''' <summary>
''' 本地数据库辅助模块
''' </summary>
Public Class LocalLiteDBHelper

    'Private Shared ReadOnly LockObject2 As New Object

#Region "数据库连接实例"
    Private Shared ReadOnly LockObject As New Object

    Private Shared _Instance As LiteDatabase
    Private Shared ReadOnly Property Instance() As LiteDatabase
        Get
            ' 第一次判断
            If _Instance Is Nothing Then

                ' 排他锁
                SyncLock LockObject

                    ' 第二次判断
                    If _Instance Is Nothing Then

                        ' 连接数据库
                        Dim Path As String = IO.Path.GetDirectoryName(AppSettingHelper.LocalLiteDBFilePath)
                        System.IO.Directory.CreateDirectory(Path)

                        _Instance = New LiteDatabase(AppSettingHelper.LocalLiteDBFilePath)

                        Init()

                    End If

                End SyncLock

            End If

            Return _Instance
        End Get
    End Property
#End Region

#Region "关闭数据库"
    Public Shared Sub Close()

        Try
            _Instance?.Dispose()

        Catch ex As Exception

        Finally
            _Instance = Nothing
        End Try

    End Sub
#End Region

#Region "初始化数据库"
    ''' <summary>
    ''' 初始化数据库
    ''' </summary>
    Public Shared Sub Init()

        Dim tmpCollection = Instance.GetCollection(Of AppConfigInfo)(NameOf(AppConfigInfo))
        tmpCollection.EnsureIndex(Function(x) x.Key)

        Dim tmpCollection1 = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        tmpCollection1.EnsureIndex(Function(x) x.Id)

    End Sub
#End Region

#Region "AppSettingInfo"

    ''' <summary>
    ''' 配置值是否存在
    ''' </summary>
    Public Shared Function OptionExists(key As String) As Boolean

        Dim tmpCollection = Instance.GetCollection(Of AppConfigInfo)(NameOf(AppConfigInfo))

        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.Key.Equals(key))

        Return tmpResult.Count > 0

    End Function

    ''' <summary>
    ''' 设置配置值
    ''' </summary>
    Public Shared Sub UpdateOrAddOption(key As String, value As Object)
        'SyncLock LockObject2

        If OptionExists(key) Then
                UpdateOption(key, JsonConvert.SerializeObject(value))
            Else
                AddOption(key, JsonConvert.SerializeObject(value))
            End If

        'End SyncLock
    End Sub

    ''' <summary>
    ''' 新增配置值
    ''' </summary>
    Private Shared Sub AddOption(key As String, value As String)

        Dim tmpCollection = Instance.GetCollection(Of AppConfigInfo)(NameOf(AppConfigInfo))

        tmpCollection.Insert(New AppConfigInfo With {.Key = key, .Value = value})

    End Sub

    ''' <summary>
    ''' 更新配置值
    ''' </summary>
    Private Shared Sub UpdateOption(key As String, value As String)

        Dim tmpCollection = Instance.GetCollection(Of AppConfigInfo)(NameOf(AppConfigInfo))

        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.Key.Equals(key)).First

        tmpResult.Value = value

        tmpCollection.Update(tmpResult)

    End Sub

    ''' <summary>
    ''' 获取配置值
    ''' </summary>
    Public Shared Function GetOption(Of T)(key As String) As T

        Dim tmpCollection = Instance.GetCollection(Of AppConfigInfo)(NameOf(AppConfigInfo))

        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.Key.Equals(key))

        If tmpResult.Count = 0 Then
            Return Nothing
        End If

        Return JsonConvert.DeserializeObject(Of T)(tmpResult.First.Value)

    End Function

#End Region

#Region "MangaChapterInfo"

    ''' <summary>
    ''' 初始化下载状态
    ''' </summary>
    Public Shared Sub InitMangaChapterInfoState()
        'SyncLock LockObject2

        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
            Dim tmpResult = tmpCollection.Query.Where(Function(x) x.State = MangaChapterInfo.TaskState.Downloading).ToList

            For Each item In tmpResult
                item.State = MangaChapterInfo.TaskState.Waiting
                tmpCollection.Update(item)
            Next

        'End SyncLock
    End Sub

    Public Shared Function GetDownloadingMangaChapterCount() As Integer
        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.State = MangaChapterInfo.TaskState.Downloading)

        Return tmpResult.Count
    End Function

    Public Shared Function GetDownloadingMangaChapterInfo() As List(Of MangaChapterInfo)

        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.State <> MangaChapterInfo.TaskState.Completed).OrderBy(Function(x) x.CreateTime)

        Return tmpResult.ToList()
    End Function

    Public Shared Function GetCompletedMangaChapterInfo() As List(Of MangaChapterInfo)
        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.State = MangaChapterInfo.TaskState.Completed).OrderBy(Function(x) x.CompletedTime)

        Return tmpResult.ToList()
    End Function

    Public Shared Function GetWaitingMangaChapterInfo() As List(Of MangaChapterInfo)
        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.State = MangaChapterInfo.TaskState.Waiting)

        Return tmpResult.ToList
    End Function

    Public Shared Sub Add(value As MangaChapterInfo)
        'SyncLock LockObject2

        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
            tmpCollection.Insert(value)

        'End SyncLock
    End Sub

    Public Shared Sub Update(value As MangaChapterInfo)
        'SyncLock LockObject2

        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
            tmpCollection.Update(value)

        'End SyncLock
    End Sub

    Public Shared Sub Delete(id As String)
        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        tmpCollection.Delete(id)
    End Sub

    Public Shared Sub ClearCompletedMangaChapterInfo()
        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.State = MangaChapterInfo.TaskState.Completed).OrderBy(Function(x) x.CompletedTime)

        For Each item In tmpResult.ToList
            tmpCollection.Delete(item.Id)
        Next

    End Sub

    ''' <summary>
    ''' 设置未完成任务状态
    ''' </summary>
    Public Shared Sub SetAllMangaChapterInfoState(value As MangaChapterInfo.TaskState)
        Dim tmpCollection = Instance.GetCollection(Of MangaChapterInfo)(NameOf(MangaChapterInfo))
        Dim tmpResult = tmpCollection.Query.Where(Function(x) x.State <> MangaChapterInfo.TaskState.Completed).ToList

        For Each item In tmpResult
            item.RetriesCount = 0
            item.ErrorMsg = String.Empty
            item.State = value
            tmpCollection.Update(item)
        Next

    End Sub

#End Region

End Class
