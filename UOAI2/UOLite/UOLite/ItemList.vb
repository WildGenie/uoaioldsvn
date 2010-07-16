Imports System.Collections
Imports System.Threading

#Const DebugItemList = False

Partial Class LiteClient

    ''' <summary>Contains a collection of UOAI.Item's</summary>
    Public Class ItemList

#Region "Enumeration Stuff"
        Implements ICollection(Of Item)

        Public Sub Clear() Implements System.Collections.Generic.ICollection(Of Item).Clear
            _ItemHashBySerial.Clear()
            _ItemHashByOffset.Clear()
            _ItemHashByType.Clear()
        End Sub

        Public Function Contains(ByVal item As Item) As Boolean Implements System.Collections.Generic.ICollection(Of Item).Contains
            Return _ItemHashBySerial.ContainsValue(item)
        End Function

        Private Sub CopyTo(ByVal array() As Item, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of Item).CopyTo

        End Sub

        Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of Item).Count
            Get
                Return _ItemHashBySerial.Count
            End Get
        End Property

        Private ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of Item).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(ByVal item As Item) As Boolean Implements System.Collections.Generic.ICollection(Of Item).Remove
            Return RemoveItem(item.Serial)
        End Function

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of Item) Implements System.Collections.Generic.IEnumerable(Of Item).GetEnumerator
            Return _ItemHashBySerial.Values.OfType(Of Item).GetEnumerator
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _ItemHashBySerial.Values.OfType(Of Item).GetEnumerator
        End Function
#End Region

#Region "Constructors And Properties"
        Private position As Integer = 0
        Friend _ItemHashBySerial As New Hashtable
        Private _ItemHashByType As New Hashtable
        Private _ItemHashByOffset As New Hashtable
        Private _ParentItem As Item
        Private _Serial As New Serial(0)
        Private _SearchReturn As Boolean = False
        Friend _MyLiteClient As LiteClient

        'Items should use this one!
        Friend Sub New(ByVal ParentItem As Item, ByVal LiteClient As LiteClient)
            _ParentItem = ParentItem
            _Serial = ParentItem.Serial
            _MyLiteClient = LiteClient
        End Sub

        'Only the LiteClient should cast this as the world object!
        Friend Sub New(ByVal Serial As Serial, ByVal LiteClient As LiteClient)
            _Serial = Serial
            _MyLiteClient = LiteClient

        End Sub

        'This is to use as a holder for search results!
        Friend Sub New()
            'Tells the rest of the code that this is just the return of a search.
            _SearchReturn = True

        End Sub

#End Region

#Region "Adding Items"

        ''' <summary>
        ''' Adds the specified item to the Item List.
        ''' </summary>
        ''' <param name="Item">The item to add.</param>
        ''' <remarks></remarks>
        Friend Sub Add(ByVal Item As Item) Implements System.Collections.Generic.ICollection(Of Item).Add

#If DebugItemList = False Then
            Try
#End If
                If _MyLiteClient Is Nothing Then
                    'dont add search results to the giant hash!
                    If Exists(Item.Serial) Then Exit Sub
                    _ItemHashBySerial.Add(Item.Serial, Item)

                ElseIf Item.Container = _Serial Then 'if the items container is this container, then add it to the hash

                    'If the item exists in the list then remove it, because this is obviously an update.
                    If Exists(Item.Serial) Then
                        _MyLiteClient.Items.RemoveItem(Item.Serial)
                    End If

                    'Add the item to this container's hash
                    _ItemHashBySerial.Add(Item.Serial, Item)

                    'Add it to the LiteClient's Allitem hash, for access as a container.
                    _MyLiteClient._AllItems.Add(Item.Serial, Item)

#If DebugItemList Then
                Console.WriteLine("-Successfully Added Item: " & Item.Serial.ToString)
#End If
                    _MyLiteClient.NewItem(Item)

                Else
                    'If this item's container is NOT this container,
                    'then look up the container by Serial in the LiteClient's master item list
                    'and add the item to that items contents.
#If DebugItemList Then
                If _MyLiteClient._AllItems.ContainsKey(Item.Container) = False Then
                    Console.WriteLine("-Item Add is about to fail because the item's container is not in the all item's list.")
                    Console.WriteLine(" Item Serial: " & Item.Serial.ToString)
                    Console.WriteLine(" Item Container Serial: " & Item.Container.ToString)
                    Exit Sub
                End If
#End If

                    DirectCast(_MyLiteClient._AllItems(Item.Container), Item).Contents.Add(Item)

                End If

#If DebugItemList = False Then
            Catch ex As Exception
                Console.WriteLine("-Failed to Add Item due to: " & ex.Message)
                Exit Sub
            End Try
