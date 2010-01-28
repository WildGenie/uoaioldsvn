Imports System.IO, System.Net, System.Threading, System.ComponentModel.Design, System.Net.Sockets, System.Net.NetworkInformation, System.Diagnostics

Public Class Form1
    Public WithEvents LoginSocket As New ClientSocket(Me)
    Public WithEvents PlaySocket As New ClientSocket(Me)
    Public ConThread As Thread

    Public GameServerAddress As String
    Public GameServerPort As Integer
    Public AccountUID As Integer

    Public EmulatedVersion() As Byte = {0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0}
    Public WithEvents PingTicker As New System.Timers.Timer(60000)
    Public PingPacket() As Byte = {115, 0}

    Public OutPutTextBuffer As ArrayList

    Public CliLocStringList As New Hashtable

    Public WithEvents BF24Ticker As New System.Timers.Timer(3800)
    Public BF24Packet() As Byte = {191, 0, 6, 0, 36, 38}

    Public Class MobileInfo
        Public Serial As UInt32 = 0
        Public MaxStam As Integer = 0
        Public Stam As Integer = 0
        Public MaxHits As Integer = 0
        Public Hits As Integer = 0
        Public MaxMana As Integer = 0
        Public Mana As Integer = 0
        Public Name As String = ""
        Public Dexterity As Integer = 0
        Public Strength As Integer = 0
        Public Intelligence As Integer = 0
        Public CurrentFollowers As Integer = 0
        Public MaxFollowers As Integer = 0
        Public PoisonLevel As Integer = 0
        Public Poisoned As Boolean = False
        Public Hidden As Boolean = False
        Public Gold As Integer = 0
        Public RetrievingUpdates As Boolean = False

        Public X As Short = 0
        Public Y As Short = 0
        Public Z As SByte = 0

        Public Sub StartStatusUpdates()
            Dim packet(9) As Byte
            Dim SerialBytes() As Byte = BitConverter.GetBytes(Serial)
            packet(0) = 52
            packet(1) = 237
            packet(2) = 237
            packet(3) = 237
            packet(4) = 237
            packet(5) = 4
            packet(6) = SerialBytes(3)
            packet(7) = SerialBytes(2)
            packet(8) = SerialBytes(1)
            packet(9) = SerialBytes(0)
            Form1.PlaySocket.Send(packet)
            RetrievingUpdates = True
        End Sub

        Public Sub StopStatusUpdates()
            Dim packet(8) As Byte
            Dim SerialBytes() As Byte = BitConverter.GetBytes(Serial)
            packet(0) = 191
            packet(1) = 9
            packet(2) = 0
            packet(3) = 0
            packet(4) = 12
            packet(5) = SerialBytes(3)
            packet(6) = SerialBytes(2)
            packet(7) = SerialBytes(1)
            packet(8) = SerialBytes(0)
            Form1.PlaySocket.Send(packet)
            RetrievingUpdates = False
        End Sub

    End Class

    Public Player As New MobileInfo
    Public PetOne As New MobileInfo
    Public PetTwo As New MobileInfo
    Public PetThree As New MobileInfo
    Public PetFour As New MobileInfo
    Public PetFive As New MobileInfo

    Public Delegate Sub PlayerStatusBarsUpdater()
    Public Delegate Sub WriteOut(ByVal Text As String)

    Public Master As New MobileInfo
    Public Meditating As Boolean = False

