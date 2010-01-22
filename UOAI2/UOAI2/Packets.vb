Partial Class UOAI

    Public Class Packet
        Friend _Data() As Byte
        Friend _Type As Enums.PacketType

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
                _Type = Enums.PacketType.Speech
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
            Private _body As UInteger
            Private _Serial As UInteger
            Private TempBytes(3) As Byte
            Private buff As BufferHandler
            Private _lang As String
            Private _name As String

            Sub New(ByVal bytes() As Byte)
                buff = New BufferHandler(bytes, True)

                'Parse the data into the fields.
                _Data = bytes
                _Type = bytes(0)

                buff.Position = 3
                _Serial = buff.readuint
                _body = buff.readuint
                _Mode = buff.readbyte
                _hue = buff.readushort
                _font = buff.readushort
                _lang = buff.readstrn(4)
                _name = buff.readstrn(30)
                _text = buff.readustr
            End Sub

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

End Class