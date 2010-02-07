Partial Class UOAI

    Public Class Item

#Region "Constructor"
        Friend Sub New(ByVal client As Client, ByVal Serial As Serial)
            Me._Serial = Serial
            _contents = New ItemList(Me, client)
            _Client = client
        End Sub

        ''' <summary>
        ''' When using this new, never ever ever try to access the contents!
        ''' </summary>
        ''' <remarks></remarks>
        Friend Sub New()
        End Sub
#End Region

#Region "Variables"

        ''' <summary>Where the actual memory offset of the item is stored</summary>
        Friend MemoryOffset As UInt32
        Friend _Client As Client
        Friend _Serial As New Serial(0)
        Friend _Type As UShort
        Friend _Layer As Enums.Layers
        Friend _StackID As Byte = 0
        Friend _Amount As UShort = 1
        Friend _X As UShort = 0
        Friend _Y As UShort = 0
        Friend _Z As Byte = 0
        Friend _Container As Serial = WorldSerial
        Friend _Hue As UShort = 0
        Friend _Direction As Enums.Direction = Enums.Direction.North
        Friend _contents As ItemList
        Friend _IsMobile As Boolean = False

#End Region

#Region "Properties"

        ''' <summary>The serial of the item.</summary>
        Public ReadOnly Property Serial() As Serial
            Get
                Return _Serial
            End Get
        End Property

        ''' <summary>The artwork number of that item. This is what determines what it looks like in game.</summary>
        Public ReadOnly Property Type() As UShort
            Get
                Return _Type
            End Get
        End Property

        ''' <summary>The number to add the the artwork number to get the artwork number of the item if it is a stack. 
        ''' Usualy this is 0x01.</summary>
        Public ReadOnly Property StackID() As Byte
            Get
                Return _StackID
            End Get
        End Property

        ''' <summary>The number of objects in a stack.</summary>
        Public ReadOnly Property Amount() As Byte
            Get
                Return _Amount
            End Get
        End Property

        ''' <summary>The location of the item on the X axis. If the item is inside of a container, 
        ''' this represents the number of pixels within the container from the left side at which 
        ''' the item will be placed.</summary>
        Public ReadOnly Property X() As UShort
            Get
                Return _X
            End Get
        End Property

        ''' <summary>The location of the item on the Y axis. If the item is inside of a container, 
        ''' this represents the number of pixels from the top of the container that the item will 
        ''' be placed</summary>
        Public ReadOnly Property Y() As UShort
            Get
                Return _Y
            End Get
        End Property

        ''' <summary>The location of the item on the Z axis.  If the item is inside of a container this
        ''' specifies the "height" of it, like if its on top of other objects.</summary>
        Public ReadOnly Property Z() As UShort
            Get
                Return _Z
            End Get
        End Property

        ''' <summary>The serial of the container of the item.</summary>
        Public ReadOnly Property Container() As Serial
            Get
                Return _Container
            End Get
        End Property

        ''' <summary>The item's hue.</summary>
        Public ReadOnly Property Hue() As UShort
            Get
                Return _Hue
            End Get
        End Property

        ''' <summary>
        ''' Returns a string containing the ASCII name of the item artwork name. Returns "Blank" if no typename can be found.
        ''' </summary>
        Public ReadOnly Property TypeName() As String
            Get
                If StrLst.Table(1036383 + _Type) Is Nothing Then
                    Return "Blank"
                End If

                Return StrLst.Table(1036383 + _Type)
            End Get
        End Property

        Public ReadOnly Property Contents() As ItemList
            Get
                Return _contents
            End Get
        End Property

        Public Overridable ReadOnly Property Layer() As Enums.Layers
            Get
                Return _Layer
            End Get
        End Property

#End Region

#Region "Functions/Subs"
        Public Sub DoubleClick()
            'Make the packet
            Dim dc As New Packets.Doubleclick

            'Assign the serial
            dc.Serial = Me.Serial

            'Send the packet to the server.
            _Client.Send(dc, Enums.PacketDestination.SERVER)
        End Sub

        Public Sub SingleClick()
            'Make the packet
            Dim sc As New Packets.Singleclick

            'Assign the serial
            sc.Serial = Me.Serial

            'Send the packet to the server.
            _Client.Send(sc, Enums.PacketDestination.SERVER)
        End Sub

        Public Sub ShowText(ByVal Text As String)
            Dim k As New Packets.Text(Text)
            k.Name = "System"
            k.Serial = Serial
            k.BodyType = Type
            k.SpeechType = Enums.SpeechTypes.Regular
            k.TextHue = CUShort(52)
            k.TextFont = Enums.Fonts.Default

#Const DebugSysMsg = False
#If DebugSysMsg Then
            Console.WriteLine("Sending SysMsg Packet to Client: " & BitConverter.ToString(k.Data))
#End If

            _Client.Send(k, Enums.PacketDestination.CLIENT)
        End Sub

        ''' <summary>
        ''' Picks up the object.
        ''' </summary>
        ''' <param name="Amount">The amount that you want to take, if it is a stack. (0 for the whole stack)</param>
        Public Sub Take(ByVal Amount As UShort)
            Dim j As Packets.TakeObject

            If Amount = 0 Then
                j = New Packets.TakeObject(_Serial, _Amount)
            Else
                j = New Packets.TakeObject(_Serial, Amount)
            End If

            _Client._ItemInHand = _Serial
            _Client.Send(j, Enums.PacketDestination.SERVER)
        End Sub

#End Region

    End Class

End Class