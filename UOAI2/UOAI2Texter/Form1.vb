Imports UOAI2, System.IO

Public Class Form1
    Private WithEvents UOAIObj As New UOAI
    Private WithEvents UOAI_Cl As UOAI.Client

    Private packetlog As StreamWriter
    Private packetnumber As Integer = 0

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ListBox1.Items.Clear()

        If UOAIObj.Clients.Count >= 1 Then
            For i As Integer = 0 To UOAIObj.Clients.Count - 1
                ListBox1.Items.Add(UOAIObj.Clients.Client(i).WindowCaption & " | " & UOAIObj.Clients.Client(i).PID)
            Next
        End If
    End Sub

    Private Sub UOAI_Cl_onClientClose() Handles UOAI_Cl.onClientExit
        'MsgBox("win")
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        UOAI_Cl.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        UOAIObj.Clients.LaunchClient()
        Do While UOAIObj.Clients.Count = 0
            Threading.Thread.Sleep(0)
        Loop
        UOAI_Cl = UOAIObj.Clients.Client(0)
        'AddHandler UOAI_Cl.onPacketReceive, AddressOf PacketHandler
    End Sub

    Private Sub PacketHandler(ByRef cl As UOAI.Client, ByRef p As UOAI.Packet)
        If p.Type = UOAI.Enums.PacketType.TextUnicode Then
            Dim up As UOAI.Packets.UnicodeTextPacket
            up = DirectCast(p, UOAI.Packets.UnicodeTextPacket)
            DoWriteToLabel(up.Name & " says : " & up.Text)
        End If
    End Sub
    Private Sub DoWriteToLabel(ByVal text As String)
        Me.Invoke(New WriteToLabelDelegate(AddressOf Writer), New Object() {text})
    End Sub
    Private Sub Writer(ByVal text As String)
        Label2.Text = text
    End Sub
    Private Delegate Sub WriteToLabelDelegate(ByVal text As String)

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        packetlog.Close()
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If UOAIObj.Clients.Count > 0 Then UOAI_Cl = UOAIObj.Clients.Client(0)
        packetlog = File.CreateText(Application.StartupPath & "\Packets.txt")
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click

    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Dim b() As Byte = System.Text.UnicodeEncoding.Unicode.GetBytes("this is a text")
        Dim bt(46 + b.Length + 1) As Byte

        For i As Byte = 0 To b.Length - 1
            bt(46 + i) = b(i)
        Next


        Dim j As New UOAI.Packets.UnicodeTextPacket(bt)
        'check to see if parsing + reading works
        MsgBox(j.Text)

        'check to see if the writing works
        j.Text = "this is a BAMF"
        MsgBox(j.Text)

        'check to make sure the origional read text is the correct size. This should throw an exception.
        j.Text = "this is a BAMF "
        MsgBox(j.Text)

    End Sub

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        If (ListBox1.SelectedIndex >= 0) Then
            UOAI_Cl = UOAIObj.Clients.Client(ListBox1.SelectedIndex)
            Label1.Text = UOAI_Cl.PID.ToString()
            AddHandler UOAI_Cl.onPacketReceive, AddressOf PacketHandler
        End If
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        UOAI_Cl.PatchEncryption()
    End Sub

    Private Sub UOAI_Cl_onPacketReceive(ByRef Client As UOAI2.UOAI.Client, ByRef packet As UOAI2.UOAI.Packet) Handles UOAI_Cl.onPacketReceive
        If CheckBox1.Checked = True Then
            packetlog.WriteLine("'Recieved Packet #" & packetnumber)
            packetlog.WriteLine("'" & BitConverter.ToString(packet.Data))
            packetlog.WriteLine("")
            packetnumber += 1
            UOAI_Cl.HandlePacket()
        End If

        Exit Sub
        Select Case packet.Type
            Case UOAI.Enums.PacketType.TextUnicode
                'Dim j As UOAI.Packets.UnicodeTextPacket = DirectCast(packet, UOAI.Packets.UnicodeTextPacket)

        End Select

    End Sub

    Private Sub UOAI_Cl_onPacketSend(ByRef Client As UOAI2.UOAI.Client, ByRef packet As UOAI2.UOAI.Packet) Handles UOAI_Cl.onPacketSend
        If CheckBox1.Checked = True Then
            packetlog.WriteLine("'Sent Packet #" & packetnumber)
            packetlog.Write("'" & BitConverter.ToString(packet.Data))
            packetlog.WriteLine("")
            packetlog.WriteLine("")
            packetnumber += 1
            UOAI_Cl.HandlePacket()
        End If

        Exit Sub
        Select Case packet.Type
            Case UOAI.Enums.PacketType.SpeechUnicode

        End Select

    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        UOAI_Cl.Macros.Say(TextBox1.Text)
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged

    End Sub
End Class
