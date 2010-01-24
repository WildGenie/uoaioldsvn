Imports System.IO
Imports System.Text

Partial Class UOAI

    ''' <summary>The base packet class, inherited by all classes in UOAI.Packets</summary>
    Public Class Packet
        Friend _Data() As Byte
        Friend _Type As Enums.PacketType
        Friend _size As UShort
        Friend buff As BufferHandler

        Public Sub New(ByVal Type As Enums.PacketType)
            _Type = Type
        End Sub

        ''' <summary>Returns the raw packet data as a byte array.</summary>
        Public Overridable ReadOnly Property Data() As Byte()
            Get
                Return _Data
            End Get
        End Property

        Public ReadOnly Property Type() As Enums.PacketType
            Get
                Return _Type
            End Get
        End Property

        Public ReadOnly Property Size() As UShort
            Get
                Return _size
            End Get
        End Property

    End Class

    ''' <summary>A namespace encapsulating the packet types classes.</summary>
    Public Class Packets

        ''' <summary>Clients send this packet when talking.</summary>
        ''' <remarks>Packet 0xAD</remarks>
        Public Class UnicodeSpeechPacket
            Inherits UOAI.Packet

            Private _mode As Byte
            Private _hue As UShort
            Private _font As UShort
            Private _lang As String
            Private _text As String

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.SpeechUnicode)
                _Data = bytes

                buff = New BufferHandler(bytes)

                _Type = Enums.PacketType.SpeechUnicode

                buff.Position = 1

                buff.networkorder = False
                '1-2
                _size = buff.readushort
                buff.networkorder = True

                '3
                _mode = buff.readbyte

                '4-5
                _hue = buff.readushort

                '6-7
                _font = buff.readushort

                '8-11
                _lang = buff.readustrn(4)

                '12-(Size - 1)
                _text = buff.readustr

            End Sub

            ''' <summary>Gets or Sets the speech type as <see cref="UOAI.Enums.SpeechTypes"/>.</summary>
            Public Property Mode() As Enums.SpeechTypes
                Get
                    Return _mode
                End Get
                Set(ByVal value As Enums.SpeechTypes)
                    _mode = value
                    buff.Position = 3
                    buff.writebyte(_mode)
                End Set
            End Property

            ''' <summary>Gets or Sets the hue of the text.</summary>
            Public Property Hue() As UShort
                Get
                    Return _hue
                End Get
                Set(ByVal value As UShort)
                    _hue = value
                    buff.Position = 4
                    buff.writeushort(value)
                End Set
            End Property

            ''' <summary>Gets or Sets the font of the text.</summary>
            Public Property Font() As Enums.Fonts
                Get
                    Return _font
                End Get
                Set(ByVal value As Enums.Fonts)
                    _hue = value
                    buff.Position = 6
                    buff.writeushort(value)
                End Set
            End Property

            ''' <summary>Gets or Sets the language key of the packet. This only effects how it is interpreted, it does NOT change the actual language.</summary>
            Public Property Language() As String
                Get
                    Return _lang
                End Get
                Set(ByVal value As String)
                    If value.Length <= 4 Then
                        _lang = value
                        buff.Position = 8
                        buff.writestrn(_lang, 4)
                    Else
                        Throw New ConstraintException("Language string must be 4 characters or less!")
                    End If
                End Set
            End Property

            ''' <summary>Gets or Sets the text that will be displayed. Will not allow a value longer than the current one.</summary>
            Public Property Text() As String
                Get
                    Return _text
                End Get
                Set(ByVal value As String)
                    If value.Length <= _text.Length Then
                        buff.Position = 12
                        buff.writeustrn(value, _text.Length)
                        _text = value
                    Else
                        Throw New ConstraintException("You cannot set the text value to a value longer than its current one.")
                    End If
                End Set
            End Property

        End Class

        ''' <summary>This is sent from the server to tell the client that someone is talking.</summary>
        ''' <remarks>Packet 0xAE</remarks>
        Public Class UnicodeTextPacket
            Inherits Packet
            Private _text As String
            Private _Mode As Enums.SpeechTypes
            Private _hue As UShort
            Private _font As Enums.Fonts
            Private _body As UShort
            Private _Serial As Serial
            Private _lang As String
            Private _name As String

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.TextUnicode)
                _Data = bytes

                buff = New BufferHandler(bytes, True)

                'Parse the data into the fields.
                _Data = bytes

                '0
                _Type = bytes(0)

                buff.Position = 1
                buff.networkorder = False

                '1-2
                _size = buff.readushort()
                buff.networkorder = True

                '3-6
                _Serial = buff.readuint

                '7-8
                _body = buff.readushort

                '9
                _Mode = buff.readbyte

                '10-11
                _hue = buff.readushort

                '12-13
                _font = buff.readushort

                '14-17
                _lang = buff.readstrn(4)

                '18-48
                _name = buff.readstr

                buff.Position = 48

                '48-(Size - 1)
                _text = buff.readustr

            End Sub

            ''' <summary>Gets or Sets the text that will be displayed. Will not allow a value longer than the current one.</summary>
            Public Property Text() As String
                Get
                    Return _text
                End Get
                Set(ByVal value As String)
                    If value.Length <= _text.Length Then
                        buff.Position = 48
                        buff.writeustrn(value, _text.Length)
                        _text = value
                    Else
                        Throw New ConstraintException("You cannot set the text value to a value longer than its current one.")
                    End If
                End Set
            End Property

            ''' <summary>Gets or Sets the name of the speaker. Maximum of 30 characters.</summary>
            Public Property Name() As String
                Get
                    Return _name
                End Get
                Set(ByVal value As String)
                    If Name.Length <= 30 Then
                        _name = value
                        buff.Position = 18
                        buff.writestrn(value, 30)
                    Else
                        Throw New ApplicationException("Name is too long, much be 30 characters or less.")
                    End If
                End Set
            End Property

            ''' <summary>Gets or Sets the speech type as <see cref="UOAI.Enums.SpeechTypes"/>.</summary>
            Public Property Mode() As Enums.SpeechTypes
                Get
                    Return _Mode
                End Get
                Set(ByVal value As Enums.SpeechTypes)
                    _Mode = value
                    buff.Position = 9
                    buff.writebyte(_Mode)
                End Set
            End Property

            ''' <summary>Gets or sets the serial of the person or object speaking. 0xFFFFFFFF is used for system.</summary>
            Public Property Serial() As Serial
                Get
                    Return _Serial
                End Get
                Set(ByVal value As Serial)
                    _Serial = value
                    buff.Position = 3
                    buff.writeuint(_Serial)
                End Set
            End Property

            ''' <summary>Gets or Sets the body value of the character that is talking. 0xFFFF is used for system.</summary>
            Public Property Body() As UShort
                Get
                    Return _body
                End Get
                Set(ByVal value As UShort)
                    _body = value
                    buff.Position = 7
                    buff.writeushort(value)
                End Set
            End Property

            ''' <summary>Gets or Sets the hue of the text.</summary>
            Public Property Hue() As UShort
                Get
                    Return _hue
                End Get
                Set(ByVal value As UShort)
                    _hue = value
                    buff.Position = 10
                    buff.writeushort(value)
                End Set
            End Property

            ''' <summary>Gets or Sets the font of the text.</summary>
            Public Property Font() As Enums.Fonts
                Get
                    Return _font
                End Get
                Set(ByVal value As Enums.Fonts)
                    _hue = value
                    buff.Position = 12
                    buff.writeushort(value)
                End Set
            End Property

            ''' <summary>Gets or Sets the language key of the packet. This only effects how it is interpreted, it does NOT change the actual language.</summary>
            Public Property Language() As String
                Get
                    Return _lang
                End Get
                Set(ByVal value As String)
                    If value.Length <= 4 Then
                        _lang = value
                        buff.Position = 14
                        buff.writestrn(_lang, 4)
                    Else
                        Throw New ConstraintException("Language string must be 4 characters or less!")
                    End If
                End Set
            End Property

        End Class

        ''' <summary>
        ''' This is sent by the server to open a container or game board (which is also a container).
        ''' </summary>
        ''' <remarks>Packet 0x24</remarks>
        Public Class OpenContainer
            Inherits Packet

            Private _Serial As Serial
            Private _model As UShort

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.OpenContainer)
                _Data = bytes
                buff = New BufferHandler(bytes)

                buff.Position = 1

                _size = 7
                '1-4
                _Serial = buff.readuint

                '5-6
                _model = buff.readushort

            End Sub

            ''' <summary>
            ''' The serial of the container being opened.
            ''' </summary>
            Public Property Serial() As Serial
                Get
                    Return _Serial
                End Get
                Set(ByVal value As Serial)
                    _Serial = value
                    buff.Position = 1
                    buff.writeuint(value)
                End Set
            End Property

            ''' <summary>
            ''' The model of the container being opened.
            ''' </summary>
            Public Property Model() As UShort
                Get
                    Return _model
                End Get
                Set(ByVal value As UShort)
                    _model = value
                    buff.Position = 5
                    buff.writeushort(value)
                End Set
            End Property

        End Class

        ''' <summary>
        ''' This is sent by the server to add a single item to a container. (not to display its contents)
        ''' </summary>
        ''' <remarks>Packet 0x25</remarks>
        Public Class ObjectToObject
            Inherits Packet

            Private _Serial As Serial
            Private _Itemtype As ItemType
            Private _stackID As Byte
            Private _amount As UShort
            Private _X As UShort
            Private _Y As UShort
            Private _Container As Serial
            Private _Hue As UShort

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.ObjecttoObject)
                _Data = bytes
                buff = New BufferHandler(bytes)

                _size = &H14

                buff.Position = 1
                '1-4
                _Serial = buff.readuint

                '5-6
                _Itemtype = buff.readushort

                '7
                _stackID = buff.readbyte

                '8-9
                _amount = buff.readushort

                '10-11
                _X = buff.readushort

                '12-13
                _Y = buff.readushort

                '14-17
                _Container = buff.readuint

                '18-19
                _Hue = buff.readushort

            End Sub

            ''' <summary>
            ''' The serial of the item to add.
            ''' </summary>
            Public Property Serial() As Serial
                Get
                    Return _Serial
                End Get
                Set(ByVal value As Serial)
                    _Serial = value
                    buff.Position = 1
                    buff.writeuint(value)
                End Set
            End Property

            ''' <summary>
            ''' The artwork number of the item.
            ''' </summary>
            Public Property ItemType() As ItemType
                Get
                    Return _Itemtype
                End Get
                Set(ByVal value As ItemType)
                    _Itemtype = value
                    buff.Position = 5
                    buff.writeushort(value.BaseValue)
                End Set
            End Property

            Public Property StackID() As Byte
                Get
                    Return _stackID
                End Get
                Set(ByVal value As Byte)
                    _stackID = value
                    buff.Position = 7
                    buff.writebyte(value)
                End Set
            End Property

            Public Property amount() As UShort
                Get
                    Return _amount
                End Get
                Set(ByVal value As UShort)
                    _amount = value
                    buff.Position = 8
                    buff.writeushort(_amount)
                End Set
            End Property

            Public Property X() As UShort
                Get
                    Return _X
                End Get
                Set(ByVal value As UShort)
                    _X = value
                    buff.Position = 10
                    buff.writeushort(_X)
                End Set
            End Property

            Public Property Y() As UShort
                Get
                    Return _Y
                End Get
                Set(ByVal value As UShort)
                    _Y = value
                    buff.Position = 12
                    buff.writeushort(_Y)
                End Set
            End Property

            Public Property Container() As Serial
                Get
                    Return _Container
                End Get
                Set(ByVal value As Serial)
                    _Container = value
                    buff.Position = 14
                    buff.writeuint(value)
                End Set
            End Property

            Public Property Hue() As UShort
                Get
                    Return _Hue
                End Get
                Set(ByVal value As UShort)
                    _Hue = value
                    buff.Position = 18
                    buff.writeushort(value)
                End Set
            End Property

        End Class

        ''' <summary>
        ''' This is sent to deny the player's request to get an item.
        ''' </summary>
        ''' <remarks></remarks>
        Public Class GetItemFailed
            Inherits Packet
            Private _reason As Enums.GetItemFailedReason

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.ObjecttoObject)
                _Data = bytes
                buff = New BufferHandler(bytes)

                _size = 2

                '1
                _reason = buff.readbyte

            End Sub

            Public Property Reason() As Enums.GetItemFailedReason
                Get
                    Return _reason
                End Get
                Set(ByVal value As Enums.GetItemFailedReason)
                    _reason = value
                    buff.Position = 1
                    buff.writebyte(_reason)
                End Set
            End Property

        End Class

        ''' <summary>
        ''' This is sent by the server to equip a single item on a character.
        ''' </summary>
        ''' <remarks></remarks>
        Public Class EquipItem
            Inherits Packet

            Private _serial As Serial
            Private _itemtype As ItemType
            Private _layer As Enums.Layers
            Private _container As Serial
            Private _hue As UShort

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.EquipItem)
                _Data = bytes
                buff = New BufferHandler(bytes)

                buff.Position = 1
                '1-4
                _serial = buff.readuint

                '5-6
                _itemtype = buff.readushort

                '7
                'Unknown byte 0x00
                buff.Position += 1

                '8
                _layer = buff.readbyte

                '9-12
                _container = buff.readuint

                '13-14
                _hue = buff.readushort

            End Sub

            ''' <summary>
            ''' The serial of the item to equip.
            ''' </summary>
            Public Property Serial() As Serial
                Get
                    Return _serial
                End Get
                Set(ByVal value As Serial)
                    _serial = value
                    buff.Position = 1
                    buff.writeuint(value)
                End Set
            End Property

            ''' <summary>
            ''' The item's artwork number.
            ''' </summary>
            Public Property ItemType() As ItemType
                Get
                    Return _itemtype
                End Get
                Set(ByVal value As ItemType)
                    _itemtype = value
                    buff.Position = 5
                    buff.writeushort(value)
                End Set
            End Property

            ''' <summary>
            ''' The item's layer
            ''' </summary>
            Public Property Layer() As Enums.Layers
                Get
                    Return _layer
                End Get
                Set(ByVal value As Enums.Layers)
                    _layer = value
                    buff.Position = 8
                    buff.writebyte(value)
                End Set
            End Property

            ''' <summary>
            ''' The serial of the character on which the item will be equipped.
            ''' </summary>
            Public Property Container() As Serial
                Get
                    Return _container
                End Get
                Set(ByVal value As Serial)
                    _container = value
                    buff.Position = 9
                    buff.writeuint(value)
                End Set
            End Property

            ''' <summary>
            ''' The item's hue.
            ''' </summary>
            Public Property Hue() As UShort
                Get
                    Return _hue
                End Get
                Set(ByVal value As UShort)
                    _hue = value
                    buff.Position = 13
                    buff.writeushort(value)
                End Set
            End Property

        End Class

        ''' <summary>
        ''' This is sent to display the contents of a container.
        ''' </summary>
        ''' <remarks></remarks>
        Public Class ContainerContents
            Inherits Packet

            Private _ItemList() As EditableItem
            Private _Count As UShort

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.ContainerContents)
                _Data = bytes

                buff.Position = 1
                buff.networkorder = False
                '1-2
                _size = buff.readushort
                buff.networkorder = True

                '3-4
                _Count = buff.readushort

                Dim it As New EditableItem

                For i As UShort = 1 To _Count - 1
                    buff.Position = (i * 19) + 5
                    it._Serial = buff.readuint
                    it._Type = buff.readushort
                    it._StackID = buff.readbyte
                    it._X = buff.readushort
                    it._Y = buff.readushort
                    it._Container = buff.readuint
                    it._Hue = buff.readushort
                    _ItemList(i) = it
                Next

            End Sub

            Public Overloads ReadOnly Property Items() As EditableItem()
                Get
                    Return _ItemList
                End Get
            End Property

            Public Overloads Property Items(ByVal Index As UShort) As EditableItem
                Get
                    Return _ItemList(Index)
                End Get
                Set(ByVal value As EditableItem)
                    _ItemList(Index) = value
                End Set
            End Property

            Public Overrides ReadOnly Property Data() As Byte()
                Get
                    'Write the byte array dynamically
                    buff = New BufferHandler(_Data)

                    buff.Position = 1
                    buff.networkorder = False
                    '1-2
                    buff.writeushort(_size)
                    buff.networkorder = True

                    '3-4
                    buff.writeushort(_Count)

                    For i As UShort = 0 To _Count
                        buff.Position = (i * 19) + 5
                        buff.writeuint(_ItemList(i)._Serial)
                        buff.writeushort(_ItemList(i)._Type.BaseValue)
                        buff.writebyte(_ItemList(i)._StackID)
                        buff.writeushort(_ItemList(i)._X)
                        buff.writeushort(_ItemList(i)._Y)
                        buff.writeuint(_ItemList(i)._Container)
                        buff.writeushort(_ItemList(i)._Hue)
                    Next

                    Return _Data
                End Get
            End Property

        End Class

        ''' <summary>
        ''' Sent by the client to rename another mobile.
        ''' </summary>
        Public Class RenameMOB
            Inherits Packet

            Private _Serial As Serial
            Private _Name As String


            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.RenameMOB)
                _Data = bytes
                buff = New BufferHandler(bytes)

                buff.Position = 1
                '1-4
                _Serial = buff.readuint

                '5-35
                _Name = buff.readstrn(30)

            End Sub

            Public Property Serial() As Serial
                Get
                    Return _Serial
                End Get
                Set(ByVal value As Serial)
                    _Serial = value
                    buff.Position = 1
                    buff.writeuint(value)
                End Set
            End Property

            Public Property Name() As String
                Get
                    Return _Name
                End Get
                Set(ByVal value As String)
                    If value.Length <= 30 Then
                        _Name = value
                        buff.Position = 5
                        buff.writestrn(_Name, 30)
                    Else
                        Throw New ConstraintException("String specified for name is too long, it must be < 30 characters long.")
                    End If
                End Set
            End Property

        End Class

    End Class

    ''' Hide this class from the user, there is no reason from him/her to see it.
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class EditableItem
        Friend _Serial As Serial
        Friend _Type As ItemType
        Friend _StackID As Byte
        Friend _Amount As UShort
        Friend _X As UShort
        Friend _Y As UShort
        Friend _Z As SByte
        Friend _Container As Serial
        Friend _Hue As UShort
        Friend _Direction As Enums.Direction

#Region "Properties"

        ''' <summary>Gets or sets the serial of the item.</summary>
        Public Shadows Property Serial() As Serial
            Get
                Return _Serial
            End Get
            Set(ByVal value As Serial)
                _Serial = value
            End Set
        End Property

        ''' <summary>Gets or sets the artwork number of that item. This is what determines what it looks like in game.</summary>
        Public Shadows Property Type() As ItemType
            Get
                Return _Type
            End Get
            Set(ByVal value As ItemType)
                _Type = value
            End Set
        End Property

        ''' <summary>Gets or sets the number to add the the artwork number to get the artwork number of the item if it is a stack. 
        ''' Usualy this is 0x01.</summary>
        Public Shadows Property StackID() As Byte
            Get
                Return _StackID
            End Get
            Set(ByVal value As Byte)
                _StackID = value
            End Set
        End Property

        ''' <summary>Gets or sets number of objects in a stack.</summary>
        Public Shadows Property Amount() As Byte
            Get
                Return _Amount
            End Get
            Set(ByVal value As Byte)
                _Amount = value
            End Set
        End Property

        ''' <summary>Gets or sets the location of the item on the X axis. If the item is inside of a container, 
        ''' this represents the number of pixels within the container from the left side at which 
        ''' the item will be placed.</summary>
        Public Shadows Property X() As UShort
            Get
                Return _X
            End Get
            Set(ByVal value As UShort)
                _X = value
            End Set
        End Property

        ''' <summary>Gets or sets the location of the item on the Y axis. If the item is inside of a container, 
        ''' this represents the number of pixels from the top of the container that the item will 
        ''' be placed</summary>
        Public Shadows Property Y() As UShort
            Get
                Return _Y
            End Get
            Set(ByVal value As UShort)
                _Y = value
            End Set
        End Property

        ''' <summary>Gets or sets the serial of the container of the item.</summary>
        Public Shadows Property Container() As Serial
            Get
                Return _Container
            End Get
            Set(ByVal value As Serial)
                _Container = value
            End Set
        End Property

        ''' <summary>Gets or sets the item's hue.</summary>
        Public Shadows Property Hue() As UShort
            Get
                Return _Hue
            End Get
            Set(ByVal value As UShort)
                _Hue = value
            End Set
        End Property

