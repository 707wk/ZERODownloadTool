﻿#If _MyType <> "Empty" Then

Namespace My
    ''' <summary>
    ''' 用于定义“我的 WPF 命名空间”中的可用属性的模块
    ''' </summary>
    ''' <remarks></remarks>
    <Global.Microsoft.VisualBasic.HideModuleName()> _
    Module MyWpfExtension
        Private ReadOnly s_Computer As New ThreadSafeObjectProvider(Of Global.Microsoft.VisualBasic.Devices.Computer)
        Private ReadOnly s_User As New ThreadSafeObjectProvider(Of ApplicationServices.User)
        Private ReadOnly s_Windows As New ThreadSafeObjectProvider(Of MyWindows)
        Private ReadOnly s_Log As New ThreadSafeObjectProvider(Of Global.Microsoft.VisualBasic.Logging.Log)
        ''' <summary>
        ''' 返回正在运行的应用程序的应用程序对象
        ''' </summary>
        Friend ReadOnly Property Application() As Application
            Get
                Return CType(Global.System.Windows.Application.Current, Application)
            End Get
        End Property
        ''' <summary>
        ''' 返回有关主机计算机的信息。
        ''' </summary>
        Friend ReadOnly Property Computer() As Global.Microsoft.VisualBasic.Devices.Computer
            Get
                Return s_Computer.GetInstance()
            End Get
        End Property
        ''' <summary>
        ''' 返回当前用户的信息。  如果希望使用当前的 
        ''' Windows 用户凭据来运行应用程序，请调用 My.User.InitializeWithWindowsUser()。
        ''' </summary>
        Friend ReadOnly Property User() As Global.Microsoft.VisualBasic.ApplicationServices.User
            Get
                Return s_User.GetInstance()
            End Get
        End Property
        ''' <summary>
        ''' 返回应用程序日志。可以使用应用程序的配置文件配置侦听器。
        ''' </summary>
        Friend ReadOnly Property Log() As Global.Microsoft.VisualBasic.Logging.Log
            Get
                Return s_Log.GetInstance()
            End Get
        End Property

        ''' <summary>
        ''' 返回项目中定义的 Windows 集合。
        ''' </summary>
        Friend ReadOnly Property Windows() As MyWindows
            <Global.System.Diagnostics.DebuggerHidden()>
            Get
                Return s_Windows.GetInstance()
            End Get
        End Property
        <ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Never)>
        <MyGroupCollection("System.Windows.Window", "Create__Instance__", "Dispose__Instance__", "My.MyWpfExtenstionModule.Windows")>
        Friend NotInheritable Class MyWindows
#Disable Warning IDE0051 ' 删除未使用的私有成员
            <DebuggerHidden()>
            Private Shared Function Create__Instance__(Of T As {New, Global.System.Windows.Window})(ByVal Instance As T) As T
#Enable Warning IDE0051 ' 删除未使用的私有成员
                If Instance Is Nothing Then
                    If s_WindowBeingCreated IsNot Nothing Then
                        If s_WindowBeingCreated.ContainsKey(GetType(T)) = True Then
                            Throw New Global.System.InvalidOperationException("The window cannot be accessed via My.Windows from the Window constructor.")
                        End If
                    Else
                        s_WindowBeingCreated = New Global.System.Collections.Hashtable()
                    End If
                    s_WindowBeingCreated.Add(GetType(T), Nothing)
                    Return New T()
                    s_WindowBeingCreated.Remove(GetType(T))
                Else
                    Return Instance
                End If
            End Function

#Disable Warning IDE0051 ' 删除未使用的私有成员
            <Global.System.Diagnostics.DebuggerHidden()>
            Private Sub Dispose__Instance__(Of T As Global.System.Windows.Window)(ByRef instance As T)
#Enable Warning IDE0051 ' 删除未使用的私有成员
                instance = Nothing
            End Sub
            <Global.System.Diagnostics.DebuggerHidden()> _
            <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Never)> _
            Public Sub New()
                MyBase.New()
            End Sub
            <ThreadStatic()> Private Shared s_WindowBeingCreated As Global.System.Collections.Hashtable
            <Global.System.ComponentModel.EditorBrowsable(Global.System.ComponentModel.EditorBrowsableState.Never)> Public Overrides Function Equals(ByVal o As Object) As Boolean
                Return MyBase.Equals(o)
            End Function
            <Global.System.ComponentModel.EditorBrowsable(Global.System.ComponentModel.EditorBrowsableState.Never)> Public Overrides Function GetHashCode() As Integer
                Return MyBase.GetHashCode
            End Function

            <Global.System.ComponentModel.EditorBrowsable(Global.System.ComponentModel.EditorBrowsableState.Never)> _
            Friend Overloads Function [GetType]() As Global.System.Type
                Return GetType(MyWindows)
            End Function
            <Global.System.ComponentModel.EditorBrowsable(Global.System.ComponentModel.EditorBrowsableState.Never)> Public Overrides Function ToString() As String
                Return MyBase.ToString
            End Function
        End Class
    End Module
End Namespace
Partial Class Application
    Inherits Global.System.Windows.Application

    Friend ReadOnly Property Info() As Global.Microsoft.VisualBasic.ApplicationServices.AssemblyInfo
        <Global.System.Diagnostics.DebuggerHidden()>
        Get
            Return New Global.Microsoft.VisualBasic.ApplicationServices.AssemblyInfo(Global.System.Reflection.Assembly.GetExecutingAssembly())
        End Get
    End Property
End Class
#End If