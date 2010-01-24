Partial Class UOAI

    Public Class Item

#Region "Constructor"
        Friend Sub New(ByVal Client As Client) 'New(byval offset as int32)
            _contents = New ItemList(Me, Client)
        End Sub
#End Region

#Region "Variables"

        ''' <summary>Where the actual memory offset of the item is stored</summary>
        Friend MemoryOffset As Int32

        ''' <summary>Item serial as UOAI.ItemSerial, private.</summary>
        Friend _Serial As Serial
        Friend _Type As ItemType
        Friend _StackID As Byte
        Friend _Amount As UShort
        Friend _X As UShort
        Friend _Y As UShort
        Friend _Z As SByte
        Friend _Container As Serial
        Friend _Hue As UShort
        Friend _Direction As Enums.Direction
        Friend _contents As ItemList

#End Region

#Region "Properties"

        ''' <summary>The serial of the item.</summary>
        Public ReadOnly Property Serial() As Serial
            Get
                Return _Serial
            End Get
        End Property

        ''' <summary>The artwork number of that item. This is what determines what it looks like in game.</summary>
        Public ReadOnly Property Type() As ItemType
            Get
                Return _Type
            End Get
        End Property

        ''' <summary>The number to add the the artwork number to get the artwork number of the item if it is a stack. 
        ''' Usualy this is 0x01.</summary>
        Public ReadOnly Property StackID() As Byte
            Get
                Return _StackID
            End Get
        End Property

        ''' <summary>The number of objects in a stack.</summary>
        Public ReadOnly Property Amount() As Byte
            Get
                Return _Amount
            End Get
        End Property

        ''' <summary>The location of the item on the X axis. If the item is inside of a container, 
        ''' this represents the number of pixels within the container from the left side at which 
        ''' the item will be placed.</summary>
        Public ReadOnly Property X() As UShort
            Get
                Return _X
            End Get
        End Property

        ''' <summary>The location of the item on the Y axis. If the item is inside of a container, 
        ''' this represents the number of pixels from the top of the container that the item will 
        ''' be placed</summary>
        Public ReadOnly Property Y() As UShort
            Get
                Return _Y
            End Get
        End Property

        ''' <summary>The serial of the container of the item.</summary>
        Public ReadOnly Property Container() As Serial
            Get
                Return _Container
            End Get
        End Property

        ''' <summary>The item's hue.</summary>
        Public ReadOnly Property Hue() As UShort
            Get
                Return _Hue
            End Get
        End Property

        ''' <summary>
        ''' Returns a string containing the ASCII name of the item artwork name.
        ''' </summary>
        Public ReadOnly Property TypeName() As String
            Get
                Return StrLst.Table(1040391 + _Type.BaseValue).ToString()
            End Get
        End Property

        Public ReadOnly Property Contents() As ItemList
            Get
                Return _contents
            End Get
        End Property

#End Region

    End Class

    Friend Class Container
        'Private _

    End Class


End Class