#End Region

    End Class

    Partial Class Enums

        ''' <summary>
        ''' Reason enumeration for "Get Item Failed" packet. (0x27)
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum GetItemFailedReason
            ''' <summary>Displays "You cannot pick that up."</summary>
            CannotPickup

            ''' <summary>Displays "That is too far away."</summary>
            TooFar

            ''' <summary>Displays "That is out of sight."</summary>
            OutOfSight

            ''' <summary>Displays "That item does not belong to you. You will have to steal it."</summary>
            DoesntBelongToYou

            ''' <summary>Displays "You are already holding an item."</summary>
            AlreadyHoldingItem

            ''' <summary>Tells the client to delete the item from its cache.</summary>
            Cmd_DestroyTheItem

            ''' <summary>Displays no message, just doesn't let you pick it up.</summary>
            NoMessage
        End Enum

        ''' <summary>
        ''' Packet type enumeration.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum PacketType As Byte
            CharacterCreation = &H0
            Logout = &H1
            RequestMovement = &H2
            Speech = &H3
            RequestGodMode = &H4
            Attack = &H5
            DoubleClick = &H6
            TakeObject = &H7
            DropObject = &H8
            SingleClick = &H9
            Edit = &HA
            EditArea = &HB
            TileData = &HC
            NPCData = &HD
            EditTemplateData = &HE
            Paperdoll_Old = &HF
            HueData = &H10
            MobileStats = &H11
            GodCommand = &H12
            EquipItemRequest = &H13
            ChangeElevation = &H14
            Follow = &H15
            RequestScriptNames = &H16
            ScriptTreeCommand = &H17
            ScriptAttach = &H18
            NPCConversationData = &H19
            ShowItem = &H1A
            LoginConfirm = &H1B
            Text = &H1C
            Destroy = &H1D
            Animate = &H1E
            Explode = &H1F
            Teleport = &H20
            BlockMovement = &H21
            AcceptMovement_ResyncRequest = &H22
            DragItem = &H23
            OpenContainer = &H24
            ObjecttoObject = &H25
            OldClient = &H26
            GetItemFailed = &H27
            DropItemFailed = &H28
            DropItemOK = &H29
            Blood = &H2A
            GodMode = &H2B
            Death = &H2C
            Health = &H2D
            EquipItem = &H2E
            Swing = &H2F
            AttackOK = &H30
            AttackEnd = &H31
            HackMover = &H32
            Group = &H33
            ClientQuery = &H34
            ResourceType = &H35
            ResourceTileData = &H36
            MoveObject = &H37
            FollowMove = &H38
            Groups = &H39
            Skills = &H3A
            AcceptOffer = &H3B
            ContainerContents = &H3C
            Ship = &H3D
            Versions = &H3E
            UpdateStatics = &H3F
            UpdateTerrain = &H40
            UpdateTiledata = &H41
            UpdateArt = &H42
            UpdateAnim = &H43
            UpdateHues = &H44
            VerOK = &H45
            NewArt = &H46
            NewTerrain = &H47
            NewAnim = &H48
            NewHues = &H49
            DestroyArt = &H4A
            CheckVer = &H4B
            ScriptNames = &H4C
            ScriptFile = &H4D
            LightChange = &H4E
            Sunlight = &H4F
            BoardHeader = &H50
            BoardMessage = &H51
            PostMessage = &H52
            LoginReject = &H53
            Sound = &H54
            LoginComplete = &H55
            MapCommand = &H56
            UpdateRegions = &H57
            NewRegion = &H58
            NewContextFX = &H59
            UpdateContextFX = &H5A
            GameTime = &H5B
            RestartVer = &H5C
            PreLogin = &H5D
            ServerList_Olsolete = &H5E
            AddServer = &H5F
            ServerRemove = &H60
            DestroyStatic = &H61
            MoveStatic = &H62
            AreaLoad = &H63
            AreaLoadRequest = &H64
            WeatherChange = &H65
            BookContents = &H66
            SimpleEdit = &H67
            ScriptLSAttach = &H68
            Friends = &H69
            FriendNotify = &H6A
            KeyUse = &H6B
            Target = &H6C
            Music = &H6D
            Animation = &H6E
            Trade = &H6F
            Effect = &H70
            BulletinBoard = &H71
            Combat = &H72
            Ping = &H73
            ShopData = &H74
            RenameMOB = &H75
            ServerChange = &H76
            NakedMOB = &H77
            EquippedMOB = &H78
            ResourceQuery = &H79
            ResourceData = &H7A
            Sequence = &H7B
            ObjectPicker = &H7C
            PickedObject = &H7D
            GodViewQuery = &H7E
            GodViewData = &H7F
            AccountLoginRequest = &H80
            AccountLoginOK = &H81
            AccountLoginFailed = &H82
            AccountDeleteCharacter = &H83
            ChangeCharacterPassword = &H84
            DeleteCharacterFailed = &H85
            AllCharacters = &H86
            SendResources = &H87
            OpenPaperdoll = &H88
            CorpseEquipment = &H89
            TriggerEdit = &H8A
            DisplaySign = &H8B
            ServerRedirect = &H8C
            Unused3 = &H8D
            MoveCharacter = &H8E
            Unused4 = &H8F
            OpenCourseGump = &H90
            PostLogin = &H91
            UpdateMulti = &H92
            BookHeader = &H93
            UpdateSkill = &H94
            HuePicker = &H95
            GameCentralMonitor = &H96
            MovePlayer = &H97
            MOBName = &H98
            TargetMulti = &H99
            TextEntry = &H9A
            RequestAssistance = &H9B
            AssistRequest = &H9C
            GMSingle = &H9D
            ShopSell = &H9E
            ShopOffer = &H9F
            ServerSelect = &HA0
            HPHealth = &HA1
            ManaHealth = &HA2
            FatHealth = &HA3
            HardwareInfo = &HA4
            WebBrowser = &HA5
            Message = &HA6
            RequestTip = &HA7
            ServerList = &HA8
            CharacterList = &HA9
            CurrentTarget = &HAA
            StringQuery = &HAB
            StringResponse = &HAC
            SpeechUnicode = &HAD
            TextUnicode = &HAE
            DeathAnimation = &HAF
            GenericGump = &HB0
            GenericGumpTrigger = &HB1
            ChatMessage = &HB2
            ChatText = &HB3
            TargetObjectList = &HB4
            OpenChat = &HB5
            HelpRequest = &HB6
            HelpText = &HB7
            CharacterProfile = &HB8
            Features = &HB9
            Pointer = &HBA
            AccountID = &HBB
            GameSeason = &HBC
            ClientVersion = &HBD
            AssistVersion = &HBE
            GenericCommand = &HBF
            HuedFX = &HC0
            LocalizedText = &HC1
            UnicodeTextEntry = &HC2
            GlobalQueue = &HC3
            Semivisible = &HC4
            InvalidMap = &HC5
            InvalidMapEnable = &HC6
            ParticleEffect = &HC7
            ChangeUpdateRange = &HC8
            TripTime = &HC9
            UTripTime = &HCA
            GlobalQueueCount = &HCB
            LocalizedTextPlusString = &HCC
            UnknownGodPacket = &HCD
            IGRClient = &HCE
            IGRLogin = &HCF
            IGRConfiguration = &HD0
            IGRLogout = &HD1
            UpdateMobile = &HD2
            ShowMobile = &HD3
            BookInfo = &HD4
            UnknownClientPacket = &HD5
            MegaCliloc = &HD6
            AOSCommand = &HD7
            CustomHouse = &HD8
            Metrics = &HD9
            Mahjong = &HDA
            CharacterTransferLog = &HDB
        End Enum
    End Class

    'Buffer Serialization and Deserialization
    ''' Hide this class from the user, there is no reason from him/her to see it.
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Friend Class BufferHandler
        Inherits Stream
        Public curpos As Long
        Private m_buffer As Byte()
        Public networkorder As Boolean