#Region "Huffman Decompression Code"

    Public HuffmanDecompressingTree(,) As Short = _
     {{2, 1}, {4, 3}, {0, 5}, {7, 6}, _
     {9, 8}, {11, 10}, {13, 12}, {14, -256}, _
     {16, 15}, {18, 17}, {20, 19}, {22, 21}, _
     {23, -1}, {25, 24}, {27, 26}, {29, 28}, _
     {31, 30}, {33, 32}, {35, 34}, {37, 36}, _
     {39, 38}, {-64, 40}, {42, 41}, {44, 43}, _
     {45, -6}, {47, 46}, {49, 48}, {51, 50}, _
     {52, -119}, {53, -32}, {-14, 54}, {-5, 55}, _
     {57, 56}, {59, 58}, {-2, 60}, {62, 61}, _
     {64, 63}, {66, 65}, {68, 67}, {70, 69}, _
     {72, 71}, {73, -51}, {75, 74}, {77, 76}, _
     {-111, -101}, {-97, -4}, {79, 78}, {80, -110}, _
     {-116, 81}, {83, 82}, {-255, 84}, {86, 85}, _
     {88, 87}, {90, 89}, {-10, -15}, {92, 91}, _
     {93, -21}, {94, -117}, {96, 95}, {98, 97}, _
     {100, 99}, {101, -114}, {102, -105}, {103, -26}, _
     {105, 104}, {107, 106}, {109, 108}, {111, 110}, _
     {-3, 112}, {-7, 113}, {-131, 114}, {-144, 115}, _
     {117, 116}, {118, -20}, {120, 119}, {122, 121}, _
     {124, 123}, {126, 125}, {128, 127}, {-100, 129}, _
     {-8, 130}, {132, 131}, {134, 133}, {135, -120}, _
     {-31, 136}, {138, 137}, {-234, -109}, {140, 139}, _
     {142, 141}, {144, 143}, {145, -112}, {146, -19}, _
     {148, 147}, {-66, 149}, {-145, 150}, {-65, -13}, _
     {152, 151}, {154, 153}, {155, -30}, {157, 156}, _
     {158, -99}, {160, 159}, {162, 161}, {163, -23}, _
     {164, -29}, {165, -11}, {-115, 166}, {168, 167}, _
     {170, 169}, {171, -16}, {172, -34}, {-132, 173}, _
     {-108, 174}, {-22, 175}, {-9, 176}, {-84, 177}, _
     {-37, -17}, {178, -28}, {180, 179}, {182, 181}, _
     {184, 183}, {186, 185}, {-104, 187}, {-78, 188}, _
     {-61, 189}, {-178, -79}, {-134, -59}, {-25, 190}, _
     {-18, -83}, {-57, 191}, {192, -67}, {193, -98}, _
     {-68, -12}, {195, 194}, {-128, -55}, {-50, -24}, _
     {196, -70}, {-33, -94}, {-129, 197}, {198, -74}, _
     {199, -82}, {-87, -56}, {200, -44}, {201, -248}, _
     {-81, -163}, {-123, -52}, {-113, 202}, {-41, -48}, _
     {-40, -122}, {-90, 203}, {204, -54}, {-192, -86}, _
     {206, 205}, {-130, 207}, {208, -53}, {-45, -133}, _
     {210, 209}, {-91, 211}, {213, 212}, {-88, -106}, _
     {215, 214}, {217, 216}, {-49, 218}, {220, 219}, _
     {222, 221}, {224, 223}, {226, 225}, {-102, 227}, _
     {228, -160}, {229, -46}, {230, -127}, {231, -103}, _
     {233, 232}, {234, -60}, {-76, 235}, {-121, 236}, _
     {-73, 237}, {238, -149}, {-107, 239}, {240, -35}, _
     {-27, -71}, {241, -69}, {-77, -89}, {-118, -62}, _
     {-85, -75}, {-58, -72}, {-80, -63}, {-42, 242}, _
     {-157, -150}, {-236, -139}, {-243, -126}, {-214, -142}, _
     {-206, -138}, {-146, -240}, {-147, -204}, {-201, -152}, _
     {-207, -227}, {-209, -154}, {-254, -153}, {-156, -176}, _
     {-210, -165}, {-185, -172}, {-170, -195}, {-211, -232}, _
     {-239, -219}, {-177, -200}, {-212, -175}, {-143, -244}, _
     {-171, -246}, {-221, -203}, {-181, -202}, {-250, -173}, _
     {-164, -184}, {-218, -193}, {-220, -199}, {-249, -190}, _
     {-217, -230}, {-216, -169}, {-197, -191}, {243, -47}, _
     {245, 244}, {247, 246}, {-159, -148}, {249, 248}, _
     {-93, -92}, {-225, -96}, {-95, -151}, {251, 250}, _
     {252, -241}, {-36, -161}, {254, 253}, {-39, -135}, _
     {-124, -187}, {-251, 255}, {-238, -162}, {-38, -242}, _
     {-125, -43}, {-253, -215}, {-208, -140}, {-235, -137}, _
     {-237, -158}, {-205, -136}, {-141, -155}, {-229, -228}, _
     {-168, -213}, {-194, -224}, {-226, -196}, {-233, -183}, _
     {-167, -231}, {-189, -174}, {-166, -252}, {-222, -198}, _
     {-179, -188}, {-182, -223}, {-186, -180}, {-247, -245}}


    Public Function DecompressHuffman(ByVal buffer As Byte()) As Byte()
        Dim MyNode As Short = 0
        Dim DecomBytes As New MemoryStream

        For x As Integer = 0 To buffer.Length - 1

            For y As Integer = 7 To 0 Step -1
                MyNode = HuffmanDecompressingTree(MyNode, buffer(x) >> y And 1)

                Select Case MyNode
                    Case -256
                        MyNode = 0
                        Exit For
                    Case Is < 1
                        DecomBytes.WriteByte(CByte((-MyNode)))
                        MyNode = 0
                        Exit Select
                End Select

            Next

        Next


        Return DecomBytes.ToArray
    End Function

#End Region

