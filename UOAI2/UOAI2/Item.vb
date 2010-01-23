Partial Class UOAI

    Public Class Item

#Region "Constructor"
        Friend Sub New(ByVal Offset As Int32)
            MemoryOffset = Offset
        End Sub
#End Region

#Region "Variables"

        ''' <summary>Where the actual memory offset of the item is stored</summary>
        Friend MemoryOffset As Int32

        ''' <summary>Item serial as UOAI.ItemSerial, private.</summary>
        Friend _Serial As UInt32
        Friend _Artwork As UShort
        Friend _StackID As Byte
        Friend _Ammount As UShort
        Friend _X As UShort
        Friend _Y As UShort
        Friend _Container As UInt32
        Friend _Hue As UShort

#End Region

#Region "Properties"

        ''' <summary>The 32-bit memory offset for the item in the client memory.</summary>
        Friend ReadOnly Property Offset() As Int32
            Get
                Return MemoryOffset
            End Get
        End Property

        Public ReadOnly Property Serial() As UInt32
            Get
                Return _serial
            End Get
        End Property

        Public ReadOnly Property Artwork() As UShort
            Get
                Return _Artwork
            End Get
        End Property

        Public ReadOnly Property StackID() As Byte
            Get
                Return _StackID
            End Get
        End Property

        Public ReadOnly Property Ammount() As Byte
            Get
                Return _Ammount
            End Get
        End Property

#End Region

    End Class


End Class