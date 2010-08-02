Imports System.Net, System.Net.Sockets, System.Text, System.IO, System.Net.NetworkInformation, Microsoft.Win32

'Saves the packets to a file, packetlog.txt.
#Const LogPackets = False

'Prints out when data is recieved and when it is handled, and reports on buffer size.
#Const TrafficStats = False

'Prints the packet contents to the debug output.
#Const DebugPackets = False

'Debugging huffman decompression code. (not used for compressed buffer)
#Const DebugHuffman = False

'Whether or not the huffman code is used to determine packet sizes. (not used for compressed buffer)
#Const HuffManPacketSizing = False

'Decompressed the packets one at a time instead of all of them at once when they are recieved. (NEEDS TO BE ON!!)
#Const CompressedBuffer = True

Public Class LiteClient

#Region "Base Declarations"
    Protected Friend Shared ReadOnly WorldSerial = New Serial(Convert.ToUInt32(4294967295))
    Const PacketSize As Integer = 1024 * 32 '32kB packet buffer.
    Private Shared BufferSize As UInteger = 1024 * 1024 * 5 '5 Megabytes

#If HuffManPacketSizing Then
    Private Shared PacketSizeBufferSize As UInteger = 1024 * 1024 * 5 '5 Megabytes
    Private Shared PacketSizeBuffer As New CircularIntBuffer(PacketSizeBufferSize)
#End If

    Private _EmulatedVersion() As Byte = {0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0} 'Version 6.0.13.0

    'How often the packet handler handles a single packet.
    'If the client is lagging this can be decreased to handle the packets
    'faster as they come in.
    Private PacketReadInterval As Integer = 1 'ms

#If LogPackets Then
    Private PacketLog As System.IO.StreamWriter = File.CreateText(Application.StartupPath & "\packets.log")
#End If

    Friend Shared StrLst As StringList
    Private _LoginClient As TcpClient
    Private _LoginStream As NetworkStream
    Private _GameClient As TcpClient
    Private _GameStream As NetworkStream
    Protected Friend Shared ClientPath As String

    Protected Friend StepSync As Byte = 0

    Protected Friend _AllItems As New Hashtable
    Protected Friend _ItemInHand As Serial
    Protected Friend _WaitingForTarget As Boolean
    Protected Friend _Targeting As Boolean = False
    Protected Friend _TargetUID As UInteger
    Protected Friend _TargetType As Byte
    Protected Friend _TargetFlag As Byte

    Private Shared ProcessingPacket As Boolean = False

    Protected Friend _CharacterList As New ArrayList
    Public ReadOnly Property CharacterList As ArrayList
        Get
            Return _CharacterList
        End Get
    End Property

    Protected Friend _Player As Mobile

