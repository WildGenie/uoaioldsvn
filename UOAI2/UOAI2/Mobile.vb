﻿#Const DebugMobiles = False

Partial Class UOAI

    Public Class Mobile
        Inherits Item
        Friend _Layers As LayersClass

        Friend Sub New(ByVal Client As Client, ByVal Serial As Serial)
            MyBase.New(Client, Serial)
            _Layers = New LayersClass(Client)
            _Client = Client
            _Serial = Serial
            _IsMobile = True
            _Container = WorldSerial
        End Sub

#Region "Private Variables"
        Friend _Client As Client
        Friend _Name As String = ""
        Friend _Status As Enums.MobileStatus
        Friend _Notoriety As Enums.Reputation
        Friend _Hits As UShort = 1
        Friend _HitsMax As UShort = 1
        Friend _Renamable As Enums.Renamable = Enums.Renamable.NotRenamable
        Friend _DisplayMode As Enums.DisplayMode = Enums.DisplayMode.Normal
        Friend _Gender As Enums.Gender = Enums.Gender.Neutral
        Friend _Strength As UShort = 1
        Friend _Dexterity As UShort = 1
        Friend _Intelligence As UShort = 1
        Friend _Stamina As UShort = 1
        Friend _StaminaMax As UShort = 1
        Friend _Mana As UShort = 1
        Friend _ManaMax As UShort = 1
        Friend _Gold As UInt32 = 0
        Friend _ResistPhysical As UShort = 0
        Friend _Weight As UShort = 0
        Friend _PoisonLevel As Byte = 0

        'Included in Mobile Stat Packet if SF 0x03
        Friend _StatCap As UShort = 1
        Friend _Followers As Byte = 0
        Friend _FollowersMax As Byte = 5

        'Included in Mobile Stat Packet if SF 0x04
        Friend _ResistFire As UShort = 0
        Friend _ResistCold As UShort = 0
        Friend _ResistPoison As UShort = 0
        Friend _ResistEnergy As UShort = 0
        Friend _Luck As UShort = 0
        Friend _DamageMin As UShort = 0
        Friend _DamageMax As UShort = 0
        Friend _TithingPoints As UShort = 0

        'Included in Mobile Stat Packet if SF 0x05
        Friend _Race As Byte = 0
        Friend _WeightMax As UShort = 0

        'Included in Mobile Stat Packet if SF 0x06
        Friend _HitChanceIncrease As Short = 1
        Friend _SwingSpeedIncrease As Short = 1
        Friend _DamageChanceIncrease As Short = 1
        Friend _LowerReagentCost As Short = 1
        Friend _HitPointsRegeneration As Short = 1
        Friend _StaminaRegeneration As Short = 1
        Friend _ManaRegeneration As Short = 1
        Friend _ReflectPhysicalDamage As Short = 1
        Friend _EnhancePotions As Short = 1
        Friend _DefenseChanceIncrease As Short = 1
        Friend _SpellDamageIncrease As Short = 1
        Friend _FasterCastRecovery As Short = 1
        Friend _FasterCasting As Short = 1
        Friend _LowerManaCost As Short = 1
        Friend _StrengthIncrease As Short = 1
        Friend _DexterityIncrease As Short = 1
        Friend _IntelligenceIncrease As Short = 1
        Friend _HitPointsIncrease As Short = 1
        Friend _StaminaIncrease As Short = 1
        Friend _ManaIncrease As Short = 1
        Friend _MaximumHitPointsIncrease As Short = 1
        Friend _MaximumStaminaIncrease As Short = 1
        Friend _MaximumManaIncrease As Short = 1

#End Region

