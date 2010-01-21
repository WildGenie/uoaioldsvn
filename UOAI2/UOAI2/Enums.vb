Partial Class UOAI

    ''' <summary>A simple list of enumerations used primarily internally.</summary>
    Public Class Enums

        Public Enum LoginState
            AT_LOGIN_GUMP
            AT_CHARACTERLIST
            AT_SERVERLIST
            LOGIN_FAILURE
            LOGIN_SUCCESS
        End Enum

        Public Enum Macros
            'TODO: Enumerate macros
            EMPTY = 0
        End Enum

        Public Enum TargetKind
            ITEM = 0
            GROUND = 1
        End Enum

        Public Enum Spells
            'TODO: Enumberate spells
            EMPTY = 0
        End Enum

        Public Enum RegTypes
            'TODO: Enumberate Reg Types
            BLANK = 0
        End Enum

        Public Enum Facets
            'TODO: Enumerate Facets
            BLANK = 0
        End Enum

        Public Enum SkillLock
            Up
            Down
            Locked
        End Enum

        Public Enum EventTypes
            Any
            Packet
            DoubleClick
            SingleClick
            KeyUp
            KeyDown
            NewItem
            NewMobile
            ItemDeletion
            Drag
            Drop
            MobileUpdate
        End Enum

        Public Enum PacketOrigin
            FROMCLIENT
            FROMSERVER
            INTERNAL
        End Enum

        Public Enum PacketDestination
            SERVER
            CLIENT
        End Enum

        'Supported Clients
        Public Enum ClientVersions
            UOML
        End Enum

        'UOML Fonts
        Public Enum Fonts As UInteger
            BigFont = &H0
            ShadowFont = &H1
            BigShadowFont = &H2
            [Default] = &H3
            Gothic = &H4
            Italic = &H5
            SmallAndDark = &H6
            ColorFull = &H7
            Runes = &H8
            SmallAndLight = &H9
        End Enum

        'UOML Directions
        Public Enum Direction As Byte
            North = &H0
            NorthEast = &H1
            East = &H2
            SouthEast = &H3
            South = &H4
            SouthWest = &H5
            West = &H6
            NorthWest = &H7
        End Enum

        'UOML ItemFlags
        <Flags()> _
        Public Enum ItemFlags As UInteger
            Poisoned = &H4
            GoldenHealth = &H8
            WarMode = &H40
            Hidden = &H80
        End Enum

        Public Enum Reputation As UInteger
            Normal = &H0
            Innocent = &H1
            GuildMember = &H2
            Neutral = &H3
            Criminal = &H4
            Enemy = &H5
            Murderer = &H6
            Invulnerable = &H7
        End Enum

        Public Enum Layers As UInteger
            LeftHand = &H1
            RightHand = &H2
            Shoes = &H3
            Pants = &H4
            Shirt = &H5
            Head = &H6
            Gloves = &H7
            Ring = &H8
            Neck = &HA
            Hair = &HB
            Waist = &HC
            InnerTorso = &HD
            Bracelet = &HE
            FacialHair = &H10
            MiddleTorso = &H11
            Ears = &H12
            Arms = &H13
            Back = &H14
            BackPack = &H15
            OuterTorso = &H16
            OuterLegs = &H17
            InnerLegs = &H18
            Mount = &H19
            Bank = &H1D
        End Enum

        <Flags()> _
        Public Enum TypeFlags As UInteger
            Background = &H1
            Weapon = &H2
            Transparent = &H4
            Translucent = &H8
            Wall = &H10
            Damaging = &H20
            Impassable = &H40
            Wet = &H80
            Surface = &H200
            Bridge = &H400
            Stackable = &H800
            Window = &H1000
            NoShoot = &H2000
            PrefixA = &H4000
            PrevixAn = &H8000
            Internal = &H10000
            Foliage = &H20000
            PartiallyHued = &H40000
            Map = &H100000
            Container = &H200000
            Wearable = &H400000
            LightSource = &H800000
            Animated = &H1000000
            NoDiagonal = &H2000000
            Armor = &H8000000
            Roof = &H10000000
            Door = &H20000000
            StairBack = &H40000000
            'TODO: Fix this, should this be Integer, not UInteger?
            'StairRight = &H80000000
        End Enum

    End Class

End Class
