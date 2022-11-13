Imports System.Runtime.InteropServices
''' <summary>
''' 文件操作辅助模块
''' </summary>
Public NotInheritable Class FileHelper
    <Flags>
    Public Enum ShowCommands
        SW_HIDE = 0
        SW_SHOWNORMAL = 1
        SW_NORMAL = 1
        SW_SHOWMINIMIZED = 2
        SW_SHOWMAXIMIZED = 3
        SW_MAXIMIZE = 3
        SW_SHOWNOACTIVATE = 4
        SW_SHOW = 5
        SW_MINIMIZE = 6
        SW_SHOWMINNOACTIVE = 7
        SW_SHOWNA = 8
        SW_RESTORE = 9
        SW_SHOWDEFAULT = 10
        SW_FORCEMINIMIZE = 11
        SW_MAX = 11
    End Enum

    <DllImport("shell32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function ShellExecute(hwnd As IntPtr,
                                         lpOperation As String,
                                         lpFile As String,
                                         lpParameters As String,
                                         lpDirectory As String,
                                         nShowCmd As ShowCommands) As IntPtr
    End Function

    ''' <summary>
    ''' 调用系统方式打开文件
    ''' </summary>
    Public Shared Sub Open(filePath As String)
        ShellExecute(IntPtr.Zero, "open", filePath, "", "", ShowCommands.SW_SHOWNORMAL)
    End Sub

End Class
