Imports System.Diagnostics, Microsoft.Win32
Imports System.IO, System.Runtime.InteropServices

Partial Class UOAI

#If DEBUG Then
    ''' <summary>A list of Ultima Online clients.</summary>
    Public Class ClientList
        Implements Collections.Generic.ICollection(Of Client)

#Else
    ''' <summary>A list of Ultima Online clients.</summary>
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class ClientList
        Implements Collections.Generic.ICollection(Of Client)
#End If

#Region "Statements"

        Private ClientHash As New Hashtable
        Private proclist() As Process
        Friend ClientExe As String
        Friend ClientPath As String

#End Region

#Region "Constructors"
        Sub New()
            InitializeClientPaths() 'Taken almost directly from your uoai C#, Wim.
            ForceUpdateClientList()
        End Sub

        '''<summary>Gets the working directory and location of client.exe from the registry.</summary>
        Private Sub InitializeClientPaths()
            Dim hklm As RegistryKey = Registry.LocalMachine
            Dim originkey As RegistryKey = hklm.OpenSubKey("SOFTWARE\Origin Worlds Online\Ultima Online\1.0")

            If originkey Is Nothing Then
                originkey = hklm.OpenSubKey("SOFTWARE\Origin Worlds Online\Ultima Online Third Dawn\1.0")
            End If

            If originkey IsNot Nothing Then
                Dim instcdpath As String = DirectCast(originkey.GetValue("InstCDPath"), String)
                If instcdpath IsNot Nothing Then
                    ClientPath = instcdpath & "\"
                    ClientExe = "client.exe"
                    originkey.Close()
                    Exit Sub
                End If
                originkey.Close()
            End If

            'use default values
            ClientPath = "C:\Program Files\EA Games\Ultima Online Mondain's Legacy\"
            ClientExe = "client.exe"

            Exit Sub
        End Sub
#End Region

#Region "Functions"

        ''' <summary>Launches a new Client, returns true if successfull, false if not.</summary>
        Public Function LaunchClient() As Boolean
            Dim workingpath As String = ClientPath
            Dim ClientFullpath As String = ClientPath & ClientExe

            If File.Exists(ClientFullpath) Then
                'launch client suspended
                Dim pi As New [Imports].PROCESS_INFORMATION()
                Dim si As New [Imports].STARTUPINFO()
                Dim psa As New [Imports].SECURITY_ATTRIBUTES()
                Dim tsa As New [Imports].SECURITY_ATTRIBUTES()
                psa.nLength = Marshal.SizeOf(psa)
                tsa.nLength = Marshal.SizeOf(tsa)

                If [Imports].CreateProcess(ClientFullpath, "", psa, tsa, False, [Imports].CreationFlags.CREATE_SUSPENDED Or [Imports].CreationFlags.CREATE_NEW_CONSOLE, _
                 IntPtr.Zero, workingpath, si, pi) Then

                    UOClientDll.MultiClientPatch(CUInt(pi.dwProcessId))

                    'resume client
                    [Imports].ResumeThread(pi.hThread)

                    ForceUpdateClientList()

                    Return True
                End If
            End If

            Return False
        End Function

        ''' <summary>Find a UOAI.Client object by its associated Process ID.</summary>
        ''' <param name="ProcessID">The windows Process ID of the client you want to find.</param>
        ''' <returns>Returns a UOAI.Client, if it finds the correct one it returns that one.
        '''  Although if it doesn't find one matching the PID it will raise the "onError" event
        '''  and return the first client in the list.</returns>
        Public Function byPID(ByVal ProcessID As Integer) As Client
            Try
                Return ClientHash(ProcessID)
            Catch ex As Exception
                WriteErrorLog("No such client with PID: '" & ProcessID & "'. Returning nothing.")
                Throw New ApplicationException("No such client with PID: '" & ProcessID & "'. Returning nothing.")
                Return Nothing 'You lose, you get nothing!
            End Try
        End Function

#End Region

#Region "Properties"
        ''' <summary>Adds clients to the Clients arraylist, based on PID</summary>
        ''' <param name="PID">The process ID of the client you want to add.</param>
        Friend Shadows Sub Add(ByVal PID As Integer)
            Dim c As New UOAI.Client(PID)
            ClientHash.Add(PID, c)
        End Sub

        ''' <summary>Removed a client from the clients arraylist, based on PID.</summary>
        ''' <param name="PID">The process ID of the client you want to remove.</param>
        Friend Shadows Sub Remove(ByVal PID As Integer)
            DirectCast(ClientHash(PID), Client).CallEvent_onClientExit()
            ClientHash.Remove(PID)
        End Sub

        ''' <summary>Forces update of client list then returns a count of the clients.</summary>
        ''' <returns>An integer value representing the current number of UO clients running.</returns>
        Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of Client).Count
            Get
                ForceUpdateClientList()
                Return ClientHash.Count
            End Get
        End Property

        ''' <summary>Returns the UOAI.Client at the specified index.</summary>
        Default Public ReadOnly Property Client(ByVal Index As UInteger) As Client
            Get
                Return ClientHash.Values(Index)
            End Get
        End Property

#End Region

#Region "Sub's"
        ''' <summary>Forces the clientlist to update, called when UOAI.UOClientList.Count is called and on startup.</summary>
        Private Sub ForceUpdateClientList()
            'Update process list
            proclist = Process.GetProcessesByName(ClientExe.Split(".")(0))

            'Check for new clients
            For Each proc As Process In proclist

                'Just ensures it is a uo client, and the client isnt in the hash already.
                If proc.MainWindowTitle.Contains("Ultima Online") And ClientHash.ContainsKey(proc.Id) = False Then

                    'Make a new UOAI.Client with the PID and add it to the hash.
                    Add(proc.Id)

                    'Add an event handler for when the client closes.
                    proc.EnableRaisingEvents = True
                    AddHandler proc.Exited, AddressOf ClientProcessExit
                End If

            Next

            'Clients are removed from the list when they close.

        End Sub

        ''' <summary>Handles the "Process.Exited" for client processes.</summary>
        ''' <param name="sender">The System.Diagnostics.Process that is being handled.</param>
        ''' <param name="e">Nothing usefull.</param>
        Private Sub ClientProcessExit(ByVal sender As Object, ByVal e As System.EventArgs)
            Remove(sender.ID)
        End Sub

#End Region

#Region "Enumeration stuff"

        Private Sub Add1(ByVal item As Client) Implements System.Collections.Generic.ICollection(Of Client).Add

        End Sub

        Private Sub Clear() Implements System.Collections.Generic.ICollection(Of Client).Clear

        End Sub

        Private Function Contains(ByVal item As Client) As Boolean Implements System.Collections.Generic.ICollection(Of Client).Contains

        End Function

        Private Sub CopyTo(ByVal array() As Client, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of Client).CopyTo

        End Sub

        Private ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of Client).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Private Function Remove1(ByVal item As Client) As Boolean Implements System.Collections.Generic.ICollection(Of Client).Remove

        End Function

        Private Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of Client) Implements System.Collections.Generic.IEnumerable(Of Client).GetEnumerator
            Return ClientHash.Values.GetEnumerator
        End Function

        Private Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return ClientHash.Values.GetEnumerator
        End Function

#End Region

    End Class

End Class