#Region "Login Code"

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        LoginSocket.Connect(TextBox3.Text, TextBox4.Text)
    End Sub

    Private Sub LoginSocket_Connected(ByVal connected As Boolean) Handles LoginSocket.Connected
        If connected = False Then
            Button1.Enabled = True 'Connect
            Button2.Enabled = False 'Login
        Else
            Button1.Enabled = False 'Connect
            Button2.Enabled = True 'Login


            TextBox1.Enabled = True 'Account Name
            TextBox2.Enabled = True 'Password

            TextBox3.Enabled = False 'Server address
            TextBox4.Enabled = False 'Port
        End If
    End Sub

    Private Sub LoginSocket_Disconnected() Handles LoginSocket.Disconnected
        MsgBox("Connection Close by Server.")
    End Sub

    Private Sub LoginSocket_Receive(ByVal buffer() As Byte) Handles LoginSocket.Receive
        Select Case buffer(0)
            Case 168 'Server List
                'Since your login was successfull hide the connection groupbox
                GBConnection.Hide()

                'Populate and display the server list.
                Dim i As Short = 0
                Dim s As Integer = 0
                Dim ShardCountBytes(1) As Byte
                ShardCountBytes(0) = buffer(5)
                ShardCountBytes(1) = buffer(4)
                Dim ShardCount As Short = BitConverter.ToInt16(ShardCountBytes, 0)

                Dim Name As New ListViewItem
                Dim NameBytes(31) As Byte
                Dim IP As New ListViewItem.ListViewSubItem
                Dim Ping As New ListViewItem.ListViewSubItem
                Dim Load As New ListViewItem.ListViewSubItem

                ListView1.Items.Clear()
                Dim pinger As New Ping
                Dim reply As PingReply

                For i = 0 To ShardCount - 1

                    'Get The Name
                    For s = i + 8 To i + 39
                        NameBytes(s - (i + 8)) = buffer((i * 40) + s)
                    Next
                    Name.Text = System.Text.Encoding.ASCII.GetString(NameBytes)


                    IP.Text = buffer(i + 45) & "." & buffer(i + 44) & "." & buffer(i + 43) & "." & buffer(i + 42)
                    reply = pinger.Send(IP.Text)

                    Ping.Text = reply.RoundtripTime & "ms"

                    Load.Text = buffer(i + 40)

                    Name.SubItems.Add(IP)
                    Name.SubItems.Add(Ping)
                    Name.SubItems.Add(Load)
                    ListView1.Items.Add(Name)

                    Name = New ListViewItem
                    IP = New ListViewItem.ListViewSubItem
                    Ping = New ListViewItem.ListViewSubItem
                    Load = New ListViewItem.ListViewSubItem

                Next

                GBSelectCharacter.Show()
            Case 140
                GameServerAddress = buffer(1) & "." & buffer(2) & "." & buffer(3) & "." & buffer(4)

                Dim GameServerPortBytes(1) As Byte
                GameServerPortBytes(0) = buffer(6)
                GameServerPortBytes(1) = buffer(5)

                GameServerPort = BitConverter.ToInt16(GameServerPortBytes, 0)

                Dim AccountUIDBytes() As Byte = {buffer(10), buffer(9), buffer(8), buffer(7)}
                AccountUID = BitConverter.ToInt32(AccountUIDBytes, 0)

                PlaySocket.Connect(GameServerAddress, GameServerPort)

            Case Else
                'MsgBox(BitConverter.ToString(buffer))

        End Select
    End Sub

    Private Sub Login()
        'No idea what this pakcet is, but it seems necessary.
        Dim TZPacket() As Byte = {239}
        LoginSocket.Send(TZPacket)

        Dim NameBytes() As Byte = Get30Bytes(TextBox1.Text)
        Dim PassBytes() As Byte = Get30Bytes(TextBox2.Text)

        Dim LoginRequest(81) As Byte

        'User Encryption seed, sent to unencrypted servers, usualy is the local ip of the client.
        LoginRequest(0) = 192
        LoginRequest(1) = 168
        LoginRequest(2) = 1
        LoginRequest(3) = 120

        'Version number represented by 4 32 bit integers 6.0.13.0
        LoginRequest(4) = EmulatedVersion(0)
        LoginRequest(5) = EmulatedVersion(1)
        LoginRequest(6) = EmulatedVersion(2)
        LoginRequest(7) = EmulatedVersion(3)

        LoginRequest(8) = EmulatedVersion(4)
        LoginRequest(9) = EmulatedVersion(5)
        LoginRequest(10) = EmulatedVersion(6)
        LoginRequest(11) = EmulatedVersion(7)

        LoginRequest(12) = EmulatedVersion(8)
        LoginRequest(13) = EmulatedVersion(9)
        LoginRequest(14) = EmulatedVersion(10)
        LoginRequest(15) = EmulatedVersion(11)

        LoginRequest(16) = EmulatedVersion(12)
        LoginRequest(17) = EmulatedVersion(13)
        LoginRequest(18) = EmulatedVersion(14)
        LoginRequest(19) = EmulatedVersion(15)


        'LoginByte
        LoginRequest(20) = 128

        Dim i As Integer = 0

        'Add the account name
        For i = 21 To 50
            LoginRequest(i) = NameBytes(i - 21)
        Next

        'Add the password
        For i = 51 To 80
            LoginRequest(i) = PassBytes(i - 51)
        Next

        LoginRequest(81) = 93

        LoginSocket.Send(LoginRequest)
        GBSelectShard.Show()
        GBConnection.Hide()

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Login()
    End Sub

    Private Sub ListView1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListView1.DoubleClick
        Dim SelectBytes(2) As Byte
        SelectBytes(0) = 160
        SelectBytes(1) = 0
        SelectBytes(2) = ListView1.SelectedIndices(0)
        LoginSocket.Send(SelectBytes)
        GBSelectShard.Hide()
    End Sub

    Private Sub ListView2_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListView2.DoubleClick
        Dim packet(72) As Byte
        '0x5D Char Login Packet
        packet(0) = 93

        'Unknown 0xEDEDEDED
        packet(1) = 237
        packet(2) = 237
        packet(3) = 237
        packet(4) = 237

        Dim CharBytes() As Byte = Get30Bytes(ListView2.SelectedItems(0).Text)
        Dim PasswordBytes() As Byte = {0, 0, 0, 0, 0, 31, 0, 0, 0, 0, 0, 0, 0, 51, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

        'Character Name
        For i As Integer = 5 To 34
            packet(i) = CharBytes(i - 5)
        Next

        'Character Password
        For i As Integer = 35 To 64
            packet(i) = PasswordBytes(i - 35)
        Next

        'Character Slot
        packet(65) = 0
        packet(66) = 0
        packet(67) = 0
        packet(68) = ListView2.SelectedIndices(0)

        Dim EncBytes() As Byte = BitConverter.GetBytes(AccountUID)
        'User's encryption key
        packet(69) = 192
        packet(70) = 168
        packet(71) = 1
        packet(72) = 120

        'MsgBox(BitConverter.ToString(packet))
        PlaySocket.Send(packet)

        Panel1.Hide()

    End Sub

#End Region

    Public Function Get30Bytes(ByVal input As String) As Byte()
        Dim buffer(29) As Byte
        Dim encoding As New System.Text.ASCIIEncoding()
        Dim strbytes() As Byte = encoding.GetBytes(input)

        Dim i As Integer = 0

        'Put strbytes inside of the buffer
        For i = 0 To strbytes.Length - 1
            buffer(i) = strbytes(i)
        Next

        'Fill the rest of the buffer in with 0's
        For i = strbytes.Length To 29
            buffer(i) = 0
        Next

        Return buffer
    End Function

    Private Sub PlaySocket_Connected(ByVal connected As Boolean) Handles PlaySocket.Connected
        If connected = False Then Exit Sub

        LoginSocket.Close()
        GBSelectShard.Hide()

        Dim EncBytes() As Byte = BitConverter.GetBytes(AccountUID)
        Dim EncACK(3) As Byte
        EncACK(0) = EncBytes(3)
        EncACK(1) = EncBytes(2)
        EncACK(2) = EncBytes(1)
        EncACK(3) = EncBytes(0)

        PlaySocket.Send(EncACK)

        'As soon as it connects, send the username,password, and encryption key to request the character list.
        Dim CharListReq(64) As Byte
        Dim NameBytes() As Byte = Get30Bytes(TextBox1.Text)
        Dim PassBytes() As Byte = Get30Bytes(TextBox2.Text)

        CharListReq(0) = 145

        'Add the Encryption Key (accountuid)
        CharListReq(1) = EncBytes(3)
        CharListReq(2) = EncBytes(2)
        CharListReq(3) = EncBytes(1)
        CharListReq(4) = EncBytes(0)

        Dim i As Integer = 0

        'Add the account name
        For i = 5 To 34
            CharListReq(i) = NameBytes(i - 5)
        Next

        'Add the password
        For i = 35 To 64
            CharListReq(i) = PassBytes(i - 35)
        Next

        PlaySocket.Send(CharListReq)

    End Sub

    Private Sub PlaySocket_Disconnected() Handles PlaySocket.Disconnected
        MsgBox("Connection Close by Server.")
    End Sub

    Private Sub Say(ByVal Text As String)

    End Sub

    Private Sub Cast(ByVal Spell As Spell)
        Dim packet(6) As Byte
        packet(0) = 12
        packet(1) = 7
        packet(2) = 0
        packet(3) = 56
        packet(5) = Spell
        packet(6) = 0
        PlaySocket.Send(packet)
    End Sub

    Enum Spell
        Clumsy = 1
        CreateFood = 2
        Feeblemind = 3
        Heal = 4
        MagicArrow = 5
        NightSight = 6
        ReactiveArmor = 7
        Weaken = 8
        Agility = 9
        Cunning = 10
        Cure = 11
        Harm = 12
        MagicTrap = 13
        MagicUntrap = 14
        Protection = 15
        Strength = 16
        Bless = 17
        Fireball = 18
        MagicLock = 19
        Poison = 20
        Telekenisis = 21
        Teleport = 22
        Unlock = 23
        WallofStone = 24
        ArchCure = 25
        ArchProtection = 26
        Curse = 27
        FireField = 28
        GreaterHeal = 29
        Lightning = 30
        ManaDrain = 31
        Recall = 32
        BladeSpirit = 33
        DispelField = 34
        Incognito = 35
        Reflection = 36
        MindBlast = 37
        Paralyze = 38
        PoisonField = 39
        SummonCreature = 40
        Dispel = 41
        EnergyBolt = 42
        Explosion = 43
        Invisibility = 44
        Mark = 45
        MassCurse = 46
        ParalyzeField = 47
        Reveal = 48
        ChainLightning = 49
        EnergyField = 50
        FlameStrike = 51
        Gate = 52
        ManaVampire = 53
        MassDispel = 54
        MeteorShower = 55
        Polymorph = 56
        Earthquake = 57
        EnergyVortex = 58
        Ressurection = 59
        SummonAirElemental = 60
        SummonDaemon = 61
        SummonEarthElemental = 62
        SummonFireElemental = 63
        SummonWaterElemental = 64
    End Enum

    Private Sub UpdatePlayerStatusBar()
        PlayerHits.Maximum = Player.MaxHits
        PlayerHits.Value = Player.Hits

        PlayerMana.Maximum = Player.MaxMana
        PlayerMana.Value = Player.Mana

        PlayerStamina.Maximum = Player.MaxStam
        PlayerStamina.Value = Player.Stam

    End Sub

    Private Sub PacketHandler(ByVal buffer() As Byte)

    End Sub

    Private Sub PlaySocket_Receive(ByVal buffer() As Byte) Handles PlaySocket.Receive
        'Use huffman decompression to decompress the buffer
        buffer = DecompressHuffman(buffer)
        'YOU HAVE TO PARSE THE PACKETS! FUUUCK!

        'Take action based on packet type
        Select Case buffer(0)

            Case 27 '0x1B Login Confirmation
                Dim Intbuffer(3) As Byte
                Intbuffer(0) = buffer(4)
                Intbuffer(1) = buffer(3)
                Intbuffer(2) = buffer(2)
                Intbuffer(3) = buffer(1)

                Player.Serial = BitConverter.ToUInt32(Intbuffer, 0)
                PingTicker.Enabled = True
                BF24Ticker.Enabled = True

            Case 169 '0xA9 Character List 
                'Populate and display the Character list.

                Dim CharCount As Byte = buffer(3)

                Dim Name As New ListViewItem
                Dim NameBytes(29) As Byte

                Dim Password As New ListViewItem.ListViewSubItem
                Dim PasswordBytes(29) As Byte

                ListView2.Items.Clear()

                For i As Integer = 0 To CharCount - 1

                    'Get The Name
                    For s As Integer = i + 4 To i + 33
                        NameBytes(s - (i + 4)) = buffer((i * 60) + s)
                    Next

                    For s As Integer = i + 34 To i + 63
                        PasswordBytes(s - (i + 34)) = buffer((i * 60) + s)
                    Next

                    If NameBytes(0) = 0 Then
                        Exit For
                    Else
                        Name.Text = System.Text.Encoding.ASCII.GetString(NameBytes)
                        Password.Text = System.Text.Encoding.ASCII.GetString(PasswordBytes)

                        Name.SubItems.Add(Password)
                        ListView2.Items.Add(Name)

                        Name = New ListViewItem
                        Password = New ListViewItem.ListViewSubItem

                    End If
                Next

                Panel2.Show()

            Case 189 'Server Requesting Version String usualy looks like "BD-00-03"
                'Respond with 0xBD packet.
                Dim VerPacket(11) As Byte
                VerPacket(0) = 189

                'Packet Size
                VerPacket(1) = 0
                VerPacket(2) = 12

                '6.0.13.0 Version string with Null terminator.
                VerPacket(3) = 54 '6
                VerPacket(4) = 46 '.
                VerPacket(5) = 48 '0
                VerPacket(6) = 46 '.
                VerPacket(7) = 49 '1
                VerPacket(8) = 51 '3
                VerPacket(9) = 46 '.
                VerPacket(10) = 48 '0
                VerPacket(11) = 0 'Null Terminator

                PlaySocket.Send(VerPacket)

                Dim QueryPacket() As Byte = {52, 237, 237, 237, 237, 5, 0, 5, 78, 3}
                PlaySocket.Send(QueryPacket)

            Case 17 'Mobile Status packet

                'Get the serial of the mobile
                Dim Intbuffer(3) As Byte
                Intbuffer(0) = buffer(6)
                Intbuffer(1) = buffer(5)
                Intbuffer(2) = buffer(4)
                Intbuffer(3) = buffer(3)

                'See if the serial belongs to the player or his pets
                Select Case BitConverter.ToUInt32(Intbuffer, 0)
                    Case Player.Serial
                        'if they match then copy the data from the packet to the player mobileinfo object for storage
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(38)
                        ShortBuffer(1) = buffer(37)
                        Player.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        Dim NameBytes(29) As Byte
                        NameBytes(0) = buffer(7)
                        NameBytes(1) = buffer(8)
                        NameBytes(2) = buffer(9)
                        NameBytes(3) = buffer(10)
                        NameBytes(4) = buffer(11)
                        NameBytes(5) = buffer(12)
                        NameBytes(6) = buffer(13)
                        NameBytes(7) = buffer(14)
                        NameBytes(8) = buffer(15)
                        NameBytes(9) = buffer(16)
                        NameBytes(10) = buffer(17)
                        NameBytes(11) = buffer(18)
                        NameBytes(12) = buffer(19)
                        NameBytes(13) = buffer(20)
                        NameBytes(14) = buffer(21)
                        NameBytes(15) = buffer(22)
                        NameBytes(16) = buffer(23)
                        NameBytes(17) = buffer(24)
                        NameBytes(18) = buffer(25)
                        NameBytes(19) = buffer(26)
                        NameBytes(20) = buffer(27)
                        NameBytes(21) = buffer(28)
                        NameBytes(22) = buffer(29)
                        NameBytes(23) = buffer(30)
                        NameBytes(24) = buffer(31)
                        NameBytes(25) = buffer(32)
                        NameBytes(26) = buffer(33)
                        NameBytes(27) = buffer(34)
                        NameBytes(28) = buffer(35)
                        NameBytes(29) = buffer(36)
                        Player.Name = System.Text.Encoding.ASCII.GetString(NameBytes)

                        ShortBuffer(0) = buffer(39)
                        ShortBuffer(1) = buffer(40)
                        Player.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(49)
                        ShortBuffer(1) = buffer(48)
                        Player.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(51)
                        ShortBuffer(1) = buffer(50)
                        Player.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(53)
                        ShortBuffer(1) = buffer(52)
                        Player.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(55)
                        ShortBuffer(1) = buffer(54)
                        Player.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)

                        Player.CurrentFollowers = buffer(66)
                        Player.MaxFollowers = buffer(67)

                        Me.Invoke(New PlayerStatusBarsUpdater(AddressOf UpdatePlayerStatusBar))

                        Exit Select
                    Case PetOne.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(38)
                        ShortBuffer(1) = buffer(37)
                        PetOne.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(39)
                        ShortBuffer(1) = buffer(40)
                        PetOne.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(49)
                        ShortBuffer(1) = buffer(48)
                        PetOne.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(51)
                        ShortBuffer(1) = buffer(50)
                        PetOne.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(53)
                        ShortBuffer(1) = buffer(52)
                        PetOne.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(55)
                        ShortBuffer(1) = buffer(54)
                        PetOne.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)

                        PetOne.CurrentFollowers = buffer(66)
                        PetOne.MaxFollowers = buffer(67)

                        Exit Select
                    Case PetTwo.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(38)
                        ShortBuffer(1) = buffer(37)
                        PetTwo.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(39)
                        ShortBuffer(1) = buffer(40)
                        PetTwo.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(49)
                        ShortBuffer(1) = buffer(48)
                        PetTwo.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(51)
                        ShortBuffer(1) = buffer(50)
                        PetTwo.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(53)
                        ShortBuffer(1) = buffer(52)
                        PetTwo.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(55)
                        ShortBuffer(1) = buffer(54)
                        PetTwo.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)

                        PetTwo.CurrentFollowers = buffer(66)
                        PetTwo.MaxFollowers = buffer(67)

                        Exit Select
                    Case PetThree.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(38)
                        ShortBuffer(1) = buffer(37)
                        PetThree.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(39)
                        ShortBuffer(1) = buffer(40)
                        PetThree.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(49)
                        ShortBuffer(1) = buffer(48)
                        PetThree.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(51)
                        ShortBuffer(1) = buffer(50)
                        PetThree.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(53)
                        ShortBuffer(1) = buffer(52)
                        PetThree.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(55)
                        ShortBuffer(1) = buffer(54)
                        PetThree.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)

                        PetThree.CurrentFollowers = buffer(66)
                        PetThree.MaxFollowers = buffer(67)
                        Exit Select
                    Case PetFour.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(38)
                        ShortBuffer(1) = buffer(37)
                        PetFour.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(39)
                        ShortBuffer(1) = buffer(40)
                        PetFour.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(49)
                        ShortBuffer(1) = buffer(48)
                        PetFour.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(51)
                        ShortBuffer(1) = buffer(50)
                        PetFour.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(53)
                        ShortBuffer(1) = buffer(52)
                        PetFour.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(55)
                        ShortBuffer(1) = buffer(54)
                        PetFour.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)

                        PetFour.CurrentFollowers = buffer(66)
                        PetFour.MaxFollowers = buffer(67)
                        Exit Select
                    Case PetFive.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(38)
                        ShortBuffer(1) = buffer(37)
                        PetFive.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(39)
                        ShortBuffer(1) = buffer(40)
                        PetFive.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(49)
                        ShortBuffer(1) = buffer(48)
                        PetFive.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(51)
                        ShortBuffer(1) = buffer(50)
                        PetFive.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(53)
                        ShortBuffer(1) = buffer(52)
                        PetFive.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(55)
                        ShortBuffer(1) = buffer(54)
                        PetFive.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)

                        PetFive.CurrentFollowers = buffer(66)
                        PetFive.MaxFollowers = buffer(67)
                        Exit Select
                    Case Else
                        Exit Select
                End Select

            Case 161 'Hitpoint update
                'similar to the status update packet processor
                Dim Intbuffer(3) As Byte
                Intbuffer(0) = buffer(4)
                Intbuffer(1) = buffer(3)
                Intbuffer(2) = buffer(2)
                Intbuffer(3) = buffer(1)

                Select Case BitConverter.ToUInt32(Intbuffer, 0)
                    Case Player.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        Player.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        Player.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        Me.Invoke(New PlayerStatusBarsUpdater(AddressOf UpdatePlayerStatusBar))

                        Exit Select
                    Case PetOne.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetOne.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetOne.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        Exit Select
                    Case PetTwo.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetTwo.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetTwo.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)

                        Exit Select
                    Case PetThree.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetThree.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetThree.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetFour.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetFour.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetFour.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetFive.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetFive.Hits = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetFive.MaxHits = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case Else
                        Exit Select
                End Select

            Case 162 'Mana update

                'similar to the status update packet processor
                Dim Intbuffer(3) As Byte
                Intbuffer(0) = buffer(4)
                Intbuffer(1) = buffer(3)
                Intbuffer(2) = buffer(2)
                Intbuffer(3) = buffer(1)

                Select Case BitConverter.ToUInt32(Intbuffer, 0)
                    Case Player.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        Player.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        Player.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)

                        Me.Invoke(New PlayerStatusBarsUpdater(AddressOf UpdatePlayerStatusBar))

                        Exit Select
                    Case PetOne.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetOne.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetOne.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetTwo.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetTwo.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetTwo.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetThree.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetThree.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetThree.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetFour.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetFour.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetFour.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetFive.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetFive.Mana = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetFive.MaxMana = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case Else
                        Exit Select
                End Select

            Case 163 'Stamina update

                'similar to the status update packet processor
                Dim Intbuffer(3) As Byte
                Intbuffer(0) = buffer(4)
                Intbuffer(1) = buffer(3)
                Intbuffer(2) = buffer(2)
                Intbuffer(3) = buffer(1)

                Select Case BitConverter.ToUInt32(Intbuffer, 0)
                    Case Player.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        Player.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        Player.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)

                        Me.Invoke(New PlayerStatusBarsUpdater(AddressOf UpdatePlayerStatusBar))
                        Exit Select
                    Case PetOne.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetOne.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetOne.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetTwo.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetTwo.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetTwo.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetThree.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetThree.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetThree.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetFour.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetFour.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetFour.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case PetFive.Serial
                        Dim ShortBuffer(1) As Byte
                        ShortBuffer(0) = buffer(8)
                        ShortBuffer(1) = buffer(7)
                        PetFive.Stam = BitConverter.ToUInt16(ShortBuffer, 0)

                        ShortBuffer(0) = buffer(6)
                        ShortBuffer(1) = buffer(5)
                        PetFive.MaxStam = BitConverter.ToUInt16(ShortBuffer, 0)
                        Exit Select
                    Case Else
                        Exit Select
                End Select
            Case 28
                Dim NameBytes(29) As Byte
                Dim TextBytes(buffer.Length - 44) As Byte

                For i As Integer = 14 To 43
                    If buffer(i) > 0 Then
                        NameBytes(i - 14) = buffer(i)
                    Else
                        'Don't leave a 0x00 byte at the end, it is a null terminator!
                        ReDim Preserve NameBytes(i - 15)
                        Exit For
                    End If
                Next

                For i As Integer = 44 To buffer.Length - 2
                    TextBytes(i - 44) = buffer(i)
                Next

                Dim OutputString As String = System.Text.Encoding.ASCII.GetString(NameBytes) & " : " & System.Text.Encoding.BigEndianUnicode.GetString(TextBytes)
                Dim args(0) As Object
                args(0) = OutputString

                Me.Invoke(New WriteOut(AddressOf WriteOutput), args)

            Case 173

            Case 3

            Case 174
                Dim NameBytes(29) As Byte
                Dim TextBytes(buffer.Length - 48) As Byte

                For i As Integer = 18 To 47

                    If buffer(i) > 0 Then
                        NameBytes(i - 18) = buffer(i)
                    Else
                        'Don't leave a 0x00 byte at the end, it is a null terminator!
                        ReDim Preserve NameBytes(i - 19)
                        Exit For
                    End If

                Next

                For i As Integer = 48 To buffer.Length - 3 'Take away 3 to not leave a 0x00 byte at the end!
                    TextBytes(i - 48) = buffer(i)
                Next

                Dim OutputString As String = System.Text.Encoding.ASCII.GetString(NameBytes) & " : " & System.Text.Encoding.BigEndianUnicode.GetString(TextBytes)

                Dim args(0) As Object
                args(0) = OutputString

                Me.Invoke(New WriteOut(AddressOf WriteOutput), args)

            Case Else
                Dim OutputString As String = BitConverter.ToString(buffer)
                Dim args() As Object = {OutputString}

                Me.Invoke(New WriteOut(AddressOf WriteOutput), args)
                'MsgBox(OutputString)
        End Select

    End Sub

    Private Sub PingTicker_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles PingTicker.Elapsed
        If PlaySocket.IsConnected = True Then
            PlaySocket.Send(PingPacket)
        End If
    End Sub

    Public Function TypeName(ByVal TypeNumber As Integer)

        Try
            Dim ReturnString As String = CliLocStringList(1020000 + 16383 + TypeNumber)
            Return ReturnString
        Catch ex As Exception
            Return ex.Message.ToString
        End Try

    End Function

    Private Function GetCliLocString(ByVal Number As Integer) As String

        Try
            Dim ReturnString As String = CliLocStringList(Number)
            Return ReturnString
        Catch ex As Exception
            Return ex.Message.ToString
        End Try

    End Function

    Private Sub LoadCliLocStrings()
        If IO.File.Exists(Application.StartupPath & "\cliloclist.txt") = True Then
            Dim fr As IO.StreamReader = IO.File.OpenText(Application.StartupPath & "\cliloclist.txt")
            Dim ReadBuffer() As String
            Dim ReadStringBuffer As String

            Do
                ReadStringBuffer = fr.ReadLine
                If ReadStringBuffer = "" Then Exit Do
                ReadBuffer = ReadStringBuffer.Split(vbTab)
                CliLocStringList.Add(System.Convert.ToInt32(ReadBuffer(0)), ReadBuffer(1))

            Loop

            fr.Close()
        Else
            MsgBox("Unable to load clilocstring list!")
        End If
    End Sub

    Private Sub WriteOutput(ByVal Text As String)
        TextBox5.SuspendLayout()
        Me.TextBox5.Text &= vbNewLine & Text
        TextBox5.ResumeLayout()
    End Sub

    Private Sub BF24Ticker_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles BF24Ticker.Elapsed
        If PlaySocket.IsConnected = True Then
            Dim ran As New Random

            BF24Packet(5) = Math.Round(ran.Next(38, 131), 0)
            BF24Ticker.Interval = BF24Packet(5) * 100

            PlaySocket.Send(BF24Packet)
        End If
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        LoadCliLocStrings()
    End Sub

