Imports UOAI2, System.IO

Public Class Form1
    Private WithEvents jack As New UOAI
    Private WithEvents sh As UOAI.Client

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ListBox1.Items.Clear()

        If jack.Clients.Count >= 1 Then
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
        'jack.Clients.LaunchClient()
        jack.Clients.LaunchClient2()
        Do While jack.Clients.Count = 0
            Threading.Thread.Sleep(0)
        Loop
        sh = jack.Clients.Client(0)
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If jack.Clients.Count > 0 Then sh = jack.Clients.Client(0)
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        'Dump skills Enumeration.

        Dim s As StreamWriter
        s = File.CreateText(Application.StartupPath & "\skills.vb")
        s.WriteLine("Public Enum Skills")

        For i As Integer = 0 To Ultima.Skills.List.Length - 1
            s.WriteLine(vbTab & Ultima.Skills.GetSkill(i).Name.Replace(" ", "").Replace("/", "") & " = " & Ultima.Skills.GetSkill(i).ID)
        Next

        s.WriteLine("End Enum")
        s.Close()
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        'direct cast test
        Dim z As New UOAI.Packets.SpeechPacket
        z.Text = "WTF? i win!"

        'make a packet
        Dim x As UOAI.Packet

        'cast the speechpacket into the packet
        x = DirectCast(z, UOAI.Packet)

        'make a speech packet and cast the packet into it
        Dim j As UOAI.Packets.SpeechPacket = DirectCast(x, UOAI.Packets.SpeechPacket)

        'retrieve the data
        MsgBox(j.Text)

    End Sub
End Class