#Region "Public Events"

        ''' <summary>
        ''' Called when the hitpoint value of a mobile reaches zero (0).
        ''' </summary>
        ''' <param name="Client">The <see cref="UOAI.Client"/> that called this.</param>
        ''' <param name="Mobile">The <see cref="UOAI.Mobile"/> that has died.</param>
        Public Event onDeath(ByVal Client As Client, ByVal Mobile As Mobile, ByVal CorpseSerial As Serial)

        ''' <summary>
        ''' This is called immediately before the client handles an update to a mobile.
        ''' </summary>
        ''' <param name="Client">The <see cref="UOAI.Client"/> that the mobile update was handled by.</param>
        ''' <param name="Mobile">The <see cref="UOAI.Mobile"/> that was updated.</param>
        Public Event onUpdate(ByVal Client As Client, ByVal Mobile As Mobile, ByVal UpdateType As Enums.MobileUpdateType)

        ''' <summary>
        ''' This is called immediately after a packet is recieved to equip an item.
        ''' </summary>
        ''' <param name="Client">The <see cref="UOAI.Client"/> that the mobile update was handled by.</param>
        ''' <param name="Mobile">The <see cref="UOAI.Mobile"/> that was updated.</param>
        ''' <param name="EquipmentLayers">The <see cref="Enums.Layers">layers</see> that were changed.</param>
        Public Event onEquipmentUpdate(ByVal Client As Client, ByVal Mobile As Mobile, ByVal EquipmentLayers() As Enums.Layers)


        ''' <summary>
        ''' This is called immediately after the client handles an update to a mobile.
        ''' </summary>
        ''' <param name="Client">The <see cref="UOAI.Client"/> that the mobile update was handled by.</param>
        ''' <param name="Mobile">The <see cref="UOAI.Mobile"/> that was updated.</param>
        Public Event onStatusChange(ByVal Client As Client, ByVal Mobile As Mobile)

#End Region

#Region "Public Properties"
        ''' <summary>
        ''' Returns the item on the specified layer.
        ''' </summary>
        Public ReadOnly Property Layers() As LayersClass
            Get
                Return _Layers
            End Get
        End Property

        ''' <summary>
        ''' The level of poison that the mobile is aflicted with, 0 = no poison.
        ''' </summary>
        Public ReadOnly Property PoisonLevel() As Byte
            Get
                Return _PoisonLevel
            End Get
        End Property

        ''' <summary>
        ''' The character's serial.
        ''' </summary>
        Public Shadows ReadOnly Property Serial() As Serial
            Get
                Return _Serial
            End Get
        End Property

        ''' <summary>
        ''' The character's name.
        ''' </summary>
        Public ReadOnly Property Name() As String
            Get
                Return _Name
            End Get
        End Property

        ''' <summary>
        ''' The character's current amount of hit points.
        ''' </summary>
        Public ReadOnly Property Hits() As UShort
            Get
                Return _Hits
            End Get
        End Property

        ''' <summary>
        ''' The character's maximum amount of hit points.
        ''' </summary>
        Public ReadOnly Property HitsMax() As UShort
            Get
                Return _HitsMax
            End Get
        End Property

        ''' <summary>
        ''' Specifies whether or not the player can rename this mobile. Generaly only the player's pets are renamable.
        ''' This is a good way to tell if a mobile is a follower of the player. Although, It will be false for NPC followers like escorts.
        ''' </summary>
        Public ReadOnly Property Renamable() As Enums.Renamable
            Get
                Return _Renamable
            End Get
        End Property

        ''' <summary>
        ''' Specifies what of information is displayed by the client.
        ''' </summary>
        Public ReadOnly Property DisplayMode() As Enums.DisplayMode
            Get
                Return _DisplayMode
            End Get
        End Property

        ''' <summary>
        ''' The gender of the mobile.
        ''' </summary>
        Public ReadOnly Property Gender() As Enums.Gender
            Get
                Return _Gender
            End Get
        End Property

        ''' <summary>
        ''' The character's Strength.
        ''' </summary>
        Public ReadOnly Property Strength() As UShort
            Get
                Return _Strength
            End Get
        End Property

        ''' <summary>
        ''' The character's Dexterity.
        ''' </summary>
        Public ReadOnly Property Dexterity() As UShort
            Get
                Return _Dexterity
            End Get
        End Property

        ''' <summary>
        ''' The character's Intelligence.
        ''' </summary>
        Public ReadOnly Property Intelligence() As UShort
            Get
                Return _Intelligence
            End Get
        End Property

        ''' <summary>
        ''' The character's current Stamina.
        ''' </summary>
        Public ReadOnly Property Stamina() As UShort
            Get
                Return _Stamina
            End Get
        End Property

        ''' <summary>
        ''' The character's maximum Stamina.
        ''' </summary>
        Public ReadOnly Property StaminaMax() As UShort
            Get
                Return _StaminaMax
            End Get
        End Property

        ''' <summary>
        ''' The character's current Mana.
        ''' </summary>
        Public ReadOnly Property Mana() As UShort
            Get
                Return _Mana
            End Get
        End Property

        ''' <summary>
        ''' The character's maximum Mana.
        ''' </summary>
        Public ReadOnly Property ManaMax() As UShort
            Get
                Return _ManaMax
            End Get
        End Property

        ''' <summary>
        ''' How much gold the character is currently carrying.
        ''' </summary>
        Public ReadOnly Property Gold() As UInt32
            Get
                Return _Gold
            End Get
        End Property

        ''' <summary>
        ''' The character's physical resistance value (old clients: AC).
        ''' </summary>
        Public ReadOnly Property ResistPhysical() As UShort
            Get
                Return _ResistPhysical
            End Get
        End Property

        ''' <summary>
        ''' The character's current weight value.
        ''' </summary>
        Public ReadOnly Property Weight() As UShort
            Get
                Return _Weight
            End Get
        End Property

        ''' <summary>
        ''' The character's total allowable sum of Strength, Intelligence, and Dexterity.
        ''' </summary>
        Public ReadOnly Property StatCap() As UShort
            Get
                Return _StatCap
            End Get
        End Property

        ''' <summary>
        ''' The number of "Follower Slots" that are currently being used.
        ''' </summary>
        Public ReadOnly Property Followers() As Byte
            Get
                Return _Followers
            End Get
        End Property

        ''' <summary>
        ''' The maximum number of "Follower Slots" the character has available.
        ''' </summary>
        Public ReadOnly Property FollowersMax() As Byte
            Get
                Return _FollowersMax
            End Get
        End Property

        ''' <summary>
        ''' The character's resistance to fire.
        ''' </summary>
        Public ReadOnly Property ResistFire() As UShort
            Get
                Return _ResistFire
            End Get
        End Property

        ''' <summary>
        ''' The character's resistance to cold.
        ''' </summary>
        Public ReadOnly Property ResistCold() As UShort
            Get
                Return _ResistCold
            End Get
        End Property

        ''' <summary>
        ''' The character's resistance to poison.
        ''' </summary>
        Public ReadOnly Property ResistPoison() As UShort
            Get
                Return _ResistPoison
            End Get
        End Property

        ''' <summary>
        ''' The character's resistance to energy.
        ''' </summary>
        Public ReadOnly Property ResistEnergy() As UShort
            Get
                Return _ResistEnergy
            End Get
        End Property

        ''' <summary>
        ''' The character's Luck value.
        ''' </summary>
        Public ReadOnly Property Luck() As UShort
            Get
                Return _Luck
            End Get
        End Property

        ''' <summary>
        ''' The minimum amount of damage the character can deal.
        ''' </summary>
        Public ReadOnly Property DamangeMin() As UShort
            Get
                Return _DamageMin
            End Get
        End Property

        ''' <summary>
        ''' The maximum amount of damage the character can deal.
        ''' </summary>
        Public ReadOnly Property DamageMax() As UShort
            Get
                Return _DamageMax
            End Get
        End Property

        ''' <summary>
        ''' The character's current amount of Tithing points.
        ''' </summary>
        Public ReadOnly Property TithingPoints() As UShort
            Get
                Return _TithingPoints
            End Get
        End Property

        Public ReadOnly Property Notoriety() As Enums.Reputation
            Get
                Return _Notoriety
            End Get
        End Property

        Public ReadOnly Property Status() As Enums.MobileStatus
            Get
                Return _Status
            End Get
        End Property

        Public ReadOnly Property Direction() As Enums.Direction
            Get
                Return _Direction
            End Get
        End Property

        Public ReadOnly Property IsMounted() As Boolean
            Get
                If Me.Layers._Mount.Value <> 0 Then Return True
                Return False
            End Get
        End Property

        'Hide this class from the user, there is no reason from him/her to see it.
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Class LayersClass
            Friend Sub New(ByVal Client As Client)
                _Client = Client
            End Sub

            Friend Sub SetLayer(ByVal Layer As Enums.Layers, ByVal Serial As Serial)
                Select Case Layer
                    Case Enums.Layers.LeftHand
                        _LeftHand = Serial
                    Case Enums.Layers.RightHand
                        _RightHand = Serial
                    Case Enums.Layers.Shoes
                        _Shoes = Serial
                    Case Enums.Layers.Pants
                        _Pants = Serial
                    Case Enums.Layers.Shirt
                        _Shirt = Serial
                    Case Enums.Layers.Head
                        _Head = Serial
                    Case Enums.Layers.Gloves
                        _Gloves = Serial
                    Case Enums.Layers.Ring
                        _Ring = Serial
                    Case Enums.Layers.Neck
                        _Neck = Serial
                    Case Enums.Layers.Hair
                        _Hair = Serial
                    Case Enums.Layers.Waist
                        _Waist = Serial
                    Case Enums.Layers.InnerTorso
                        _InnerTorso = Serial
                    Case Enums.Layers.Bracelet
                        _Bracelet = Serial
                    Case Enums.Layers.FacialHair
                        _FacialHair = Serial
                    Case Enums.Layers.MiddleTorso
                        _MiddleTorso = Serial
                    Case Enums.Layers.Ears
                        _Ears = Serial
                    Case Enums.Layers.Arms
                        _Arms = Serial
                    Case Enums.Layers.Back
                        _Back = Serial
                    Case Enums.Layers.BackPack
                        _Back = Serial
                    Case Enums.Layers.OuterTorso
                        _OuterTorso = Serial
                    Case Enums.Layers.OuterLegs
                        _OuterLegs = Serial
                    Case Enums.Layers.InnerLegs
                        _InnerLegs = Serial
                    Case Enums.Layers.Mount
                        _Mount = Serial
                    Case Enums.Layers.Bank
                        _Bank = Serial
                End Select
            End Sub

            Friend Sub ResetLayer(ByVal Layer As Enums.Layers)
                Select Case Layer
                    Case Enums.Layers.LeftHand
                        _LeftHand = New Serial(0)
                    Case Enums.Layers.RightHand
                        _RightHand = New Serial(0)
                    Case Enums.Layers.Shoes
                        _Shoes = New Serial(0)
                    Case Enums.Layers.Pants
                        _Pants = New Serial(0)
                    Case Enums.Layers.Shirt
                        _Shirt = New Serial(0)
                    Case Enums.Layers.Head
                        _Head = New Serial(0)
                    Case Enums.Layers.Gloves
                        _Gloves = New Serial(0)
                    Case Enums.Layers.Ring
                        _Ring = New Serial(0)
                    Case Enums.Layers.Neck
                        _Neck = New Serial(0)
                    Case Enums.Layers.Hair
                        _Hair = New Serial(0)
                    Case Enums.Layers.Waist
                        _Waist = New Serial(0)
                    Case Enums.Layers.InnerTorso
                        _InnerTorso = New Serial(0)
                    Case Enums.Layers.Bracelet
                        _Bracelet = New Serial(0)
                    Case Enums.Layers.FacialHair
                        _FacialHair = New Serial(0)
                    Case Enums.Layers.MiddleTorso
                        _MiddleTorso = New Serial(0)
                    Case Enums.Layers.Ears
                        _Ears = New Serial(0)
                    Case Enums.Layers.Arms
                        _Arms = New Serial(0)
                    Case Enums.Layers.Back
                        _Back = New Serial(0)
                    Case Enums.Layers.BackPack
                        _Back = New Serial(0)
                    Case Enums.Layers.OuterTorso
                        _OuterTorso = New Serial(0)
                    Case Enums.Layers.OuterLegs
                        _OuterLegs = New Serial(0)
                    Case Enums.Layers.InnerLegs
                        _InnerLegs = New Serial(0)
                    Case Enums.Layers.Mount
                        _Mount = New Serial(0)
                    Case Enums.Layers.Bank
                        _Bank = New Serial(0)
                End Select
            End Sub

            Private _Client As Client
            Friend _LeftHand As New Serial(0)
            Friend _RightHand As New Serial(0)
            Friend _Shoes As New Serial(0)
            Friend _Pants As New Serial(0)
            Friend _Shirt As New Serial(0)
            Friend _Head As New Serial(0)
            Friend _Gloves As New Serial(0)
            Friend _Ring As New Serial(0)
            Friend _Neck As New Serial(0)
            Friend _Hair As New Serial(0)
            Friend _Waist As New Serial(0)
            Friend _InnerTorso As New Serial(0)
            Friend _Bracelet As New Serial(0)
            Friend _FacialHair As New Serial(0)
            Friend _MiddleTorso As New Serial(0)
            Friend _Ears As New Serial(0)
            Friend _Arms As New Serial(0)
            Friend _Back As New Serial(0)
            Friend _BackPack As New Serial(0)
            Friend _OuterTorso As New Serial(0)
            Friend _OuterLegs As New Serial(0)
            Friend _InnerLegs As New Serial(0)
            Friend _Mount As New Serial(0)
            Friend _Bank As New Serial(0)

            Public ReadOnly Property LeftHand() As Item
                Get
                    Return _Client._AllItems(_LeftHand)
                End Get
            End Property

            Public ReadOnly Property RightHand() As Item
                Get
                    Return _Client._AllItems(_RightHand)
                End Get
            End Property

            Public ReadOnly Property Shoes() As Item
                Get
                    Return _Client._AllItems(_Shoes)
                End Get
            End Property

            Public ReadOnly Property Pants() As Item
                Get
                    Return _Client._AllItems(_Pants)
                End Get
            End Property

            Public ReadOnly Property Shirt() As Item
                Get
                    Return _Client._AllItems(_Shirt)
                End Get
            End Property

            Public ReadOnly Property Head() As Item
                Get
                    Return _Client._AllItems(_Head)
                End Get
            End Property

            Public ReadOnly Property Gloves() As Item
                Get
                    Return _Client._AllItems(_Gloves)
                End Get
            End Property

            Public ReadOnly Property Ring() As Item
                Get
                    Return _Client._AllItems(_Ring)
                End Get
            End Property

            Public ReadOnly Property Neck() As Item
                Get
                    Return _Client._AllItems(_Neck)
                End Get
            End Property

            Public ReadOnly Property Hair() As Item
                Get
                    Return _Client._AllItems(_Hair)
                End Get
            End Property

            Public ReadOnly Property Waist() As Item
                Get
                    Return _Client._AllItems(_Waist)
                End Get
            End Property

            Public ReadOnly Property InnerTorso() As Item
                Get
                    Return _Client._AllItems(_InnerTorso)
                End Get
            End Property

            Public ReadOnly Property Bracelet() As Item
                Get
                    Return _Client._AllItems(_Bracelet)
                End Get
            End Property

            Public ReadOnly Property FacialHair() As Item
                Get
                    Return _Client._AllItems(_FacialHair)
                End Get
            End Property

            Public ReadOnly Property MiddleTorso() As Item
                Get
                    Return _Client._AllItems(_MiddleTorso)
                End Get
            End Property

            Public ReadOnly Property Ears() As Item
                Get
                    Return _Client._AllItems(_Ears)
                End Get
            End Property

            Public ReadOnly Property Arms() As Item
                Get
                    Return _Client._AllItems(_Arms)
                End Get
            End Property

            Public ReadOnly Property Back() As Item
                Get
                    Return _Client._AllItems(_Back)
                End Get
            End Property

            Public ReadOnly Property BackPack() As Item
                Get
                    Return _Client._AllItems(_BackPack)
                End Get
            End Property

            Public ReadOnly Property OuterTorso() As Item
                Get
                    Return _Client._AllItems(_OuterTorso)
                End Get
            End Property

            Public ReadOnly Property OuterLegs() As Item
                Get
                    Return _Client._AllItems(_OuterLegs)
                End Get
            End Property

            Public ReadOnly Property InnerLegs() As Item
                Get
                    Return _Client._AllItems(_InnerLegs)
                End Get
            End Property

            Public ReadOnly Property Mount() As Item
                Get
                    Return _Client._AllItems(_Mount)
                End Get
            End Property

            Public ReadOnly Property Bank() As Item
                Get
                    Return _Client._AllItems(_Bank)
                End Get
            End Property

        End Class