End Class



'==================================================================================================================================================================
'==================================================================================================================================================================


Public Class ClientSocket
    Inherits System.ComponentModel.Component

#Region " Component Designer generated code "

    Public Sub New(ByVal Form As System.Windows.Forms.Form)
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        _syncObject = Form
        '_mySocket.
    End Sub

    'Component overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
    End Sub

#End Region

#Region "Private Memebers"
    '--The Actual receive/send
    Private _mySocket As Socket
    '--Used to sync value type variables
    Private Shared _dummy As New ArrayList()
    '--Used to sync the Socket
    Private Shared _socketSyncObj As New ArrayList()
    '--Default packet size
    Private _packetSize As Int32 = 8192
    '--LineMode property
    Private _lineMode As Boolean = True
    '--Linefeed character denotes end of line
    Private _eolChar As Char = vbLf
    '--Used to convert byte arrays to strings and vice-versa
    Private _ascii As New System.Text.ASCIIEncoding()
    '--This is required to switch the context from the socket thread to the underlying calling
    '   container's thread when reasing events from the sockets receive data thread
    Private _syncObject As System.ComponentModel.ISynchronizeInvoke
#End Region

#Region "Delegates/Events"
    '--Delegates are required to raise events on the container's thread
    Private Delegate Sub RaiseReceiveEvent(ByVal buffer() As Byte)
    Private Delegate Sub RaiseConnectedEvent(ByVal connected As Boolean)
    Private Delegate Sub RaiseDisconnectedEvent()
    Private Delegate Sub RaiseExceptionEvent(ByVal ex As Exception)

    '--Event definitions
    Public Event Receive(ByVal buffer() As Byte)
    Public Event Connected(ByVal connected As Boolean)
    Public Event Disconnected()
    Public Event Exception(ByVal ex As Exception)
