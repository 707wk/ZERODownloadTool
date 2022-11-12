Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
''' <summary>
''' 全局配置辅助类
''' </summary>
Public Class AppSettingHelper
    Private Sub New()
    End Sub

    Public Const AppKey = ""

#Region "程序集GUID"
    <Newtonsoft.Json.JsonIgnore>
    Private _GUID As String
    ''' <summary>
    ''' 程序集GUID
    ''' </summary>
    <Newtonsoft.Json.JsonIgnore>
    Public ReadOnly Property GUID As String
        Get
            Return _GUID
        End Get
    End Property
#End Region

#Region "临时文件夹路径"
    <Newtonsoft.Json.JsonIgnore>
    Private _TempDirectoryPath As String
    ''' <summary>
    ''' 临时文件夹路径
    ''' </summary>
    <Newtonsoft.Json.JsonIgnore>
    Public ReadOnly Property TempDirectoryPath As String
        Get
            Return _TempDirectoryPath
        End Get
    End Property
#End Region

#Region "程序集文件版本"
    <Newtonsoft.Json.JsonIgnore>
    Private _ProductVersion As String
    ''' <summary>
    ''' 程序集文件版本
    ''' </summary>
    <Newtonsoft.Json.JsonIgnore>
    Public ReadOnly Property ProductVersion As String
        Get
            Return _ProductVersion
        End Get
    End Property
#End Region

#Region "配置参数"
    ''' <summary>
    ''' 实例
    ''' </summary>
    Private Shared _instance As AppSettingHelper
    ''' <summary>
    ''' 获取实例
    ''' </summary>
    Public Shared ReadOnly Property Instance As AppSettingHelper
        Get
            If _instance Is Nothing Then

                '序列化默认设置
                JsonConvert.DefaultSettings = New Func(Of JsonSerializerSettings)(Function()

                                                                                      '忽略值为Null的字段
                                                                                      Dim tmpSettings = New JsonSerializerSettings
                                                                                      tmpSettings.NullValueHandling = NullValueHandling.Ignore
                                                                                      tmpSettings.Formatting = Formatting.Indented

                                                                                      Return tmpSettings
                                                                                  End Function)

                'LoadFromLocaltion()
                _instance = New AppSettingHelper

                '程序集GUID
                Dim guid_attr As Attribute = Attribute.GetCustomAttribute(Reflection.Assembly.GetExecutingAssembly(), GetType(Runtime.InteropServices.GuidAttribute))
                _instance._GUID = CType(guid_attr, Runtime.InteropServices.GuidAttribute).Value

                '临时文件夹
                _instance._TempDirectoryPath = IO.Path.Combine(
                    IO.Path.GetTempPath,
                    $"{{{_instance.GUID.ToUpper}}}")
                IO.Directory.CreateDirectory(_instance._TempDirectoryPath)

                '程序集文件版本
                Dim assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location
                _instance._ProductVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion

            End If

            Return _instance
        End Get
    End Property
#End Region

#Region "从本地读取配置"
    '    ''' <summary>
    '    ''' 从本地读取配置
    '    ''' </summary>
    '    Private Shared Sub LoadFromLocaltion()
    '        Dim Path As String = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)

    '        System.IO.Directory.CreateDirectory($"{Path}\707wk\{My.Application.Info.ProductName}\Data")
    '        'System.IO.Directory.CreateDirectory($".\Data")

    '        '反序列化
    '        Try
    '            _instance = JsonConvert.DeserializeObject(Of AppSettingHelper)(
    '                System.IO.File.ReadAllText($"{Path}\707wk\{My.Application.Info.ProductName}\Data\Setting.json",
    '                                           System.Text.Encoding.UTF8))

    '            If _instance Is Nothing Then
    '                Throw New NullReferenceException
    '            End If

    '#Disable Warning CA1031 ' Do not catch general exception types
    '        Catch ex As Exception
    '            '设置默认参数
    '            _instance = New AppSettingHelper
    '#Enable Warning CA1031 ' Do not catch general exception types
    '        End Try

    '    End Sub
#End Region

