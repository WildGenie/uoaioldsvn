﻿Partial Class UOAI

    ''' <summary>A simple list of enumerations used primarily internally.</summary>
    Public Class Enums
        ''' <summary>Enumeration of Login state.</summary>
        Public Enum LoginState
            AT_LOGIN_GUMP
            AT_CHARACTERLIST
            AT_SERVERLIST
            LOGIN_FAILURE
            LOGIN_SUCCESS
        End Enum

        ''' <summary>Enumeration of Entity types.</summary>
        Public Enum EntityType
            Item
            Mobile
            ItemList
            Gump
            GumpList
            GumpElement
            GumpElementList
            Journal
            JournalEntry
            PacketQueue
            Packet
        End Enum

        ''' <summary>Enumeration of Spells.</summary>
        Public Enum Spell
            ' First circle
            Clumsy = 1
            CreateFood
            Feeblemind
            Heal
            MagicArrow
            NightSight
            ReactiveArmor
            Weaken

            ' Second circle                                    
            Agility
            Cunning
            Cure
            Harm
            MagicTrap
            RemoveTrap
            Protection
            Strength

            ' Third circle                                     
            Bless
            Fireball
            MagicLock
            Poison
            Telekinesis
            Teleport
            Unlock
            WallOfStone

            ' Fourth circle                                    
            ArchCure
            ArchProtection
            Curse
            FireField
            GreaterHeal
            Lightning
            ManaDrain
            Recall

            ' Fifth circle                                     
            BladeSpirits
            DispelField
            Incognito
            MagicReflect
            MindBlast
            Paralyze
            PoisonField
            SummonCreature

            ' Sixth circle                                     
            Dispel
            EnergyBolt
            Explosion
            Invisibility
            Mark
            MassCurse
            ParalyzeField
            Reveal

            ' Seventh circle                                   
            ChainLightning
            EnergyField
            FlameStrike
            GateTravel
            ManaVampire
            MassDispel
            MeteorSwarm
            Polymorph

            ' Eighth circle                                    
            Earthquake
            EnergyVortex
            Resurrection
            AirElemental
            SummonDaemon
            EarthElemental
            FireElemental
            WaterElemental

            'Necromancy
            AnimateDead = 101
            BloodOath
            CorpseSkin
            CurseWeapon
            EvilOmen
            HorrificBeast
            LichForm
            MindRot
            PainSpike
            PoisonStrike
            Strangle
            SummonFamiliar
            VampiricEmbrace
            VengefulSpirit
            Wither
            WraithForm
            Exorcism

            'Chevalry
            CleanseByFire = 201
            CloseWounds
            ConsecrateWeapon
            DispelEvil
            DivineFury
            EnemyOfOne
            HolyLight
            NobleSacrifice
            RemoveCurse
            SacredJourney

            'TODO: add ninjitsu, bushido and spellweaving
        End Enum

        ''' <summary>Enumeration of macro types.</summary>
        Public Enum Macros
            Say = 1
            Emote
            Whisper
            Yell
            Walk
            ToggleWarMode
            Paste
            Open
            Close
            Minimize
            Maximize
            OpenDoor
            UseSkill
            LastSkill
            CastSpell
            LastSpell
            Bow
            Salute
            QuitGame
            AllNames
            LastTarget
            TargetSelf
            ArmOrDisarm
            WaitForTarget
            TargetNext
            AttackLast
            Delay
            CircleTrans
            CloseAllGumps
            AlwaysRun
            SaveDesktop
            KillGumpOpen
            UsePrimaryAbility
            UseSecondaryAbility
            EquipLastWeapon
            SetUpdateRange
            ModifyUpdateRange
            IncreaseUpdateRange
            DecreaseUpdateRange
            MaxUpdateRange
            MinUpdateRange
            DefaultUpdateRange
            UpdateRangeInfo
            EnableRangeColor
            DisableRangeColor
            InvokeVirtue
        End Enum

        ''' <summary>Enumeration of element types.</summary>
        Public Enum Element
            Configuration
            Paperdoll
            Status
            Journal
            Skills
            MageSpellbook
            Chat
            Backpack
            Overview
            Mail
            PartyManifest
            PartyChat
            NecroSpellbook
            PaladinSpellbook
            CombatBook
            BushidoSpellbook
            NinjitsuSpellbook
            Guild
            SpellWeavingSpellbook
            QuestLog
        End Enum

        <Flags()> _
        Public Enum Features As Short
            enableT2Afeatures_chatbutton_regions = &H1
            enablerenaissancefeatures = &H2
            enablethirddownfeatures = &H4
            enableLBRfeatures_skills_map = &H8
            enableAOSfeatures_skills_spells_map_fightbook = &H10
            enable6thcharacterslot = &H20
            enableSEfeatures_spells_skills_map = &H40
            enableMLfeatures_elvenrace_spells_skills = &H80
            enableTheEightAgesplashscreen = &H100
            enableTheNinthAgesplashscreen = &H200
            enable7thcharacterslot = &H1000
            enableTheTenthAgeKRfaces = &H2000
        End Enum

        ''' <summary>Enumeration of mobile gender.</summary>
        Public Enum Gender As Byte
            Male = &H0
            Female = &H1
            Neutral = &H2
        End Enum

        ''' <summary>Enumeration of mobile status.</summary>
        Public Enum MobileStatus
            Normal = &H0
            Unknown = &H1
            CanAlterPaperdoll = &H2
            Poisoned = &H4
            GoldenHealth = &H8
            Unknown2 = &H10
            Unknown3 = &H20
            WarMode = &H40
            Hidden = &H80
        End Enum

        ''' <summary>Enumeration of the different facets.</summary>
        Public Enum Facets
            Felucca
            Trammel
            Ilshenar
            Malas
            Tokuno
            Internal = &H7F
        End Enum

        Public Enum SkillLock
            Up
            Down
            Locked
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
            NorthRunning = &H80
            NorthEastRunning = &H81
            EastRunning = &H82
            SouthEastRunning = &H83
            SouthRunning = &H84
            SouthWestRunning = &H85
            WestRunning = &H86
            NorthWestRunning = &H87
        End Enum

        Public Enum WarMode
            Disabled
            Enabled
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

        Public Enum Layers As Byte
            None = &H0
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
            StairRight = 4294967295
        End Enum

        Public Enum Skills
            Alchemy = 1
            Anatomy = 2
            AnimalLore = 3
            ItemIdentification = 4
            ArmsLore = 5
            Parrying = 6
            Begging = 7
            Blacksmithy = 8
            BowcraftFletching = 9
            Peacemaking = 10
            Camping = 11
            Carpentry = 12
            Cartography = 13
            Cooking = 14
            DetectingHidden = 15
            Discordance = 16
            EvaluatingIntelligence = 17
            Healing = 18
            Fishing = 19
            ForensicEvaluation = 20
            Herding = 21
            Hiding = 22
            Provocation = 23
            Inscription = 24
            Lockpicking = 25
            Magery = 26
            ResistingSpells = 27
            Tactics = 28
            Snooping = 29
            Musicianship = 30
            Poisoning = 31
            Archery = 32
            SpiritSpeak = 33
            Stealing = 34
            Tailoring = 35
            AnimalTaming = 36
            TasteIdentification = 37
            Tinkering = 38
            Tracking = 39
            Veterinary = 40
            Swordsmanship = 41
            MaceFighting = 42
            Fencing = 43
            Wrestling = 44
            Lumberjacking = 45
            Mining = 46
            Meditation = 47
            Stealth = 48
            RemoveTrap = 49
            Necromancy = 50
            Focus = 51
            Chivalry = 52
            Bushido = 53
            Ninjitsu = 54
            Spellweaving = 55
        End Enum

        Public Enum Virtues
            Honor
            Sacrifice
            Valor
        End Enum

        Public Enum SpeechTypes
            Regular = &H0
            Broadcast = &H1
            Emote = &H2
            System = &H6
            Whisper = &H8
            Yell = &H9
        End Enum

        Public Enum EventTypeConstants As UInt32
            received_packet = 0
            sent_packet = 1
            key_down = 2
            key_up = 3
            connection_loss = 8
            packet_handled = 16
            object_destroyed = 32
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

        Public Enum CommonHues
            BlueDark = 3
            Blue = 99
            BlueLight = 101

            RedDark = 32
            Red = 33
            RedLight = 35

            YellowDark = 52
            Yellow = 53
            YellowLight = 55

            GreenDark = 67
            Green = 73
            GreenLight = 70

            VioletDark = 17
            Violet = 18
            VioletLight = 21

            OrangeDark = 42
            Orange = 43
            OrangeLight = 45

            AquaDark = 82
            Aqua = 83
            AquaLight = 85
        End Enum

    End Class

End Class
