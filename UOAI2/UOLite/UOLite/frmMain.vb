Public Class frmMain
    Public WithEvents Client As New LiteClient

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Debug.WriteLine("Attempting to connect to server")
        Client.GetServerList(TextBox1.Text, TextBox2.Text, TextBox3.Text, TextBox4.Text)
        'MsgBox(Client.ServerList.Length)
    End Sub

    Private Sub Client_LoginDenied(ByRef Reason As String) Handles Client.LoginDenied
        MsgBox(Reason)
    End Sub

    Private Sub Client_RecievedServerList() Handles Client.RecievedServerList
        Client.ChooseServer(0)
    End Sub
End Class