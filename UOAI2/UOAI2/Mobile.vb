Partial Class UOAI

    Public Class Mobile
        Inherits Item

        Friend Sub New(ByVal Serial As Serial, ByVal Client As Client)
            MyBase.New(Client)
        End Sub

#Region "Private Variables"
        Friend _Name As String
        Friend _Hits As UShort
        Friend _HitsMax As UShort
        Friend _Renamable As Enums.Renamable
        Friend _DisplayMode As Enums.DisplayMode
        Friend _Gender As Enums.Gender
        Friend _Strength As UShort
        Friend _Dexterity As UShort
        Friend _Intelligence As UShort
        Friend _Stamina As UShort
        Friend _StaminaMax As UShort
        Friend _Mana As UShort
        Friend _ManaMax As UShort
        Friend _Gold As UInt32
        Friend _ResistPhysical As UShort
        Friend _Weight As UShort
        Friend _StatCap As UShort
        Friend _Followers As Byte
        Friend _FollowersMax As Byte
        Friend _ResistFire As UShort
        Friend _ResistCold As UShort
        Friend _ResistPoison As UShort
        Friend _ResistEnergy As UShort
        Friend _Luck As UShort
        Friend _DamageMin As UShort
        Friend _DamageMax As UShort
        Friend _TithingPoints As UShort

#End Region

#Region "Public Properties"
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

        ''' <summary>
        ''' Returns the item on the specified layer.
        ''' </summary>
        Public Class Layers
            Friend _LeftHand As Item
            Friend _RightHand As Item
            Friend _Shoes As Item
            Friend _Pants As Item
            Friend _Shirt As Item
            Friend _Head As Item
            Friend _Gloves As Item
            Friend _Ring As Item
            Friend _Neck As Item
            Friend _Hair As Item
            Friend _Waist As Item
            Friend _InnerTorso As Item
            Friend _Bracelet As Item
            Friend _FacialHair As Item
            Friend _MiddleTorso As Item
            Friend _Ears As Item
            Friend _Arms As Item
            Friend _Back As Item
            Friend _BackPack As Item
            Friend _OuterTorso As Item
            Friend _OuterLegs As Item
            Friend _InnerLegs As Item
            Friend _Mount As Item
            Friend _Bank As Item

            Public ReadOnly Property LeftHand()
                Get
                    If _LeftHand Is Nothing Then Return Nothing
                    Return _LeftHand
                End Get
            End Property

            Public ReadOnly Property RightHand()
                Get
                    If _RightHand Is Nothing Then Return Nothing
                    Return _RightHand
                End Get
            End Property

            Public ReadOnly Property Shoes()
                Get
                    If _Shoes Is Nothing Then Return Nothing
                    Return _Shoes
                End Get
            End Property

            Public ReadOnly Property Pants()
                Get
                    If _Pants Is Nothing Then Return Nothing
                    Return _Pants
                End Get
            End Property

            Public ReadOnly Property Shirt()
                Get
                    If _Shirt Is Nothing Then Return Nothing
                    Return _Shirt
                End Get
            End Property

            Public ReadOnly Property Head()
                Get
                    If _Head Is Nothing Then Return Nothing
                    Return _Head
                End Get
            End Property

            Public ReadOnly Property Gloves()
                Get
                    If _Gloves Is Nothing Then Return Nothing
                    Return _Gloves
                End Get
            End Property

            Public ReadOnly Property Ring()
                Get
                    If _Ring Is Nothing Then Return Nothing
                    Return _Ring
                End Get
            End Property

            Public ReadOnly Property Neck()
                Get
                    If _Neck Is Nothing Then Return Nothing
                    Return _Neck
                End Get
            End Property

            Public ReadOnly Property Hair()
                Get
                    If _Hair Is Nothing Then Return Nothing
                    Return _Hair
                End Get
            End Property

            Public ReadOnly Property Waist()
                Get
                    If _Waist Is Nothing Then Return Nothing
                    Return _Waist
                End Get
            End Property

            Public ReadOnly Property InnerTorso()
                Get
                    If _InnerTorso Is Nothing Then Return Nothing
                    Return _InnerTorso
                End Get
            End Property

            Public ReadOnly Property Bracelet()
                Get
                    If _Bracelet Is Nothing Then Return Nothing
                    Return _Bracelet
                End Get
            End Property

            Public ReadOnly Property FacialHair()
                Get
                    If _FacialHair Is Nothing Then Return Nothing
                    Return _FacialHair
                End Get
            End Property

            Public ReadOnly Property MiddleTorso()
                Get
                    If _MiddleTorso Is Nothing Then Return Nothing
                    Return _MiddleTorso
                End Get
            End Property

            Public ReadOnly Property Ears()
                Get
                    If _Ears Is Nothing Then Return Nothing
                    Return _Ears
                End Get
            End Property

            Public ReadOnly Property Arms()
                Get
                    If _Arms Is Nothing Then Return Nothing
                    Return _Arms
                End Get
            End Property

            Public ReadOnly Property Back()
                Get
                    If _Back Is Nothing Then Return Nothing
                    Return _Back
                End Get
            End Property

            Public ReadOnly Property BackPack()
                Get
                    If _BackPack Is Nothing Then Return Nothing
                    Return _BackPack
                End Get
            End Property

            Public ReadOnly Property OuterTorso()
                Get
                    If _OuterTorso Is Nothing Then Return Nothing
                    Return _OuterTorso
                End Get
            End Property

            Public ReadOnly Property OuterLegs()
                Get
                    If _OuterLegs Is Nothing Then Return Nothing
                    Return _OuterLegs
                End Get
            End Property

            Public ReadOnly Property InnerLegs()
                Get
                    If _InnerLegs Is Nothing Then Return Nothing
                    Return _InnerLegs
                End Get
            End Property

            Public ReadOnly Property Mount()
                Get
                    If _Mount Is Nothing Then Return Nothing
                    Return _Mount
                End Get
            End Property

            Public ReadOnly Property Bank()
                Get
                    If _Bank Is Nothing Then Return Nothing
                    Return _Bank
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
            StatCap
            StatCap_Followers
            StatCap_Followers_Resistances
        End Enum

    End Class

End Class