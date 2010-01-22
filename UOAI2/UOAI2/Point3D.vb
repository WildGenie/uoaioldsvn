Partial Class UOAI
    ''' <summary>Represents an in-game 3D location.</summary>
    Public Structure Point3D
        Implements IComparable
        Implements IComparable(Of Point3D)
        Friend ReadOnly m_X As Integer
        Friend ReadOnly m_Y As Integer
        Friend ReadOnly m_Z As Integer

        Public Shared ReadOnly Zero As New Point3D(0, 0, 0)

        ''' <summary>
        ''' Constructs a Point given its coordinates.
        ''' </summary>
        ''' <param name="x">X-coordinate</param>
        ''' <param name="y">Y-coordinate</param>
        ''' <param name="z">Z-coordinate</param>
        Public Sub New(ByVal x As Integer, ByVal y As Integer, ByVal z As Integer)
            m_X = x
            m_Y = y
            m_Z = z
        End Sub

        ''' <summary>
        ''' X coordinate
        ''' </summary>
        Public ReadOnly Property X() As Integer
            Get
                Return m_X
            End Get
        End Property

        ''' <summary>
        ''' Y coordinate
        ''' </summary>
        Public ReadOnly Property Y() As Integer
            Get
                Return m_Y
            End Get
        End Property

        ''' <summary>
        ''' Z coordinate
        ''' </summary>
        Public ReadOnly Property Z() As Integer
            Get
                Return m_Z
            End Get
        End Property

        ''' <summary>
        ''' String representation of this location.
        ''' </summary>
        ''' <returns>String representation of this location.</returns>
        Public Overloads Overrides Function ToString() As String
            Return [String].Format("({0}, {1}, {2})", m_X, m_Y, m_Z)
        End Function

        Public Overloads Overrides Function Equals(ByVal o As Object) As Boolean
            If o Is Nothing OrElse Not (TypeOf o Is Point3D) Then
                Return False
            End If

            Dim p As Point3D = DirectCast(o, Point3D)

            Return m_X = p.X AndAlso m_Y = p.Y AndAlso m_Z = p.Z
        End Function

        Public Overloads Overrides Function GetHashCode() As Integer
            Return m_X Xor m_Y Xor m_Z
        End Function

        Public Shared Operator =(ByVal l As Point3D, ByVal r As Point3D) As Boolean
            Return l.m_X = r.m_X AndAlso l.m_Y = r.m_Y AndAlso l.m_Z = r.m_Z
        End Operator

        Public Shared Operator <>(ByVal l As Point3D, ByVal r As Point3D) As Boolean
            Return l.m_X <> r.m_X OrElse l.m_Y <> r.m_Y OrElse l.m_Z <> r.m_Z
        End Operator

        Public Function CompareTo(ByVal other As Point3D) As Integer Implements IComparable(Of UOAI2.UOAI.Point3D).CompareTo
            Dim v As Integer = (m_X.CompareTo(other.m_X))

            If v = 0 Then
                v = (m_Y.CompareTo(other.m_Y))

                If v = 0 Then
                    v = (m_Z.CompareTo(other.m_Z))
                End If
            End If

            Return v
        End Function

        Public Function CompareTo(ByVal other As Object) As Integer Implements IComparable.CompareTo
            If TypeOf other Is Point3D Then
                Return Me.CompareTo(DirectCast(other, Point3D))
            ElseIf other Is Nothing Then
                Return -1
            End If

            Throw New ArgumentException()
        End Function
    End Structure


End Class