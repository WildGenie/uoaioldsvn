Imports System.IO
Imports System.Text

Partial Class UOAI

    Public Class Packet
        Friend _Data() As Byte
        Friend _Type As Enums.PacketType

        Public Sub New(ByVal PType As Enums.PacketType)
            _Type = PType
        End Sub

        ''' <summary>Returns the raw packet data as a byte array.</summary>
        Public ReadOnly Property Data() As Byte()
            Get
                Return _Data
            End Get
        End Property

        Public ReadOnly Property Type() As Enums.PacketType
            Get
                Return _Type
            End Get
        End Property
    End Class

    Public Class Packets

        Public Class SpeechPacket
            Inherits UOAI.Packet
            Private _text As String

            Sub New()
                MyBase.New(Enums.PacketType.Speech)
            End Sub

            Public Property Text() As String
                Get
                    Return _text
                End Get
                Set(ByVal value As String)
                    _text = value
                End Set
            End Property

        End Class

        Public Class UnicodeTextPacket
            Inherits Packet
            Private _text As String
            Private _Mode As Enums.SpeechTypes
            Private _hue As UShort
            Private _font As Enums.Fonts
            Private _body As UShort
            Private _Serial As UInteger
            Private TempBytes(3) As Byte
            Private buff As BufferHandler
            Private _lang As String
            Private _name As String

            Sub New(ByVal bytes() As Byte)
                MyBase.New(Enums.PacketType.TextUnicode)

                Dim _size As UShort

                buff = New BufferHandler(bytes, True)

                'Parse the data into the fields.
                _Data = bytes
                _Type = bytes(0)

                buff.Position = 1
                buff.networkorder = False
                _size = buff.readushort()
                buff.networkorder = True
                _Serial = buff.readuint
                _body = buff.readushort
                _Mode = buff.readbyte
                _hue = buff.readushort
                _font = buff.readushort
                _lang = buff.readstrn(4)
                _name = buff.readstrn(30)
                'buff.networkorder = False
                _text = buff.readustr
                'buff.networkorder = True
            End Sub
            Public Property Text() As String
                Get
                    Return _text
                End Get
                Set(ByVal value As String)
                    'to be implemented
                End Set
            End Property

            Public Property Name() As String
                Get
                    Return _name
                End Get
                Set(ByVal value As String)
                    If Name.Length <= 30 Then
                        _name = value
                        'buff.Position = (wherever the name starts)
                        'make sure ti writes those extra blank characters to make it exactly 30.
                    Else
                        Throw New ApplicationException("Name is too long, much be 30 characters or less.")
                    End If
                End Set
            End Property
        End Class


    End Class

    Partial Class Enums
        Public Enum PacketType
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
        Public Sub New(ByVal size As UInteger)
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
        Public Sub New(ByVal size As UInteger, ByVal bNetworkOrder As Boolean)
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

#Region "Integer/UInteger"
        Public Function readuint() As UInteger
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

        Public Function ReadUChar() As Short
            If Length > 0 Then
                curpos = curpos + 1
                Return CSByte(m_buffer(curpos - 1))
            Else
                Return 0
            End If
        End Function

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

            While (readbyte() <> 0) AndAlso (Length > 0)
                count += 1
            End While

            curpos = prevpos

            Return readstrn(count)
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

            While (readushort() <> 0) AndAlso (Length > 0)
                count += 1
            End While

            curpos = prevpos

            Return readustrn(count)
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

        Public Sub writeuint(ByVal towrite As UInteger)
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