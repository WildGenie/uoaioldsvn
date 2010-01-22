Partial Class UOAI
    Friend Class Interfaces
        'Client interface
        Friend Interface Client
            ReadOnly Property Version() As Enums.ClientVersions
            ReadOnly Property Items() As ItemCollection
            ReadOnly Property Mobiles() As ItemCollection
            ReadOnly Property Player() As PlayerMobile
            Private _LastDraggedItem as Item
            Property LastDraggedItem() As Item
            ReadOnly Property Journal() As Journal
            ReadOnly Property LoggedIn() As Boolean
            Private _LastSkill As Skill
            Property LastSkill() As Skill
            Private _LastSpell As Spell
            Property LastSpell() As Spell
            Private _LastObject as Item
            Property LastObject() As Item
            Private _LastTargetKind As Enums.TargetKind
            Property LastTargetKind() As Enums.TargetKind
            Private _LastTarget as Item
            Property LastTarget() As Item
            Private _LastTargetLocation As Point3D
            Property LastTargetLocation() As Point3D
            Private _Targeting As Boolean
            Property Targeting() As Boolean
            ReadOnly Property PlayerName() As String
            ReadOnly Property ServerName() As String
            'IPAddress ServerAddress { get; }
            ReadOnly Property Facet() As Facets
            ReadOnly Property Statics() As FlexMap(Of TypeInfo)
            ReadOnly Property LandTiles() As FlexMap(Of LandTile)
            Function Login(ByVal host As IPAddress, ByVal port As UShort, ByVal username As String, ByVal password As String) As LoginResponse
            Sub ShowMessage(ByVal message As String)
            Sub ShowMessage(ByVal font As Fonts, ByVal message As String)
            Sub ShowMessage(ByVal font As Fonts, ByVal color As UInteger, ByVal message As String)
            Function Drag(ByVal todrag As Item, ByVal Amount As UShort) As Boolean
            Function Drop(ByVal location As Point3D) As Boolean
            Function Drop(ByVal Container As Item) As Boolean
            Function Drop(ByVal Container As Item, ByVal x As UShort, ByVal y As UShort) As Boolean
            Sub ExecuteMacro(ByVal macronumber As Macros, ByVal uint_parameter As UInteger, ByVal string_parameter As String)
            Function FindItem(ByVal serial As Serial) As ItemCollection
            Function GetItemTarget() As Item
            Function GetGroundTarget() As Point3D
            Function CreateEventQueue() As EventQueue
            Function CreateEventQueue(ByVal timeout As TimeSpan) As EventQueue
            Function CreateEventQueue(ByVal filters As List(Of EventDesc)) As EventQueue
            Function CreateEventQueue(ByVal timeout As TimeSpan, ByVal filters As List(Of EventDesc)) As EventQueue

            Function Send(ByVal tosend As Packet, ByVal destination As PacketDestination) As Boolean

            'client events
            Event onPacketReceive As HandlePacketDelegate
            Event onPacketSend As HandlePacketDelegate
            Event onKeyUp As HandleKeyDelegate
            Event onKeyDown As HandleKeyDelegate
            Event onNewItem As HandleItemDelegate
            Event onItemDeletion As HandleItemDelegate
            Event onNewMobile As HandleMobileDelegate
            Event onMobileUpdate As HandleMobileDelegate
        End Interface

        'Item interface
        Friend Interface Item
            ReadOnly Property ID() As Serial
Private _Type As Serial
            Property Type() As Serial
            ReadOnly Property Kind() As UInteger
Private _Position As Point3D
            Property Position() As Point3D
            ReadOnly Property Distance() As Double
Private _StackCount As UShort
            Property StackCount() As UShort
Private _Color As UShort
            Property Color() As UShort
Private _HighlightColor As UShort
            Property HighlightColor() As UShort
Private _Direction As Direction
            Property Direction() As Direction
Private _Container as Item
            Property Container() As Item
            ReadOnly Property Gump() As Gump
            ReadOnly Property Contents() As ItemCollection