#End Region

#Region "Public methods"

#Region "Connect overloads"

    Public Sub Connect(ByVal hostNameOrAddress As String, ByVal port As Int32)
        Dim serverAddress As IPAddress
        Try
            serverAddress = Dns.GetHostEntry(hostNameOrAddress).AddressList(0)
        Catch ex As Exception
            Throw New Exception("Could not resolve Host name or Address.", ex)
        End Try

        Try
            Me.Connect(serverAddress, port)
        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Public Sub Connect(ByVal serverAddress As IPAddress, ByVal port As Int32)
        Dim ep As New IPEndPoint(serverAddress, port)
        Try
            Connect(ep)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub Connect(ByVal endPoint As IPEndPoint)
        '--Create a new socket
        _mySocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

        Try
            _mySocket.Connect(endPoint)
            If IsConnected() = True Then
                RaiseEvent Connected(True)
            End If
        Catch ex As Exception
            Throw New Exception("Count not connect", ex)
        End Try

        Dim bytes(_packetSize - 1) As Byte
        Try
            '-- This tells the socket to wait for received data on a new thread from the system thread pool.
            '  it passes the byte array as the buffer to receive the data and the array adain as a reference that will be passed
            '  back to the ReceiveCallback event
            _mySocket.BeginReceive(bytes, 0, bytes.Length, SocketFlags.None, AddressOf ReceiveCallBack, bytes)
        Catch ex As Exception
            Throw New Exception("Error receiving data", ex)
            If IsConnected() = False Then
                RaiseEvent Connected(False)
            End If
        End Try

    End Sub

