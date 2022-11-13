Imports System.Linq.Expressions
Imports LiteDB
Imports Newtonsoft.Json

''' <summary>
''' 本地数据库辅助模块
''' </summary>
Public Class LocalLiteDBHelper

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

        'Dim tmpCollection1 = Instance.GetCollection(Of DocumentInfo)(NameOf(DocumentInfo))
        'tmpCollection1.EnsureIndex(Function(x) x.TE001)
        'tmpCollection1.EnsureIndex(Function(x) x.TE002)
        'tmpCollection1.EnsureIndex(Function(x) x.TE003)

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

        If OptionExists(key) Then
            UpdateOption(key, JsonConvert.SerializeObject(value))
        Else
            AddOption(key, JsonConvert.SerializeObject(value))
        End If

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

#Region "DocumentInfo"

    'Public Shared Sub ClearDocumentInfo()

    '    Dim tmpCollection = Instance.GetCollection(Of DocumentInfo)(NameOf(DocumentInfo))
    '    tmpCollection.DeleteAll()

    'End Sub

    'Public Shared Sub Add(value As DocumentInfo)

    '    If String.IsNullOrWhiteSpace(value.TE001) OrElse
    '        String.IsNullOrWhiteSpace(value.TE001) OrElse
    '        String.IsNullOrWhiteSpace(value.TE001) Then

    '        Debug.WriteLine($"{value.TE001}-{value.TE002}-{value.TE003} 存在空值")
    '        Exit Sub
    '    End If

    '    If Exists(value) Then
    '        Debug.WriteLine($"{value.TE001}-{value.TE002}-{value.TE003} 记录已存在")
    '        Exit Sub
    '    End If

    '    Dim tmpCollection = Instance.GetCollection(Of DocumentInfo)(NameOf(DocumentInfo))
    '    tmpCollection.Insert(value)

    'End Sub

    'Public Shared Function Exists(value As DocumentInfo) As Boolean

    '    Dim tmpCollection = Instance.GetCollection(Of DocumentInfo)(NameOf(DocumentInfo))

    '    Dim tmpResult = tmpCollection.Query.Where(Function(x) x.TE001.Equals(value.TE001) AndAlso
    '                                                  x.TE002.Equals(value.TE002) AndAlso
    '                                                  x.TE003.Equals(value.TE003))

    '    Return tmpResult.Count > 0

    'End Function

    'Public Shared Function GetValue(value As DocumentInfo) As DocumentInfo

    '    Dim tmpCollection = Instance.GetCollection(Of DocumentInfo)(NameOf(DocumentInfo))

    '    Dim tmpResult = tmpCollection.Query.Where(Function(x) x.TE001.Equals(value.TE001) AndAlso
    '                                                  x.TE002.Equals(value.TE002) AndAlso
    '                                                  x.TE003.Equals(value.TE003)).First

    '    Return tmpResult

    'End Function

#End Region

End Class
