Partial Class UOAI

    Public Class Serial
        Implements IComparable
        Implements IComparable(Of Serial)
        Public Shared ReadOnly MinusOne As New Serial(Convert.ToUInt32(4294967295))
        Public Shared ReadOnly Zero As New Serial(Convert.ToUInt32(0))

        Private m_IntValue As New UInt32

        Public Sub New(ByVal val As UInt32)
            m_IntValue = val
        End Sub

        Public Property Value() As UInt32
            Get
                Return m_IntValue
            End Get
            Friend Set(ByVal value As UInt32)
                m_IntValue = value
            End Set
        End Property

        Public ReadOnly Property IsValid() As Boolean
            Get
                Return ((m_IntValue > 0) AndAlso (m_IntValue <> &HFFFFFFFF))
            End Get
        End Property

        Public Overloads Overrides Function GetHashCode() As Integer
            'Console.Writeline(m_IntValue.GetHashCode.ToString)
            Return CInt(m_IntValue.GetHashCode)
        End Function

        Public Function CompareTo(ByVal other As Serial) As Integer Implements IComparable(Of Serial).CompareTo
            Return m_IntValue.CompareTo(other.m_IntValue)
        End Function

        Public Function CompareTo(ByVal other As Object) As Integer Implements IComparable.CompareTo
            If TypeOf other Is Serial Then
                Return Me.CompareTo(DirectCast(other, Serial))
            ElseIf other Is Nothing Then
                Return -1
            End If

            Throw New ArgumentException()
        End Function

        Public Overloads Overrides Function Equals(ByVal o As Object) As Boolean
            If o Is Nothing OrElse Not (TypeOf o Is Serial) Then
                Return False
            End If

            Return DirectCast(o, Serial).m_IntValue = m_IntValue
        End Function

        Public Overloads Shared Operator =(ByVal l As Serial, ByVal r As Serial) As Boolean
            Return l.m_IntValue = r.m_IntValue
        End Operator

        Public Overloads Shared Operator <>(ByVal l As Serial, ByVal r As Serial) As Boolean
            Return l.m_IntValue <> r.m_IntValue
        End Operator

        Public Overloads Shared Operator >(ByVal l As Serial, ByVal r As Serial) As Boolean
            Return l.m_IntValue > r.m_IntValue
        End Operator

        Public Overloads Shared Operator <(ByVal l As Serial, ByVal r As Serial) As Boolean
            Return l.m_IntValue < r.m_IntValue
        End Operator

        Public Overloads Shared Operator >=(ByVal l As Serial, ByVal r As Serial) As Boolean
            Return l.m_IntValue >= r.m_IntValue
        End Operator

        Public Overloads Shared Operator <=(ByVal l As Serial, ByVal r As Serial) As Boolean
            Return l.m_IntValue <= r.m_IntValue
        End Operator

        Public Function ToEasyUOString() As String
            Dim euostring As New List(Of Char)()
            Dim i As UInteger
            Dim cA As Char = "A"c

            i = (CUInt(m_IntValue) Xor &H45) + 7
            While i <> 0
                euostring.Add(Chr(((i Mod 26) + Convert.ToInt16(cA))))
                i /= 26
            End While

            Return New String(euostring.ToArray())
        End Function

        Public Function ToRazorString() As String
            Return BitConverter.ToString(BitConverter.GetBytes(m_IntValue)).Replace("-", "")
        End Function

        Public Overloads Overrides Function ToString() As String
            Return m_IntValue.ToString
        End Function

        Public Shared Widening Operator CType(ByVal a As Serial) As UInt32
            Return a.m_IntValue
        End Operator

        Public Shared Widening Operator CType(ByVal a As UInteger) As Serial
            Return New Serial(a)
        End Operator

        Public Shared Widening Operator CType(ByVal s As String) As Serial
            Return New Serial(s)
        End Operator
    End Class

End Class