#End If

            Reset()

        End Sub

        Friend Sub Add(ByVal Packet As Packets.ContainerContents)
            Dim j As Item

            For Each i As Item In Packet.Items
                If Exists(i.Serial) Then Continue For

                j = New Item(_MyLiteClient, i.Serial)

                j._Type = i._Type
                j._StackID = i._StackID
                j._X = i._X
                j._Y = i._Y
                j._Container = i._Container
                j._Hue = i._Hue

#If DebugItemList Then
                Console.WriteLine("-Adding Item by ContainerContents: Container: " & j._Container.ToString & " Serial:" & i.Serial.ToString)
#End If

                Add(j)
            Next

        End Sub

        Friend Sub Add(ByVal Packet As Packets.ObjectToObject)
            If Exists(Packet.Serial) Then Exit Sub

            Dim j As New Item(_MyLiteClient, Packet.Serial)

            j._Type = Packet._Itemtype
            j._StackID = Packet._stackID
            j._Amount = Packet._amount
            j._X = Packet._X
            j._Y = Packet._Y
            j._Container = Packet._Container
            j._Hue = Packet._Hue

#If DebugItemList Then
            Console.WriteLine("-Adding Item by ObjectToObject: " & j.Serial.ToString)
#End If

            Add(j)
        End Sub

        Friend Sub Add(ByVal Packet As Packets.ShowItem)
            If Exists(Packet.Serial) Then Exit Sub

            Dim j As New Item(_MyLiteClient, Packet.Serial)

            j._Container = WorldSerial 'Set the container to the worldserial, because thats where this is.
            j._Type = Packet.ItemType
            j._Amount = Packet.Amount
            j._StackID = Packet.StackID
            j._X = Packet.X
            j._Y = Packet.Y
            j._Direction = Packet.Direction
            j._Z = Packet.Z
            j._Hue = Packet.Hue

#Const DEBUGShowItem = False
#If DEBUGShowItem = True Then
            Console.WriteLine("-Adding Item by Show Item.")
            Console.WriteLine(" Packet: " & BitConverter.ToString(Packet.Data))
            Console.WriteLine(" Serial: " & j.Serial.ToString)
            Console.WriteLine(" Serial#: " & j.Serial.Value)
#End If

#If DebugItemList Then
            Console.WriteLine("-Adding Item by ShowItem: " & j.Serial.ToString)
#End If

            Add(j)
        End Sub

        Friend Sub HashByOffset(ByVal Serial As Serial, ByVal Offset As UInt32)
            _ItemHashByOffset.Add(Offset, Item(Serial))
        End Sub

#End Region

#Region "Removing Items"

        ''' <summary>
        ''' Attempts to remove an item from the item list by its Serial. Returns true if it found the item, returns false if it didn't.
        ''' </summary>
        ''' <param name="ItemSerial">The serial of the item to be removed.</param>
        Friend Function RemoveItem(ByVal ItemSerial As Serial) As Boolean
            Try
                If _SearchReturn = True Then
                    'use the serial hash to locate the item inside the offset hash and remove it
                    _ItemHashByOffset.Remove(DirectCast(_ItemHashBySerial(ItemSerial), Item).MemoryOffset)

                    'use the serial to find the item in the serial hash and remove it.
                    _ItemHashBySerial.Remove(ItemSerial)
                ElseIf Exists(ItemSerial) Then 'This is the item's container.
                    'Remove the item from its container's contents.
                    'Check if this is the container of the item.

                    'use the serial to find the item in the serial hash and remove it.
                    _ItemHashBySerial.Remove(ItemSerial)

                    'Remvoe it from the all items hash.
                    _MyLiteClient._AllItems.Remove(ItemSerial)

                Else 'This is not the item's container
                    'Perform a reverse lookup of the container's ITEM class using the GIANTSerialHash, 
                    'and have the item removed from its container's contents
                    DirectCast(_MyLiteClient._AllItems(ItemSerial).Container, Item).Contents.RemoveItem(ItemSerial)

                End If

            Catch ex As Exception
                Return False
            End Try

            Reset()

#If DebugItemList Then
            Console.WriteLine("-Item deleted successfuly: " & ItemSerial.ToString)
#End If

            Return True
        End Function

#End Region

