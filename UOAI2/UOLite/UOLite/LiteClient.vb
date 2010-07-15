Imports System.Net, System.Net.Sockets, System.Text, System.IO, System.Net.NetworkInformation

Public Class LiteClient
    Private WithEvents _LoginClient As TcpClient
    Private WithEvents _LoginStream As NetworkStream
    Private _Username As String
    Private _Password As String
    Private _EmulatedVersion() As UInteger = {6, 0, 13, 0}
    Private _LoginServerAddress As IPAddress
    Private LoginPort As UShort = 2593
    Private _LoginStreamReader As NetworkStream
    Private _GameServerList(0) As GameServerInfo

    Public ReadOnly Property ServerList As GameServerInfo()
        Get
            Return _GameServerList
        End Get
    End Property

    ''' Hide this class from the user, there is no reason from him/her to see it.
    ''' <summary>Simply a class to hold information about game servers when recieved from the login server.</summary>
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class GameServerInfo
        ''' <summary>The IP address of the server.</summary>
        Public Address As System.Net.IPAddress

        ''' <summary>The name of the server, as provided by the login server.</summary>
        Public Name As String

        ''' <summary>The latency from the client to the server and back in milliseconds (ms). This is retrieved by sending a ping to the server when this property is called.</summary>
        Public ReadOnly Property Latency As Integer
            Get
                Dim pinger As New Ping
                Dim reply As PingReply
                reply = pinger.Send(Address)
                Return reply.RoundtripTime
            End Get
        End Property
    End Class

    ''' <summary>Connects to the specified login server and populates the ServerList property.</summary>
    ''' <param name="Address">The address of the login server to connect to.</param>
    ''' <param name="Port">The port to connect to (default is 2593).</param>
    ''' <param name="Username">The username to connect with.</param>
    ''' <param name="Password">The cooresponding password for the supplied username.</param>
    Public Overloads Function ConnectToLoginServer(ByVal Username As String, ByVal Password As String, ByVal Address As String, Optional ByVal Port As UShort = 2593) As Integer
        Try
            _LoginClient = New TcpClient(Address, Port)
        Catch ex As Exception
            MsgBox("Unable to connect to the login server: " & ex.Message)
            Return 0
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
        InsertBytes(BitConverter.GetBytes(_EmulatedVersion(0)), LoginPacket, 0, 4, 4)
        InsertBytes(BitConverter.GetBytes(_EmulatedVersion(1)), LoginPacket, 0, 8, 4)
        InsertBytes(BitConverter.GetBytes(_EmulatedVersion(2)), LoginPacket, 0, 12, 4)
        InsertBytes(BitConverter.GetBytes(_EmulatedVersion(3)), LoginPacket, 0, 16, 4)

        'Necessary...
        LoginPacket(20) = 128

        'Add the username.
        InsertBytes(GetBytesFromString(30, Username), LoginPacket, 0, 21, 30)

        'Add the password.
        InsertBytes(GetBytesFromString(30, Password), LoginPacket, 0, 51, 30)

        'Add umm 93?
        LoginPacket(81) = 93

        _LoginStream.Write(LoginPacket, 0, 81)

        '_LoginStream.BeginRead()

        Return 1

    End Function


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

    Private Class UOPacket
        Private _Data() As Byte

        Default ReadOnly Property Data(ByVal index As Integer) As Byte
            Get
                Return _Data(index)
            End Get
        End Property

        Public ReadOnly Property Glob() As Byte()
            Get
                Return _Data
            End Get
        End Property

        Public ReadOnly Property Length() As Integer
            Get
                Return _Data.Length
            End Get
        End Property

        Public Sub New(ByRef SourceGlob() As Byte, ByVal Size As Integer, ByVal GlobPosition As Integer)
            Dim ByteArray(Size - 1) As Byte

            For i As Integer = GlobPosition To GlobPosition + Size - 1
                ByteArray(i - GlobPosition) = SourceGlob(i)
            Next

            _Data = ByteArray

        End Sub

    End Class

End Class
