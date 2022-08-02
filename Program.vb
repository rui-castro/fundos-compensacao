Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Text.RegularExpressions

Module Program
    Sub Main(args As String())
        Dim username = args(0)
        Dim password = args(1)
        Dim client As New HttpClient()
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        Login(client, username, password)
        Dim report = GetReport(client)
        Console.WriteLine(report)
    End Sub

    Private Sub Login(client as HttpClient, username as String, password as String)
        Const url = "https://www.fundoscompensacao.pt/sso/login"
        Dim result = client.GetAsync(url).Result
        Dim body = result.Content.ReadAsStringAsync().Result

        Dim lt = ExtractInputValue(body, "lt")
        Dim execution = ExtractInputValue(body, "execution")
        Dim eventId = ExtractInputValue(body, "_eventId")
        Dim bypasswarning = ExtractInputValue(body, "bypasswarning")
        Dim submitBtn = ExtractInputValue(body, "submitBtn")

        Dim params = New Dictionary(Of String, String)
        params.Add("username", username)
        params.Add("password", password)
        params.Add("lt", lt)
        params.Add("execution", execution)
        params.Add("bypasswarning", bypasswarning)
        params.Add("_eventId", eventId)
        params.Add("submitBtn", submitBtn)
        result = client.PostAsync(url, new FormUrlEncodedContent(params)).Result
        'Console.WriteLine(result.Content.ReadAsStringAsync().Result)
    End Sub
    
    Private Function GetReport(client as HttpClient) as String
        Const url = "https://www.fundoscompensacao.pt/fc/gfct/pagamentos/emitir/documento-pagamento"
        Dim result = client.GetAsync(url).Result
        Dim body = result.Content.ReadAsStringAsync().Result
        
        Dim dswid = ExtractValueWithRegex(body, "documento-pagamento\?dswid=(-?\d+)""")
        Dim viewState = ExtractInputValue(body, "javax.faces.ViewState")

        Dim params = New Dictionary(Of String, String)
        params.Add("form", "form")
        params.Add("form:btnExportReport", "")
        params.Add("javax.faces.ViewState", viewState)
        
        result = client.PostAsync($"{url}?dswid={dswid}", new FormUrlEncodedContent(params)).Result
        body = result.Content.ReadAsStringAsync().Result
        
        Return body
    End Function
    
    Private Function ExtractInputValue(text, inputName) As String
        return ExtractValueWithRegex(text, $"name=""{inputName}"" value=""([^""]+)""")
    End Function
    
    Private Function ExtractValueWithRegex(text, regex) As String
        Dim r As New Regex(regex)
        Dim match As Match = r.Match(text)
        return match.Groups(1).Value
    End Function

End Module
