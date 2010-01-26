Imports System.Collections
Imports System.Threading

Partial Class UOAI

    ''' <summary>Contains a list of UOAI.Item's</summary>
    Public Class ItemList
        Implements IEnumerable
        Private _ItemHashBySerial As New Hashtable
        Private _ItemHashByType As New Hashtable
        Private _ItemHashByOffset As New Hashtable
        Private _ParentItem As Item
        Private _Serial As Serial
        Private _SearchReturn As Boolean = False
        Private _MyClient As Client

        'Items should use this one!
        Friend Sub New(ByVal ParentItem As Item, ByVal Client As Client)
            _ParentItem = ParentItem
            _Serial = ParentItem.Serial
            _MyClient = Client
        End Sub

        'Only the Client should cast this as the world object!
        Friend Sub New(ByVal Serial As Serial, ByVal Client As Client)
            _Serial = Serial
            _MyClient = Client
        End Sub

        'This is to use as a holder for search results!
        Friend Sub New()
            'Tells the rest of the code that this is just the return of a search.
            _SearchReturn = True
        End Sub

        ''' <summary>
        ''' Adds the specified item to the Item List.
        ''' </summary>
        ''' <param name="Item">The item to add.</param>
        ''' <remarks></remarks>
        Friend Sub AddItem(ByVal Item As Item)
            Try
                If _MyClient Is Nothing Then
                    'dont add search results to the giant hash!
                    _ItemHashBySerial.Add(Item.Serial, Item)
                ElseIf _MyClient.Mobiles.Exists(Item.Container) Then 'Check to see if the item's container is a mobile.
                    'If it is, then add the item to the _AllItems hash and bypass the recursive container checks.
                    'There is no need to add it to a container. The removal process does the same thing.
                    _MyClient._AllItems.Add(Item.Serial, Item)
                ElseIf Item.Container = _Serial Then
                    'if the items container is this container, then add it to the hash

                    'Add the item to this container's hash
                    _ItemHashBySerial.Add(Item.Serial, Item)

                    'Add it to the client's Allitem list, for access as a container.
                    _MyClient._AllItems.Add(Item.Serial, Item)
                Else
                    'If this item's container is NOT this container,
                    'then look up the container by Serial in the client's master item list
                    'and add the item to that items contents.
                    _MyClient._AllItems(Item.Container).Contents.AddItem(Item)

                End If
            Catch ex As Exception
                Console.WriteLine("Failed to Added Item: " & Item.Serial.ToString & " " & ex.Message)
            End Try

            Console.WriteLine("Successfully Added Item: " & Item.Serial.ToString)
        End Sub

        Friend Sub Additem(ByVal Packet As Packets.ContainerContents)
            Dim j As Item

            For Each i As Item In Packet.Items
                If Exists(i.Serial) Then Continue For

                Console.WriteLine("Adding Item by ContainerContents: " & i.Serial.ToString)

                j = New Item(_MyClient)
                j._Serial = i._Serial
                j._Type = i._Type
                j._StackID = i._StackID
                j._X = i._X
                j._Y = i._Y
                j._Container = i._Container
                j._Hue = i._Hue

                AddItem(j)
            Next

        End Sub

        Friend Sub AddItem(ByVal Packet As Packets.ObjectToObject)
            If Exists(Packet.Serial) Then Exit Sub

            Dim j As New Item(_MyClient)

            Console.WriteLine("Adding Item by ObjectToObject: " & j.Serial.ToString)

            j._Serial = Packet._Serial
            j._Type = Packet._Itemtype
            j._StackID = Packet._stackID
            j._Amount = Packet._amount
            j._X = Packet._X
            j._Y = Packet._Y
            j._Container = Packet._Container
            j._Hue = Packet._Hue

            AddItem(j)
        End Sub

        Friend Sub AddItem(ByVal Packet As Packets.ShowItem)
            If Exists(Packet.Serial) Then Exit Sub

            Dim j As New Item(_MyClient)

            Console.WriteLine("Adding Item by ShowItem: " & j.Serial.ToString)

            j._Serial = Packet.Serial
            j._Type = Packet.ItemType
            j._Amount = Packet.Amount
            j._StackID = Packet.StackID
            j._X = Packet.X
            j._Y = Packet.Y
            j._Direction = Packet.Direction
            j._Z = Packet.Z
            j._Hue = Packet.Hue

            AddItem(j)

        End Sub

        Friend Sub HashByOffset(ByVal Serial As Serial, ByVal Offset As UInt32)
            _ItemHashByOffset.Add(Offset, Item(Serial))
        End Sub

        ''' <summary>
        ''' Attempts to remove an item from the item list by its Serial. Returns true if it found the item, returns false if it didn't.
        ''' </summary>
        ''' <param name="ItemSerial">The serial of the item to be removed.</param>
        Friend Function RemoveItem(ByVal ItemSerial As UOAI.Serial) As Boolean
            Try
                If _SearchReturn = True Then
                    'use the serial hash to locate the item inside the offset hash and remove it
                    _ItemHashByOffset.Remove(DirectCast(_ItemHashBySerial(ItemSerial), Item).MemoryOffset)
                    'use the serial to find the item in the serial hash and remove it.
                    _ItemHashBySerial.Remove(ItemSerial)
                ElseIf Exists(ItemSerial) Then 'This is the item's container.
                    'Remove the item from its container's contents.
                    'Check if this is the container of the item.

                    'Check if the item's container is a mobile.
                    If _MyClient.Mobiles.Exists(_MyClient._AllItems(ItemSerial).Container) Then
                        'Remove the item from the mobile's respective layer.
                        _MyClient.Mobiles.Mobile(DirectCast(_MyClient._AllItems(ItemSerial).Container, Serial)).RemoveItemFromLayer(DirectCast(_MyClient._AllItems(ItemSerial), Item).Layer)

                        'Remove it from the all items hash.
                        _MyClient._AllItems.Remove(ItemSerial)
                    Else
                        'use the serial to find the item in the serial hash and remove it.
                        _ItemHashBySerial.Remove(ItemSerial)

                        'Remvoe it from the all items hash.
                        _MyClient._AllItems.Remove(ItemSerial)
                    End If

                Else 'This is not the item's container
                    'Perform a reverse lookup of the container's ITEM class using the GIANTSerialHash, 
                    'and have the item removed from its container's contents
                    DirectCast(_MyClient._AllItems(ItemSerial).Container, Item).Contents.RemoveItem(ItemSerial)

                End If

            Catch ex As Exception
                Console.WriteLine("Item deletion failed due to: " & ex.Message)
                Return False
            End Try

            Console.WriteLine("Item deleted successfuly: " & ItemSerial.ToString)
            Return True
        End Function

        ''' <summary>
        ''' Returns the specified item, either by index or <see cref="UOAI.serial"/>
        ''' </summary>
        ''' <param name="Serial">The serial of the item to be returned.</param>
        Public ReadOnly Property Item(ByVal Serial As UOAI.Serial) As UOAI.Item
            Get
                If _Serial = WorldSerial Then
                    Return _MyClient._AllItems(Serial)
                Else
                    Return _ItemHashBySerial(Serial)
                End If
            End Get
        End Property

        ''' <param name="OffSet">The offset of the item in the client's memory.</param>
        Friend ReadOnly Property Item(ByVal OffSet As Int32) As UOAI.Item
            Get
                Return _ItemHashByOffset(OffSet)
            End Get
        End Property

        ''' <summary>
        ''' Returns an itemlist containing the items of the specified type.
        ''' </summary>
        ''' <param name="Type">The type of item you want to search for.</param>
        Public ReadOnly Property byType(ByVal Type As ItemType) As ItemList
            Get
                Dim k As New ItemList()

                'check each item in the hash 
                For Each s As Serial In _ItemHashBySerial.Keys
                    'to see if its type maches the one specified.
                    If Me.Item(s).Type = Type Then
                        'Then add that to the itemlist to return
                        k.AddItem(Me.Item(s))
                    End If
                Next

                Return k
            End Get
        End Property

        ''' <summary>
        ''' Checks to see if the specified item exists in the item list.
        ''' </summary>
        ''' <param name="Serial">The serial of the item you want to check the existance of.</param>
        Public ReadOnly Property Exists(ByVal Serial As Serial) As Boolean
            Get
                If _Serial = WorldSerial Then
                    Return _MyClient._AllItems.ContainsKey(Serial)
                Else
                    Return _ItemHashBySerial.ContainsKey(Serial)
                End If

            End Get
        End Property

        ''' <param name="Offset">The offset item you want to check the existance of.</param>
        Friend ReadOnly Property Exists(ByVal Offset As Int32) As Boolean
            Get
                Return _ItemHashByOffset.ContainsKey(Offset)
            End Get
        End Property

        ''' <param name="Item">The item you want to check the existance of.</param>
        Public ReadOnly Property Exists(ByVal Item As Item) As Boolean
            Get
                If _Serial = WorldSerial Then
                    Return _MyClient._AllItems.ContainsValue(Item)
                Else
                    Return _ItemHashBySerial.ContainsValue(Item)
                End If

            End Get
        End Property

        ''' <summary>
        ''' Returns the number of items in the list.
        ''' </summary>
        Public ReadOnly Property Count() As Integer
            Get
                Return _ItemHashBySerial.Count
            End Get
        End Property

        Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _ItemHashBySerial.Values.GetEnumerator
        End Function

    End Class

End Class