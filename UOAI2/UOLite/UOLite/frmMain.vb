Public Class frmMain
    Dim Client As New LiteClient


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        MsgBox(Client.ConnectToLoginServer(TextBox1.Text, TextBox2.Text, TextBox3.Text, TextBox4.Text))
    End Sub
End Class