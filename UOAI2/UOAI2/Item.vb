Partial Class UOAI

    Public Class Item

#Region "Constructor"
        Friend Sub New(ByVal Offset As Int32)
            MemoryOffset = Offset
        End Sub
#End Region

#Region "Variables"

        ''' <summary>Where the actual memory offset of the item is stored</summary>
        Private MemoryOffset As Int32

        ''' <summary>Item serial as UOAI.ItemSerial, private.</summary>
        Private _Serial As UInt32
        Private _AnimationType As Integer
        Private _Color As Integer
        Private _Direction As Integer
        Private _EntityType

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

#End Region

    End Class


End Class