#Region "constructors"
        Public Sub New(ByVal frombuffer As Byte(), ByVal bNetworkOrder As Boolean)
            m_buffer = frombuffer
            networkorder = bNetworkOrder
            curpos = 0
        End Sub
        Public Sub New(ByVal size As UInt32)
            m_buffer = New Byte(size - 1) {}
            networkorder = False
            curpos = 0
        End Sub
        Public Sub New(ByVal size As Integer)
            m_buffer = New Byte(size - 1) {}
            networkorder = False
            curpos = 0
        End Sub
        Public Sub New(ByVal frombuffer As Byte())
            m_buffer = frombuffer
            curpos = 0
            networkorder = False
        End Sub
        Public Sub New(ByVal size As UInt32, ByVal bNetworkOrder As Boolean)
            m_buffer = New Byte(size - 1) {}
            curpos = 0
            networkorder = bNetworkOrder
        End Sub
#End Region

        Public Property buffer() As Byte()
            Get
                Return m_buffer
            End Get
            Set(ByVal value As Byte())
                m_buffer = value
            End Set
        End Property

        Default Public Property Item(ByVal index As Integer) As Byte
            Get
                If (m_buffer IsNot Nothing) AndAlso (m_buffer.Length > index) Then
                    Return m_buffer(index)
                Else
                    Return 0
                End If
            End Get
            Set(ByVal value As Byte)
                If m_buffer IsNot Nothing Then
                    If m_buffer.Length > index Then
                        m_buffer(index) = value
                    End If
                End If
            End Set
        End Property

