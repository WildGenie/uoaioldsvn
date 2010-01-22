Imports System.Net.Sockets, System.Net, System.Threading

Partial Class UOAI

    ''' <summary>Represents an Ultima Online client.</summary>
    Public Class Client
        Private ProcessID As Integer
        Friend InjectedDll As UOClientDll
        Friend PStream As ProcessStream
        Private WithEvents _EventSocket As System.Net.Sockets.Socket
        Private _Items As ItemList
        Private _Mobiles As ItemList
        Private EventThread As Thread
        Private m_EventBufferHandler As New BufferHandler(4096)
        Private Shared droppacketmessage As Byte() = New Byte() {2, 0, 0, 0}
        Private Shared continuemessage As Byte() = New Byte() {0, 0, 0, 0}


        ''' <summary>
        ''' Called when the client process closes.
        ''' </summary>
        ''' <param name="Client">The client that exited, for multi-clienting event handlers in VB.NET</param>
        Public Event onClientExit(ByVal Client As Client)

        Friend Sub New(ByVal PID As Integer)
            Dim PID_COPY As Integer
            Dim TID As Integer

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

            InitializeState()
        End Sub

        Private Sub InitializeState()
            'a. setup event system
            Dim eventport As Integer

            eventport = InjectedDll.GetEventPort()
            _EventSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            _EventSocket.Connect(IPAddress.Loopback, eventport)
            If _EventSocket.Connected = False Then
                Throw New Exception("Could not connect to the event server setup within the client's excecutable!" & vbLf)
            End If

            'b. setup item list; mobile list and gumplist
            _Items = New ItemList
            _Mobiles = New ItemList
            'Client.InitializeUOMLItemCollection(m_Items, Me, ReadUInt(m_Callibrations.pItemList), m_Callibrations.oItemNext, 0)

            'c. other state inits
            '- playermobile

            'd. Setup event invokation timer
            'm_InvokeEvents = True
            'm_EventTimer = New Timer(New TimerCallback(InvokeEvents), Nothing, 0, -1)

            'Make event thread = a new thread out of EventHandler
            EventThread = New Thread(AddressOf EventHandler)
            'Start the thread
            EventThread.Start()
        End Sub

        Private Sub EventHandler()
            Do

                m_EventHandled = False
                m_EventBufferHandler.Position = 0
                eventtype = DirectCast(m_EventBufferHandler.readuint(), Enums.EventTypeConstants)
                eventdatasize = m_EventBufferHandler.readint()

                Select Case eventtype
                    Case Enums.EventTypeConstants.object_destroyed
                        DeleteObject(m_EventBufferHandler.readuint())
                        m_EventSocket.Send(Client.continuemessage)
                        Exit Select
                    Case Enums.EventTypeConstants.packet_handled
                        Return
                        'exit nested packet handler
                    Case Enums.EventTypeConstants.connection_loss
                        m_EventSocket.Send(Client.continuemessage)
                        Exit Select
                    Case Enums.EventTypeConstants.key_down
                        Dim newkdevent As KeyDownEvent
                        vkeycode = m_EventBufferHandler.readuint()
                        newkdevent = New KeyDownEvent(vkeycode)
                        RaiseEvent onKeyDown(vkeycode)
                        For Each curevent As EventDesc In OtherEventQueues(EventTypes.KeyDown)
                            If curevent.parameters.Length = 0 Then
                                DirectCast(curevent.OnQueue, UOMLEventQueue).Enqueue(DirectCast(newkdevent, [Event]))
                            ElseIf CUInt(curevent.parameters(0)) = vkeycode Then
                                DirectCast(curevent.OnQueue, UOMLEventQueue).Enqueue(DirectCast(newkdevent, [Event]))
                            End If
                        Next
                        m_EventSocket.Send(Client.continuemessage)
                        Exit Select
                    Case Enums.EventTypeConstants.key_up
                        Dim newkuevent As KeyUpEvent
                        vkeycode = m_EventBufferHandler.readuint()
                        newkuevent = New KeyUpEvent(vkeycode)
                        RaiseEvent onKeyUp(vkeycode)
                        For Each curevent As EventDesc In OtherEventQueues(EventTypes.KeyDown)
                            If curevent.parameters.Length = 0 Then
                                DirectCast(curevent.OnQueue, UOMLEventQueue).Enqueue(DirectCast(newkuevent, [Event]))
                            ElseIf CUInt(curevent.parameters(0)) = vkeycode Then
                                DirectCast(curevent.OnQueue, UOMLEventQueue).Enqueue(DirectCast(newkuevent, [Event]))
                            End If
                        Next
                        m_EventSocket.Send(Client.continuemessage)
                        Exit Select
                    Case Enums.EventTypeConstants.received_packet
                        m_CurrentPacket = BuildPacket(m_EventBufferHandler.readbytes(eventdatasize), PacketOrigin.FROMSERVER)
                        'invoke early events here
                        If m_EarlyPacketHandlers(m_CurrentPacket.CMD) IsNot Nothing Then
                            m_EarlyPacketHandlers(m_CurrentPacket.CMD)(m_CurrentPacket)
                        End If
                        'unlock event, user might want it to be handled
                        m_EventLocked = False
                        'invoke user events
                        '- async packet events (eventqueues)
                        DefaultPacketEventTrigger(m_CurrentPacket)
                        '- sync packet events
                        RaiseEvent onPacketReceive(m_CurrentPacket)
                        'make sure it is handled
                        [Continue]()
                        'which will trigger all other async/sync events
                        Exit Select
                    Case Enums.EventTypeConstants.sent_packet
                        m_CurrentPacket = BuildPacket(m_EventBufferHandler.readbytes(eventdatasize), PacketOrigin.FROMCLIENT)
                        'invoke early events here
                        If m_EarlyPacketHandlers(m_CurrentPacket.CMD) IsNot Nothing Then
                            m_EarlyPacketHandlers(m_CurrentPacket.CMD)(m_CurrentPacket)
                        End If
                        'unlock event, suer might want it to be handled
                        m_EventLocked = False
                        'invoke user events
                        '- async packet events
                        DefaultPacketEventTrigger(m_CurrentPacket)
                        '- sync packet events
                        RaiseEvent onPacketSend(m_CurrentPacket)
                        'make sure it is handled
                        [Continue]()
                        Exit Select
                    Case Else
                        Exit Select
                End Select

                Thread.Sleep(0)
            Loop
        End Sub

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

        ''' <summary>Causes the UOClient object to raise the onClientExit event.</summary>
        Friend Sub CallEvent_onClientExit()
            Dim eargs As New EventArgs
            RaiseEvent onClientExit(Me)
        End Sub

        ''' <summary>
        ''' Kills the client process, which subsequently calls the "onClientExit" event for this client instance.
        ''' </summary>
        Public Sub Close()
            Process.GetProcessById(PID).Kill()
        End Sub

        ''' <summary>
        ''' <para>A method used to get the title of the client window as a string.</para>
        ''' </summary>
        ''' <returns>The client window title, generally something like "Ultima Online"</returns>
        Public ReadOnly Property WindowCaption() As String
            Get
                Return Process.GetProcessById(PID).MainWindowTitle
            End Get
        End Property

    End Class



End Class