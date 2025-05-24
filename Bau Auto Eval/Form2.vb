Public Class Form2
    Public selectedvalue As Integer = 5
    Dim classname_ As String
    Public ratetext_ As String = "أوافق دائما"
    Sub New(classname As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Text = classname & " --- " & RadioButton5.Text

        Label1.Text = classname
        classname_ = classname
        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton5.CheckedChanged, RadioButton4.CheckedChanged, RadioButton3.CheckedChanged, RadioButton2.CheckedChanged, RadioButton1.CheckedChanged
        For Each radiobutton In Me.Controls.OfType(Of RadioButton)
            If radiobutton.Checked = True Then
                selectedvalue = radiobutton.TabIndex
                ratetext_ = radiobutton.Text
                Me.Text = classname_ & " --- " & radiobutton.Text
                Exit For
            End If
        Next
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub
End Class