#End Region

#Region "Public Functions"
        Public Function Rename(ByVal NewName As String) As Boolean
            If Me.Renamable = Enums.Renamable.Renamable Then
                Dim buffer(35) As Byte
                Dim j As New Packets.RenameMOB(buffer)

                If NewName.Length <= 30 Then
                    j.Name = NewName
                    j.Serial = _Serial
                    'TODO: Add send packet to the server
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Public Sub AddItemToLayer(ByVal Layer As Enums.Layers, ByVal Serial As Serial)
            '
            _Layers.SetLayer(Layer, Serial)

        End Sub

        Public Sub RemoveItemFromLayer(ByVal Layer As Enums.Layers)
            'Set the layer's item serial to 0
            _Layers.ResetLayer(Layer)

            RaiseEvent onUpdate(_Client, Me, Layer)
        End Sub

        ''' <summary>
        ''' Updates the class given a mobile related packet.
        ''' </summary>
        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.MobileStats)
            Select Case Packet.DisplayMode
                Case 0, 1, 2
                    _Name = Packet.Name
                    _Hits = Packet.Hits
                    _HitsMax = Packet.HitsMax
                    _Renamable = Packet.Renamable
                    _DisplayMode = Packet.DisplayMode
                    _Gender = Packet.Gender
                    _Strength = Packet.Strength
                    _Dexterity = Packet.Dexterity
                    _Intelligence = Packet.Intelligence
                    _Stamina = Packet.Stamina
                    _StaminaMax = Packet.StaminaMax
                    _Mana = Packet.Mana
                    _ManaMax = Packet.ManaMax
                    _Gold = Packet.Gold
                    _ResistPhysical = Packet.ResistPhysical
                    _Weight = Packet.Weight
                Case 3
                    _Name = Packet.Name
                    _Hits = Packet.Hits
                    _HitsMax = Packet.HitsMax
                    _Renamable = Packet.Renamable
                    _DisplayMode = Packet.DisplayMode
                    _Gender = Packet.Gender
                    _Strength = Packet.Strength
                    _Dexterity = Packet.Dexterity
                    _Intelligence = Packet.Intelligence
                    _Stamina = Packet.Stamina
                    _StaminaMax = Packet.StaminaMax
                    _Mana = Packet.Mana
                    _ManaMax = Packet.ManaMax
                    _Gold = Packet.Gold
                    _ResistPhysical = Packet.ResistPhysical
                    _Weight = Packet.Weight
                    _StatCap = Packet.StatCap
                    _Followers = Packet.Followers
                    _FollowersMax = Packet.FollowersMax

                Case 4
                    _Name = Packet.Name
                    _Hits = Packet.Hits
                    _HitsMax = Packet.HitsMax
                    _Renamable = Packet.Renamable
                    _DisplayMode = Packet.DisplayMode
                    _Gender = Packet.Gender
                    _Strength = Packet.Strength
                    _Dexterity = Packet.Dexterity
                    _Intelligence = Packet.Intelligence
                    _Stamina = Packet.Stamina
                    _StaminaMax = Packet.StaminaMax
                    _Mana = Packet.Mana
                    _ManaMax = Packet.ManaMax
                    _Gold = Packet.Gold
                    _ResistPhysical = Packet.ResistPhysical
                    _Weight = Packet.Weight
                    _StatCap = Packet.StatCap
                    _Followers = Packet.Followers
                    _FollowersMax = Packet.FollowersMax
                    _ResistFire = Packet.ResistFire
                    _ResistCold = Packet.ResistCold
                    _ResistPoison = Packet.ResistPoison
                    _ResistEnergy = Packet.ResistEnergy
                    _Luck = Packet.Luck
                    _DamageMin = Packet.DamageMin
                    _DamageMax = Packet.DamageMax
                    _TithingPoints = Packet.TithingPoints

                Case 5
                    _Name = Packet.Name
                    _Hits = Packet.Hits
                    _HitsMax = Packet.HitsMax
                    _Renamable = Packet.Renamable
                    _DisplayMode = Packet.DisplayMode
                    _Gender = Packet.Gender
                    _Strength = Packet.Strength
                    _Dexterity = Packet.Dexterity
                    _Intelligence = Packet.Intelligence
                    _Stamina = Packet.Stamina
                    _StaminaMax = Packet.StaminaMax
                    _Mana = Packet.Mana
                    _ManaMax = Packet.ManaMax
                    _Gold = Packet.Gold
                    _ResistPhysical = Packet.ResistPhysical
                    _Weight = Packet.Weight
                    _StatCap = Packet.StatCap
                    _Followers = Packet.Followers
                    _FollowersMax = Packet.FollowersMax
                    _ResistFire = Packet.ResistFire
                    _ResistCold = Packet.ResistCold
                    _ResistPoison = Packet.ResistPoison
                    _ResistEnergy = Packet.ResistEnergy
                    _Luck = Packet.Luck
                    _DamageMin = Packet.DamageMin
                    _DamageMax = Packet.DamageMax
                    _TithingPoints = Packet.TithingPoints
                    _Race = Packet.Race
                    _WeightMax = Packet.WeightMax

                Case 6
                    _Name = Packet.Name
                    _Hits = Packet.Hits
                    _HitsMax = Packet.HitsMax
                    _Renamable = Packet.Renamable
                    _DisplayMode = Packet.DisplayMode
                    _Gender = Packet.Gender
                    _Strength = Packet.Strength
                    _Dexterity = Packet.Dexterity
                    _Intelligence = Packet.Intelligence
                    _Stamina = Packet.Stamina
                    _StaminaMax = Packet.StaminaMax
                    _Mana = Packet.Mana
                    _ManaMax = Packet.ManaMax
                    _Gold = Packet.Gold
                    _ResistPhysical = Packet.ResistPhysical
                    _Weight = Packet.Weight
                    _StatCap = Packet.StatCap
                    _Followers = Packet.Followers
                    _FollowersMax = Packet.FollowersMax
                    _ResistFire = Packet.ResistFire
                    _ResistCold = Packet.ResistCold
                    _ResistPoison = Packet.ResistPoison
                    _ResistEnergy = Packet.ResistEnergy
                    _Luck = Packet.Luck
                    _DamageMin = Packet.DamageMin
                    _DamageMax = Packet.DamageMax
                    _TithingPoints = Packet.TithingPoints
                    _Race = Packet.Race
                    _WeightMax = Packet.WeightMax
                    _HitChanceIncrease = Packet.HitChanceIncrease
                    _SwingSpeedIncrease = Packet.SwingSpeedIncrease
                    _DamageChanceIncrease = Packet.DamageChanceIncrease
                    _LowerReagentCost = Packet.LowerReagentCost
                    _HitPointsRegeneration = Packet.HitPointsRegeneration
                    _StaminaRegeneration = Packet.StaminaRegeneration
                    _ManaRegeneration = Packet.ManaRegeneration
                    _ReflectPhysicalDamage = Packet.ReflectPhysicalDamage
                    _EnhancePotions = Packet.EnhancePotions
                    _DefenseChanceIncrease = Packet.DefenseChanceIncrease
                    _SpellDamageIncrease = Packet.SpellDamageIncrease
                    _FasterCastRecovery = Packet.FasterCastRecovery
                    _FasterCasting = Packet.FasterCasting
                    _LowerManaCost = Packet.LowerManaCost
                    _StrengthIncrease = Packet.StrengthIncrease
                    _DexterityIncrease = Packet.DexterityIncrease
                    _IntelligenceIncrease = Packet.IntelligenceIncrease
                    _HitPointsIncrease = Packet.HitPointsIncrease
                    _StaminaIncrease = Packet.StaminaIncrease
                    _ManaIncrease = Packet.ManaIncrease
                    _MaximumHitPointsIncrease = Packet.MaximumHitPointsIncrease
                    _MaximumStaminaIncrease = Packet.MaximumStaminaIncrease
                    _MaximumManaIncrease = Packet.MaximumManaIncrease

            End Select

#If DebugMobiles Then
            Console.WriteLine("-Updated Mobile Status: " & Packet.Serial.ToString)
#End If

        End Sub

        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.HPHealth)
            _Hits = Packet.Hits
            _HitsMax = Packet.HitsMax

#If DebugMobiles Then
            Console.WriteLine("-Updated Mobile Hitpoints: " & Packet.Serial.ToString)
#End If
            RaiseEvent onUpdate(_Client, Me, Enums.MobileUpdateType.Health)
        End Sub

        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.ManaHealth)
            _Mana = Packet.Mana
            _ManaMax = Packet.ManaMax
#If DebugMobiles Then
            Console.WriteLine("-Updated Mobile Mana: " & Packet.Serial.ToString)
#End If
            RaiseEvent onUpdate(_Client, Me, Enums.MobileUpdateType.Mana)
        End Sub

        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.FatHealth)
            _Stamina = Packet.Stam
            _StaminaMax = Packet.StamMax
#If DebugMobiles Then
            Console.WriteLine("-Updated Mobile Stamina: " & Packet.Serial.ToString)
#End If
            RaiseEvent onUpdate(_Client, Me, Enums.MobileUpdateType.Stamina)
        End Sub

        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.EquipItem)
            'Create a new item on the client that this mobile belongs to.
            Dim NewItem As New Item(_Client, Packet.Serial)

            'Assign the item's properties based on the packet info
            NewItem._Type = Packet.ItemType
            NewItem._Container = WorldSerial
            NewItem._Layer = Packet.Layer
            NewItem._Hue = Packet.Hue

#If DebugMobiles Then
            Console.WriteLine("-HandleUpdatePacket(ByVal Packet As Packets.EquipItem)")
            Console.WriteLine(" Adding Item as Mobile Equipment:")
            Console.WriteLine(" Mobile:" & Me.Serial.ToString)
            Console.WriteLine(" Item:" & NewItem.Serial.ToString)
            Console.WriteLine(" Layer:" & NewItem.Layer)
#End If

            'Add the item to the itemlist.
            _Client.Items.Add(NewItem)

            'Assign the item to the proper layer on this mobile.
            Layers.SetLayer(Packet.Layer, Packet.Serial)

            RaiseEvent onUpdate(_Client, Me, Packet.Layer)
        End Sub

        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.EquippedMobile)
            Me._Type = Packet.BodyType
            Me._X = Packet.X
            Me._Y = Packet.Y
            Me._Z = Packet.Z
            Me._Direction = Packet.Direction
            Me._Hue = Packet.Hue
            Me._Notoriety = Packet.Notoriety
            Me._Status = Packet.Status

            If Packet.Count >= 1 Then
                Dim k(Packet.EquippedItems.Count - 1) As Enums.Layers

                'Loop through the items and add their serials to the proper layers for later reference
                For i As Byte = 0 To Packet.Count - 1
                    'TODO: Fix ItemList Enumeration.
                    Select Case Packet.EquippedItems(i).Layer
                        Case Enums.Layers.Arms
                            Me._Layers.SetLayer(Enums.Layers.Arms, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Arms

                        Case Enums.Layers.Back
                            Me._Layers.SetLayer(Enums.Layers.Back, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Back

                        Case Enums.Layers.BackPack
                            Me._Layers.SetLayer(Enums.Layers.BackPack, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.BackPack

                        Case Enums.Layers.Bank
                            Me._Layers.SetLayer(Enums.Layers.Bank, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Bank

                        Case Enums.Layers.Bracelet
                            Me._Layers.SetLayer(Enums.Layers.Bracelet, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Bracelet

                        Case Enums.Layers.Ears
                            Me._Layers.SetLayer(Enums.Layers.Ears, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Ears

                        Case Enums.Layers.FacialHair
                            Me._Layers.SetLayer(Enums.Layers.FacialHair, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.FacialHair

                        Case Enums.Layers.Gloves
                            Me._Layers.SetLayer(Enums.Layers.Gloves, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Gloves

                        Case Enums.Layers.Hair
                            Me._Layers.SetLayer(Enums.Layers.Hair, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Hair

                        Case Enums.Layers.Head
                            Me._Layers.SetLayer(Enums.Layers.Head, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Head

                        Case Enums.Layers.InnerLegs
                            Me._Layers.SetLayer(Enums.Layers.InnerLegs, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.InnerLegs

                        Case Enums.Layers.InnerTorso
                            Me._Layers.SetLayer(Enums.Layers.InnerTorso, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.InnerTorso

                        Case Enums.Layers.LeftHand
                            Me._Layers.SetLayer(Enums.Layers.LeftHand, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.LeftHand

                        Case Enums.Layers.MiddleTorso
                            Me._Layers.SetLayer(Enums.Layers.MiddleTorso, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.MiddleTorso

                        Case Enums.Layers.Mount
                            Me._Layers.SetLayer(Enums.Layers.Mount, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Mount

                        Case Enums.Layers.Neck
                            Me._Layers.SetLayer(Enums.Layers.Neck, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Neck

                        Case Enums.Layers.OuterLegs
                            Me._Layers.SetLayer(Enums.Layers.OuterLegs, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.OuterLegs

                        Case Enums.Layers.OuterTorso
                            Me._Layers.SetLayer(Enums.Layers.OuterTorso, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.OuterTorso

                        Case Enums.Layers.Pants
                            Me._Layers.SetLayer(Enums.Layers.Pants, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Pants

                        Case Enums.Layers.RightHand
                            Me._Layers.SetLayer(Enums.Layers.RightHand, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.RightHand

                        Case Enums.Layers.Ring
                            Me._Layers.SetLayer(Enums.Layers.Ring, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Ring

                        Case Enums.Layers.Shirt
                            Me._Layers.SetLayer(Enums.Layers.Shirt, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Shirt

                        Case Enums.Layers.Shoes
                            Me._Layers.SetLayer(Enums.Layers.Shoes, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Shoes

                        Case Enums.Layers.None
                            Me._Layers.SetLayer(Enums.Layers.None, Packet.EquippedItems(i).Serial)
                            k(i) = Enums.Layers.Waist

                    End Select

                    'Adds the item to the world item list for later reference.
                    'The container is set to the Mobile Serial


                    If _Client._AllItems.ContainsKey(Packet.EquippedItems(i)._Serial) = False Then
#If DebugMobiles Then
                    Console.WriteLine("-HandleUpdatePacket(ByVal Packet As Packets.EquippedMobile)")
                    Console.WriteLine(" Adding Item as Mobile Equipment: ")
                    Console.WriteLine(" Mobile:" & Me.Serial.ToString)
                    Console.WriteLine(" Item:" & Packet.EquippedItems(i).Serial.ToString)
                    Console.WriteLine(" Layer:" & Packet.EquippedItems(i).Layer)
#End If
                        'Instantiate the contents veriable of the time.
                        Packet.EquippedItems(i)._contents = New ItemList(DirectCast(Packet.EquippedItems(i), Item), _Client)

                        _Client.Items.Add(Packet.EquippedItems(i))

                    End If

                Next
            End If

#If DebugMobiles Then
            Console.WriteLine("-Updated Mobile Equipped: " & Packet.Serial.ToString)
#End If

        End Sub

        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.NakedMobile)
            _Type = Packet.BodyType
            _X = Packet.X
            _Y = Packet.Y
            _Z = Packet.Z
            _Direction = Packet.Direction
            _Hue = Packet.Hue
            _Notoriety = Packet.Notoriety
            _Status = Packet.Status
#If DebugMobiles Then
            Console.WriteLine("-Updated Mobile Naked: " & Packet.Serial.ToString)
#End If
        End Sub

        Friend Sub HandleUpdatePacket(ByVal Packet As Packets.HealthBarStatusUpdate)
            If Packet.Color = 1 And Packet.Flag <> _PoisonLevel Then
                _PoisonLevel = Packet.Flag
                RaiseEvent onUpdate(_Client, Me, Enums.MobileUpdateType.Poison)
            ElseIf Packet.Color = 0 And _PoisonLevel <> 0 Then
                RaiseEvent onUpdate(_Client, Me, Enums.MobileUpdateType.Poison)
            End If
        End Sub

        Friend Sub HandleDeathPacket(ByVal packet As Packets.DeathAnimation)
            RaiseEvent onDeath(_Client, Me, packet.CorpseSerial)
        End Sub

#End Region

    End Class

    Partial Class Enums

        ''' <summary>
        ''' Whether or not a mobile can be renamed by the player.
        ''' </summary>
        Public Enum Renamable As Byte
            Renamable = &H0
            NotRenamable = &HFF
        End Enum

        ''' <summary>
        ''' What data is displayed about the mobile.
        ''' </summary>
        Public Enum DisplayMode As Byte
            Normal = 1
            StatCap = 2
            StatCap_Followers = 3
            StatCap_Followers_Resistances = 4
            SupportedFeatures5 = 5
            KR = 6
        End Enum

        Public Enum MobileUpdateType As Byte
            Health
            Stamina
            Mana
            Status
            FullUpdate
            Move
            EquipItem
            Poison
        End Enum

    End Class

End Class