#Region "Events"

    ''' <summary>
    ''' Called when the client recieves a login confirm packets from the game server, and the player character is created.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event onLoginConfirm(ByRef Player As Mobile)

    ''' <summary> Called when an error occures within UOLite. It is important that you have something here to handle this.</summary>
    ''' <param name="Description">The description of the error and possibly extra details.</param>
    Public Event onError(ByRef Description As String)

    ''' <summary>Called when the clientis completely logged in, after all the items and everything loads completely.</summary>
    Public Event onLoginComplete()

    ''' <summary>
    ''' Called when a Packet arrives on this client.
    ''' </summary>
    ''' <param name="Client">Client on which the packet was received</param>
    ''' <param name="bytes">The received packet</param>
    Public Event onPacketReceive(ByRef Client As LiteClient, ByRef bytes() As Byte)

    ''' <summary>
    ''' Called when a Packet is sent formt he client to the server.
    ''' </summary>
    ''' <param name="Client">Client from which the packet was sent.</param>
    ''' <param name="bytes">The sent packet.</param>
    Public Event onPacketSend(ByRef Client As LiteClient, ByRef bytes() As Byte)

    ''' <summary>
    ''' Called when the client loses its network connection to the server.
    ''' </summary>
    ''' <param name="Client">The client that lost its connection.</param>
    Public Event onConnectionLoss(ByRef Client As LiteClient)

    ''' <summary>
    ''' Called when a mobile is created and added to the mobile list.
    ''' </summary>
    ''' <param name="Client">The client to which this applies.</param>
    ''' <param name="Mobile">The new mobile.</param>
    Public Event onNewMobile(ByRef Client As LiteClient, ByVal Mobile As Mobile)

    ''' <summary>
    ''' Called after a new item is created and added to the item list.
    ''' </summary>
    ''' <param name="Client">The client to which this applies.</param>
    ''' <param name="Item">The new item.</param>
    Public Event onNewItem(ByRef Client As LiteClient, ByVal Item As Item)

    ''' <summary>
    ''' Called when the server sends the client a CliLoc speech packet. This is after the client processes the packet.
    ''' </summary>
    ''' <param name="Client">The client to which this applies.</param>
    ''' <param name="Serial">The serial of the mobile/item speaking. 0xFFFFFFFF for System</param>
    ''' <param name="BodyType">The bodytype/artwork of the mobile/item speaking. 0xFFFF for System</param>
    ''' <param name="SpeechType">The type of speech.</param>
    ''' <param name="Hue">The hue of the message.</param>
    ''' <param name="Font">The font of the message.</param>
    ''' <param name="CliLocNumber">The cliloc number.</param>
    ''' <param name="Name">The name of the speaker. "SYSTEM" for System.</param>
    ''' <param name="ArgsString">The arguements string, for formatting the speech. Each arguement is seperated by a "\t".</param>
    Public Event onCliLocSpeech(ByRef Client As LiteClient, ByVal Serial As Serial, ByVal BodyType As UShort, ByVal SpeechType As Enums.SpeechTypes, ByVal Hue As UShort, ByVal Font As Enums.Fonts, ByVal CliLocNumber As UInteger, ByVal Name As String, ByVal ArgsString As String)

    ''' <summary>Called when the client recieves a "text" or "Unicode Text" packet from the server.</summary>
    ''' <param name="Client">The client to which this applies.</param>
    ''' <param name="Serial">The serial of the mobile/item speaking. 0xFFFFFFFF for System</param>
    ''' <param name="BodyType">The bodytype/artwork of the mobile/item speaking. 0xFFFF for System</param>
    ''' <param name="SpeechType">The type of speech.</param>
    ''' <param name="Hue">The hue of the message.</param>
    ''' <param name="Font">The font of the message.</param>
    ''' <param name="Text">The text to be displayed.</param>
    ''' <param name="Name">The name of the speaker. "SYSTEM" for System.</param>
    Public Event onSpeech(ByRef Client As LiteClient, ByVal Serial As Serial, ByVal BodyType As UShort, ByVal SpeechType As Enums.SpeechTypes, ByVal Hue As UShort, ByVal Font As Enums.Fonts, ByVal Text As String, ByVal Name As String)

    ''' <summary>Called when the server sends the list of characters.</summary>
    ''' <param name="Client">The client making the call</param>
    ''' <param name="CharacterList">The list of characters as <see cref="CharListEntry">CharacterListEntry</see>'s.</param>
    Public Event onCharacterListReceive(ByRef Client As LiteClient, ByVal CharacterList As ArrayList)

#End Region

    Friend Sub NewItem(ByVal Item As Item)
        RaiseEvent onNewItem(Me, Item)
    End Sub

    Private Shared GameBuffer As New CircularBuffer(BufferSize)
    Private WithEvents GameBufferTimer As New System.Timers.Timer(PacketReadInterval) With {.Enabled = False}


#Region "Properties"
    ''' <summary>The password used to connect to the server.</summary>
    Public Property Password As String

    ''' <summary>The username used to connect to the server.</summary>
    Public Property Username As String

    Private _LoginServerAddress As String
    ''' <summary>Returns the ip address that is used when connecting to the login server to get the game server list.</summary>
    Public ReadOnly Property LoginServerAddress As String
        Get
            Return _LoginServerAddress
        End Get
    End Property

    Private _LoginPort As UShort = 2593
    ''' <summary>Returns the port that is used when connecting to the login server to get the game server list.</summary>
    Public ReadOnly Property LoginPort As UShort
        Get
            Return _LoginPort
        End Get
    End Property

    Private _GameServerAddress As String
    ''' <summary>Returns the ip address of the game server that you are connected to.</summary>
    Public ReadOnly Property GameServerAddress As String
        Get
            Return _GameServerAddress
        End Get
    End Property

    Public ReadOnly Property Connected As Boolean
        Get
            If _LoginClient.Connected Then
                Return True
            ElseIf _GameClient.Connected Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Private _GameServerPort As UShort = 2595
    Public ReadOnly Property GameServerPort As UShort
        Get
            Return _GameServerPort
        End Get
    End Property

    Private _AccountUID As UInt32 = 0
    Public ReadOnly Property AccountUID As UInt32
        Get
            Return _AccountUID
        End Get
    End Property

    Public ReadOnly Property Player() As Mobile
        Get
            Return _Player
        End Get
    End Property

    Protected Friend _Items As New Item(Me, WorldSerial)
    Protected Friend _Mobiles As New MobileList(Me)

    ''' <summary>
    ''' Returns the client's itemlist.
    ''' </summary>
    Public ReadOnly Property Items() As ItemList
        Get
            'Should this be all items?
            Return _Items.Contents
        End Get
    End Property

    Public ReadOnly Property Mobiles() As MobileList
        Get
            Return _Mobiles
        End Get
    End Property

#End Region

    Private _LoginStreamReader As NetworkStream
    Private _GameServerList() As GameServerInfo

    Public Event RecievedServerList(ByRef ServerList() As GameServerInfo)
    Public Event LoginDenied(ByRef Reason As String)

#End Region

#Region "Low Level Connection stuff"

#Region "Stupid BF-xx-xx-00-24 timer and emulation stuff"
    Public WithEvents BF24Ticker As New System.Timers.Timer(3800)
    Public BF24Packet() As Byte = {191, 0, 6, 0, 36, 38}

    Private Sub BF24Ticker_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles BF24Ticker.Elapsed
        If _GameClient.Connected = True Then
            Dim ran As New Random

            BF24Packet(5) = Math.Round(ran.Next(38, 131), 0)
            BF24Ticker.Interval = BF24Packet(5) * 100

            _GameStream.Write(BF24Packet, 0, 6)
        End If
    End Sub

    'dont know what this does... but the real client sends it.
    Private specialpacket() As Byte = {&H3, &H0, &H32, &H20, &H2, &HB2, &H0, &H3, &HDB, &H13, &H14, &H3F, &H45, &H2C, &H68, &H38, &H3, &H4D, &H39, &H47, &H54, &H9C, &H7B, &H9, &HB5, &H76, &H51, &HF7, &H3C, &H35, &HFE, &H7A, &H9B, &H94, &H73, &H64, &HF0, &HEC, &H61, &HB0, &HE5, &H59, &H7F, &H4F, &H3F, &H3C, &H13, &H46, &H1F, &H28}

#End Region

    '''<summary>Gets the working directory and location of client.exe from the registry.</summary>
    Private Sub InitializeClientPaths()
        Dim hklm As RegistryKey = Registry.LocalMachine
        Dim originkey As RegistryKey = hklm.OpenSubKey("SOFTWARE\Origin Worlds Online\Ultima Online\1.0")

        If originkey Is Nothing Then
            originkey = hklm.OpenSubKey("SOFTWARE\Origin Worlds Online\Ultima Online Third Dawn\1.0")
        End If

        If originkey IsNot Nothing Then
            Dim instcdpath As String = DirectCast(originkey.GetValue("InstCDPath"), String)
            If instcdpath IsNot Nothing Then
                ClientPath = instcdpath & "\"
                originkey.Close()
                Exit Sub
            End If
            originkey.Close()
        End If

        'use default values
        ClientPath = "C:\Program Files\EA Games\Ultima Online Mondain's Legacy\"

        Exit Sub
    End Sub

    Public ReadOnly Property ServerList As GameServerInfo()
        Get
            If _GameServerList Is Nothing Then
                Throw New ApplicationException("The ServerList was accessed, but it hasn't been populated yet! This is a fatal exception!")
            End If

            Return _GameServerList
        End Get
    End Property

    ''' Hide this class from the user, there is no reason from him/her to see it.
    ''' <summary>Simply a class to hold information about game servers when recieved from the login server.</summary>
    Public Class GameServerInfo

        Public Sub New(ByRef Name As String, ByRef Address As String, ByRef Load As Byte)
            _Name = Name
            _Address = Address
            _Load = Load
        End Sub

        ''' <summary>The IP address of the server.</summary>
        Private _Address As String
        Public ReadOnly Property Address As String
            Get
                Return _Address
            End Get
        End Property

        ''' <summary>The name of the server, as provided by the login server.</summary>
        Private _Name As String
        Public ReadOnly Property Name As String
            Get
                Return _Name
            End Get
        End Property

        Private _Load As Integer = 0
        Public ReadOnly Property Load As Integer
            Get
                Return _Load
            End Get
        End Property

        ''' <summary>The latency from the client to the server and back in milliseconds (ms). This is retrieved by sending a ping to the server when this property is called.</summary>
        Public ReadOnly Property Latency As Integer
            Get
                Dim pinger As New Ping
                Dim reply As PingReply
                reply = pinger.Send(_Address)
                Return reply.RoundtripTime
            End Get
        End Property

    End Class

    Public Sub ReverseByteArray(ByRef Bytes() As Byte)
        Dim tempbyte As Byte = 0

        For i As Integer = 0 To Bytes.Length \ 2
            tempbyte = Bytes(i)
            Bytes(i) = Bytes(Bytes.Length - 1 - i)
            Bytes(Bytes.Length - 1 - i) = tempbyte
        Next

    End Sub

    Public Overloads Sub Send(ByRef Packet As Packet)
        If _LoginClient.Connected Then
            _LoginStream.Write(Packet.Data, 0, Packet.Size)
        ElseIf _GameClient.Connected Then
            _GameStream.Write(Packet.Data, 0, Packet.Size)
        Else
            Throw New ApplicationException("Unable to send packet, you are not connected!")
        End If
    End Sub

    Public Overloads Sub Send(ByRef Packet() As Byte)
        If _LoginClient.Connected Then
            _LoginStream.Write(Packet, 0, Packet.Length)
        ElseIf _GameClient.Connected Then
            _GameStream.Write(Packet, 0, Packet.Length)
        Else
            Throw New ApplicationException("Unable to send packet, you are not connected!")
        End If
    End Sub

    ''' <summary>Connects to the specified login server and populates the ServerList property.</summary>
    ''' <param name="Address">The address of the login server to connect to.</param>
    ''' <param name="Port">The port to connect to (default is 2593).</param>
    ''' <param name="Username">The username to connect with.</param>
    ''' <param name="Password">The cooresponding password for the supplied username.</param>
    Public Overloads Function GetServerList(ByVal Username As String, ByVal Password As String, Optional ByVal Address As String = "login.uogamers.com", Optional ByVal Port As UShort = 2593) As String
        Try
            _LoginClient = New TcpClient()
            _LoginClient.ReceiveBufferSize = PacketSize
            _LoginClient.Connect(Address, Port)

        Catch ex As Exception
            'MsgBox("Unable to connect to the login server: " & ex.Message)
            Return "ERROR: Unable to connect to login server: " & ex.Message
        End Try

        _LoginStream = _LoginClient.GetStream

        'idk wtf this is for, but it seems to need it to make the server happy.
        Dim TZPacket() As Byte = {239}
        _LoginStream.Write(TZPacket, 0, TZPacket.Length)

        Dim LoginPacket(81) As Byte

        'Generate the encryption seed.
        Dim seed() As Byte = {192, 168, 1, 145}

        'add the seed to the login packet.
        InsertBytes(seed, LoginPacket, 0, 0, 4)

        'Add the version number.
        LoginPacket(4) = _EmulatedVersion(0)
        LoginPacket(5) = _EmulatedVersion(1)
        LoginPacket(6) = _EmulatedVersion(2)
        LoginPacket(7) = _EmulatedVersion(3)

        LoginPacket(8) = _EmulatedVersion(4)
        LoginPacket(9) = _EmulatedVersion(5)
        LoginPacket(10) = _EmulatedVersion(6)
        LoginPacket(11) = _EmulatedVersion(7)

        LoginPacket(12) = _EmulatedVersion(8)
        LoginPacket(13) = _EmulatedVersion(9)
        LoginPacket(14) = _EmulatedVersion(10)
        LoginPacket(15) = _EmulatedVersion(11)

        LoginPacket(16) = _EmulatedVersion(12)
        LoginPacket(17) = _EmulatedVersion(13)
        LoginPacket(18) = _EmulatedVersion(14)
        LoginPacket(19) = _EmulatedVersion(15)

        'Necessary...
        LoginPacket(20) = 128

        'Add the username.
        InsertBytes(GetBytesFromString(30, Username), LoginPacket, 0, 21, 30)
        _Username = Username

        'Add the password.
        InsertBytes(GetBytesFromString(30, Password), LoginPacket, 0, 51, 30)
        _Password = Password

        'Add umm 93?
        LoginPacket(81) = 93

        'Synchronouslysend the packet to the server.
        _LoginStream.Write(LoginPacket, 0, LoginPacket.Length)
        _LoginServerAddress = DirectCast(_LoginClient.Client.RemoteEndPoint, System.Net.IPEndPoint).Address.ToString

        'Set up asynchronous reading.
        Dim RecBuffer(PacketSize) As Byte
        _LoginStream.BeginRead(RecBuffer, 0, RecBuffer.Length, AddressOf LoginRecieve, RecBuffer)

        Return "SUCCESS"

    End Function

    Public Overloads Function ChooseServer(ByRef Index As Byte) As Boolean
        'If not connected then simply give up and return false.
        If Not Connected Then Return False

        Dim SelectBytes(2) As Byte
        SelectBytes(0) = 160
        SelectBytes(1) = 0
        SelectBytes(2) = Index

        _LoginStream.Write(SelectBytes, 0, SelectBytes.Length)

        Return True
    End Function

    Private Sub LoginRecieve(ByVal ar As IAsyncResult)
        '--Retreive array of bytes
        Dim bytes() As Byte = ar.AsyncState

        '--Get number of bytes received and also clean up resources that was used from beginReceive
        Dim numBytes As Int32 = _LoginStream.EndRead(ar)

        '--Did we receive anything?
        If numBytes > 0 Then
            '--Resize the array to match the number of bytes received. Also keep the current data
            ReDim Preserve bytes(numBytes - 1)

            Select Case bytes(0)
                Case 168 'Server List

                    'Populate and display the server list.
                    Dim i As Short = 0
                    Dim s As Integer = 0
                    Dim ShardCountBytes(1) As Byte
                    ShardCountBytes(0) = bytes(5)
                    ShardCountBytes(1) = bytes(4)
                    Dim ShardCount As Short = BitConverter.ToInt16(ShardCountBytes, 0)

                    Dim NameBytes(31) As Byte
                    Dim svrlist(ShardCount - 1) As GameServerInfo

                    For i = 0 To ShardCount - 1
                        'Get The Name
                        For s = i + 8 To i + 39
                            NameBytes(s - (i + 8)) = bytes((i * 40) + s)
                        Next

                        'Create the gameserverinfo object.
                        svrlist(i) = New GameServerInfo(System.Text.Encoding.ASCII.GetString(NameBytes).Replace(Chr(0), ""), bytes(i + 45) & "." & bytes(i + 44) & "." & bytes(i + 43) & "." & bytes(i + 42), bytes(i + 40))

                    Next

                    'copy the game server list into the GameServerList...
                    _GameServerList = svrlist

                    RaiseEvent RecievedServerList(ServerList)

                Case 140

                    _GameServerAddress = bytes(1) & "." & bytes(2) & "." & bytes(3) & "." & bytes(4)

                    Dim GameServerPortBytes(1) As Byte
                    GameServerPortBytes(0) = bytes(6)
                    GameServerPortBytes(1) = bytes(5)

                    _GameServerPort = BitConverter.ToInt16(GameServerPortBytes, 0)

                    Dim AccountUIDBytes() As Byte = {bytes(10), bytes(9), bytes(8), bytes(7)}
                    _AccountUID = BitConverter.ToUInt32(AccountUIDBytes, 0)

                    Try
                        _GameClient = New TcpClient
                        _GameClient.ReceiveBufferSize = PacketSize
                        _GameClient.Connect(GameServerAddress, GameServerPort)
                        _GameStream = _GameClient.GetStream
                    Catch ex As Exception
                        RaiseEvent onError("Failed to connect to game server: " & ex.Message)
                    End Try

                    Dim EncBytes() As Byte = BitConverter.GetBytes(AccountUID)
                    Dim EncACK(3) As Byte
                    EncACK(0) = EncBytes(3)
                    EncACK(1) = EncBytes(2)
                    EncACK(2) = EncBytes(1)
                    EncACK(3) = EncBytes(0)
                    _GameStream.Write(EncACK, 0, EncACK.Length)

                    'As soon as it connects, send the username,password, and encryption key to request the character list.
                    Dim CharListReq(64) As Byte
                    Dim NameBytes() As Byte = GetBytesFromString(30, _Username)
                    Dim PassBytes() As Byte = GetBytesFromString(30, _Password)

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

                    _GameStream.Write(CharListReq, 0, CharListReq.Length)

                    'Set up asynchronous reading.
                    Dim RecBuffer(PacketSize) As Byte
                    _GameStream.BeginRead(RecBuffer, 0, RecBuffer.Length, AddressOf GameRecieve, RecBuffer)

                    'Close the connection to the server
                    _LoginClient.Close()
                    _LoginStream.Close()
                    Exit Sub

                Case 130
                    Select Case bytes(1)
                        Case 0
                            RaiseEvent LoginDenied("Invalid Username/Password")
                        Case 1
                            RaiseEvent LoginDenied("Someone is already using this account!")
                        Case 2
                            RaiseEvent LoginDenied("Your account has been locked!")
                        Case 3
                            RaiseEvent LoginDenied("Your account credentials are invalid!")
                        Case 4
                            RaiseEvent LoginDenied("Commmunication Problem.")
                        Case 5
                            RaiseEvent LoginDenied("The IGR concurrency limit has been met")
                        Case 6
                            RaiseEvent LoginDenied("The IGR time limit has been met")
                        Case 7
                            RaiseEvent LoginDenied("General IGR authentication failure")
                        Case Else
                            RaiseEvent LoginDenied("Login Denied: Unknown Reason")
                    End Select
                Case Else
                    'Handle unknown packet?!?
                    '...
                    'na, just ignore it....

            End Select


        End If

        '--Are we stil conncted?
        If _LoginClient.Connected = False Then
            'Do something about being disconnected?

        Else
            '--Yes, then resize bytes to packet size
            ReDim bytes(PacketSize - 1)

            '--Call BeginReceive again, catching any error
            Try
                _LoginStream.BeginRead(bytes, 0, bytes.Length, AddressOf LoginRecieve, bytes)
            Catch ex As Exception
                'Deal with the disconnect?
            End Try
        End If
    End Sub

    ''' <summary>Returns the given string as a byte array, padded as specified.</summary>
    ''' <param name="Length">The size of the array you want back.</param>
    ''' <param name="Text">The text you want encoded in bytes.</param>
    ''' <param name="Unicode">Whether or not you want unicode or not.</param>
    ''' <param name="NullTerminate">Whether to add the null bytes to the end of the string.</param>
    Private Function GetBytesFromString(ByRef Length As Integer, ByRef Text As String, Optional ByRef NullTerminate As Boolean = False, Optional ByRef Unicode As Boolean = False) As Byte()
        Dim bytes(Length - 1) As Byte 'make an empty array the size specified.
        Dim encoding As New System.Text.ASCIIEncoding() 'make an encoder.
        Dim strbytes() As Byte = encoding.GetBytes(Text) 'get the encoder to encode the bytes.

        'copy the bytes into the new array.
        For i As Integer = 0 To strbytes.Length - 1
            bytes(i) = strbytes(i)
        Next

        'return the new array of bytes with the ascii string into it.
        Return bytes
    End Function

    ''' <summary>Copies bytes from one array to another.</summary>
    ''' <param name="SourceArray">Where to get the bytes.</param>
    ''' <param name="TargetArray">Where to put the bytes.</param>
    ''' <param name="SourceStartIndex">The position in the source array to start reading.</param>
    ''' <param name="TargetStartIndex">The position in the target array to start writing.</param>
    ''' <param name="Size">The number of bytes to copy.</param>
    Private Sub InsertBytes(ByRef SourceArray() As Byte, ByRef TargetArray() As Byte, ByRef SourceStartIndex As Integer, ByRef TargetStartIndex As Integer, ByRef Size As Integer)
        For i As Integer = TargetStartIndex To TargetStartIndex + Size - 1
            TargetArray(i) = SourceArray(i - TargetStartIndex + SourceStartIndex)
        Next
    End Sub

    ''' <summary>Asynchronously recieves packets and writes them to the head of the packet buffer.</summary>
    Private Sub GameRecieve(ByVal ar As IAsyncResult)
        '--Retreive array of bytes
        Dim bytes() As Byte = ar.AsyncState

        '--Get number of bytes received and also clean up resources that was used from beginReceive
        Dim numBytes As Int32 = _GameStream.EndRead(ar)

        '--Did we receive anything?
        If numBytes > 0 Then
            '--Resize the array to match the number of bytes received. Also keep the current data
            ReDim Preserve bytes(numBytes - 1)

            'Disable the timer to pause handling packets.
            GameBufferTimer.Enabled = False

            'just wait for the timer thread to come to a halt.
            While ProcessingPacket
                'do nothing...
                Threading.Thread.Sleep(PacketReadInterval + 1)
            End While

