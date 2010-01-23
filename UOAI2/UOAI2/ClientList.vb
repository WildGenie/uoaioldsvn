Imports System.Diagnostics, Microsoft.Win32
Imports System.IO, System.Runtime.InteropServices

Partial Class UOAI

    ''' <summary>A list of Ultima Online clients.</summary>
    Public Class ClientList

#Region "Statements"

        Private proclist() As Process
        Private Clients As New ArrayList
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
                    'TODO:get this to work.
                    'apply multiclient patch
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
            For Each i As Client In Clients
                If i.PID = ProcessID Then Return i
            Next

            Throw New ApplicationException("No such client with PID: '" & ProcessID & "'. Returning client(0) to avoid null-reference exception")
            Return Nothing 'You lose, you get nothing!
        End Function


#End Region

#Region "Properties"
        ''' <summary>Adds clients to the Clients arraylist, based on PID</summary>
        ''' <param name="PID">The process ID of the client you want to add.</param>
        Friend Shadows Sub Add(ByVal PID As Integer)
            Dim c As New UOAI.Client(PID)
            Clients.Add(c)
        End Sub

        ''' <summary>Removed a client from the clients arraylist, based on PID.</summary>
        ''' <param name="PID">The process ID of the client you want to remove.</param>
        Friend Shadows Sub Remove(ByVal PID As Integer)
            For Each c As UOAI.Client In Clients
                If c.PID = PID Then
                    Clients.Remove(c)
                    c.CallEvent_onClientExit()
                    Exit Sub
                End If
            Next
        End Sub

        ''' <summary>Forces update of client list then returns a count of the clients.</summary>
        ''' <returns>An integer value representing the current number of UO clients running.</returns>
        Public ReadOnly Property Count() As Integer
            Get
                ForceUpdateClientList()
                Return Clients.Count
            End Get
        End Property

        Default Public ReadOnly Property Item(ByVal index As Integer) As Client
            Get
                Return Clients(index)
            End Get
        End Property

        ''' <summary>Returns the UOAI.Client at the specified index.</summary>
        Public ReadOnly Property Client() As Client()
            Get
                Return Clients.ToArray(GetType(Client))
            End Get
        End Property

#End Region

#Region "Sub's"
        ''' <summary>Forces the clientlist to update, called when UOAI.UOClientList.Count is called and on startup.</summary>
        ''' <remarks></remarks>
        Private Sub ForceUpdateClientList()
            'Update process list
            proclist = Process.GetProcessesByName(ClientExe.Split(".")(0))

            Dim ProcChk As Boolean = False

            'Check for clients
            For Each proc As Process In proclist
                'Just ensures it is a uo client
                If proc.MainWindowTitle.Contains("Ultima Online") Then
                    'See If the client is already in the list, if not then add it.
                    For Each c As Client In Clients
                        'If the PID of "proc" = PID of any of the clients in "Clients" then
                        'procchk is set to true.
                        If c.PID = proc.Id Then ProcChk = True
                    Next

                    'Instatiate the "NewClient" from before, set it's PID and add it to the list.
                    If ProcChk = False Then
                        Add(proc.Id)

                        'Add an event handler for when the client closes.
                        proc.EnableRaisingEvents = True
                        AddHandler proc.Exited, AddressOf ClientProcessExit

                    Else
                        ProcChk = False
                        Continue For
                    End If

                End If
            Next
        End Sub

        ''' <summary>Handles the "Process.Exited" for client processes.</summary>
        ''' <param name="sender">The System.Diagnostics.Process that is being handled.</param>
        ''' <param name="e">Nothing usefull.</param>
        Private Sub ClientProcessExit(ByVal sender As Object, ByVal e As System.EventArgs)
            Remove(sender.ID)
        End Sub
#End Region


    End Class
End Class