#Region "Retrieving Items/Checking Existance of an Item"

        ''' <summary>
        ''' Returns the specified item, either by index or <see cref="serial"/>
        ''' </summary>
        ''' <param name="Serial">The serial of the item to be returned.</param>
        Public Property Item(ByVal Serial As Serial) As Item
            Get
                If _Serial = WorldSerial Then
                    Return DirectCast(_MyLiteClient._AllItems(Serial), Item)
                Else
                    Return DirectCast(_ItemHashBySerial(Serial), Item)
                End If
            End Get
            Friend Set(ByVal value As Item)
                _MyLiteClient._AllItems(Serial) = value
                _ItemHashBySerial(Serial) = value
            End Set
        End Property

        ''' <param name="OffSet">The offset of the item in the LiteClient's memory.</param>
        Friend ReadOnly Property Item(ByVal OffSet As Int32) As Item
            Get
                Return _ItemHashByOffset(OffSet)
            End Get
        End Property

        ''' <summary>
        ''' Checks to see if the specified item exists in the item list.
        ''' </summary>
        ''' <param name="Serial">The serial of the item you want to check the existance of.</param>
        Public ReadOnly Property Exists(ByVal Serial As Serial) As Boolean
            Get
                Try
                    If _Serial = WorldSerial Then
                        Return _MyLiteClient._AllItems.ContainsKey(Serial)
                    Else
                        Return _ItemHashBySerial.ContainsKey(Serial)
                    End If
                Catch ex As Exception
                    Return True
                End Try
            End Get
        End Property

        ''' <param name="Offset">The offset item you want to check the existance of.</param>
        Friend ReadOnly Property Exists(ByVal Offset As Int32) As Boolean
            Get
                Return _ItemHashByOffset.ContainsKey(Offset)
            End Get
        End Property

        ''' <param name="Item">The item you want to check the existance of.</param>
        Friend ReadOnly Property Exists(ByVal Item As Item) As Boolean
            Get
                Try
                    If _Serial = WorldSerial Then
                        Return _MyLiteClient._AllItems.ContainsValue(Item)
                    Else
                        Return _ItemHashBySerial.ContainsValue(Item)
                    End If
                Catch ex As Exception
                    Return True
                End Try
            End Get
        End Property

#End Region

#Region "Searching And Counting Items"


        Public ReadOnly Property byType(ByVal Type() As UShort, ByVal Recursive As Boolean) As ItemList
            Get
                Dim k As New ItemList

                For Each u As UShort In Type
                    For Each i As Item In byType(u, Recursive)
                        k.Add(i)
                    Next
                Next

                Return k
            End Get
        End Property

        ''' <summary>
        ''' Returns an itemlist containing the items of the specified type.
        ''' </summary>
        ''' <param name="Type">The type of item you want to search for.</param>
        Public ReadOnly Property byType(ByVal Type As UShort, ByVal Recursive As Boolean) As ItemList
            Get
                Dim j As New ItemList

                If Recursive Then
                    Console.WriteLine("Searching for type: " & Type)
                    byTypeRecursive(Type, _ParentItem, j)
                Else

                    'check each item in the hash 
                    For Each s As Serial In _ItemHashBySerial.Keys
                        'to see if its type maches the one specified.
                        If Me.Item(s).Type = Type Then
                            'Then add that to the itemlist to return
#If DebugItemList Then
                        Console.WriteLine("-Adding item to byType search result.")
                        Console.WriteLine(" Serial: " & s.ToString)
                        Console.WriteLine(" Type: " & Type.ToString)
#End If
                            j.Add(Me.Item(s))
                        End If
                    Next

                End If

                Return j
            End Get
        End Property

        Friend Sub byTypeRecursive(ByVal Type As UShort, ByVal BaseItem As Item, ByVal Itemlist As ItemList)

            For Each i As Item In BaseItem._contents
                'For each item, check if that item is the type we are looking for. If it is, then add it to the list.
                If i.Type = Type Then
                    Itemlist.Add(i)
                End If
            Next

            For Each i As Item In BaseItem._contents
                'Then search that item's contents for items of the specified type.
                i._contents.byTypeRecursive(Type, i, Itemlist)
            Next

        End Sub

        ''' <summary>
        ''' Returns the total number of items of the specified type. This takes stack count into consideration as well.
        ''' </summary>
        ''' <param name="Type">The type of item that you want the count of.</param>
        ''' <param name="Recursive">Set to true if you want it to count the items in sub containers.</param>
        Public ReadOnly Property CountByType(ByVal Type As UShort, ByVal Recursive As Boolean)
            Get
                Dim _count As Integer = 0

                If Recursive Then
                    For Each i As Item In _ParentItem.Contents.byType(Type, True)
                        _count += i.Amount
                    Next
                Else
                    For Each i As Item In _ParentItem.Contents.byType(Type, False)
                        _count += i.Amount
                    Next
                End If

                Return _count
            End Get
        End Property

#End Region

    End Class

End Class