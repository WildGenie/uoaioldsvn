Partial Class UOAI

    'Hide this class from the user, there is no reason from him/her to see it.
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class MobileList
        Implements IEnumerable
        Private _MobileHashBySerial As New Hashtable
        Private _MobileHashByOffset As New Hashtable
        Private _Client As Client
        Private _IsSearchResult As Boolean = False

        Friend Sub New(ByRef Client As Client)
            _Client = Client
        End Sub

        Friend Sub New()
            _IsSearchResult = True
        End Sub

        ''' <summary>
        ''' Adds the specified mobile to the mobile list.
        ''' </summary>
        ''' <param name="Mobile"></param>
        Public Sub AddMobile(ByVal Mobile As Mobile)

            _MobileHashBySerial.Add(Mobile.Serial, Mobile)
            'TODO: Make an AddHash sub for post-packet handling to add this to the memory offset hash.
            '_MobileHashByOffset.Add(Mobile.MemoryOffset, Mobile)
            Console.WriteLine("Added Mobile: " & Mobile.Serial.ToString)

        End Sub

        ''' <summary>
        ''' Adds the a new mobile to the <see cref="UOAI.MobileList"/> based on a supplied packet.
        ''' </summary>
        ''' <param name="Packet">The packet as a <see cref="UOAI.Packets.EquippedMobile"/>.</param>
        Friend Sub AddMobile(ByVal Packet As Packets.EquippedMobile)
            If Exists(Packet.Serial) Then
                'Instead of trying to create a new mobile, just tell the mobile to udpate itself with the given packet.
                Me.Mobile(Packet.Serial).HandleUpdatePacket(Packet)

                'Return to the sub that called this.
                Exit Sub
            End If

            'Create a new empty mobile on the current client.
            Dim NewMobile As New Mobile(_Client)

            NewMobile._Serial = Packet.Serial
            NewMobile._Type.BaseValue = Packet.BodyType
            NewMobile._X = Packet.X
            NewMobile._Y = Packet.Y
            NewMobile._Z = Packet.Z
            NewMobile._Direction = Packet.Direction
            NewMobile._Hue = Packet.Hue
            NewMobile._Notoriety = Packet.Notoriety
            NewMobile._Status = Packet.Status

            'Loop through the items and add their serials to the proper layers for later reference
            For Each i As Item In Packet.EquippedItems
                Select Case i._Layer
                    Case Enums.Layers.Arms
                        NewMobile._Layers.SetLayer(Enums.Layers.Arms, i.Serial)

                    Case Enums.Layers.Back
                        NewMobile._Layers.SetLayer(Enums.Layers.Back, i.Serial)

                    Case Enums.Layers.BackPack
                        NewMobile._Layers.SetLayer(Enums.Layers.BackPack, i.Serial)

                    Case Enums.Layers.Bank
                        NewMobile._Layers.SetLayer(Enums.Layers.Bank, i.Serial)

                    Case Enums.Layers.Bracelet
                        NewMobile._Layers.SetLayer(Enums.Layers.Bracelet, i.Serial)

                    Case Enums.Layers.Ears
                        NewMobile._Layers.SetLayer(Enums.Layers.Ears, i.Serial)

                    Case Enums.Layers.FacialHair
                        NewMobile._Layers.SetLayer(Enums.Layers.FacialHair, i.Serial)

                    Case Enums.Layers.Gloves
                        NewMobile._Layers.SetLayer(Enums.Layers.Gloves, i.Serial)

                    Case Enums.Layers.Hair
                        NewMobile._Layers.SetLayer(Enums.Layers.Hair, i.Serial)

                    Case Enums.Layers.Head
                        NewMobile._Layers.SetLayer(Enums.Layers.Head, i.Serial)

                    Case Enums.Layers.InnerLegs
                        NewMobile._Layers.SetLayer(Enums.Layers.InnerLegs, i.Serial)

                    Case Enums.Layers.InnerTorso
                        NewMobile._Layers.SetLayer(Enums.Layers.InnerTorso, i.Serial)

                    Case Enums.Layers.LeftHand
                        NewMobile._Layers.SetLayer(Enums.Layers.LeftHand, i.Serial)

                    Case Enums.Layers.MiddleTorso
                        NewMobile._Layers.SetLayer(Enums.Layers.MiddleTorso, i.Serial)

                    Case Enums.Layers.Mount
                        NewMobile._Layers.SetLayer(Enums.Layers.Mount, i.Serial)

                    Case Enums.Layers.Neck
                        NewMobile._Layers.SetLayer(Enums.Layers.Neck, i.Serial)

                    Case Enums.Layers.OuterLegs
                        NewMobile._Layers.SetLayer(Enums.Layers.OuterLegs, i.Serial)

                    Case Enums.Layers.OuterTorso
                        NewMobile._Layers.SetLayer(Enums.Layers.OuterTorso, i.Serial)

                    Case Enums.Layers.Pants
                        NewMobile._Layers.SetLayer(Enums.Layers.Pants, i.Serial)

                    Case Enums.Layers.RightHand
                        NewMobile._Layers.SetLayer(Enums.Layers.RightHand, i.Serial)

                    Case Enums.Layers.Ring
                        NewMobile._Layers.SetLayer(Enums.Layers.Ring, i.Serial)

                    Case Enums.Layers.Shirt
                        NewMobile._Layers.SetLayer(Enums.Layers.Shirt, i.Serial)

                    Case Enums.Layers.Shoes
                        NewMobile._Layers.SetLayer(Enums.Layers.Shoes, i.Serial)

                    Case Enums.Layers.None
                        NewMobile._Layers.SetLayer(Enums.Layers.None, i.Serial)

                End Select

                'Adds the item to the world item list for later reference.
                'The container is set to the mobile serial.
                _Client.Items.AddItem(i)
            Next

            AddMobile(NewMobile)
        End Sub

        ''' <param name="Packet">The packet as a <see cref="UOAI.Packets.NakedMobile"/>.</param>
        Friend Sub AddMobile(ByVal Packet As Packets.NakedMobile)
            If Exists(Packet.Serial) Then
                'Instead of trying to create a new mobile, just tell the mobile to udpate itself with the given packet.
                Me.Mobile(Packet.Serial).HandleUpdatePacket(Packet)

                'Return to the sub that called this.
                Exit Sub
            End If

            'Create a new empty mobile on the current client.
            Dim NewMobile As New Mobile(_Client)

            'Fill in the mobile's data.
            NewMobile._Serial = Packet.Serial
            NewMobile._Type.BaseValue = Packet.BodyType
            NewMobile._X = Packet.X
            NewMobile._Y = Packet.Y
            NewMobile._Z = Packet.Z
            NewMobile._Direction = Packet.Direction
            NewMobile._Hue = Packet.Hue
            NewMobile._Notoriety = Packet.Notoriety
            NewMobile._Status = Packet.Status

            AddMobile(NewMobile)
        End Sub

        Friend Sub HashByOffset(ByVal Serial As Serial, ByVal Offset As UInt32)
            _MobileHashByOffset.Add(Offset, Mobile(Serial))
        End Sub

        'Friend Sub AddPlayer(byval 

        ''' <summary>
        ''' Removes the specified mobile formt he MobileList.
        ''' </summary>
        ''' <param name="Serial">The serial of the mobile to be removed.</param>
        Public Function RemoveMobile(ByVal Serial As Serial)
            Try
                'TODO: Memory offset
                '_MobileHashByOffset.Remove(DirectCast(_MobileHashBySerial(Serial), Mobile).MemoryOffset)
                If Exists(Serial) Then
                    _MobileHashBySerial.Remove(Serial)
                Else
                    Throw New ApplicationException("The mobile does not exist in the hash!")
                End If
            Catch ex As Exception
                Console.WriteLine("Remove Mobile by Serial Failed: " & Serial.ToString)
                Return False
            End Try
            Console.WriteLine("Removed Mobile by Serial: " & Serial.ToString)
            Return True
        End Function

        ''' <param name="Offset">The memory offset of the mobile to be removed.</param>
        Public Function RemoveMobile(ByVal Offset As UInt32)
            Try
                _MobileHashBySerial.Remove(DirectCast(_MobileHashBySerial(Offset), Mobile).Serial)
                _MobileHashByOffset.Remove(Offset)
            Catch ex As Exception
                Console.WriteLine("Remove Mobile by Offset Failed: " & Offset)
                Return False
            End Try
            Console.WriteLine("Removed Mobile by Offset: " & Offset)
            Return True
        End Function

        Friend Sub RemoveMobile(ByVal DeathPacket As Packets.DeathAnimation)
            DirectCast(_MobileHashBySerial(DeathPacket.Serial), Mobile).HandleDeathPacket(DeathPacket)
            RemoveMobile(DeathPacket.Serial)
            Console.WriteLine("Removed Mobile by Death Packet: " & DeathPacket.Serial.ToString)
        End Sub

        ''' <summary>
        ''' Checks to see if the specified mobile exists in the MobileList.
        ''' </summary>
        ''' <param name="Serial">The serial of the mobile to check for.</param>
        Public Function Exists(ByVal Serial As Serial) As Boolean
            Return _MobileHashBySerial.Contains(Serial)
        End Function

        ''' <param name="Offset">The memory offset of the mobile to check for.</param>
        Public Function Exists(ByVal Offset As UInt32) As Boolean
            Return _MobileHashByOffset.Contains(Offset)
        End Function

        ''' <summary>
        ''' Returns a MobileList containing all of the mobiles with the specified name.
        ''' </summary>
        ''' <param name="Name">The name you want to search for.</param>
        Public Function byName(ByVal Name As String) As MobileList
            Dim ml As New MobileList

            For Each s As Serial In _MobileHashBySerial.Keys
                If DirectCast(_MobileHashBySerial(s), Mobile).Name = Name Then ml.AddMobile(DirectCast(_MobileHashBySerial(s), Mobile))
            Next

            Return ml
        End Function

        ''' <summary>
        ''' Returns mobile specified by serial.
        ''' </summary>
        ''' <param name="Serial">The serial of the mobile.</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Mobile(ByVal Serial As Serial) As Mobile
            Get
                Return _MobileHashBySerial(Serial)
            End Get
        End Property

        ''' <summary>
        ''' Returns mobile specified by offset
        ''' </summary>
        ''' <param name="Offset">The offset of the mobile object in the client's memory.</param>
        Friend ReadOnly Property Mobile(ByVal Offset As Int32) As Mobile
            Get
                Return _MobileHashByOffset(Offset)
            End Get
        End Property

        Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _MobileHashBySerial.Values.GetEnumerator()
        End Function
    End Class

End Class
