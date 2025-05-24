Imports System.IO
Imports System.Net
Imports System.Security.Policy
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web

Public Class Form1
    Dim CookieJar As New CookieContainer
    Dim studentid As String
    Dim pass As String
    Dim nationalid As String
    Dim cureval As Integer = 1
    Dim t As New Stopwatch
    Dim tq As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        TextBox1.ForeColor = Color.LimeGreen
        TextBox1.ReadOnly = True

    End Sub
    Function ConvertToArabic(inp As String)
        Dim btCharBytes As Byte() = Encoding.Default.GetBytes(inp)

        Dim strOutput As New String(Encoding.GetEncoding(1256).GetChars(btCharBytes))
        Return strOutput
    End Function
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Button1.Enabled = False
        t.Start()
        studentid = txt_student.Text
        nationalid = txt_national.Text
        pass = txt_pwd.Text
        Log("Logging in as " & studentid & "...")
        Dim tt As New Threading.Thread(Sub()

                                           Try
                                               While True
                                                   DoPost("https://app2.bau.edu.jo:7799/eval/LoginCode.jsp", CookieJar, "tbstdno=" & studentid & "&tbstdpass=" & HttpUtility.UrlEncode(pass) & "&tbstdnatno=" & nationalid)
                                                   RegularPage("http://app2.bau.edu.jo:7777/eval/Evaluation.jsp", CookieJar)
                                                   AnswerQuestions()
                                                   Log("Authentication Successful")

                                               End While


                                           Catch ex As Exception
                                               t.Stop()
                                               Button1.Enabled = True
                                               If tq = 0 Then

                                                   MsgBox("There seems to be an error", MsgBoxStyle.Critical)
                                                   TextBox1.ForeColor = Color.Red
                                                   Log("There seems to be an error")
                                                   Log("------------------------------------------------")
                                                   Log(ex.Message)
                                                   Log(ex.StackTrace)
                                                   Log("------------------------------------------------")

                                               Else
                                                   Log("----- Done & answered " & tq & " questions in " & t.ElapsedMilliseconds / 1000 & " Seconds -----")
                                                   MsgBox("Done & answered " & tq & " questions in " & t.ElapsedMilliseconds / 1000 & " Seconds")
                                                   Process.Start("https://app2.bau.edu.jo:7799/reg_new/actions/login?username=" & studentid & "&password=" & HttpUtility.UrlEncode(pass))
                                               End If

                                           End Try
                                       End Sub)
        tt.Start()
    End Sub
    Dim ccc As Integer = 0
    Sub AnswerQuestions()
        Dim page1 As String = RegularPage("https://app2.bau.edu.jo:7799/eval/Evaluation.jsp?qno=", CookieJar)

        Dim classs As String = Getbetween("class=""lbl3""><b>", "</b></span>", ConvertToArabic(page1))
        Dim prof As String = Getbetween("id=""lblInstName"">", "</span>", ConvertToArabic(page1))
        Dim rate As Integer = 5
        Dim ratetext As String = "أوافق دائما"
        If CheckBox1.Checked = True Then
            Dim f2 As New Form2(classs)
            f2.ShowDialog()
            rate = f2.selectedvalue
            ratetext = f2.ratetext_
        End If

        Log("Evaluating Professor (" & prof & ")...")
        For i As Integer = 0 To 15

            Dim dd As String = RegularPage("https://app2.bau.edu.jo:7799/eval/Evaluation.jsp?qno=" & (i + 1), CookieJar)
            '  IO.File.WriteAllText(ccc & ".txt", dd)
            ccc += 1
            DoPost("https://app2.bau.edu.jo:7799/eval/EvaluationSetAnswer.jsp", CookieJar, "evalqno=" & i & "&evalans=" & rate)
            Log("Answered Question #" & i + 1 & "  = " & ratetext)
            tq += 1
        Next
        DoPost("https://app2.bau.edu.jo:7799/eval/EvaluationSetAnswer.jsp", CookieJar, "evalqno=16&evalans=" & rate)
        Log("Answered Question #17" & "  = " & ratetext)
        Log("Searching for captcha code...")
        Dim source_ As String = RegularPage("https://app2.bau.edu.jo:7799/eval/Evaluation.jsp?qno=17", CookieJar)
        Dim code As String = ExtractCode(source_)
        If code = "ERR" Then
            For i As Integer = 17 To 22
                RegularPage("https://app2.bau.edu.jo:7799/eval/Evaluation.jsp?qno=" & i, CookieJar)
                DoPost("https://app2.bau.edu.jo:7799/eval/EvaluationSetAnswer.jsp", CookieJar, "evalqno=" & i & "&evalans=" & rate)
                source_ = RegularPage("https://app2.bau.edu.jo:7799/eval/Evaluation.jsp?qno=" & i, CookieJar)
                code = ExtractCode(source_)
                Log("Answered Question #" & i + 1 & "  = " & ratetext)
                tq += 1
                If code = "ERR" Then

                Else
                    Exit For
                End If

            Next
        End If
        Try

        Catch ex As Exception
            For i As Integer = 17 To 69
                RegularPage("https://app2.bau.edu.jo:7799/eval/Evaluation.jsp?qno=" & i, CookieJar)
                DoPost("https://app2.bau.edu.jo:7799/eval/EvaluationSetAnswer.jsp", CookieJar, "evalqno=" & i & "&evalans=" & rate)
                source_ = RegularPage("https://app2.bau.edu.jo:7799/eval/Evaluation.jsp?qno=" & i, CookieJar)

                Log("Answered Question #" & i + 1 & "  = " & ratetext)
                tq += 1
            Next
        End Try
        tq += 2
        Log("Captcha Code Found and is '" & code & "'")
        DoPost("https://app2.bau.edu.jo:7799/eval/EvaluationSubmit.jsp", CookieJar, "captcha=" & code)
        Log("Applying code...")
        RegularPage("https://app2.bau.edu.jo:7799/eval/Finish.jsp", CookieJar)
        Log("Professor (" & prof & ") has been evaluated successfully")
        cureval += 1
    End Sub
    Function ExtractCode(Source As String)
        Try
            Dim cap As String = Source
            cap = Split(cap, "<!-- START CAPTCHA -->", -1, CompareMethod.Text)(1)
            cap = Split(cap, "<br/>", -1, CompareMethod.Text)(0)
            Dim myString As String = cap
            Dim ResultString As String

            Dim i As Integer

            For i = 0 To myString.Length - 1
                If IsNumeric(myString.Chars(i)) = True Then
                    ResultString &= myString.Chars(i)
                End If
            Next

            Return (ResultString)
        Catch ex As Exception
            Return "ERR"
        End Try



    End Function

    Private Function DoPost(ByVal URL As String, ByRef CookieJar As CookieContainer, ByVal PostData As String)
        Dim reader As StreamReader

        Dim Request As HttpWebRequest = HttpWebRequest.Create(URL)

        Request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1.14) Gecko/20080404 Firefox/2.0.0.14"
        Request.CookieContainer = CookieJar
        Request.AllowAutoRedirect = False
        Request.ContentType = "application/x-www-form-urlencoded"
        Request.Method = "POST"
        Request.ContentLength = PostData.Length

        Dim requestStream As Stream = Request.GetRequestStream()
        Dim postBytes As Byte() = Encoding.ASCII.GetBytes(PostData)

        requestStream.Write(postBytes, 0, postBytes.Length)
        requestStream.Close()

        Dim Response As HttpWebResponse = Request.GetResponse()

        For Each tempCookie In Response.Cookies
            CookieJar.Add(tempCookie)
        Next

        reader = New StreamReader(Response.GetResponseStream())
        Return HttpUtility.HtmlDecode(reader.ReadToEnd())
        Response.Close()
    End Function

    Private Function RegularPage(ByVal URL As String, ByVal CookieJar As CookieContainer) As String
        Dim reader As StreamReader

        Dim Request As HttpWebRequest = HttpWebRequest.Create(URL)
        Request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1.14) Gecko/20080404 Firefox/2.0.0.14"
        Request.AllowAutoRedirect = False
        Request.CookieContainer = CookieJar

        Dim Response As HttpWebResponse = Request.GetResponse()

        reader = New StreamReader(Response.GetResponseStream(), Encoding.UTF7)

        '  Return HttpUtility.HtmlDecode(reader.ReadToEnd())
        Return reader.ReadToEnd()

        Response.Close()
    End Function
    Sub Log(text As String)
        TextBox1.AppendText(text & vbNewLine)

    End Sub
    Function Getbetween(first As String, second As String, thing As String)
        Return Split(Split(thing, first, , CompareMethod.Text)(1), second, , CompareMethod.Text)(0)
    End Function

End Class
