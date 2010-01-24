Imports System.Net.Sockets, System.Net, System.Threading

Partial Class UOAI

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
        Private _Items As ItemList
        Friend _AllItems As New Hashtable
        Private _MobileList As MobileList
        Private _Macros As New UOMacros(Me)
        Private _CallibrationInfo As CallibrationInfo

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
                Return _Items
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
            _Items = New ItemList(New Serial(Convert.ToUInt32(4294967295)))

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

            Select Case DirectCast(packetbuffer(0), Enums.PacketType)
                Case Enums.PacketType.TextUnicode
                    Return New Packets.UnicodeTextPacket(packetbuffer)
                Case Enums.PacketType.SpeechUnicode
                    Return New Packets.UnicodeSpeechPacket(packetbuffer)

                Case Else
                    Dim j As New Packet(packetbuffer(0))
                    j._Data = packetbuffer
                    j._size = packetbuffer.Length
                    Return j 'dummy until we have what we need
            End Select

        End Function

        Public Sub HandlePacket()
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
                LatePacketHandling(m_CurrentPacket)
                'release client
                Try
                    m_EventSocket.Send(continuemessage)
                Catch ex As Exception
                    Return
                End Try
            End If
        End Sub

        Private Sub EarlyPacketHandling(ByRef currentpacket As Packet)
            'whatever we need to do with the current packet BEFORE the client handled it goes here
        End Sub

        Private Sub LatePacketHandling(ByRef currentpacket As Packet)
            'whatever we need to do with the current packet AFTER the client handled it goes here
        End Sub

        ''' <summary>Remove an item, mobile, etc... from the collections</summary>
        ''' <param name="address">The 32-bit address in the client's memory of the object as <see cref="UInteger"/></param>
        Private Sub RemoveObject(ByVal address As UInteger)
            'TODO: I recommend using "serial" for this, since thats the key i use in the collections.
            'Although i could just add a function to remove by address.

            'remove item, mobile, ... from the collections
        End Sub

        Private Sub CreateNewItem(ByVal address As UInteger)

        End Sub

#End Region

#Region "Events"
        ''' <summary>
        ''' Called when the client process closes.
        ''' </summary>
        ''' <param name="Client">The client that exited, for multi-clienting event handlers in VB.NET</param>
        Public Event onClientExit(ByRef Client As Client)

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
        ''' ''' <param name="VirtualKeyCode">Virtual Key Code of the released key (a list of key codes can be founrd at http://msdn.microsoft.com/en-us/library/ms927178.aspx) </param>
        Public Event onKeyUp(ByRef Client As Client, ByVal VirtualKeyCode As UInteger)

        ''' <summary>
        ''' Called when the user of the client holds down a key.
        ''' </summary>
        ''' <param name="Client">The client on which the key was pressed</param>
        ''' ''' <param name="VirtualKeyCode">Virtual Key Code of the pressed key (a list of key codes can be founrd at http://msdn.microsoft.com/en-us/library/ms927178.aspx) </param>
        Public Event onKeyDown(ByRef Client As Client, ByVal VirtualKeyCode As UInteger)

        ''' <summary>
        ''' Called when the client loses its network connection to the server.
        ''' </summary>
        ''' <param name="Client">The client that lost its connection.</param>
        Public Event onConnectionLoss(ByRef Client As Client)
#End Region

#Region "Public Functions and Subs"
        ''' <summary>Disables the client encryption.</summary>
        ''' <remarks>For free servers, you should patch encryption. Encryption is required on OSI servers.</remarks>
        Public Sub PatchEncryption()
            InjectedDll.PatchEncryption()
        End Sub

        ''' <summary>Used from within the onRecievePacket/onPacketSend events to commit the changes to the packet and pass it on.</summary>
        ''' <param name="newpacket">The packet object as a <see cref="UOAI.Packet"/></param>
        Public Sub UpdatePacket(ByRef newpacket As Packet)
            'force an update of the packet
        End Sub

        ''' <summary>Used from within the onRecievePacket/onPacketSend events to tell UOAI to delete the packet so the client never gets it.</summary>
        Public Sub DropPacket()
            'current packet is never handled by the client
        End Sub

        ''' <summary>
        ''' Kills the client process, which subsequently calls the "onClientExit" event for this client instance.
        ''' </summary>
        Public Sub Close()
            Process.GetProcessById(PID).Kill()
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
                    'remove the destroyed object
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
                    Exit Select
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

                    'internal handling before the client handled this packet
                    EarlyPacketHandling(m_CurrentPacket)

                    'unlock event, user might want it to be handled
                    m_EventLocked = False

                    'packet event
                    RaiseEvent onPacketReceive(Me, m_CurrentPacket)

                    'make sure it is handled
                    HandlePacket()

                    Return True
                Case Enums.EventTypeConstants.sent_packet
                    'build this packet
                    m_CurrentPacket = BuildPacket(m_EventBufferHandler.readbytes(eventdatasize), Enums.PacketOrigin.FROMCLIENT)

                    'internal handling before the client handled this packet
                    EarlyPacketHandling(m_CurrentPacket)

                    'unlock event, user might want it to be handled
                    m_EventLocked = False

                    'packet event
                    RaiseEvent onPacketSend(Me, m_CurrentPacket)

                    'make sure it is handled
                    HandlePacket()

                    Return True
                Case Else
                    Return False
            End Select

        End Function
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