Private _Flags as ItemFlags
            Property Flags() As ItemFlags

Private _Visible As Boolean
            Property Visible() As Boolean

            Sub ShowMessage(ByVal message As String)
            Sub ShowMessage(ByVal font As Fonts, ByVal message As String)
            Sub ShowMessage(ByVal font As Fonts, ByVal color As UInteger, ByVal message As String)
            Sub Click()
            Sub DoubleClick()
            Sub Target()
            Function Equip(ByVal layer As Enums.Layers) As Boolean
            Function Drag() As Boolean
            Function Drag(ByVal Amount As UInteger) As Boolean
            Function DragTo(ByVal container As Item) As Boolean
            Function DragTo(ByVal container As Item, ByVal Amount As UInteger) As Boolean
            Function DragTo(ByVal container As Item, ByVal Amount As UInteger, ByVal x As UShort, ByVal y As UShort) As Boolean
            Function DragTo(ByVal location As Point3D) As Boolean
            Function DragTo(ByVal location As Point3D, ByVal Amount As UInteger) As Boolean

            'Item Events
            Event OnDelete As SimpleDelegate
            Event OnClick As SimpleDelegate
            Event OnDoubleClick As SimpleDelegate
            Event OnDrag As HandleDragDelegate
        End Interface

        Friend Interface StatusInfo
Private _Name As String
            Property Name() As String
            ReadOnly Property Hits() As UShort
            ReadOnly Property MaxHits() As UShort
            ReadOnly Property Renameable() As Boolean
            ReadOnly Property Female() As Boolean
            ReadOnly Property Strength() As UShort
            ReadOnly Property Dexterity() As UShort
            ReadOnly Property Intelligence() As UShort
            ReadOnly Property Stamina() As UShort
            ReadOnly Property MaxStamina() As UShort
            ReadOnly Property Mana() As UShort
            ReadOnly Property MaxMana() As UShort
            ReadOnly Property GoldCount() As UInteger
            ReadOnly Property AR() As UShort
            ReadOnly Property Weight() As UShort
            ReadOnly Property MaxWeight() As UShort
            ReadOnly Property StatCap() As UShort
            ReadOnly Property FollowerCount() As Byte
            ReadOnly Property MaxFollowerCount() As Byte
            ReadOnly Property FireResistance() As UShort
            ReadOnly Property ColdResistance() As UShort
            ReadOnly Property PoisonResistance() As UShort
            ReadOnly Property EnergyResistance() As UShort
            ReadOnly Property Luck() As UShort
            ReadOnly Property MinimumDamage() As UShort
            ReadOnly Property MaximumDamage() As UShort
            ReadOnly Property TitchingPoints() As UInteger
            ReadOnly Property Race() As Byte
        End Interface

        Friend Interface Mobile
            Inherits Item
            Function Layer(ByVal LayerEnum As Enums.Layers) As Item
            Default Property Item(ByVal layer As Enums.Layers) As Item
            ReadOnly Property StatusGump() As StatusGump
            ReadOnly Property StatusInfo() As StatusInfo
            ReadOnly Property Paperdoll() As PaperdollGump
            Sub OpenPaperdoll()
            Sub OpenStatus()
            Sub Attack()
            Sub Give(ByVal togive As Item)
            ReadOnly Property Reputation() As Enums.Reputation

            'Mobile Events
            Event OnUpdate As SimpleDelegate
        End Interface

        Friend Interface PlayerMobile
            Inherits Mobile
            'Party
            ReadOnly Property Party() As Party
            'Macros
            'Skills
            ReadOnly Property Skills() As SkillList
            'Spells
            ReadOnly Property Spells() As SpellList
            'Speech
            Sub Say(ByVal tosay As String)
            'Misc
