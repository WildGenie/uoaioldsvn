Imports UOAI2

Public Class Form1
    Private WithEvents jack As New UOAI
    Private WithEvents sh As UOAI.Client

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If jack.Clients.Count >= 1 Then
            ListBox1.Items.Clear()

            For i As Integer = 0 To jack.Clients.Count - 1
                ListBox1.Items.Add(jack.Clients.Client(i).WindowCaption & " | " & jack.Clients.Client(i).PID)
            Next

        End If
    End Sub

    Private Sub sh_onClientClose() Handles sh.onClientExit
        'MsgBox("win")
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        sh.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        jack.Clients.LaunchClient()
        sh = jack.Clients.Client(0)
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        sh = jack.Clients.Client(0)
    End Sub

End Class