#Region "Stream members"

        Public Overloads Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overloads Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overloads Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overloads Overrides Sub Flush()
            Throw New NotImplementedException()
        End Sub

        Public Overloads Overrides ReadOnly Property Length() As Long
            Get
                Return (m_buffer.Length - curpos)
            End Get
        End Property

        Public Overloads Overrides Property Position() As Long
            Get
                Return curpos
            End Get
            Set(ByVal value As Long)
                curpos = value
            End Set
        End Property

        Public Overloads Overrides Function Read(ByVal destbuffer As Byte(), ByVal offset As Integer, ByVal count As Integer) As Integer
            Dim i As Integer = 0
            For i = 0 To count - 1
                If curpos < m_buffer.Length Then
                    If networkorder Then
                        destbuffer(offset + count - 1 - i) = m_buffer(curpos)
                    Else
                        destbuffer(offset + i) = m_buffer(curpos)
                    End If
                    curpos += 1
                Else
                    Exit For
                End If
            Next
            Return i
        End Function

        Public Overloads Overrides Function Seek(ByVal offset As Long, ByVal origin As SeekOrigin) As Long
            Select Case origin
                Case SeekOrigin.Begin
                    If offset < m_buffer.Length Then
                        curpos = offset
                    End If
                    Exit Select
                Case SeekOrigin.Current
                    If (curpos + offset) < m_buffer.Length Then
                        curpos += offset
                    End If
                    Exit Select
                Case SeekOrigin.[End]
                    If offset < m_buffer.Length Then
                        curpos = m_buffer.Length - 1 - offset
                    End If
                    Exit Select
                Case Else
                    Exit Select
            End Select
            Return curpos
        End Function

        Public Overloads Overrides Sub SetLength(ByVal value As Long)
            Throw New NotImplementedException()
        End Sub

        Public Overloads Overrides Sub Write(ByVal destbuffer As Byte(), ByVal offset As Integer, ByVal count As Integer)
            Dim i As Integer = 0

            If (m_buffer Is Nothing) OrElse (count > Length) Then
                Throw New Exception("Not enough space on buffer!")
            End If

            For i = 0 To count - 1
                If curpos < m_buffer.Length Then
                    If networkorder Then
                        m_buffer(curpos) = destbuffer(offset + count - 1 - i)
                    Else
                        m_buffer(curpos) = destbuffer(offset + i)
                    End If
                    curpos += 1
                Else
                    Exit For
                End If
            Next
            Exit Sub
        End Sub

