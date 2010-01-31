Imports System.Threading
#Const DebugTargeting = False

Partial Class UOAI

    Partial Class Client

        Private _WaitingForTarget As Boolean = False

        Public Sub TargetPrompt(ByVal UID As UInt32, ByVal Type As Enums.TargetRequestType)
            Dim TarPack As New Packets.Target

            TarPack.Serial = New Serial(UID)

            TarPack.TargetType = Type
            _WaitingForTarget = True

#If DebugTargeting Then
            Console.WriteLine("Sent Packet Data: " & BitConverter.ToString(TarPack.Data))
#End If

            Send(DirectCast(TarPack, Packet), Enums.PacketDestination.CLIENT)
        End Sub


        ''' <summary>
        ''' Called whenever the client chooses a target.
        ''' </summary>
        ''' <param name="TargetInfo">The TargetInfo object containting the information on the target response.</param>
        Public Event onTargetResponse(ByVal TargetInfo As TargetInfo)

    End Class

    Partial Class Enums
        Public Enum TargetType As Byte
            Canceled
            Ground
            Item
            Mobile
        End Enum

        Public Enum TargetRequestType As Byte
            ItemOrMobile
            GroundOrStaticTile
        End Enum

    End Class

#If DEBUG Then
    Public Class TargetInfo
#Else
    'Hide this class from the user, there is no reason from him/her to see it.
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class TargetInfo
#End If

        Friend _Target As New Serial(CUInt(0))
        Friend _Type As Enums.TargetType = Enums.TargetType.Canceled
        Friend _X As UShort = 0
        Friend _Y As UShort = 0
        Friend _Z As UShort = 0
        Friend _UID As UInt32 = 0

#If DebugTargeting Then

        Friend _Packet As Packets.Target

#End If

        Protected Friend Sub New(ByVal Client As Client, ByVal Packet As Packets.Target)
            'place the target response information in the right variables.

#If DebugTargeting Then

            _Packet = Packet

#End If
            _Target = Packet.Target
            _X = Packet.X
            _Y = Packet.Y
            _Z = Packet.Z
            _UID = Packet.Serial.Value

            If _Target.Value = 0 Then _Type = Enums.TargetType.Ground
            If Packet.TargetType = 1 Then _Type = Enums.TargetType.Ground

            If Packet.TargetType = 0 Then _Type = Enums.TargetType.Item
            If Client.Mobiles.Exists(_Target) Then _Type = Enums.TargetType.Mobile
            If Client.Items.Exists(_Target) Then _Type = Enums.TargetType.Item

            If Packet.Flag = 3 Then _Type = Enums.TargetType.Canceled

        End Sub

#If DebugTargeting Then
        Public ReadOnly Property TargetPacket() As Packets.Target
            Get
                Return _Packet
            End Get
        End Property
#End If

        Public ReadOnly Property Target() As Serial
            Get
                Return _Target
            End Get
        End Property

        Public ReadOnly Property Type() As Enums.TargetType
            Get
                Return _Type
            End Get
        End Property

        Public ReadOnly Property UID() As UInt32
            Get
                Return _UID
            End Get
        End Property

        Public ReadOnly Property X() As UShort
            Get
                Return _X
            End Get
        End Property

        Public ReadOnly Property Y() As UShort
            Get
                Return _Y
            End Get
        End Property

        Public ReadOnly Property Z() As UShort
            Get
                Return _Z
            End Get
        End Property

    End Class


End Class