#Region "保存配置到本地"
    '    ''' <summary>
    '    ''' 保存配置到本地
    '    ''' </summary>
    '    Public Shared Sub SaveToLocaltion()
    '        Dim Path As String = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)

    '        System.IO.Directory.CreateDirectory($"{Path}\707wk\{My.Application.Info.ProductName}\Data")
    '        'System.IO.Directory.CreateDirectory($".\Data")

    '        '序列化
    '        Try
    '            Using t As System.IO.StreamWriter = New System.IO.StreamWriter(
    '                    $"{Path}\707wk\{My.Application.Info.ProductName}\Data\Setting.json",
    '                    False,
    '                    System.Text.Encoding.UTF8)

    '                t.Write(JsonConvert.SerializeObject(_instance))
    '            End Using

    '#Disable Warning CA1031 ' Do not catch general exception types
    '        Catch ex As Exception
    '            MsgBox(ex.ToString, MsgBoxStyle.Exclamation, My.Application.Info.Title)
    '#Enable Warning CA1031 ' Do not catch general exception types
    '        End Try

    '    End Sub
#End Region

#Region "导出配置"
    '''' <summary>
    '''' 导出配置
    '''' </summary>
    'Public Shared Sub ExportSettings(filePath As String)

    '    '序列化
    '    Using t As System.IO.StreamWriter = New System.IO.StreamWriter(
    '        filePath,
    '        False,
    '        System.Text.Encoding.UTF8)

    '        t.Write(JsonConvert.SerializeObject(_instance))
    '    End Using

    'End Sub
#End Region

#Region "导入配置"
    '''' <summary>
    '''' 导入配置
    '''' </summary>
    'Public Shared Sub ImportSettings(filePath As String)

    '    '反序列化
    '    Dim tmpInstance = JsonConvert.DeserializeObject(Of AppSettingHelper)(
    '        System.IO.File.ReadAllText(filePath,
    '                                   System.Text.Encoding.UTF8))

    '    '需要导入的变量

    '    SaveToLocaltion()

    'End Sub
#End Region

#Region "日志记录"
    ''' <summary>
    ''' 日志记录
    ''' </summary>
    <Newtonsoft.Json.JsonIgnore>
    Public Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger()
#End Region

#Region "清理临时文件"
    ''' <summary>
    ''' 清理临时文件
    ''' </summary>
    Public Sub ClearTempFiles()

        '删除文件
        For Each item In IO.Directory.EnumerateFiles(Me.TempDirectoryPath)
            Try
                IO.File.Delete(item)

#Disable Warning CA1031 ' Do not catch general exception types
            Catch ex As Exception
#Enable Warning CA1031 ' Do not catch general exception types
            End Try
        Next

        '删除文件夹
        For Each item In IO.Directory.EnumerateDirectories(Me.TempDirectoryPath)
            Try
                IO.Directory.Delete(item, True)

#Disable Warning CA1031 ' Do not catch general exception types
            Catch ex As Exception
#Enable Warning CA1031 ' Do not catch general exception types
            End Try
        Next

    End Sub
#End Region

#Region "获取临时文件大小"
    ''' <summary>
    ''' 获取临时文件大小
    ''' </summary>
    Public Function GetTempFilesSizeByMB() As Decimal
        Dim sizeByMB As Decimal = 0

        For Each item In Directory.EnumerateFiles(Me.TempDirectoryPath)
            Dim tmpFileInfo = New FileInfo(item)
            sizeByMB += tmpFileInfo.Length
        Next

        sizeByMB = sizeByMB \ 1024 \ 1024

        Return sizeByMB
    End Function
#End Region

    ''' <summary>
    ''' 设备 ID
    ''' </summary>
    <JsonIgnore>
    Public ReadOnly Property DeviceID As String
        Get
            Dim tmpValue = LocalLiteDBHelper.GetOption(Of String)(NameOf(DeviceID))

            If String.IsNullOrWhiteSpace(tmpValue) Then
                Dim tmpDeviceID = ServiceBaseLib.GUIDHelper.NewID
                LocalLiteDBHelper.UpdateOrAddOption(NameOf(DeviceID), tmpDeviceID)
                Return tmpDeviceID
            End If

            Return tmpValue
        End Get
    End Property

    '''' <summary>
    '''' 本地数据库文件路径
    '''' </summary> 
    'Public Shared LocalDatabaseFilePath As String = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\707wk\{My.Application.Info.ProductName}\Data\LocalDatabase.db"

    ''' <summary>
    ''' 本地数据库文件路径
    ''' </summary> 
    Public Shared LocalLiteDBFilePath As String = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\707wk\{My.Application.Info.ProductName}\Data\LocalLiteDB.ldb"

End Class