#If Not CompressedBuffer Then
            'Decompress the bytes and add them to the GameBuffer.
            Dim buffbytes() As Byte = DecompressHuffman(bytes)
            GameBuffer.Write(buffbytes)
#Else
            'Write compressed data to the buffer.
            GameBuffer.Write(bytes)
#End If


            If GameBuffer.Size > BufferSize - 1 Then Throw New ApplicationException("Game Buffer Overflow!")

#If TrafficStats Then
#If Not CompressedBuffer Then
            Debug.WriteLine("Recieved " & buffbytes.Length & " bytes (" & bytes.Length & " compressed) from the server.")
#Else
            Debug.WriteLine("Recieved " & bytes.Length & " compressed bytes from the server.")
#End If
#End If

            'Re-enable the timer, to resume handling packets.
            GameBufferTimer.Enabled = True

            'GameBufferWrite.Write(DecompressHuffman(bytes))

        End If

        '--Are we still connected?
        If _GameClient.Connected = False Then
            'Do something about being disconnected?

        Else
            '--Yes, then resize bytes to packet size
            ReDim bytes(PacketSize - 1)

            '--Call BeginReceive again, catching any error
            Try
                _GameStream.BeginRead(bytes, 0, bytes.Length, AddressOf GameRecieve, bytes)
            Catch ex As Exception
                'Deal with the disconnect?
            End Try
        End If
    End Sub

    ''' <summary>Reads bytes from the tail of the packet buffer and parses them into actual game packets. Then calls the appropriet subroutine to handle them.</summary>
    Private Sub HandleGamePacket()
        ProcessingPacket = True

#If Not CompressedBuffer Then
#If HuffManPacketSizing Then
        If PacketSizeBuffer.Size = 0 Then
            'There are not any mroe complete packets to handle.
#If DebugPackets Then
            Debug.WriteLine("INFO: No complete packets in the buffer to handle yet, waiting for more data from the server.")
#End If
            GameBufferTimer.Enabled = False
            ProcessingPacket = False
            Exit Sub
        End If

        Dim PacketSize As UInteger = PacketSizeBuffer.ReadInt
#Else
        Dim PacketSize As UInteger = GetPacketSize(GameBuffer.Peek(0))

        If GameBuffer.Size < PacketSize Then
#If DebugPackets Then
            Debug.WriteLine("WARNING: Not enough data in the buffer for this whole packet! Waiting for next packet from the server!")
#End If
            ProcessingPacket = False
            GameBufferTimer.Enabled = False
            Exit Sub
        End If
#End If
#End If

#If Not CompressedBuffer Then
        'make the byte array to hold the individual packet.
        Dim Packet(PacketSize - 1) As Byte

        'Fill the packet byte array with the bytes from the packet.
        InsertBytes(GameBuffer.Read(PacketSize), Packet, 0, 0, PacketSize)
#Else

        Dim Packet() As Byte

        'Attempt to decompress a single packet.
        'If the decompression algorithm returned nothing then display a warning and exit.
        If DecompressSinglePacket(True) Is Nothing Then
#If DebugPackets Then
            Debug.WriteLine("WARNING: Not enough data in the buffer for this whole packet! Waiting for next packet from the server!")
#End If
            ProcessingPacket = False
            GameBufferTimer.Enabled = False
            Exit Sub
        Else
            'otherwise, decompress the packet again, this time, no peeking!
            Packet = DecompressSinglePacket(False)
        End If


#End If

#If LogPackets Then
        PacketLog.WriteLine("PACKET: " & BitConverter.ToString(Packet))
#End If

#If DebugPackets Then
        Debug.WriteLine("PACKET: " & BitConverter.ToString(Packet))
#End If

        'Raise the onPacketReceive event
        RaiseEvent onPacketReceive(Me, Packet)

        'Build the actual packet object and send it to get handled.
        PacketHandling(BuildPacket(Packet))

#If TrafficStats Then
        Debug.WriteLine(Packet.Length & " byte packet handled, " & GameBuffer.Size & " bytes left in buffer.")
#End If

        'Let everything else know that you finished handling this packet.
        ProcessingPacket = False
    End Sub

    ''' <summary>Handles a packet however it needs to be handled.</summary>
    ''' <param name="currentpacket">The packet to process.</param>
    Private Sub PacketHandling(ByRef currentpacket As Packet)
        Select Case currentpacket.Type
            Case Enums.PacketType.ClientVersion
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

                'Respond with version string.
                _GameStream.Write(VerPacket, 0, VerPacket.Length)

            Case Enums.PacketType.CharacterList
                _CharacterList = DirectCast(currentpacket, Packets.CharacterList).CharacterList
                RaiseEvent onCharacterListReceive(Me, _CharacterList)

#If DebugPackets Then
                Debug.WriteLine("Character List")
#End If
            Case Enums.PacketType.MobileStats
                'We already know now that the mobile exists, because this packet isnt sent until after the MOB is created
                'So there is no need to check for the existance of the MOB. Just send the packet to the mobile for it to update itself.
                'This is done through direct casts and hash tables, so its REALLY fast.
                _Mobiles.Mobile(DirectCast(currentpacket, Packets.MobileStats).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.MobileStats))

#If DebugPackets Then
                Debug.WriteLine("Mobile Stats")
#End If
            Case Enums.PacketType.HPHealth
                _Mobiles.Mobile(DirectCast(currentpacket, Packets.HPHealth).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.HPHealth))

#If DebugPackets Then
                Debug.WriteLine("HP Health")
#End If

            Case Enums.PacketType.FatHealth
                _Mobiles.Mobile(DirectCast(currentpacket, Packets.FatHealth).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.FatHealth))

#If DebugPackets Then
                Debug.WriteLine("Fat Health")
#End If

            Case Enums.PacketType.ManaHealth
                _Mobiles.Mobile(DirectCast(currentpacket, Packets.ManaHealth).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.ManaHealth))

#If DebugPackets Then
                Debug.WriteLine("Mana Health")
#End If

            Case Enums.PacketType.NakedMOB
                _Mobiles.AddMobile(DirectCast(currentpacket, Packets.NakedMobile))

#If DebugPackets Then
                Debug.WriteLine("Naked MOB")
#End If

            Case Enums.PacketType.EquippedMOB
                _Mobiles.AddMobile(DirectCast(currentpacket, Packets.EquippedMobile))

#If DebugPackets Then
                Debug.WriteLine("Equipped MOB")
#End If

            Case Enums.PacketType.DeathAnimation
                _Mobiles.Mobile(DirectCast(currentpacket, Packets.DeathAnimation).Serial).HandleDeathPacket(DirectCast(currentpacket, Packets.DeathAnimation))

#If DebugPackets Then
                Debug.WriteLine("Death Animation")
#End If

            Case Enums.PacketType.Destroy
                RemoveObject(DirectCast(currentpacket, Packets.Destroy).Serial)

#If DebugPackets Then
                Debug.WriteLine("Destroy Object")
#End If

            Case Enums.PacketType.EquipItem
                _Mobiles.Mobile(DirectCast(currentpacket, Packets.EquipItem).Container).HandleUpdatePacket(DirectCast(currentpacket, Packets.EquipItem))

#If DebugPackets Then
                Debug.WriteLine("Equip Item")
#End If

            Case Enums.PacketType.ContainerContents
                Items.Add(DirectCast(currentpacket, Packets.ContainerContents))

#If DebugPackets Then
                Debug.WriteLine("Container Contents")
#End If

            Case Enums.PacketType.ObjecttoObject
                Items.Add(DirectCast(currentpacket, Packets.ObjectToObject))

#If DebugPackets Then
                Debug.WriteLine("Object To Object")
#End If

            Case Enums.PacketType.ShowItem
                Items.Add(DirectCast(currentpacket, Packets.ShowItem))

#If DebugPackets Then
                Debug.WriteLine("Show Item")
#End If

            Case Enums.PacketType.Target
#If DebugPackets Then
                Debug.WriteLine("Target Request")
#End If

            Case Enums.PacketType.HuePicker
#If DebugPackets Then
                Debug.WriteLine("Hue Picker")
#End If

            Case Enums.PacketType.LoginConfirm
                'Make a new playerclass
                Dim pl As New Mobile(Me, DirectCast(currentpacket, Packets.LoginConfirm).Serial)

                'Apply the packet's info to the new playerclass
                pl._Type = DirectCast(currentpacket, Packets.LoginConfirm).BodyType
                pl._X = DirectCast(currentpacket, Packets.LoginConfirm).X
                pl._Y = DirectCast(currentpacket, Packets.LoginConfirm).Y
                pl._Z = DirectCast(currentpacket, Packets.LoginConfirm).Z
                pl._Direction = DirectCast(currentpacket, Packets.LoginConfirm).Direction

                _Player = pl

                'Cast the player as a mobile and add it to the mobile list.
                _Mobiles.AddMobile(pl)

#If DebugPackets Then
                Debug.WriteLine("Login Confirm")
#End If

                RaiseEvent onLoginConfirm(Player)
            Case Enums.PacketType.HealthBarStatusUpdate
                _Mobiles.Mobile(DirectCast(currentpacket, Packets.HealthBarStatusUpdate).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.HealthBarStatusUpdate))

#If DebugPackets Then
                Debug.WriteLine("Health Bar Status Update")
#End If

            Case Enums.PacketType.LocalizedText
                RaiseEvent onCliLocSpeech(Me, DirectCast(currentpacket, Packets.LocalizedText).Serial, _
                                          DirectCast(currentpacket, Packets.LocalizedText).BodyType, _
                                           DirectCast(currentpacket, Packets.LocalizedText).SpeechType, _
                                            DirectCast(currentpacket, Packets.LocalizedText).Hue, _
                                            DirectCast(currentpacket, Packets.LocalizedText).Font, _
                                            DirectCast(currentpacket, Packets.LocalizedText).CliLocNumber, _
                                            DirectCast(currentpacket, Packets.LocalizedText).Name, _
                                            DirectCast(currentpacket, Packets.LocalizedText).ArgString)

#If DebugPackets Then
                Debug.WriteLine("Localized Text")
#End If

            Case Enums.PacketType.Text
                RaiseEvent onSpeech(Me, DirectCast(currentpacket, Packets.Text).Serial, _
                                          DirectCast(currentpacket, Packets.Text).BodyType, _
                                          DirectCast(currentpacket, Packets.Text).SpeechType, _
                                          DirectCast(currentpacket, Packets.Text).TextHue, _
                                          DirectCast(currentpacket, Packets.Text).TextFont, _
                                          DirectCast(currentpacket, Packets.Text).Text, _
                                          DirectCast(currentpacket, Packets.Text).Name)

#If DebugPackets Then
                Debug.WriteLine("Text")
#End If

            Case Enums.PacketType.TextUnicode
                RaiseEvent onSpeech(Me, DirectCast(currentpacket, Packets.UnicodeText).Serial, _
                      DirectCast(currentpacket, Packets.UnicodeText).Body, _
                      DirectCast(currentpacket, Packets.UnicodeText).Mode, _
                      DirectCast(currentpacket, Packets.UnicodeText).Hue, _
                      DirectCast(currentpacket, Packets.UnicodeText).Font, _
                      DirectCast(currentpacket, Packets.UnicodeText).Text, _
                      DirectCast(currentpacket, Packets.UnicodeText).Name)

#If DebugPackets Then
                Debug.WriteLine("Unicode Text")
#End If

            Case Enums.PacketType.LoginComplete
                'Start sending keepalive packets.
                BF24Ticker.Enabled = True

                'send special packet??
                _GameStream.Write(specialpacket, 0, specialpacket.Length)

                RaiseEvent onLoginComplete()

#If DebugPackets Then
                Debug.WriteLine("Login Complete")
#End If

        End Select
    End Sub

    Private Sub RemoveObject(ByRef Serial As Serial)
        Try
            If Serial.Value >= 1073741824 Then
                Me.Items.RemoveItem(Serial)
            Else
                _Mobiles.RemoveMobile(Serial)
            End If
        Catch ex As Exception
            Debug.WriteLine("Failed to delete item: " & ex.Message)
        End Try

    End Sub

    Private Function BuildPacket(ByRef packetbuffer As Byte()) As Packet

#If PacketLogging = True Then
            If origin = Enums.PacketOrigin.FROMCLIENT Then
                Console.WriteLine("Sent Packet: " & BitConverter.ToString(packetbuffer))
            Else
                Console.WriteLine("Recieved Packet: " & BitConverter.ToString(packetbuffer))
            End If
#End If
        Try

            Select Case DirectCast(packetbuffer(0), Enums.PacketType)
                Case Enums.PacketType.TakeObject
                    Dim k As New Packets.TakeObject(packetbuffer)
                    _ItemInHand = k.Serial
                    Return k

                Case Enums.PacketType.DropObject
                    Return New Packets.DropObject(packetbuffer)

                Case Enums.PacketType.TextUnicode
                    Return New Packets.UnicodeText(packetbuffer)

                Case Enums.PacketType.SpeechUnicode
                    Return New Packets.UnicodeSpeechPacket(packetbuffer)

                Case Enums.PacketType.NakedMOB
                    Return New Packets.NakedMobile(packetbuffer)

                Case Enums.PacketType.EquippedMOB
                    Return New Packets.EquippedMobile(packetbuffer)

                Case Enums.PacketType.FatHealth
                    Return New Packets.FatHealth(packetbuffer)

                Case Enums.PacketType.HPHealth
                    Return New Packets.HPHealth(packetbuffer)

                Case Enums.PacketType.ManaHealth
                    Return New Packets.ManaHealth(packetbuffer)

                Case Enums.PacketType.DeathAnimation
                    Return New Packets.DeathAnimation(packetbuffer)

                Case Enums.PacketType.Destroy
                    Return New Packets.Destroy(packetbuffer)

                Case Enums.PacketType.MobileStats
                    Return New Packets.MobileStats(packetbuffer)

                Case Enums.PacketType.EquipItem
                    Return New Packets.EquipItem(packetbuffer)

                Case Enums.PacketType.ContainerContents
                    Return New Packets.ContainerContents(packetbuffer)

                Case Enums.PacketType.ObjecttoObject
                    Return New Packets.ObjectToObject(packetbuffer)

                Case Enums.PacketType.ShowItem
                    Return New Packets.ShowItem(packetbuffer)

                Case Enums.PacketType.Target
                    Dim k As Packets.Target = New Packets.Target(packetbuffer)

                    'Set the targeting variables.


                    Return k
                Case Enums.PacketType.DoubleClick
                    Return New Packets.Doubleclick(packetbuffer)

                Case Enums.PacketType.SingleClick
                    Return New Packets.Singleclick(packetbuffer)

                Case Enums.PacketType.Text
                    Return New Packets.Text(packetbuffer)

                Case Enums.PacketType.LoginConfirm
                    Return New Packets.LoginConfirm(packetbuffer)

                Case Enums.PacketType.HealthBarStatusUpdate
                    Return New Packets.HealthBarStatusUpdate(packetbuffer)

                Case Enums.PacketType.GenericCommand

                    Select Case DirectCast(CUShort(packetbuffer(4)), Enums.BF_Sub_Commands)

                        Case Enums.BF_Sub_Commands.ContextMenuRequest
                            Return New Packets.ContextMenuRequest(packetbuffer)

                        Case Enums.BF_Sub_Commands.ContextMenuResponse
                            Return New Packets.ContextMenuResponse(packetbuffer)

                        Case Else
                            Dim j As New Packet(packetbuffer(0))
                            j._Data = packetbuffer
                            j._size = packetbuffer.Length
                            Return j 'dummy until we have what we need
                    End Select

                Case Enums.PacketType.HuePicker
                    Return New Packets.HuePicker(packetbuffer)

                Case Enums.PacketType.LocalizedText
                    Return New Packets.LocalizedText(packetbuffer)

                Case Enums.PacketType.LoginComplete
                    Return New Packets.LoginComplete(packetbuffer)

                Case Enums.PacketType.CharacterList
                    Return New Packets.CharacterList(packetbuffer)

                Case Else
                    Dim j As New Packet(packetbuffer(0))
                    j._Data = packetbuffer
                    j._size = packetbuffer.Length
                    Return j 'dummy until we have what we need
            End Select

        Catch ex As Exception
            Dim k() As Byte = {0}
            Dim j As New Packet(k(0))

            j._Data = packetbuffer
            j._size = packetbuffer.Length

            Return j
        End Try

    End Function

    Public Sub ChooseCharacter(ByRef CharacterName As String, ByRef Password As String, ByVal Slot As Byte)
        If Not _GameClient.Connected Then Exit Sub

        Dim packet(72) As Byte
        '0x5D Char Login Packet
        packet(0) = 93

        'Unknown 0xEDEDEDED
        packet(1) = 237
        packet(2) = 237
        packet(3) = 237
        packet(4) = 237

        Dim CharBytes() As Byte = GetBytesFromString(30, CharacterName)
        Dim PasswordBytes() As Byte = GetBytesFromString(30, Password)

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
        packet(68) = Slot

        'Dim EncBytes() As Byte = BitConverter.GetBytes(AccountUID)
        'User's encryption key
        packet(69) = 192
        packet(70) = 168
        packet(71) = 1
        packet(72) = 120

        'MsgBox(BitConverter.ToString(packet))
        Send(packet)

    End Sub

#Region "Huffman Decompression Code"

#Region "Huffman Tree"

    Private HuffmanDecompressingTree(,) As Short = _
    {{2, 1},
     {4, 3},
     {0, 5},
     {7, 6},
     {9, 8},
     {11, 10},
     {13, 12},
     {14, -256},
     {16, 15},
     {18, 17},
     {20, 19},
     {22, 21},
     {23, -1},
     {25, 24},
     {27, 26},
     {29, 28},
     {31, 30},
     {33, 32},
     {35, 34},
     {37, 36},
     {39, 38},
     {-64, 40},
     {42, 41},
     {44, 43},
     {45, -6},
     {47, 46},
     {49, 48},
     {51, 50},
     {52, -119},
     {53, -32},
     {-14, 54},
     {-5, 55},
     {57, 56},
     {59, 58},
     {-2, 60},
     {62, 61},
     {64, 63},
     {66, 65},
     {68, 67},
     {70, 69},
     {72, 71},
     {73, -51},
     {75, 74},
     {77, 76},
     {-111, -101},
     {-97, -4},
     {79, 78},
     {80, -110},
     {-116, 81},
     {83, 82},
     {-255, 84},
     {86, 85},
     {88, 87},
     {90, 89},
     {-10, -15},
     {92, 91},
     {93, -21},
     {94, -117},
     {96, 95},
     {98, 97},
     {100, 99},
     {101, -114},
     {102, -105},
     {103, -26},
     {105, 104},
     {107, 106},
     {109, 108},
     {111, 110},
     {-3, 112},
     {-7, 113},
     {-131, 114},
     {-144, 115},
     {117, 116},
     {118, -20},
     {120, 119},
     {122, 121},
     {124, 123},
     {126, 125},
     {128, 127},
     {-100, 129},
     {-8, 130},
     {132, 131},
     {134, 133},
     {135, -120},
     {-31, 136},
     {138, 137},
     {-234, -109},
     {140, 139},
     {142, 141},
     {144, 143},
     {145, -112},
     {146, -19},
     {148, 147},
     {-66, 149},
     {-145, 150},
     {-65, -13},
     {152, 151},
     {154, 153},
     {155, -30},
     {157, 156},
     {158, -99},
     {160, 159},
     {162, 161},
     {163, -23},
     {164, -29},
     {165, -11},
     {-115, 166},
     {168, 167},
     {170, 169},
     {171, -16},
     {172, -34},
     {-132, 173},
     {-108, 174},
     {-22, 175},
     {-9, 176},
     {-84, 177},
     {-37, -17},
     {178, -28},
     {180, 179},
     {182, 181},
     {184, 183},
     {186, 185},
     {-104, 187},
     {-78, 188},
     {-61, 189},
     {-178, -79},
     {-134, -59},
     {-25, 190},
     {-18, -83},
     {-57, 191},
     {192, -67},
     {193, -98},
     {-68, -12},
     {195, 194},
     {-128, -55},
     {-50, -24},
     {196, -70},
     {-33, -94},
     {-129, 197},
     {198, -74},
     {199, -82},
     {-87, -56},
     {200, -44},
     {201, -248},
     {-81, -163},
     {-123, -52},
     {-113, 202},
     {-41, -48},
     {-40, -122},
     {-90, 203},
     {204, -54},
     {-192, -86},
     {206, 205},
     {-130, 207},
     {208, -53},
     {-45, -133},
     {210, 209},
     {-91, 211},
     {213, 212},
     {-88, -106},
     {215, 214},
     {217, 216},
     {-49, 218},
     {220, 219},
     {222, 221},
     {224, 223},
     {226, 225},
     {-102, 227},
     {228, -160},
     {229, -46},
     {230, -127},
     {231, -103},
     {233, 232},
     {234, -60},
     {-76, 235},
     {-121, 236},
     {-73, 237},
     {238, -149},
     {-107, 239},
     {240, -35},
     {-27, -71},
     {241, -69},
     {-77, -89},
     {-118, -62},
     {-85, -75},
     {-58, -72},
     {-80, -63},
     {-42, 242},
     {-157, -150},
     {-236, -139},
     {-243, -126},
     {-214, -142},
     {-206, -138},
     {-146, -240},
     {-147, -204},
     {-201, -152},
     {-207, -227},
     {-209, -154},
     {-254, -153},
     {-156, -176},
     {-210, -165},
     {-185, -172},
     {-170, -195},
     {-211, -232},
     {-239, -219},
     {-177, -200},
     {-212, -175},
     {-143, -244},
     {-171, -246},
     {-221, -203},
     {-181, -202},
     {-250, -173},
     {-164, -184},
     {-218, -193},
     {-220, -199},
     {-249, -190},
     {-217, -230},
     {-216, -169},
     {-197, -191},
     {243, -47},
     {245, 244},
     {247, 246},
     {-159, -148},
     {249, 248},
     {-93, -92},
     {-225, -96},
     {-95, -151},
     {251, 250},
     {252, -241},
     {-36, -161},
     {254, 253},
     {-39, -135},
     {-124, -187},
     {-251, 255},
     {-238, -162},
     {-38, -242},
     {-125, -43},
     {-253, -215},
     {-208, -140},
     {-235, -137},
     {-237, -158},
     {-205, -136},
     {-141, -155},
     {-229, -228},
     {-168, -213},
     {-194, -224},
     {-226, -196},
     {-233, -183},
     {-167, -231},
     {-189, -174},
     {-166, -252},
     {-222, -198},
     {-179, -188},
     {-182, -223},
     {-186, -180},
     {-247, -245}}

#End Region

    Private Function DecompressHuffman(ByVal buffer As Byte()) As Byte()
        Dim MyNode As Short = 0
        Dim DecomBytes As New MemoryStream(PacketSize * 2)
        Dim OldPos As UInteger = 0

        For x As Integer = 0 To buffer.Length - 1

            For y As Integer = 7 To 0 Step -1
                MyNode = HuffmanDecompressingTree(MyNode, buffer(x) >> y And 1)

                Select Case MyNode
                    Case -256
                        MyNode = 0

#If HuffManPacketSizing Then
#If DebugHuffman Then
                        Debug.WriteLine("HUFFMAN SAYS: Packet Size " & DecomBytes.Position - OldPos)
#End If
                        'Add the packet size to the packet size buffer for later parsing.
                        PacketSizeBuffer.WriteInt(DecomBytes.Position - OldPos)

                        'Reset old position for measuring offset for next packet size.
                        OldPos = DecomBytes.Position
#End If

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

#If Not HuffManPacketSizing Then

    Private PacketLengths() As UShort = {&H68, &H5, &H7, &H0, &H2, &H5, &H5, &H7,
                                          &HE, &H5, &H7, &H7, &H0, &H3, &H0, &H3D,
                                          &HD7, &H0, &H0, &HA, &H6, &H9, &H1, &H0,
                                          &H0, &H0, &H0, &H25, &H0, &H5, &H4, &H8,
                                          &H13, &H8, &H3, &H1A, &H7, &H14, &H5, &H2,
                                          &H5, &H1, &H5, &H2, &H2, &H11, &HF, &HA,
                                          &H5, &H1, &H2, &H2, &HA, &H28D, &H0, &H8,
                                          &H7, &H9, &H0, &H0, &H0, &H2, &H25, &H0,
                                          &HC9, &H0, &H0, &H229, &H2C9, &H5, &H0, &HB,
                                          &H49, &H5D, &H5, &H9, &H0, &H0, &H6, &H2,
                                          &H0, &H0, &H0, &H2, &HC, &H1, &HB, &H6E,
                                          &H6A, &H0, &H0, &H4, &H2, &H49, &H0, &H31,
                                          &H5, &H9, &HF, &HD, &H1, &H4, &H0, &H15,
                                          &H0, &H0, &H3, &H9, &H13, &H3, &HE, &H0,
                                          &H1C, &H0, &H5, &H2, &H0, &H23, &H10, &H11,
                                          &H0, &H9, &H0, &H2, &H0, &HD, &H2, &H0,
                                          &H3E, &H0, &H2, &H27, &H45, &H2, &H0, &H0,
                                          &H42, &H0, &H0, &H0, &HB, &H0, &H0, &H0,
                                          &H13, &H41, &H0, &H63, &H0, &H9, &H0, &H2,
                                          &H0, &H1A, &H0, &H102, &H135, &H33, &H0, &H0,
                                          &H3, &H9, &H9, &H9, &H95, &H0, &H0, &H4,
                                          &H0, &H0, &H5, &H0, &H0, &H0, &H0, &HD,
                                          &H0, &H0, &H0, &H0, &H0, &H40, &H9, &H0,
                                          &H0, &H3, &H6, &H9, &H3, &H0, &H0, &H0,
                                          &H24, &H0, &H0, &H0, &H6, &HCB, &H1, &H31,
                                          &H2, &H6, &H6, &H7, &H0, &H1, &H0, &H4E,
                                          &H0, &H2, &H19, &H0, &H0, &H0, &H0, &H0,
                                          &H0, &H10C, &HFFFF, &HFFFF, &H9, &H0, &HFFFF, &HFFFF,
                                          &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF,
                                          &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &H15,
                                          &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF,
                                          &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF, &HFFFF}

    Public Function GetPacketSize(ByRef Type As Byte) As UInteger
        Dim ret As UInteger = PacketLengths(Type)



        Select Case ret
            Case 0
                ret = BitConverter.ToUInt16({GameBuffer.Peek(2), GameBuffer.Peek(1)}, 0)
            Case &HFFFF
                Throw New ApplicationException("Invalid Packet Size!")
        End Select

        'Should be 14, but for some reason isnt always 14...
        If Type = &H78 And ret <= GameBuffer.Size Then
            'Copy the entire buffer in a byte array.
            Dim buff() As Byte = GameBuffer.ToArray

            For i As UInteger = 0 To buff.Length
                If buff(i) = 0 Then
                    If buff(i + 1) = 0 Then
                        If buff(i + 2) = 0 Then
                            If buff(i + 3) = 0 Then
                                '4 zeroes in a row
                                ret = i + 4
                                Exit For
                            End If
                        End If
                    End If
                End If
            Next

            'Something so high that it thinks that it has to wait to recieve the rest and waits to process.
            If ret = 0 Then ret = BufferSize + 1

        End If

        Return ret
    End Function

    Public Function DecompressSinglePacket(ByRef Peek As Boolean) As Byte()
        Dim MyNode As Short = 0
        Dim DecomBytes As New MemoryStream(PacketSize * 2)
        Dim OldPos As UInteger = 0

        For x As Integer = 0 To GameBuffer.Size - 1

            For y As Integer = 7 To 0 Step -1
                MyNode = HuffmanDecompressingTree(MyNode, GameBuffer.Peek(x) >> y And 1)

                Select Case MyNode
                    Case -256
                        MyNode = 0
                        If Not Peek Then GameBuffer.AdvanceTail(x + 1)
                        Return DecomBytes.ToArray

                        'No need to exit for, it will never come back.
                        'Exit For
                    Case Is < 1
                        DecomBytes.WriteByte(CByte((-MyNode)))
                        MyNode = 0
                        Exit Select
                End Select

            Next

        Next

        'Return pure endless emptyness...
        Return Nothing
    End Function

#End If

#End Region

    Private Sub GameBufferTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles GameBufferTimer.Elapsed
        'GameBufferTimer.Interval = PacketReadInterval

        If _GameClient.Connected Then
            HandleGamePacket()
        Else
            GameBufferTimer.Enabled = False
            Debug.WriteLine("Game Client Disconnected")
        End If

    End Sub

    Public Sub New()
        'Localize()
        _AllItems.Add(WorldSerial, _Items)
    End Sub

    Private Sub Localize()
        'Sets the default language of the string list to the language of the OS
        'used for clilocs, item types, etc. That way people with non-english can get the
        'right info.
        'Languages: enu,chs,cht,deu,esp,fra,jpn,kor
        Select Case My.Application.Culture.ThreeLetterISOLanguageName
            Case "chi" 'Chinese
                StrLst = New StringList("chs")
            Case "zho" 'Chinese Traditional
                StrLst = New StringList("cht")

            Case "eng" 'English
                StrLst = New StringList("enu")
            Case "enm" 'English
                StrLst = New StringList("enu")

            Case "fre" 'French
                StrLst = New StringList("fra")
            Case "fra" 'French
                StrLst = New StringList("fra")
            Case "frm" 'French
                StrLst = New StringList("fra")
            Case "fro" 'French
                StrLst = New StringList("fra")

            Case "ger" 'German
                StrLst = New StringList("deu")
            Case "deu" 'German
                StrLst = New StringList("deu")
            Case "gmh" 'German
                StrLst = New StringList("deu")
            Case "goh" 'German
                StrLst = New StringList("deu")

            Case "spa" 'Spanish
                StrLst = New StringList("esp")

            Case "jpn" 'Japanese
                StrLst = New StringList("jpn")

            Case "kor" 'Korean
                StrLst = New StringList("kor")

            Case Else 'Don't know what to set it to? Then English.
                StrLst = New StringList("enu")

        End Select
    End Sub

    Private Class CircularBuffer
        Private Shared Bytes() As Byte
        Private ReadPosition As UInteger = 0
        Private WritePosition As UInteger = 0
        Private _Size As Integer = 0

        Public Sub New(ByRef Size As UInteger)
            Dim b(Size) As Byte
            Bytes = b
        End Sub

        Public Sub AdvanceTail(Optional ByRef NumberOfBytes As Integer = 1)
            If ReadPosition + NumberOfBytes <= WritePosition Then
                ReadPosition += NumberOfBytes
                _Size -= NumberOfBytes
            Else
                Throw New ApplicationException("Tried to advance the tail past the head!")
            End If
        End Sub

#Region "Read"

        Public Function ReadByte() As Byte
            Dim j As Byte = Bytes(ReadPosition)

            SyncLock Me
                ReadPosition += 1
                If ReadPosition = Bytes.Length Then ReadPosition = 0
                _Size -= 1
            End SyncLock

            Return j
        End Function

        Public Function Read(ByRef Number As Integer) As Byte()
            Dim ReturnBytes(Number) As Byte

            SyncLock Me
                For i As Integer = 0 To Number - 1
                    ReturnBytes(i) = Bytes(ReadPosition)
                    ReadPosition += 1
                    If ReadPosition = Bytes.Length Then ReadPosition = 0
                Next
            End SyncLock

            _Size -= Number

            Return ReturnBytes
        End Function

#End Region

#Region "Write"

        Public Sub WriteByte(ByRef ByteToWrite As Byte)
            SyncLock Me
                Bytes(WritePosition) = ByteToWrite
                WritePosition += 1
                If WritePosition = Bytes.Length Then WritePosition = 0

                _Size += 1
            End SyncLock
        End Sub

        Public Sub Write(ByRef BytesToWrite() As Byte)
            SyncLock Me
                For Each b As Byte In BytesToWrite
                    Bytes(WritePosition) = b
                    WritePosition += 1
                    If WritePosition = Bytes.Length Then WritePosition = 0
                Next
                _Size += BytesToWrite.Length
            End SyncLock
        End Sub

#End Region

#Region "Peek"

        Public Function PeekBytes(ByRef Size As UInteger) As Byte()
            Dim retbytes(Size - 1) As Byte
            SyncLock Me
                For i As UInteger = 0 To Size - 1
                    retbytes(i) = Bytes(ReadPosition + i)
                Next
            End SyncLock
            Return retbytes
        End Function

        Public Function Peek(ByRef Offset As UInteger) As Byte
            Dim k As Byte
            SyncLock Me
                k = Bytes(ReadPosition + Offset)
            End SyncLock
            Return k
        End Function

#End Region

#Region "Properties"

        Public ReadOnly Property Size As Integer
            Get
                Return _Size
            End Get
        End Property

        Public Sub Skip(ByRef Amount As UInteger)
            ReadPosition += Amount
        End Sub

        Public ReadOnly Property TailPosition As UInteger
            Get
                Return ReadPosition
            End Get
        End Property

        Public ReadOnly Property HeadPosition As UInteger
            Get
                Return WritePosition
            End Get
        End Property

        Public ReadOnly Property ToArray As Byte()
            Get
                Dim ReturnBytes(Size) As Byte
                Dim CurrPos As UInteger = ReadPosition

                SyncLock Me
                    For i As Integer = 0 To Size - 1
                        ReturnBytes(i) = Bytes(CurrPos)
                        CurrPos += 1
                        If CurrPos = Bytes.Length Then CurrPos = 0
                    Next
                End SyncLock

                Return ReturnBytes
            End Get
        End Property

#End Region

    End Class

#Region "CircularIntBuffer"

#If HuffManPacketSizing Then

    Private Class CircularIntBuffer
        Private Shared ints() As UInteger
        Private ReadPosition As UInteger = 0
        Private WritePosition As UInteger = 0
        Private _Size As Integer = 0

        Public Sub New(ByRef Size As UInteger)
            Dim i(Size) As UInteger
            ints = i
        End Sub

#Region "Read"

        Public Function ReadInt() As UInteger
            Dim j As UInteger = ints(ReadPosition)
            ReadPosition += 1
            If ReadPosition = ints.Length Then ReadPosition = 0
            _Size -= 1
            Return j
        End Function

        Public Function Read(ByRef Number As UInteger) As UInteger()
            Dim ReturnInts(Number) As UInteger

            For i As Integer = 0 To Number - 1
                ReturnInts(i) = ints(ReadPosition)
                ReadPosition += 1
                If ReadPosition = ints.Length Then ReadPosition = 0
            Next

            _Size -= Number

            Return ReturnInts
        End Function

#End Region

#Region "Write"

        Public Sub WriteInt(ByRef IntsToWrite As UInteger)
            ints(WritePosition) = IntsToWrite
            WritePosition += 1
            If WritePosition = ints.Length Then WritePosition = 0

            _Size += 1
        End Sub

        Public Sub Write(ByRef IntsToWrite() As UInteger)
            For Each i As Byte In IntsToWrite
                ints(WritePosition) = i
                WritePosition += 1
                If WritePosition = ints.Length Then WritePosition = 0
            Next

            _Size += ints.Length

        End Sub

#End Region

#Region "Peek"

        Public Function PeekInts(ByRef Size As UInteger) As UInteger()
            Dim retbytes(Size - 1) As UInteger
            For i As UInteger = 0 To Size - 1
                retbytes(i) = ints(ReadPosition + i)
            Next
            Return retbytes
        End Function

        Public Function Peek(ByRef Offset As UInteger) As UInteger
            Return ints(ReadPosition + Offset)
        End Function

#End Region

#Region "Properties"

        Public ReadOnly Property Size As Integer
            Get
                Return _Size
            End Get
        End Property

        Public Sub Skip(ByRef Amount As UInteger)
            ReadPosition += Amount
        End Sub

        Public ReadOnly Property TailPosition As UInteger
            Get
                Return ReadPosition
            End Get
        End Property

        Public ReadOnly Property HeadPosition As UInteger
            Get
                Return WritePosition
            End Get
        End Property

        Public ReadOnly Property ToArray As UInteger()
            Get
                Dim ReturnBytes(Size) As UInteger
                Dim CurrPos As UInteger = ReadPosition

                For i As Integer = 0 To Size - 1
                    ReturnBytes(i) = ints(CurrPos)
                    CurrPos += 1
                    If CurrPos = ints.Length Then CurrPos = 0
                Next

                Return ReturnBytes
            End Get
        End Property

#End Region

    End Class

#End If

#End Region


#End Region

#Region "Actions: walk/talk/etc..."

    Public Sub Speak(ByRef Text As String, ByRef Type As Enums.SpeechTypes)
        Dim packet As New Packets.UnicodeSpeechPacket(Enums.SpeechTypes.Regular, CUShort(52), Enums.Fonts.Default, "ENU", Text)
        Send(packet)
    End Sub

    Public Sub Walk(ByRef Direction As Enums.Direction, Optional ByRef NumberOfSteps As UInteger = 1)

    End Sub

#End Region


End Class
