Imports System.IO
Imports System.Web
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
''' <summary>
''' 全局配置辅助类
''' </summary>
Public Class AppSettingHelper
    Private Sub New()
    End Sub

    '''' <summary>
    '''' 程序升级密钥
    '''' </summary>
    Public Const AppKey = "e879e579-ddf0-41c0-9e50-fc7a3bddd9b4"

#Region "程序集GUID"
    Private Shared _GUID As String
    ''' <summary>
    ''' 程序集GUID
    ''' </summary>
    Public Shared ReadOnly Property GUID As String
        Get

            If String.IsNullOrWhiteSpace(_GUID) Then
                Dim guid_attr As Attribute = Attribute.GetCustomAttribute(Reflection.Assembly.GetExecutingAssembly(), GetType(Runtime.InteropServices.GuidAttribute))
                _GUID = CType(guid_attr, Runtime.InteropServices.GuidAttribute).Value
            End If

            Return _GUID
        End Get
    End Property
#End Region

#Region "临时文件夹路径"
    Private Shared _TempDirectoryPath As String
    ''' <summary>
    ''' 临时文件夹路径
    ''' </summary>
    Public Shared ReadOnly Property TempDirectoryPath As String
        Get

            If String.IsNullOrWhiteSpace(_TempDirectoryPath) Then
                _TempDirectoryPath = IO.Path.Combine(
                    IO.Path.GetTempPath,
                    $"{{{GUID.ToUpper}}}")
                IO.Directory.CreateDirectory(_TempDirectoryPath)
            End If

            Return _TempDirectoryPath
        End Get
    End Property
#End Region

#Region "程序集文件版本"
    Private Shared _ProductVersion As String
    ''' <summary>
    ''' 程序集文件版本
    ''' </summary>
    Public Shared ReadOnly Property ProductVersion As String
        Get

            If String.IsNullOrWhiteSpace(_ProductVersion) Then
                Dim assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location
                _ProductVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion
            End If

            Return _ProductVersion
        End Get
    End Property
#End Region

#Region "初始化配置"
    ''' <summary>
    ''' 初始化配置
    ''' </summary>
    Public Shared Sub Init()

        '序列化默认设置
        JsonConvert.DefaultSettings = New Func(Of JsonSerializerSettings)(Function()

                                                                              '忽略值为Null的字段
                                                                              Dim tmpSettings = New JsonSerializerSettings
                                                                              tmpSettings.NullValueHandling = NullValueHandling.Ignore
                                                                              tmpSettings.Formatting = Formatting.Indented

                                                                              Return tmpSettings
                                                                          End Function)

    End Sub

#End Region

#Region "日志记录"
    ''' <summary>
    ''' 日志记录
    ''' </summary>
    <Newtonsoft.Json.JsonIgnore>
    Public Shared Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger()
#End Region

#Region "清理临时文件"
    ''' <summary>
    ''' 清理临时文件
    ''' </summary>
    Public Shared Sub ClearTempFiles()

        Try
            IO.Directory.Delete(TempDirectoryPath, True)

        Catch ex As Exception
        End Try

    End Sub
#End Region

#Region "设备 ID"
    ''' <summary>
    ''' 设备 ID
    ''' </summary>
    Public Shared ReadOnly Property DeviceID As String
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
#End Region

    '''' <summary>
    '''' 本地数据库文件路径
    '''' </summary> 
    'Public Shared LocalDatabaseFilePath As String = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\707wk\{My.Application.Info.ProductName}\Data\LocalDatabase.db"

    ''' <summary>
    ''' 本地数据库文件路径
    ''' </summary> 
    Public Shared LocalLiteDBFilePath As String = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\707wk\{My.Application.Info.ProductName}\Data\LocalLiteDB.ldb"

#Region "用户名"
    ''' <summary>
    ''' 用户名
    ''' </summary>
    Public Shared Property UserName As String
        Get
            Return LocalLiteDBHelper.GetOption(Of String)(NameOf(UserName))
        End Get
        Set(ByVal value As String)
            LocalLiteDBHelper.UpdateOrAddOption(NameOf(UserName), value)
        End Set
    End Property
#End Region

#Region "密码"
    ''' <summary>
    ''' 密码
    ''' </summary>
    Public Shared Property UserPassword As String
        Get
            Return LocalLiteDBHelper.GetOption(Of String)(NameOf(UserPassword))
        End Get
        Set(ByVal value As String)
            LocalLiteDBHelper.UpdateOrAddOption(NameOf(UserPassword), value)
        End Set
    End Property
#End Region

#Region "下载后保存位置"
    ''' <summary>
    ''' 下载后保存位置
    ''' </summary>
    Public Shared Property SaveFolderPath As String
        Get
            Dim tmpValue = LocalLiteDBHelper.GetOption(Of String)(NameOf(SaveFolderPath))
            If String.IsNullOrWhiteSpace(tmpValue) Then
                Return "D:\Downloads"
            End If

            Return tmpValue
        End Get
        Set(ByVal value As String)
            LocalLiteDBHelper.UpdateOrAddOption(NameOf(SaveFolderPath), value)
        End Set
    End Property
#End Region

#Region "服务器域名"
    ''' <summary>
    ''' 服务器域名
    ''' </summary>
    Public Shared Property HostName As String
        Get
            Return LocalLiteDBHelper.GetOption(Of String)(NameOf(HostName))
        End Get
        Set(ByVal value As String)
            LocalLiteDBHelper.UpdateOrAddOption(NameOf(HostName), value)
        End Set
    End Property
#End Region

End Class
