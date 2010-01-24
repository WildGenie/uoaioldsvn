Partial Class UOAI

    Public Class Mobile
        Inherits Item
        Private _Layers As LayersClass

        Friend Sub New(ByVal Client As Client)
            MyBase.New(Client)
            _Layers = New LayersClass(Client)
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

#Region "Public Events"
        Public Event onDeath(ByVal Client As Client, ByVal Mobile As Mobile)
        Public Event onDeath(ByVal Client As Client, ByVal Mobile As Mobile)

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

        'Hide this class from the user, there is no reason from him/her to see it.
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Class LayersClass
            Friend Sub New(ByVal Client As Client)
                _Client = Client
            End Sub

            Private _Client As Client
            Friend _LeftHand As Serial
            Friend _RightHand As Serial
            Friend _Shoes As Serial
            Friend _Pants As Serial
            Friend _Shirt As Serial
            Friend _Head As Serial
            Friend _Gloves As Serial
            Friend _Ring As Serial
            Friend _Neck As Serial
            Friend _Hair As Serial
            Friend _Waist As Serial
            Friend _InnerTorso As Serial
            Friend _Bracelet As Serial
            Friend _FacialHair As Serial
            Friend _MiddleTorso As Serial
            Friend _Ears As Serial
            Friend _Arms As Serial
            Friend _Back As Serial
            Friend _BackPack As Serial
            Friend _OuterTorso As Serial
            Friend _OuterLegs As Serial
            Friend _InnerLegs As Serial
            Friend _Mount As Serial
            Friend _Bank As Serial

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