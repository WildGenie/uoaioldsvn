Imports UOAI2, System.Diagnostics

Public Class Form1
    Private WithEvents jack As New UOAI
    Private WithEvents sh As UOAI.UOClient


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        MsgBox(jack.Clients.Count)

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
End Class
