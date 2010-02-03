Imports UOAI2.UOAI, System.IO, UOAI2

Public Class Form1
    Private WithEvents UOAIObj As New UOAI2.UOAI
    Private WithEvents UOAI_Cl As Client
    Private WithEvents DeathMobile As Mobile
    Private WithEvents Player As UOAI2.UOAI.Client.PlayerClass

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

    Private Sub PacketHandler(ByRef cl As Client, ByRef p As Packet)
        If p.Type = Enums.PacketType.TextUnicode Then
            Dim up As Packets.UnicodeTextPacket
            up = DirectCast(p, Packets.UnicodeTextPacket)
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

    Private Sub UOAI_Cl_onClientSpeech(ByVal client As UOAI2.UOAI.Client, ByVal Text As String, ByVal Font As UOAI2.UOAI.Enums.Fonts, ByVal Hue As UShort, ByVal Language As String, ByVal SpeechType As UOAI2.UOAI.Enums.SpeechTypes) Handles UOAI_Cl.onClientSpeech
        If Text.IndexOf(".") = 0 Then
            UOAI_Cl.DropPacket()

            Dim x() As String = Text.Substring(1).Split(" ")

            Select Case LCase(x(0))
                Case "sysmsg"
                    UOAI_Cl.SysMsg(Text.Substring(8))

                Case "set"
                    Select Case LCase(x(1))
                        Case "deathtest"
                            UOAI_Cl.TargetPrompt(CUInt(5568956), Enums.TargetRequestType.ItemOrMobile)

                        Case "help", "?"
                            UOAI_Cl.SysMsg(".set deathtest - Designates a target as a test subject for death test.")
                            UOAI_Cl.SysMsg(".set help - Displays this information.")

                        Case Else
                            UOAI_Cl.SysMsg("Unrecognized Command: " & Chr(34) & Text & Chr(34))

                    End Select

                Case "webhelp"
                    System.Diagnostics.Process.Start("http://www.decelle.be/UOAI/forum/index.php")

                Case "recursivetest"
                    UOAI_Cl.TargetPrompt(CUInt(5568957), Enums.TargetRequestType.ItemOrMobile)

                Case "help", "?"
                    UOAI_Cl.SysMsg(".sysmsg - Display a system message.")
                    UOAI_Cl.SysMsg(".set - Sets a plethora of stuff.")
                    UOAI_Cl.SysMsg(".help or .? - Displays help with the given command.")
                    UOAI_Cl.SysMsg(".webhelp - Opens your default web browser to the UOAI forums.")

                Case Else
                    UOAI_Cl.SysMsg("Unrecognized Command: " & Chr(34) & Text & Chr(34))

            End Select
        End If

    End Sub

    Private Sub UOAI_Cl_onHueResponse(ByVal UID As UShort, ByVal Hue As UShort) Handles UOAI_Cl.onHueResponse
        Console.WriteLine("Hue selected: " & Hue)
    End Sub

    Private Sub UOAI_Cl_onPacketReceive(ByRef Client As UOAI2.UOAI.Client, ByRef packet As UOAI2.UOAI.Packet) Handles UOAI_Cl.onPacketReceive
        If CheckBox1.Checked = True Then
            packetlog.WriteLine("'Recieved Packet #" & packetnumber)
            packetlog.WriteLine("'" & BitConverter.ToString(packet.Data))
            packetlog.WriteLine("")
            packetnumber += 1
            UOAI_Cl.HandlePacket(Enums.PacketOrigin.FROMSERVER)
        End If

        Exit Sub
        Select Case packet.Type
            Case Enums.PacketType.TextUnicode
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
            UOAI_Cl.HandlePacket(Enums.PacketOrigin.FROMCLIENT)
        End If

        Exit Sub
        Select Case packet.Type
            Case Enums.PacketType.SpeechUnicode

        End Select

    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        UOAI_Cl.PromptForHue(CUShort(2345))
    End Sub

    Private Sub UOAI_Cl_onTargetResponse(ByVal TargetInfo As UOAI2.UOAI.TargetInfo) Handles UOAI_Cl.onTargetResponse
        Select Case TargetInfo.UID
            Case 5568957
                Dim lay As Mobile.LayersClass = UOAI_Cl.Player.Layers
                Dim bp As Item = lay.BackPack
                Dim ilt As ItemList = bp.Contents
                Dim il As ItemList = UOAI_Cl.Player.Layers.BackPack.Contents.byType(TargetInfo.Type, True)

                Console.WriteLine("Number of results: " & il.Count)

            Case 5568956 'Used in the test example
                Select Case TargetInfo.Type
                    Case Enums.TargetType.Item
                        'its an item, so write out the serial
                        Console.WriteLine("Targeted Item, Serial:" & TargetInfo.Target.ToString)
                        If UOAI_Cl.Items.Exists(TargetInfo.Target) Then
                            Console.WriteLine("Item Exists In Database: " & TargetInfo.Target.ToString)
                            UOAI_Cl.Items.Item(TargetInfo.Target).ShowText("TEST!!")
                        Else

                            Console.WriteLine("Serial Does Not Exist: " & TargetInfo.Target.Value)

                        End If
                        '
                    Case Enums.TargetType.Mobile
                        'its a mobile so write the mobile's name out
                        Console.WriteLine("Targeted Mobile Named:" & UOAI_Cl.Mobiles.Mobile(TargetInfo.Target).Name)
                        DeathMobile = UOAI_Cl.Mobiles.Mobile(TargetInfo.Target)
                        UOAI_Cl.Mobiles.Mobile(TargetInfo.Target).ShowText("Assigned as death test!")

                    Case Enums.TargetType.Ground
                        'Print the coordinates to the console output
                        Console.WriteLine("Targeted Ground or Static Tile at location: X:" & TargetInfo.X & " Y:" & TargetInfo.Y & " Z:" & TargetInfo.Z)
                    Case Enums.TargetType.Canceled
                        'Do nothing, or display a message or w/e
                        Console.WriteLine("Target Canceled")
                End Select
            Case Else

