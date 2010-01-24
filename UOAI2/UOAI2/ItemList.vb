Imports System.Collections
Imports System.Threading

Partial Class UOAI

    ''' <summary>Contains a list of UOAI.Item's</summary>
    Public Class ItemList
        Implements IEnumerable
        Private _ItemHashBySerial As Hashtable
        Private _ItemHashByType As Hashtable
        Private _ItemHashByOffset As Hashtable
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
        Friend Sub New(ByVal Serial As Serial)
            _Serial = Serial
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
            'Check if this is instance of itemlist is a result of a search
            If _SearchReturn = True Then 'dont add search results to the giant hash!
                _ItemHashBySerial.Add(Item.Serial, Item)
                _ItemHashByOffset.Add(Item.MemoryOffset, Item)
            Else 'add it to the giant hash for later recursive removal.

                'if the items container is this container, then add it to the hash
                If Item.Container = _Serial Then
                    _ItemHashBySerial.Add(Item.Serial, Item)
                    _ItemHashByOffset.Add(Item.MemoryOffset, Item)

                    'Add the hash to the GIANTSerialHash, for later reverse lookup.
                    _MyClient.GIANTSerialHash.Add(Item.Serial, _ParentItem)

                    'Add it to the client's Allitem list, for access to as a container.
                    _MyClient._AllItems.AddItem(Item)
                Else
                    'If this item's container is NOT this container,
                    'then look up the container by Serial in the client's master item list
                    'and add the item to that items contents.
                    _MyClient._AllItems.bySerial(Item.Container).Contents.AddItem(Item)
                End If
            End If

        End Sub

        ''' <summary>
        ''' Attempts to remove an item from the item list by its Serial. Returns true if it found the item, returns false if it didn't.
        ''' </summary>
        ''' <param name="ItemSerial">The serial of the item to be removed.</param>
        '''<param name="ContainerSerial">The serial of the Container.</param>
        Friend Function RemoveItem(ByVal ItemSerial As UOAI.Serial, ByVal ContainerSerial As Serial) As Boolean
            Try
                If _SearchReturn = True Then
                    'use the serial hash to locat the item inside the offset hash and remove it
                    _ItemHashByOffset.Remove(DirectCast(_ItemHashBySerial(ItemSerial), Item).MemoryOffset)
                    'use the serial to find the item in the serial hash and remove it.
                    _ItemHashBySerial.Remove(ItemSerial)
                Else
                    'Remove the item from its container's contents.
                    'Check if this is the container of the item.
                    If Exists(ItemSerial) Then 'This is the item's container.
                        'use the serial hash to locat the item inside the offset hash and remove it
                        _ItemHashByOffset.Remove(DirectCast(_ItemHashBySerial(ItemSerial), Item).MemoryOffset)
                        'use the serial to find the item in the serial hash and remove it.
                        _ItemHashBySerial.Remove(ItemSerial)

                        'Remove it from the giant hash using the serial
                        _MyClient.GIANTSerialHash.Remove(ContainerSerial)

                        'Remvoe it from the all items hash.
                        _MyClient._AllItems.RemoveItem(ItemSerial, ContainerSerial)

                    Else 'This is not the item's container
                        'Perform a reverse lookup of the container's ITEM class using the GIANTSerialHash, 
                        'and has the item removed from its container's contents
                        DirectCast(_MyClient.GIANTSerialHash(ContainerSerial), Item).Contents.RemoveItem(ItemSerial, ContainerSerial)

                    End If
                End If
            Catch ex As Exception
                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Returns the specified item, either by index or <see cref="UOAI.serial"/>
        ''' </summary>
        ''' <param name="Serial">The serial of the item to be returned.</param>
        Public ReadOnly Property bySerial(ByVal Serial As UOAI.Serial) As UOAI.Item
            Get
                Return _ItemHashBySerial(Serial)
            End Get
        End Property

        ''' <param name="OffSet">The offset of the item in the client's memory.</param>
        Friend ReadOnly Property byOffset(ByVal OffSet As Int32) As UOAI.Item
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
                    If Me.bySerial(s).Type = Type Then
                        'Then add that to the itemlist to return
                        k.AddItem(Me.bySerial(s))
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
                Return _ItemHashBySerial.ContainsKey(Serial)
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
                Return _ItemHashBySerial.ContainsValue(Item)
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