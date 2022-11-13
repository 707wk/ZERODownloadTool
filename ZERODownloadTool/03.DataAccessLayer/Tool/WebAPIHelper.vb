Imports System.Net.Http
Imports Newtonsoft.Json
Imports ServiceBaseLib

''' <summary>
''' WebAPI辅助模块, 默认为 JSON格式
''' </summary>
Public NotInheritable Class WebAPIHelper

    Private Shared ReadOnly tmpHttpClient As New HttpClient With {
        .Timeout = New TimeSpan(0, 0, 10)
    }

    Public Shared Function GetData(Of T)(url As String) As ReceiveMsg(Of T)

        Try

            tmpHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "application/json")

            Dim tmpResponse = tmpHttpClient.GetAsync(url).GetAwaiter.GetResult
            tmpResponse.EnsureSuccessStatusCode()

            '接收数据
            Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult
            Return JsonConvert.DeserializeObject(Of ReceiveMsg(Of T))(contentStr)

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Return New ReceiveMsg(Of T) With {.Code = 404, .Message = ex.Message}
        End Try

    End Function

    Public Shared Function PostData(Of T)(url As String, postValue As Object) As ReceiveMsg(Of T)

        Try

            Dim tmpStringContent = New StringContent(JsonConvert.SerializeObject(postValue), System.Text.Encoding.UTF8, "application/json")

            Dim tmpResponse = tmpHttpClient.PostAsync(url, tmpStringContent).GetAwaiter.GetResult
            tmpResponse.EnsureSuccessStatusCode()

            '接收数据
            Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult
            Return JsonConvert.DeserializeObject(Of ReceiveMsg(Of T))(contentStr)

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Return New ReceiveMsg(Of T) With {.Code = 404, .Message = ex.Message}
        End Try

    End Function

End Class