Private _AlwaysRun As Boolean
            Property AlwaysRun() As Boolean
            Function PathfindTo(ByVal destination As Point3D) As Boolean
            Function PathfindTo(ByVal todisplay As String, ByVal destination As Point3D) As Boolean

            'player mobile events
            Event onPartyInvitation As HandlePartyInvitationDelegate
        End Interface

        Friend Interface Gump
            ReadOnly Property ID() As Serial
            ReadOnly Property Kind() As UInteger
Private _X As UInteger
            Property X() As UInteger
Private _Y As UInteger
            Property Y() As UInteger
            ReadOnly Property Width() As UInteger
            ReadOnly Property Height() As UInteger
            ReadOnly Property Closable() As Boolean
            Sub Close()
        End Interface

        Friend Interface StatusGump
            Inherits Gump
            Inherits StatusInfo
        End Interface

        Friend Interface PaperdollGump
            Inherits Gump
            ReadOnly Property Title() As String
        End Interface

        Friend Interface [Event]
            ReadOnly Property EventType() As EventTypes
        End Interface

        Friend Interface Packet
            Inherits [Event]
            ReadOnly Property CMD() As Byte
            ReadOnly Property Size() As UShort
            ReadOnly Property buffer() As Byte()
            ReadOnly Property Origin() As PacketOrigin
        End Interface

        Friend Interface EventQueue
Private _TimeOut As TimeSpan
            Property TimeOut() As TimeSpan
            Sub AddFilter(ByVal event_to_filter_for As EventDesc)
            Function HasEvent() As Boolean
            Function WaitForEvent(ByRef resultingevent As [Event]) As Boolean
            Sub Reset()
            Sub Clear()
        End Interface

        Friend Interface Journal
        End Interface

        Friend Interface Spell
            ReadOnly Property Name() As String
            'IDictionary<RegTypes,uint> RequiredRegs {get;}
            Sub Cast()
            'bool CheckRegs();
            ReadOnly Property SpellNumber() As UInteger
        End Interface

        Friend Interface Skill
            ReadOnly Property SkillNumber() As UInteger
            ReadOnly Property Name() As String
            ReadOnly Property Value() As UInteger
            ReadOnly Property RealValue() As UInteger
Private _Lock As SkillLock
            Property Lock() As SkillLock
            Sub Use()
            ReadOnly Property Usable() As Boolean
        End Interface

        Friend Interface SkillList
            Inherits IEnumerable(Of Skill)
            Default ReadOnly Property Item(ByVal skillnumber As UInteger) As Skill
            Function byName(ByVal skillname As String) As Skill
        End Interface

        Friend Interface SpellList
            Inherits IEnumerable(Of Spell)
            Default ReadOnly Property Item(ByVal spellnumber As Enums.Spells) As Spell
            Function byName(ByVal spellname As String) As Spell
        End Interface


Friend Interface LoginResponse
        ReadOnly Property State() As LoginState
End Interface

Friend Interface LandTile
            ReadOnly Property Flags() As TypeFlags
            ReadOnly Property Name() As String
            ReadOnly Property Type() As UShort
        End Interface

        Friend Interface TypeInfo
            ReadOnly Property Flags() As TypeFlags
            ReadOnly Property Name() As String
            ReadOnly Property Type() As UShort
            ReadOnly Property Weight() As Byte
            ReadOnly Property Height() As Byte
            ReadOnly Property HBitmap() As UInteger
            ReadOnly Property AnimationType() As UShort
        End Interface

        Friend Interface PartyInvitation
            ReadOnly Property partyleader() As Serial
            Sub Accept()
            Sub Decline()
        End Interface

        Friend Interface Party
            ReadOnly Property partyleader() As Serial
            ReadOnly Property Members() As List(Of Serial)
Private _PartyCanLootMe As Boolean
            Property PartyCanLootMe() As Boolean
            Sub SendInvitation(ByVal [to] As Serial)
            Sub RemovePartyMember(ByVal toremove As Serial)
            Sub Say(ByVal Message As String)
            Sub Say(ByVal [to] As Serial, ByVal message As String)
        End Interface

    End Class
End Class