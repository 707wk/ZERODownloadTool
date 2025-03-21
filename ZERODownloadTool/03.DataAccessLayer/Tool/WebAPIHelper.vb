Imports System.Net.Http
Imports Newtonsoft.Json
Imports Wangk.Base

''' <summary>
''' WebAPI辅助模块, 默认为 JSON格式
''' </summary>
Public NotInheritable Class WebAPIHelper

    Private Shared ReadOnly tmpHttpClient As New HttpClient With {
        .Timeout = New TimeSpan(0, 0, 10)
    }

    Public Shared Function GetData(Of T)(url As String) As ResultMsg(Of T)

        Try

            tmpHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "application/json")

            Dim tmpResponse = tmpHttpClient.GetAsync(url).GetAwaiter.GetResult
            tmpResponse.EnsureSuccessStatusCode()

            '接收数据
            Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult
            Return JsonConvert.DeserializeObject(Of ResultMsg(Of T))(contentStr)

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Return ex
        End Try

    End Function

    Public Shared Function PostData(Of T)(url As String, postValue As Object) As ResultMsg(Of T)

        Try

            Dim tmpStringContent = New StringContent(JsonConvert.SerializeObject(postValue), System.Text.Encoding.UTF8, "application/json")

            Dim tmpResponse = tmpHttpClient.PostAsync(url, tmpStringContent).GetAwaiter.GetResult
            tmpResponse.EnsureSuccessStatusCode()

            '接收数据
            Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult
            Return JsonConvert.DeserializeObject(Of ResultMsg(Of T))(contentStr)

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Return ex
        End Try

    End Function

    Public Shared Function GetDataWithJWT(Of T)(url As String,
                                                token As String) As T
        AddPublicHeaders()

        Try

            tmpHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "application/json")

            tmpHttpClient.DefaultRequestHeaders.Remove("Authorization")
            tmpHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}")

            Dim tmpResponse = tmpHttpClient.GetAsync(url).GetAwaiter.GetResult
            tmpResponse.EnsureSuccessStatusCode()

            '接收数据
            Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult

            tmpHttpClient.DefaultRequestHeaders.Remove("Authorization")

            Return JsonConvert.DeserializeObject(Of T)(contentStr)

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Throw ex
        End Try

    End Function


    Public Shared Function PostDataWithJWT(Of T)(url As String,
                                                 postValue As Object,
                                                 token As String) As T
        AddPublicHeaders()

        Try

            tmpHttpClient.DefaultRequestHeaders.Remove("Authorization")
            tmpHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}")

            Dim tmpStringContent = New StringContent(JsonConvert.SerializeObject(postValue), System.Text.Encoding.UTF8, "application/json")

            Dim tmpResponse = tmpHttpClient.PostAsync(url, tmpStringContent).GetAwaiter.GetResult
            tmpResponse.EnsureSuccessStatusCode()

            '接收数据
            Dim contentStr = tmpResponse.Content.ReadAsStringAsync().GetAwaiter.GetResult

            tmpHttpClient.DefaultRequestHeaders.Remove("Authorization")

            Return JsonConvert.DeserializeObject(Of T)(contentStr)

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Throw ex
        End Try

    End Function

    Private Shared Sub AddPublicHeaders()

        'tmpHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Host", $"{My.Application.Info.AssemblyName} V{AppSettingHelper.Instance.ProductVersion}")
        'tmpHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"{My.Application.Info.AssemblyName} V{AppSettingHelper.Instance.ProductVersion}")

        tmpHttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request")

    End Sub

End Class
