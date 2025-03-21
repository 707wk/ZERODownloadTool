''' <summary>
''' 日志辅助模块
''' </summary>
Public Class LogHelper

    Public Shared Sub LogEvent(eventName As String)

        ' 调试时不记录
        If Debugger.IsAttached Then
            Exit Sub
        End If

        'Analytics.TrackEvent(eventName)

        'Dim tmpLogInfo As New LogInfo With {
        '    .SystemName = System.Reflection.Assembly.GetExecutingAssembly().GetName.Name,
        '    .EventName = eventName,
        '    .DeviceID = AppSettingHelper.DeviceID
        '}

        'Dim tmpHttpContent = New StringContent(JsonConvert.SerializeObject(tmpLogInfo))
        'tmpHttpContent.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/json")

        'PostLogHttpClient.PostAsync("https://online.csyes.com:9001/api/Tools/Log/Post", tmpHttpContent)

    End Sub

End Class
