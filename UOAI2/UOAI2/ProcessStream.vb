Imports System.Text

Partial Class UOAI

    'default implementation of the process interface
    Friend Class ProcessStream
        'private members
        Private m_PID As UInteger
        Private m_Handle As UInteger

        'constructor(s)
        Sub New(ByVal PID As UInt32)
            m_PID = PID
            m_Handle = [Imports].OpenProcess([Imports].ProcessAccess.VMRead Or [Imports].ProcessAccess.VMWrite Or [Imports].ProcessAccess.QueryInformation Or [Imports].ProcessAccess.CreateThread Or [Imports].ProcessAccess.VMOperation, False, PID)
            If m_Handle = 0 Then
                Throw New Exception("Failed to open a Client Process (pid = " & PID.ToString() & ")" & vbLf & "Try running this application with Administrator privileges!")
            End If
        End Sub

        'Process Interface implementation
        ReadOnly Property PID() As UInteger
            Get
                Return m_PID
            End Get
        End Property

        ReadOnly Property Handle() As UInteger
            Get
                Return m_Handle
            End Get
        End Property

        ReadOnly Property IsRunning() As Boolean
            Get
                Dim exitcode As UInt32

                If m_Handle <> 0 Then
                    If [Imports].GetExitCodeProcess(m_Handle, exitcode) Then
                        If exitcode = 259 Then
                            'STILL_ACTIVE
                            Return True
                        End If
                    End If
                End If

                Return False
            End Get
        End Property

        Function Read(ByVal address As UInteger, ByVal bytecount As UInteger) As Byte()
            Dim toreturn As Byte() = Nothing
            Dim prevprotect As UInt32
            Dim bytesread As UInt32

            If [Imports].VirtualProtectEx(m_Handle, address, bytecount, [Imports].PAGE_READWRITE, prevprotect) Then
                toreturn = New Byte(bytecount - 1) {}

                [Imports].ReadProcessMemory(m_Handle, address, toreturn, bytecount, bytesread)

                [Imports].VirtualProtectEx(m_Handle, address, bytecount, prevprotect, prevprotect)
            End If

            Return toreturn
        End Function

        Function ReadUInt(ByVal address As UInteger) As UInteger
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToUInt32(bytes, 0)
            Else
                Return 0

            End If
        End Function
        Function ReadInt(ByVal address As UInteger) As Integer
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToInt32(bytes, 0)
            Else
                Return 0
            End If
        End Function

        Function ReadUShort(ByVal address As UInteger) As UShort
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToUInt16(bytes, 0)
            Else
                Return 0
            End If
        End Function

        Function ReadShort(ByVal address As UInteger) As Short
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToInt16(bytes, 0)
            Else
                Return 0
            End If
        End Function

        Function ReadByte(ByVal address As UInteger) As Byte
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return bytes(0)
            Else
                Return 0
            End If
        End Function

        Function ReadChar(ByVal address As UInteger) As Byte
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return bytes(0)
            Else
                Return 0
            End If
        End Function

        Function ReadStr(ByVal address As UInteger) As String
            Dim characters As Char() = New Char(255) {}

            Dim i As UInt32 = 0

            While (BitConverter.GetBytes((InlineAssignHelper(characters(i), Chr(ReadUShort(address + i)))))(0) <> 0) AndAlso (i < 256)
                i += 1
            End While

            Return New String(characters, 0, CInt(i))
        End Function

        Function ReadUStr(ByVal address As UInteger) As String
            Dim characters As Char() = New Char(255) {}

            Dim i As UInt32 = 0

            While (BitConverter.GetBytes((InlineAssignHelper(characters(i), Chr(ReadUShort(address + i)))))(0) <> 0) AndAlso (i < 256)
                i += 1
            End While

            Return New String(characters, 0, CInt(i))
        End Function

        Function ReadStrn(ByVal address As UInteger, ByVal length As UInteger) As String
            Dim characters As Char() = New Char(length - 1) {}

            For i As UInt32 = 0 To length - 1
                characters(i) = Chr(ReadByte(address + i))
            Next

            Return New String(characters)
        End Function

        Function ReadUStrn(ByVal address As UInteger, ByVal length As UInteger) As String
            Dim characters As Char() = New Char(length - 1) {}

            For i As UInt32 = 0 To length - 1
                characters(i) = Chr(ReadShort(address + i))
            Next

            Return New String(characters)
        End Function

        Function Write(ByVal address As UInteger, ByVal towrite As Byte()) As Boolean
            Dim prevprotect As UInt32
            Dim byteswritten As UInt32
            Dim bytecount As UInt32 = System.Convert.ToUInt32(towrite.Count())

            If [Imports].VirtualProtectEx(m_Handle, address, bytecount, [Imports].PAGE_READWRITE, prevprotect) Then
                If [Imports].WriteProcessMemory(m_Handle, address, towrite, bytecount, byteswritten) Then
                    If byteswritten = bytecount Then
                        [Imports].VirtualProtectEx(m_Handle, address, bytecount, prevprotect, prevprotect)
                        Return True
                    End If
                End If

                [Imports].VirtualProtectEx(m_Handle, address, bytecount, prevprotect, prevprotect)
            End If

            Return False
        End Function

        Function WriteStr(ByVal address As UInteger, ByVal towrite As String) As Boolean
            Dim bytes As Byte() = ASCIIEncoding.ASCII.GetBytes(towrite)

            Write(address, bytes)
            Write(address + System.Convert.ToUInt32(bytes.Length), New Byte() {0})
            'null termination
            Return True
        End Function

        Function WriteUStr(ByVal address As UInteger, ByVal towrite As String) As Boolean
            Dim bytes As Byte() = New Byte((towrite.Length + 1) * 2 - 1) {}
            For i As Integer = 0 To towrite.Length - 1
                bytes(i * 2) = CByte((BitConverter.GetBytes(towrite(i))(0) Mod 256))
                bytes(i * 2 + 1) = CByte((BitConverter.GetBytes(towrite(i))(0) / 256))
            Next

            bytes(towrite.Length * 2) = 0
            bytes(towrite.Length * 2 + 1) = 0
            Return Write(address, bytes)
        End Function

        Function WriteUInt(ByVal address As UInteger, ByVal towrite As UInteger) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function

        Function WriteInt(ByVal address As UInteger, ByVal towrite As Integer) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function

        Function WriteShort(ByVal address As UInteger, ByVal towrite As Short) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function

        Function WriteUShort(ByVal address As UInteger, ByVal towrite As UShort) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function

        Function WriteByte(ByVal address As UInteger, ByVal towrite As Byte) As Boolean
            Return Write(address, New Byte() {towrite})
        End Function

        Function WriteChar(ByVal address As UInteger, ByVal towrite As Byte) As Boolean
            Return Write(address, New Byte() {towrite})
        End Function

        Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
            target = value
            Return value
        End Function

    End Class
End Class