#End Region

    '--It will return true only if the socket is connected, it will also check for nothing. Though the socket object
    '  has a connected property, it does bot accurately reflect the state of the socket connection.
    Public Function IsConnected() As Boolean
        Dim result As Boolean
        If _mySocket Is Nothing Then
            Return False
        End If

        SyncLock _socketSyncObj
            result = _mySocket.Poll(1, SelectMode.SelectRead)
        End SyncLock
        '--Still can be nothing
        If _mySocket Is Nothing Then
            Return False
        End If
        Dim temp As Int32
        SyncLock _socketSyncObj
            temp = _mySocket.Available
        End SyncLock

        '--In order to successfully test to see if the socket is connected, we AND the results of the Poll methos
        '  and the avialable property.
        If result = True And temp = 0 Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Sub Send(ByVal buffer() As Byte)
        If IsConnected() = False Then
            RaiseEvent Connected(False)

        Else
            '--Send the string data and block the thread until all data is sent
            SyncLock _socketSyncObj
                _mySocket.Send(buffer, buffer.Length, SocketFlags.None)
            End SyncLock
        End If

    End Sub

    Public Sub Close()
        If IsConnected() = True Then
            SyncLock _socketSyncObj
                _mySocket.Shutdown(SocketShutdown.Both)
                _mySocket.Close()
                _mySocket = Nothing
                If IsConnected() = False Then
                    RaiseEvent Connected(False)
                End If
            End SyncLock
        End If
    End Sub

