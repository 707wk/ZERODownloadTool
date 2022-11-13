Imports System.Runtime.InteropServices

Public Class StringHelper
    <DllImport("Shlwapi.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function StrCmpLogicalW(psz1 As String, psz2 As String) As Integer

    End Function

    Public Shared Function StrCmpLogical(psz1 As String, psz2 As String) As Integer
        Return StrCmpLogicalW(psz1, psz2)
    End Function

    Public Class StrCmpLogicalWCompare
        Implements IComparer(Of String)

        Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
            Return StrCmpLogicalW(x, y)
        End Function
    End Class

End Class