#End Region

#Region "reading"

        Public Function readbytes(ByVal count As Integer) As Byte()
            Dim targetbuffer As Byte() = New Byte(count - 1) {}
            Read(targetbuffer, 0, count)
            Return targetbuffer
        End Function

#Region "Integer/UInt32"
        Public Function readuint() As UInt32
            Return BitConverter.ToUInt32(readbytes(4), 0)
        End Function

        Public Function readint() As Integer
            Return BitConverter.ToInt32(readbytes(4), 0)
        End Function
#End Region

#Region "Short/UShort"
        Public Function readushort() As UShort
            Return BitConverter.ToUInt16(readbytes(2), 0)
        End Function

        Public Function readshort() As Short
            Return BitConverter.ToInt16(readbytes(2), 0)
        End Function
#End Region

#Region "Byte/Character"

        Public Function readchar() As SByte
            If Length > 0 Then
                curpos = curpos + 1
                Return CSByte(m_buffer(curpos - 1))
            Else
                Return 0
            End If
        End Function

        Public Shadows Function readbyte() As Byte
            If True Then
                If Length > 0 Then
                    curpos = curpos + 1
                    Return m_buffer(curpos - 1)
                Else
                    Return 0
                End If
            End If
        End Function
#End Region

#Region "ASCII Strings"
        Public Function readstr() As String
            Dim prevpos As Long = curpos
            Dim count As Integer = 1

            While (readbyte() > 0) And (Length > 0)
                count += 1
            End While

            curpos = prevpos

            Return readstrn(count - 1)
        End Function

        Public Function readstrn(ByVal size As Integer) As String
            Dim characterarray As Char() = New Char(size - 1) {}
            For i As Integer = 0 To size - 1
                characterarray(i) = Chr(readbyte())
            Next
            Return New String(characterarray, 0, size)
        End Function