#End Region

#Region "Overridable routines called from the container"

    Protected Overridable Sub OnReceive(ByVal buffer() As Byte)
        RaiseEvent Receive(buffer)
    End Sub

    Protected Overridable Sub OnConnected(ByVal connected As Boolean)
        RaiseEvent Connected(connected)
    End Sub

    Protected Overridable Sub OnDisconnected()
        RaiseEvent Disconnected()
    End Sub

    Protected Overridable Sub OnExcpetion(ByVal ex As Exception)
        RaiseEvent Exception(ex)
    End Sub

#End Region

#Region "Receive Callback"
    Private Sub ReceiveCallBack(ByVal ar As IAsyncResult)
        '--Retreive array of bytes
        Dim bytes() As Byte = ar.AsyncState

        '--Get number of bytes received and also clean up resources that was used from beginReceive
        Dim numBytes As Int32 = _mySocket.EndReceive(ar)

        '--Did we receive anything?
        If numBytes > 0 Then
            '--Resize the array to match the number of bytes received. Also keep the current data
            ReDim Preserve bytes(numBytes - 1)

            '--Now we need to raise the received event. 
            '  args() is used to pass an argument from this thread to the synchronized container's ui thread.
            Dim args(0) As Object

            '--Create a new delegate for the OnReceive event
            Dim d As New RaiseReceiveEvent(AddressOf OnReceive)

            '--Not line mode. Pass the entire string at once with only one event
            args(0) = bytes

            '--Invoke the private delegate from the thread. 
            _syncObject.Invoke(d, args)

        End If

        '--Are we stil conncted?
        If IsConnected() = False Then
            '--Raise the connect event
            Dim args() As Object = {False}
            Dim d As New RaiseConnectedEvent(AddressOf OnConnected)
            _syncObject.Invoke(d, args)

        Else
            '--Yes, then resize bytes to packet size
            ReDim bytes(PacketSize - 1)

            '--Call BeginReceive again, catching any error
            Try
                _mySocket.BeginReceive(bytes, 0, bytes.Length, SocketFlags.None, AddressOf ReceiveCallBack, bytes)
            Catch ex As Exception
                '--Raise the exception event 
                Dim args() As Object = {ex}
                Dim d As New RaiseExceptionEvent(AddressOf OnExcpetion)
                _syncObject.Invoke(d, args)

                '--If not connected, raise the connected event
                If IsConnected() = False Then
                    args(0) = False
                    Dim dl As New RaiseConnectedEvent(AddressOf OnConnected)
                    _syncObject.Invoke(dl, args)
                End If
            End Try
        End If

    End Sub
#End Region

#Region "Properties"

    Public Property PacketSize() As Int32
        Get
            Dim pk As Int32
            SyncLock _dummy
                pk = _packetSize
            End SyncLock
            Return pk
        End Get
        Set(ByVal Value As Int32)
            SyncLock _dummy
                _packetSize = Value
            End SyncLock
        End Set
    End Property

    <System.ComponentModel.Browsable(False)> _
    Public Property SynchronizingObject() As System.ComponentModel.ISynchronizeInvoke
        Get
            If _syncObject Is Nothing And Me.DesignMode Then
                Dim designer As IDesignerHost = Me.GetService(GetType(IDesignerHost))
                If Not (designer Is Nothing) Then
                    _syncObject = designer.RootComponent
                End If
            End If
            Return _syncObject
        End Get
        Set(ByVal Value As System.ComponentModel.ISynchronizeInvoke)
            If Not Me.DesignMode Then
                If Not (_syncObject Is Nothing) And Not (_syncObject Is Value) Then
                    Throw New Exception("Property ca not be set at run-time")
                Else
                    _syncObject = Value
                End If
            End If
        End Set
    End Property
#End Region

End Class
