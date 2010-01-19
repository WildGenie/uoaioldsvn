Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Runtime.InteropServices


Namespace UOAI
    'Process interface
    'public interface Process
    '    {
    '        uint PID { get; }
    '        uint Handle { get; }
    '        bool IsRunning { get; }
    '    }
    '
    '    public interface ProcessStream
    '    {
    '        byte[] Read(uint address, uint bytecount);
    '        uint ReadUInt(uint address);
    '        int ReadInt(uint address);
    '        ushort ReadUShort(uint address);
    '        short ReadShort(uint address);
    '        byte ReadByte(uint address);
    '        sbyte ReadChar(uint address);
    '        string ReadStr(uint address);
    '        string ReadUStr(uint address);
    '        string ReadStrn(uint address, uint length);
    '        string ReadUStrn(uint address, uint length);
    '        bool Write(uint address, byte[] towrite);
    '        bool WriteStr(uint address, string towrite);
    '        bool WriteUStr(uint address, string towrite);
    '        bool WriteUInt(uint address, uint towrite);
    '        bool WriteInt(uint address, int towrite);
    '        bool WriteShort(uint address, short towrite);
    '        bool WriteUShort(uint address, ushort towrite);
    '        bool WriteByte(uint address, byte towrite);
    '        bool WriteChar(uint address, sbyte towrite);
    '    }


    'default implementation of the process interface
    Public Class ProcessStream
        'private members
        Private m_PID As UInteger
        Private m_Handle As UInteger

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Public Shared Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, <Out()> ByVal lpBuffer() As Byte, ByVal dwSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Public Shared Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, <Out(), MarshalAs(UnmanagedType.AsAny)> ByVal lpBuffer As Object, ByVal dwSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Public Shared Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal lpBuffer As IntPtr, ByVal iSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Public Function GetExitCodeProcess(ByVal hProcess As IntPtr, ByRef lpExitCode As System.UInt32) As Boolean
        End Function

        'constructor(s)
        Public Sub New(ByVal PID As UInt32)
            m_PID = PID
            m_Handle = Process.GetProcessById(PID).Handle
            If m_Handle = 0 Then
                Throw New Exception("Failed to open a Client Process (pid = " & PID.ToString() & ")" & vbLf & "Try running this application with Administrator privileges!")
            End If
        End Sub

        'Process Interface implementation
        Public ReadOnly Property PID() As UInteger
            Get
                Return m_PID
            End Get
        End Property
        Public ReadOnly Property Handle() As UInteger
            Get
                Return m_Handle
            End Get
        End Property
        Public ReadOnly Property IsRunning() As Boolean
            Get
                Dim exitcode As UInt32

                If m_Handle <> 0 Then
                    If GetExitCodeProcess(m_Handle, exitcode) Then
                        If exitcode = 259 Then
                            'STILL_ACTIVE
                            Return True
                        End If
                    End If
                End If

                Return False
            End Get
        End Property

        Public Function Read(ByVal address As UInteger, ByVal bytecount As UInteger) As Byte()
            Dim toreturn As Byte() = Nothing
            Dim prevprotect As UInt32
            Dim bytesread As UInt32

            If VirtualProtectEx(m_Handle, address, bytecount, PAGE_READWRITE, prevprotect) Then
                toreturn = New Byte(bytecount - 1) {}

                ReadProcessMemory(m_Handle, address, toreturn, bytecount, bytesread)

                VirtualProtectEx(m_Handle, address, bytecount, prevprotect, prevprotect)
            End If

            Return toreturn
        End Function
        Public Function ReadUInt(ByVal address As UInteger) As UInteger
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToUInt32(bytes, 0)
            Else
                Return 0

            End If
        End Function
        Public Function ReadInt(ByVal address As UInteger) As Integer
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToInt32(bytes, 0)
            Else
                Return 0
            End If
        End Function
        Public Function ReadUShort(ByVal address As UInteger) As UShort
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToUInt16(bytes, 0)
            Else
                Return 0
            End If
        End Function
        Public Function ReadShort(ByVal address As UInteger) As Short
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return BitConverter.ToInt16(bytes, 0)
            Else
                Return 0
            End If
        End Function
        Public Function ReadByte(ByVal address As UInteger) As Byte
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return bytes(0)
            Else
                Return 0
            End If
        End Function
        Public Function ReadChar(ByVal address As UInteger) As SByte
            Dim bytes As Byte() = Read(address, 4)
            If bytes IsNot Nothing Then
                Return CSByte(bytes(0))
            Else
                Return 0
            End If
        End Function
        Public Function ReadStr(ByVal address As UInteger) As String
            Dim characters As Char() = New Char(255) {}

            Dim i As UInt32 = 0

            While ((InlineAssignHelper(characters(i), CChar(ReadByte(address + i)))) <> 0) AndAlso (i < 256)
                i += 1
            End While

            Return New String(characters, 0, CInt(i))
        End Function
        Public Function ReadUStr(ByVal address As UInteger) As String
            Dim characters As Char() = New Char(255) {}

            Dim i As UInt32 = 0

            While ((InlineAssignHelper(characters(i), CChar(ReadUShort(address + i)))) <> 0) AndAlso (i < 256)
                i += 1
            End While

            Return New String(characters, 0, CInt(i))
        End Function
        Public Function ReadStrn(ByVal address As UInteger, ByVal length As UInteger) As String
            Dim characters As Char() = New Char(length - 1) {}

            For i As UInt32 = 0 To length - 1
                characters(i) = CChar(ReadByte(address + i))
            Next

            Return New String(characters)
        End Function
        Public Function ReadUStrn(ByVal address As UInteger, ByVal length As UInteger) As String
            Dim characters As Char() = New Char(length - 1) {}

            For i As UInt32 = 0 To length - 1
                characters(i) = CChar(ReadShort(address + i))
            Next

            Return New String(characters)
        End Function
        Public Function Write(ByVal address As UInteger, ByVal towrite As Byte()) As Boolean
            Dim prevprotect As UInt32
            Dim byteswritten As UInt32
            Dim bytecount As UInt32 = DirectCast(towrite.Count(), UInt32)

            If VirtualProtectEx(m_Handle, address, bytecount, PAGE_READWRITE, prevprotect) Then
                If WriteProcessMemory(m_Handle, address, towrite, bytecount, byteswritten) Then
                    If byteswritten = bytecount Then
                        VirtualProtectEx(m_Handle, address, bytecount, prevprotect, prevprotect)
                        Return True
                    End If
                End If

                VirtualProtectEx(m_Handle, address, bytecount, prevprotect, prevprotect)
            End If

            Return False
        End Function
        Public Function WriteStr(ByVal address As UInteger, ByVal towrite As String) As Boolean
            Dim bytes As Byte() = ASCIIEncoding.ASCII.GetBytes(towrite)

            Write(address, bytes)
            Write(address + DirectCast(Convert.ToUInt32(bytes.Length), UInt32), New Byte() {0})
            'null termination
            Return True
        End Function
        Public Function WriteUStr(ByVal address As UInteger, ByVal towrite As String) As Boolean
            Dim bytes As Byte() = New Byte((towrite.Length + 1) * 2 - 1) {}
            For i As Integer = 0 To towrite.Length - 1
                bytes(i * 2) = CByte((towrite(i) Mod 256))
                bytes(i * 2 + 1) = CByte((towrite(i) / 256))
            Next
            bytes(towrite.Length * 2) = 0
            bytes(towrite.Length * 2 + 1) = 0
            Return Write(address, bytes)
        End Function
        Public Function WriteUInt(ByVal address As UInteger, ByVal towrite As UInteger) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function
        Public Function WriteInt(ByVal address As UInteger, ByVal towrite As Integer) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function
        Public Function WriteShort(ByVal address As UInteger, ByVal towrite As Short) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function
        Public Function WriteUShort(ByVal address As UInteger, ByVal towrite As UShort) As Boolean
            Return Write(address, BitConverter.GetBytes(towrite))
        End Function
        Public Function WriteByte(ByVal address As UInteger, ByVal towrite As Byte) As Boolean
            Return Write(address, New Byte() {towrite})
        End Function
        Public Function WriteChar(ByVal address As UInteger, ByVal towrite As SByte) As Boolean
            Return Write(address, New Byte() {CByte(towrite)})
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace
