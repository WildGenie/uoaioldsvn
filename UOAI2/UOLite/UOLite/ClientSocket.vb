Imports System.IO, System.Net, System.Threading, System.ComponentModel.Design, System.Net.Sockets, System.Net.NetworkInformation, System.Diagnostics

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