#End Region

#Region "Unicode Strings"

        Public Function readustr() As String

            Dim prevpos As Long = curpos
            Dim count As Integer = 1

            While (readushort() > 0) And (Length > 0)
                count += 1
            End While

            curpos = prevpos

            Return readustrn(count - 1)
        End Function

        Public Function readustrn(ByVal size As Integer) As String

            Dim characterarray As Char() = New Char(size - 1) {}

            For i As Integer = 0 To size - 1
                characterarray(i) = ChrW(readushort())
            Next

            Return New String(characterarray, 0, size)
        End Function
#End Region

#End Region

#Region "writing"

        Public Sub writeuint(ByVal towrite As UInt32)
            Write(BitConverter.GetBytes(towrite), 0, 4)
        End Sub

        Public Sub writeint(ByVal towrite As Integer)
            Write(BitConverter.GetBytes(towrite), 0, 4)
        End Sub

        Public Sub writeushort(ByVal towrite As UShort)
            Write(BitConverter.GetBytes(towrite), 0, 2)
        End Sub

        Public Sub writeshort(ByVal towrite As Short)
            Write(BitConverter.GetBytes(towrite), 0, 2)
        End Sub

        Public Overrides Sub writebyte(ByVal towrite As Byte)
            If Length > 0 Then
                m_buffer(curpos) = towrite
                curpos += 1
            End If
        End Sub

        Public Sub writechar(ByVal towrite As SByte)
            If Length > 0 Then
                m_buffer(curpos) = CByte(towrite)
                curpos += 1
            End If
        End Sub

        Public Sub writestr(ByVal towrite As String)
            Dim strbytes As Byte() = ASCIIEncoding.ASCII.GetBytes(towrite)
            For i As Integer = 0 To strbytes.Length - 1
                writebyte(strbytes(i))
            Next
            If strbytes(strbytes.Length - 1) <> 0 Then
                'ensure '\0'-termination
                writebyte(0)
            End If
        End Sub

        Public Sub writestrn(ByVal towrite As String, ByVal length As Integer)
            Dim strbytes As Byte() = ASCIIEncoding.ASCII.GetBytes(towrite)
            For i As Integer = 0 To length - 1
                If i < strbytes.Length Then
                    writebyte(strbytes(i))
                Else
                    writebyte(0)
                End If
            Next
        End Sub

        Public Sub writeustr(ByVal towrite As String)
            Dim strbytes As Char() = towrite.ToCharArray()
            For i As Integer = 0 To strbytes.Length - 1
                writeushort(BitConverter.GetBytes(strbytes(i))(0))
            Next

            If BitConverter.GetBytes(strbytes(strbytes.Length - 1))(0) <> 0 Then
                'ensure '\0'-termination
                writeushort(0)
            End If
        End Sub

        Public Sub writeustrn(ByVal towrite As String, ByVal length As Integer)
            Dim strbytes As Char() = towrite.ToCharArray()
            For i As Integer = 0 To length - 1
                If i < strbytes.Length Then
                    writeushort(BitConverter.GetBytes(strbytes(i))(0))
                Else
                    writeushort(0)
                End If
            Next
        End Sub

#End Region

    End Class

End Class