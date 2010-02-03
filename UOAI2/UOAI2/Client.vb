Imports System.Net.Sockets, System.Net, System.Threading

Partial Class UOAI
    'The Serial for the world, its where you store the mobiles, and items that mobiles are wearing.
    'And items that are on the ground, basicaly anything thats not in some sort of container.
    Friend Shared ReadOnly WorldSerial As New Serial(Convert.ToUInt32(4294967295))

    ''' <summary>Represents an Ultima Online client.</summary>
    Public Class Client

#Region "Properties"
        Private ProcessID As Integer
        Friend InjectedDll As UOClientDll
        Friend PStream As ProcessStream
        Private m_EventSocket As System.Net.Sockets.Socket
        Private m_EventBufferHandler As New BufferHandler(4096)
        Private Shared droppacketmessage As Byte() = New Byte() {2, 0, 0, 0}
        Private Shared continuemessage As Byte() = New Byte() {0, 0, 0, 0}
        Private m_EventHandled As Boolean
        Private m_EventLocked As Boolean
        Private m_CurrentPacket As Packet
        Private m_EventTimer As System.Threading.Timer
        Private m_ShutdownEventTimer As Boolean
        Private _Items As Item
        Friend _AllItems As New Hashtable
        Private _MobileList As New MobileList(Me)
        Private _Macros As New UOMacros(Me)
        Private _CallibrationInfo As CallibrationInfo
        Friend _ItemInHand As Item '<---For drag/drop stuff, to remove the item from the item list and place it here until its dropped.
        Friend _Player As PlayerClass

        ''' <summary>
        ''' Gets the windows process ID of the client. This is used as the unique identifier for each client running.
        ''' </summary>
        ''' <value>The windows Process ID of the client.</value>
        Public Property PID() As Integer
            Get
                Return ProcessID
            End Get
            Friend Set(ByVal value As Integer)
                ProcessID = value
            End Set
        End Property

        ''' <summary>
        ''' A method used to get the title of the client window as a string.
        ''' </summary>
        ''' <returns>The client window title, generally something like "Ultima Online"</returns>
        Public ReadOnly Property WindowCaption() As String
            Get
                Return Process.GetProcessById(PID).MainWindowTitle
            End Get
        End Property

        ''' <summary>
        ''' Returns the client's itemlist.
        ''' </summary>
        Public ReadOnly Property Items() As ItemList
            Get
                Return _Items.Contents
            End Get
        End Property

        ''' <summary>
        ''' Returns a list of the client's mobiles.
        ''' </summary>
        Public ReadOnly Property Mobiles() As MobileList
            Get
                Return _MobileList
            End Get
        End Property

        Public ReadOnly Property Macros() As UOMacros
            Get
                Return _Macros
            End Get
        End Property

        Public ReadOnly Property Player() As PlayerClass
            Get
                Return _Player
            End Get
        End Property

#End Region

#Region "Constructor and InjectedDll Communication"

        Friend Sub New(ByVal PID As Integer)
            Dim PID_COPY As Integer
            Dim TID As Integer

            'Gives _SyncForm a hWnd so that way it can be used as a syncobject.
            '_SyncForm.Show()
            '_SyncForm.Hide()

            'assign process id
            ProcessID = PID

            'setup process stream
            PStream = New ProcessStream(PID)

            'get main window thread of the client process
            TID = [Imports].GetWindowThreadProcessId(Process.GetProcessById(PID).MainWindowHandle, PID_COPY)

            'if there is no window yet, the first thread is probably the best guess
            If TID = 0 Then TID = Process.GetProcessById(PID).Threads(0).Id

            'inject the UOClientDll on this thread
            InjectedDll = New UOClientDll(PStream, TID)

            'get callibration info from the injected dll
            _CallibrationInfo = InjectedDll.GetCallibrations()

            'lock the client's state... this means the itemlists, etc. can not change since the client isn't handling packets
            If InjectedDll.Lock() Then
                InitializeState() 'we get the itemlist and setup our event system
                InjectedDll.Unlock() 'we now unlock, all subsequent packets should be handled and our itemlist is therefore synchronized
            Else
                Throw New Exception("Client Initialization failed: Couldn't lock the client's state!")
            End If
        End Sub

        Protected Overloads Overrides Sub Finalize()
            m_ShutdownEventTimer = True
            MyBase.Finalize()
        End Sub 'Finalize

        Private Sub InitializeState()
            'a. setup event system
            Dim eventport As Integer

            'get the communication port from the client
            eventport = InjectedDll.GetEventPort()

            'setup an event socket and connect to the IPC channel in the client
            m_EventSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            m_EventSocket.Connect(IPAddress.Loopback, eventport)

            'check the connection, we cannot continoue without it
            If m_EventSocket.Connected = False Then
                Throw New Exception("Could not connect to the event server setup within the client's excecutable!" & vbLf)
            End If

            'b. setup item list; mobile list and gumplist
            _Items = New Item(Me, WorldSerial)
            _AllItems.Add(WorldSerial, _Items)

            'c. timer
            m_ShutdownEventTimer = False
            m_EventTimer = New System.Threading.Timer(AddressOf EventTimerProc, Nothing, 0, Timeout.Infinite)

        End Sub

        Private Sub EventTimerProc(ByVal state As Object)
            'Handle Packets (needs to be a seperate function to allow for recursivity!)
            If HandlePackets() And Not m_ShutdownEventTimer Then
                'set the timer again
                m_EventTimer = New System.Threading.Timer(AddressOf EventTimerProc, Nothing, 0, Timeout.Infinite)
            End If
        End Sub

        Private Function BuildPacket(ByRef packetbuffer As Byte(), ByVal origin As Enums.PacketOrigin) As Packet


#Const PacketLogging = False

#If PacketLogging = True Then
            If origin = Enums.PacketOrigin.FROMCLIENT Then
                Console.WriteLine("Sent Packet: " & BitConverter.ToString(packetbuffer))
            Else
                Console.WriteLine("Recieved Packet: " & BitConverter.ToString(packetbuffer))
            End If
#End If


            Select Case DirectCast(packetbuffer(0), Enums.PacketType)

                Case Enums.PacketType.TextUnicode
                    Return New Packets.UnicodeTextPacket(packetbuffer)

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
                    Return New Packets.Target(packetbuffer)

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

                Case Else
                    Dim j As New Packet(packetbuffer(0))
                    j._Data = packetbuffer
                    j._size = packetbuffer.Length
                    Return j 'dummy until we have what we need
            End Select

        End Function

        Public Sub HandlePacket(ByVal Origin As Enums.PacketOrigin)
            Dim backup As Packet

            'magically stops this from taking up 100% cpu on w/e core its on.
            Thread.Sleep(0)

            'force client to handle the current packet
            If ((m_EventHandled = False) And (m_EventLocked = False)) Then
                'back up current packet as it might get replaced by another while we are handling packets
                backup = m_CurrentPacket
                'handle this packet
                Try
                    m_EventSocket.Send(continuemessage)
                Catch ex As Exception
                    Return
                End Try
                'while it's handling we might need to handle other packets
                Do While HandlePackets()
                    Thread.Sleep(0)
                Loop

                'should return when packet is handled
                m_CurrentPacket = backup
                m_EventHandled = True
                m_EventLocked = True
                'Late packet handling...
                LatePacketHandling(m_CurrentPacket, Origin)
                'release client
                Try
                    m_EventSocket.Send(continuemessage)
                Catch ex As Exception
                    Return
                End Try
            End If
        End Sub

        Private Sub EarlyPacketHandling(ByRef currentpacket As Packet, ByVal Origin As Enums.PacketOrigin)
            'whatever we need to do with the current packet BEFORE the client handled it goes here
            Select Case currentpacket.Type
                Case Enums.PacketType.SpeechUnicode
                    Dim k As Packets.UnicodeSpeechPacket = DirectCast(currentpacket, Packets.UnicodeSpeechPacket)
                    RaiseEvent onClientSpeech(Me, k.Text, k.Font, k.Hue, k.Language, k.SpeechType)

                Case Enums.PacketType.GenericCommand

                    Select Case DirectCast(CUShort(currentpacket.Data(4)), Enums.BF_Sub_Commands)
                        Case Enums.BF_Sub_Commands.ContextMenuRequest
                            If Me.Mobiles.Exists(DirectCast(currentpacket, Packets.ContextMenuRequest).Serial) Then
                                Me.Mobiles.Mobile(DirectCast(currentpacket, Packets.ContextMenuRequest).Serial).HandleContextMenuRequest(DirectCast(currentpacket, Packets.ContextMenuRequest))
                            Else
                                Me.Items.Item(DirectCast(currentpacket, Packets.ContextMenuRequest).Serial).HandleContextMenuRequest(DirectCast(currentpacket, Packets.ContextMenuRequest))
                            End If

                        Case Enums.BF_Sub_Commands.ContextMenuResponse
                            If Me.Mobiles.Exists(DirectCast(currentpacket, Packets.ContextMenuResponse).Serial) Then
                                Me.Mobiles.Mobile(DirectCast(currentpacket, Packets.ContextMenuResponse).Serial).HandleContextMenuResponse(DirectCast(currentpacket, Packets.ContextMenuResponse))
                            Else
                                Me.Items.Item(DirectCast(currentpacket, Packets.ContextMenuResponse).Serial).HandleContextMenuResponse(DirectCast(currentpacket, Packets.ContextMenuResponse))
                            End If

                        Case Else
                            Exit Select

                    End Select

                Case Else
                    Exit Select
            End Select
        End Sub

        Private Sub LatePacketHandling(ByRef currentpacket As Packet, ByVal Origin As Enums.PacketOrigin)
            Select Case currentpacket.Type
                Case Enums.PacketType.MobileStats
                    'We already know now that the mobile exists, because this packet isnt sent until after the MOB is created
                    'So there is no need to check for the existance of the MOB. Just send the packet to the mobile for it to update itself.
                    'This is done through direct casts and hash tables, so its REALLY fast.
                    Mobiles.Mobile(DirectCast(currentpacket, Packets.MobileStats).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.MobileStats))

                Case Enums.PacketType.HPHealth
                    Mobiles.Mobile(DirectCast(currentpacket, Packets.HPHealth).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.HPHealth))

                Case Enums.PacketType.FatHealth
                    Mobiles.Mobile(DirectCast(currentpacket, Packets.FatHealth).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.FatHealth))

                Case Enums.PacketType.ManaHealth
                    Mobiles.Mobile(DirectCast(currentpacket, Packets.ManaHealth).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.ManaHealth))

                Case Enums.PacketType.NakedMOB
                    Mobiles.AddMobile(DirectCast(currentpacket, Packets.NakedMobile))

                Case Enums.PacketType.EquippedMOB
                    Mobiles.AddMobile(DirectCast(currentpacket, Packets.EquippedMobile))

                Case Enums.PacketType.DeathAnimation
                    Mobiles.Mobile(DirectCast(currentpacket, Packets.DeathAnimation).Serial).HandleDeathPacket(DirectCast(currentpacket, Packets.DeathAnimation))

                Case Enums.PacketType.Destroy
                    RemoveObject(DirectCast(currentpacket, Packets.Destroy).Serial)

                Case Enums.PacketType.EquipItem
                    Mobiles.Mobile(DirectCast(currentpacket, Packets.EquipItem).Container).HandleUpdatePacket(DirectCast(currentpacket, Packets.EquipItem))

                Case Enums.PacketType.ContainerContents
                    Items.Add(DirectCast(currentpacket, Packets.ContainerContents))

                Case Enums.PacketType.ObjecttoObject
                    Items.Add(DirectCast(currentpacket, Packets.ObjectToObject))

                Case Enums.PacketType.ShowItem
                    Items.Add(DirectCast(currentpacket, Packets.ShowItem))

                Case Enums.PacketType.Target
                    If _WaitingForTarget = True Then
                        Dim jimbo As New TargetInfo(Me, DirectCast(currentpacket, Packets.Target))
                        RaiseEvent onTargetResponse(jimbo)
                    End If

                Case Enums.PacketType.HuePicker
                    If _WaitingForHue Then
                        RaiseEvent onHueResponse(DirectCast(currentpacket, Packets.HuePicker).Serial.Value, DirectCast(currentpacket, Packets.HuePicker).Hue)
                    End If

                Case Enums.PacketType.LoginConfirm
                    'Make a new playerclass
                    Dim pl As New PlayerClass(Me, DirectCast(currentpacket, Packets.LoginConfirm).Serial)

                    'Apply the packet's info to the new playerclass
                    pl._Type = DirectCast(currentpacket, Packets.LoginConfirm).BodyType
                    pl._X = DirectCast(currentpacket, Packets.LoginConfirm).X
                    pl._Y = DirectCast(currentpacket, Packets.LoginConfirm).Y
                    pl._Z = DirectCast(currentpacket, Packets.LoginConfirm).Z
                    pl._Direction = DirectCast(currentpacket, Packets.LoginConfirm).Direction

                    'Assign it to player
                    _Player = pl

                    'Cast the player as a mobile and add it to the mobile list.
                    Mobiles.AddMobile(DirectCast(_Player, Mobile))

                Case Enums.PacketType.HealthBarStatusUpdate
                    Mobiles.Mobile(DirectCast(currentpacket, Packets.HealthBarStatusUpdate).Serial).HandleUpdatePacket(DirectCast(currentpacket, Packets.HealthBarStatusUpdate))

            End Select
        End Sub

        'handles all IPC packets, including sent/received packets on the client
        Private Function HandlePackets() As Boolean
            Dim eventtype As Enums.EventTypeConstants
            Dim eventdatasize As Integer
            Dim received As Integer
            Dim vkeycode As UInteger

            m_EventLocked = True

            Try
                received = m_EventSocket.Receive(m_EventBufferHandler.buffer)
            Catch ex As Exception
                Return False
            End Try

            m_EventHandled = False 'new packet, so not handled yet
            m_EventBufferHandler.Position = 0 'we start at the beginning of the new packet
            eventtype = DirectCast(m_EventBufferHandler.readuint(), Enums.EventTypeConstants) 'we get the type of event
            eventdatasize = m_EventBufferHandler.readint() 'and the size of the data

            Select Case eventtype
                Case Enums.EventTypeConstants.object_destroyed
                    'remove the destroyed object by its Offset
                    RemoveObject(m_EventBufferHandler.readuint())
                    'continue
                    Try
                        m_EventSocket.Send(continuemessage)
                    Catch ex As Exception
                        Return False
                    End Try
                    Return True
                Case Enums.EventTypeConstants.packet_handled
                    Return False 'exit nested packet handler, by returning false the nested loop will get broken
                Case Enums.EventTypeConstants.connection_loss
                    'connection loss event
                    RaiseEvent onConnectionLoss(Me)
                    'continue
                    Try
                        m_EventSocket.Send(continuemessage)
                    Catch ex As Exception
                        Return False
                    End Try
                    Return True
                Case Enums.EventTypeConstants.key_down
                    'get virtual key code
                    vkeycode = m_EventBufferHandler.readuint()
                    'raise key down event here with the specified virtual key code (vkeycode)
                    RaiseEvent onKeyDown(Me, vkeycode)
                    'continue
                    Try
                        m_EventSocket.Send(continuemessage)
                    Catch ex As Exception
                        Return False
                    End Try
                    Return True
                Case Enums.EventTypeConstants.key_up
                    'get virtual key code
                    vkeycode = m_EventBufferHandler.readuint()
                    ' raise key up event with the specified key code here
                    RaiseEvent onKeyUp(Me, vkeycode)
                    'continue
                    Try
                        m_EventSocket.Send(continuemessage)
                    Catch ex As Exception
                        Return False
                    End Try
                    Return True
                Case Enums.EventTypeConstants.received_packet
                    'build the packet
                    m_CurrentPacket = BuildPacket(m_EventBufferHandler.readbytes(eventdatasize), Enums.PacketOrigin.FROMSERVER)

                    'unlock event, user might want it to be handled
                    m_EventLocked = False

                    'internal handling before the client handled this packet
                    EarlyPacketHandling(m_CurrentPacket, Enums.PacketOrigin.FROMSERVER)

                    'packet event
                    RaiseEvent onPacketReceive(Me, m_CurrentPacket)

                    'make sure it is handled
                    HandlePacket(Enums.PacketOrigin.FROMSERVER)

                    Return True
                Case Enums.EventTypeConstants.sent_packet
                    'build this packet
                    m_CurrentPacket = BuildPacket(m_EventBufferHandler.readbytes(eventdatasize), Enums.PacketOrigin.FROMCLIENT)

                    'unlock event, user might want it to be handled
                    m_EventLocked = False

                    'internal handling before the client handled this packet
                    EarlyPacketHandling(m_CurrentPacket, Enums.PacketOrigin.FROMCLIENT)

                    'packet event
                    RaiseEvent onPacketSend(Me, m_CurrentPacket)

                    'make sure it is handled
                    HandlePacket(Enums.PacketOrigin.FROMCLIENT)

                    Return True
                Case Else
                    Return False
            End Select

        End Function

        ''' <summary>Remove an item, mobile, etc... from the collections</summary>
        ''' <param name="address">The 32-bit address in the client's memory of the object as <see cref="UInteger"/></param>
        Private Sub RemoveObject(ByVal address As UInteger)
            'remove item, mobile, ... from the collections

        End Sub

        Private Sub RemoveObject(ByVal Serial As Serial)
            'RemoveObject(Me.Items.Item(Serial).MemoryOffset)
            If Me.Items.Exists(Serial) Then
                Me.Items.RemoveItem(Serial)
            Else
                Mobiles.RemoveMobile(Serial)
            End If
        End Sub

#End Region

#Region "Events"
        ''' <summary>
        ''' Called when the client process closes.
        ''' </summary>
        ''' <param name="Client">The client that exited, for multi-clienting event handlers in VB.NET</param>
        Public Event onClientExit(ByRef Client As Client)

        ''' <summary>
        ''' Called when the client recieves a login confirm packets from the game server, and the player character is created.
        ''' </summary>
        ''' <remarks></remarks>
        Public Event onLogin()

        ''' <summary>
        ''' Called when a Packet arrives on this client.
        ''' </summary>
        ''' <param name="Client">Client on which the packet was received</param>
        ''' <param name="packet">The received packet</param>
        Public Event onPacketReceive(ByRef Client As Client, ByRef packet As Packet)

        ''' <summary>
        ''' Called when a Packet is sent formt he client to the server.
        ''' </summary>
        ''' <param name="Client">Client from which the packet was sent.</param>
        ''' <param name="packet">The sent packet.</param>
        Public Event onPacketSend(ByRef Client As Client, ByRef packet As Packet)

        ''' <summary>
        ''' Called when the user of the client releases a pressed key.
        ''' </summary>
        ''' <param name="Client">The client on which the key was released</param>
        ''' <param name="KeyCode">Virtual Key Code of the released key (a list of key codes can be founrd at http://msdn.microsoft.com/en-us/library/ms927178.aspx) </param>
        Public Event onKeyUp(ByRef Client As Client, ByVal KeyCode As System.Windows.Forms.Keys)

        ''' <summary>
        ''' Called when the user of the client holds down a key.
        ''' </summary>
        ''' <param name="Client">The client on which the key was pressed</param>
        ''' <param name="KeyCode">Virtual Key Code of the pressed key (a list of key codes can be founrd at http://msdn.microsoft.com/en-us/library/ms927178.aspx) </param>
        Public Event onKeyDown(ByRef Client As Client, ByVal KeyCode As System.Windows.Forms.Keys)

        ''' <summary>
        ''' Called when the client loses its network connection to the server.
        ''' </summary>
        ''' <param name="Client">The client that lost its connection.</param>
        Public Event onConnectionLoss(ByRef Client As Client)

        ''' <summary>
        ''' Called when the client sends a speech packet to the server.
        ''' </summary>
        ''' <param name="client">The client sending the packet.</param>
        ''' <param name="Text">The text in the packet.</param>
        ''' <param name="Font">The font as <see cref="UOAI.Enums.Fonts"/></param>
        ''' <param name="Hue">The hue of the text.</param>
        ''' <param name="Language">The language code that applies to the text.</param>
        ''' <param name="SpeechType">The speech type as <see cref="UOAI.Enums.SpeechTypes"/></param>
        Public Event onClientSpeech(ByVal client As Client, ByVal Text As String, ByVal Font As Enums.Fonts, ByVal Hue As UShort, ByVal Language As String, ByVal SpeechType As Enums.SpeechTypes)

#End Region

#Region "Public Functions and Subs"
        ''' <summary>
        ''' Displays a message in game, on the bottom left corner of the screen.
        ''' </summary>
        ''' <param name="Text">The text you want to display.</param>
        Public Sub SysMsg(ByVal Text As String)
            SysMsg(Text, Enums.Fonts.Default, CUShort(52))
        End Sub

        Public Sub SysMsg(ByVal Text As String, ByVal Font As Enums.Fonts)
            SysMsg(Text, Font, CUShort(52))
        End Sub

        Public Sub SysMsg(ByVal Text As String, ByVal Hue As UShort)
            SysMsg(Text, Enums.Fonts.Default, Hue)
        End Sub

        Public Sub SysMsg(ByVal Text As String, ByVal Font As Enums.Fonts, ByVal Hue As UShort)
            Dim k As New Packets.Text(Text)
            k.Name = "System"
            k.Serial = New Serial(CUInt(4294967295))
            k.BodyType = CUShort(&HFFFF)
            k.SpeechType = Enums.SpeechTypes.System
            k.TextHue = Hue
            k.TextFont = Font

#Const DebugSysMsg = False
#If DebugSysMsg Then
            Console.WriteLine("Sending SysMsg Packet to Client: " & BitConverter.ToString(k.Data))
#End If

            Send(k, Enums.PacketDestination.CLIENT)
        End Sub

        ''' <summary>
        ''' Pings the specified address and returns the latency as a UShort.
        ''' </summary>
        ''' <param name="ServerAddress">The address of the server to ping.</param>
        Public Function Latency(ByVal ServerAddress As String) As UShort
            Dim pinger As New System.Net.NetworkInformation.Ping
            Dim reply As System.Net.NetworkInformation.PingReply = pinger.Send(ServerAddress)
            Return reply.RoundtripTime
        End Function

        Public Sub Send(ByVal tosend As Packet, ByVal destination As Enums.PacketDestination)
            Dim networkobject As UInteger
            Dim remotebuffer As UInteger

            'read the network address of the client's c++ object that handles network traffic
            networkobject = PStream.ReadUInt(_CallibrationInfo.pSockObject)

            'allocate a buffer to hold the packet
            remotebuffer = InjectedDll.allocate(tosend.Size)

            'write the packet
            PStream.Write(remotebuffer, tosend.Data)

            'send the packet
            Select Case destination
                Case Enums.PacketDestination.CLIENT
                    InjectedDll.stdthiscall(_CallibrationInfo.pOriginalHandlePacket, networkobject, New UInteger() {remotebuffer})
                    Exit Select
                Case Enums.PacketDestination.SERVER
                    InjectedDll.skipuoai_stdthiscall(_CallibrationInfo.pLoginCrypt, networkobject, New UInteger() {remotebuffer})
                    Exit Select
            End Select

            'clean up packet buffer on the client
            InjectedDll.free(remotebuffer)
        End Sub

        ''' <summary>Disables the client encryption.</summary>
        ''' <remarks>For free servers, you should patch encryption. Encryption is required on OSI servers.</remarks>
        Public Sub PatchEncryption()
            InjectedDll.PatchEncryption()
        End Sub

        ''' <summary>Used from within the onRecievePacket/onPacketSend events to commit the changes to the packet and pass it on.</summary>
        ''' <param name="newpacket">The packet object as a <see cref="UOAI.Packet"/></param>
        Public Sub UpdatePacket(ByRef newpacket As Packet)
            'force an update of the packet
            Dim updatemessage As BufferHandler
            Dim pBuffer() As Byte

            If ((Not m_EventHandled) And (Not m_EventLocked)) Then
                If (newpacket.Size > m_CurrentPacket.Size) Then
                    Throw New Exception("Update a packet requires the size of the new packet to be smaller or equal to the old packet!")
                Else
                    pBuffer = newpacket.Data 'we do this once, to make sure the dynamic Data properties don't have to do serialization more than once
                    updatemessage = New BufferHandler(8 + pBuffer.Length)
                    updatemessage.writeuint(1)
                    updatemessage.writeint(pBuffer.Length)
                    updatemessage.Write(pBuffer, 0, pBuffer.Length)
                    m_EventSocket.Send(updatemessage.buffer)
                    m_CurrentPacket = newpacket
                End If
            End If
        End Sub

        ''' <summary>Used from within the onRecievePacket/onPacketSend events to tell UOAI to delete the packet so the client never gets it.</summary>
        Public Sub DropPacket()
            'current packet is never handled by the client
            If ((Not m_EventHandled) And (Not m_EventLocked)) Then
                m_EventSocket.Send(droppacketmessage)
                m_EventHandled = True
                m_EventLocked = True
            End If
        End Sub

        ''' <summary>
        ''' Kills the client process, which subsequently calls the "onClientExit" event for this client instance.
        ''' </summary>
        Public Sub Close()
            Process.GetProcessById(PID).Kill()
        End Sub

#End Region

#Region "Window Control"
        ''' <summary>Causes the UOClient object to raise the onClientExit event.</summary>
        Friend Sub CallEvent_onClientExit()
            Dim eargs As New EventArgs
            RaiseEvent onClientExit(Me)
        End Sub
#End Region

    End Class

End Class