#Const DebugTargeting = False

#If DebugTargeting Then
                Console.WriteLine("Packet: " & BitConverter.ToString(TargetInfo.TargetPacket.Data))
#End If

                Console.WriteLine("Unknown Target Response UID:" & TargetInfo.UID)

                Select Case TargetInfo.Type
                    Case Enums.TargetType.Item
                        'its an item, so write out the serial
                        Console.WriteLine("Targeted Item, Serial:" & TargetInfo.Target.ToString)
                    Case Enums.TargetType.Mobile
                        'its a mobile so write the mobile's name out
                        Console.WriteLine("Targeted Mobile Named:" & UOAI_Cl.Mobiles.Mobile(TargetInfo.Target).Name)
                    Case Enums.TargetType.Ground
                        'Print the coordinates to the console output
                        Console.WriteLine("Targeted Ground or Static Tile at location: X:" & TargetInfo.X & " Y:" & TargetInfo.Y & " Z:" & TargetInfo.Z)
                    Case Enums.TargetType.Canceled
                        'Do nothing, or display a message or w/e
                        Console.WriteLine("Target Canceled")
                End Select


        End Select
    End Sub

    Private Sub DeathMobile_onDeath(ByVal Client As UOAI2.UOAI.Client, ByVal Mobile As UOAI2.UOAI.Mobile, ByVal CorpseSerial As UOAI2.UOAI.Serial) Handles DeathMobile.onDeath
        UOAI_Cl.Items.Item(CorpseSerial).ShowText("I LOSE! 'Cause I'm Dead!")
    End Sub

    Private Sub Player_ContextMenuResponse(ByVal Index As UShort) Handles Player.ContextMenuResponse
        Select Case Index
            Case 0
                Console.WriteLine("Responded: 0")
            Case 1
                Console.WriteLine("Responded: 1")
            Case 2
                Console.WriteLine("Responded: 2")
        End Select
    End Sub
End Class
