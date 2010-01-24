Partial Class UOAI

    'Hide this class from the user, there is no reason from him/her to see it.
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class MobileList
        Implements IEnumerable
        Private _MobileHashBySerial As Hashtable
        Private _MobileHashByOffset As Hashtable
        Private _Client As Client
        Private _IsSearchResult As Boolean = False

        Friend Sub New(ByRef Client As Client)
            _Client = Client
        End Sub

        Friend Sub New()
            _IsSearchResult = True
        End Sub

        Public Sub AddMobile(ByVal Mobile As Mobile)
            _MobileHashBySerial.Add(Mobile.Serial, Mobile)
            _MobileHashByOffset.Add(Mobile.MemoryOffset, Mobile)
        End Sub

        ''' <summary>
        ''' Removes the specified mobile formt he MobileList.
        ''' </summary>
        ''' <param name="Serial">The serial of the mobile to be removed.</param>
        Public Function RemoveMobile(ByVal Serial As Serial)
            Try
                _MobileHashByOffset.Remove(DirectCast(_MobileHashBySerial(Serial), Mobile).MemoryOffset)
                _MobileHashBySerial.Remove(Serial)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function

        ''' <param name="Offset">The memory offset of the mobile to be removed.</param>
        Public Function RemoveMobile(ByVal Offset As UInt32)
            Try
                _MobileHashBySerial.Remove(DirectCast(_MobileHashBySerial(Offset), Mobile).Serial)
                _MobileHashByOffset.Remove(Offset)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function

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

        Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _MobileHashBySerial.Values.GetEnumerator()
        End Function
    End Class

End Class
