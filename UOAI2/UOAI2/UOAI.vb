Imports System.Diagnostics, Microsoft.Win32, System.IO, System.Runtime.InteropServices

Public Class UOAI

#Region "UOAI Variables"
    Private ClientList As New UOClientList
    Shared UOClientDllPath As String = My.Application.Info.DirectoryPath
#End Region

#Region "UOAI Properties"

    ''' <summary>The list of running Ultima Online 2D clients as UOAI.Client objects.</summary>
    Public ReadOnly Property Clients() As UOClientList
        Get
            Return ClientList
        End Get
    End Property

    ''' <summary>Gets or sets the path of the client executable, usualy something like 
    ''' "C:\Program Files\EA Games\Ultima Online Mondain's Legacy\". This is also the 
    ''' working directory for the clients launched using UOAI.</summary>
    ''' <returns>The path to the client executable.</returns>
    Public Property ClientPath() As String
        Get
            Return ClientList.ClientPath
        End Get
        Set(ByVal value As String)
            ClientList.ClientPath = value
        End Set
    End Property

    ''' <summary>Gets or sets the name of the client executable, normally "client.exe"</summary>
    ''' <returns>The name of the client executable.</returns>
    Public Property ClientExe() As String
        Get
            Return ClientList.ClientExe
        End Get
        Set(ByVal ExeName As String)
            ClientList.ClientExe = ExeName
        End Set
    End Property

#End Region

#Region "UOAI Constructor"
    Sub New()
        'Checks for to see if the current user is part of the administrators group, throws an exception if it fails
        If My.User.IsInRole(Microsoft.VisualBasic.ApplicationServices.BuiltInRole.Administrator) = False Then
            Throw New ApplicationException("You need to be part of the administrators group to use UOAI2.")
        End If

    End Sub
#End Region

#Region "UOAI Events"
    Public Event onError(ByVal ErrorText As String)

    Private Sub RaiseErrorEvent(ByVal Message As String)
        RaiseEvent onError(Message)
    End Sub
#End Region

    ''' <summary>Represents an Ultima Online client.</summary>
    Public Class Client
        'Inherits ProcessStream

        Private ProcessID As Integer
        Friend InjectedDll As UOClientDll
        Friend PStream As ProcessStream

        ''' <summary>
        ''' Called when the client process closes.
        ''' </summary>
        ''' <param name="Client">The client that exited, for multi-clienting event handlers in VB.NET</param>
        Public Event onClientExit(ByVal Client As Client)

        Sub New(ByVal PID As Integer)
            'TODO: add injection code here.
            PStream = New ProcessStream(PID)
        End Sub

        ''' <summary>
        ''' Gets the windows process ID of the client. This is used as the unique identifier for each client running.
        ''' </summary>
        ''' <value>The windows Process ID of the client.</value>
        Public Property PID() As Integer
            Get
                Return ProcessID
            End Get
            Friend Set(ByVal value As Integer)
                ProcessID = value
            End Set
        End Property

        ''' <summary>Causes the UOClient object to raise the onClientExit event.</summary>
        Friend Sub CallEvent_onClientExit()
            Dim eargs As New EventArgs
            RaiseEvent onClientExit(Me)
        End Sub

        ''' <summary>
        ''' Kills the client process, which subsequently calls the "onClientExit" event for this client instance.
        ''' </summary>
        Public Sub Close()
            Process.GetProcessById(PID).Kill()
        End Sub

        ''' <summary>
        ''' <para>A method used to get the title of the client window as a string.</para>
        ''' </summary>
        ''' <returns>The client window title, generally something like "Ultima Online"</returns>
        Public ReadOnly Property WindowCaption() As String
            Get
                Return Process.GetProcessById(PID).MainWindowTitle
            End Get
        End Property

    End Class

    ''' <summary>A list of Ultime Online clients.</summary>
    Public Class UOClientList

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

#Region "Events"
        Public Event onError(ByVal ErrorText As String)

        Private Sub RaiseErrorEvent(ByVal Message As String)
            RaiseEvent onError(Message)
        End Sub
#End Region

#Region "Functions"
        ''' <summary>Launches a new Client, returns true if successfull, false if not.</summary>
        Public Function LaunchClient() As Boolean
            'Create the process object.
            Dim p As New Process
            p.StartInfo.FileName = ClientPath & ClientExe
            p.StartInfo.WorkingDirectory = ClientPath

            'Start the client.
            p.Start()

            'TODO: add multi-clienting patch here

            If p.HasExited = False Then
                'Add the new client to the client array list.
                Add(p.Id)

                'Enable event handlers.
                p.EnableRaisingEvents = True
                AddHandler p.Exited, AddressOf ClientProcessExit

                Return True
            Else
                'Client launch failure.
                Return False
            End If
        End Function

        ''' <summary>DEBUG FUNCTION: Just trying to get the multi-client patch to work.</summary>
        Public Function LaunchClient2() As Boolean
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
                    'UOClientDll.MultiClientPatch(CUInt(pi.dwProcessId))

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

            RaiseErrorEvent("No such client with PID: '" & ProcessID & "'. Returning client(0) to avoid null-reference exception")
            Return Clients.Item(0)
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

        ''' <summary>Returns the UOAI.Client at the specified index.</summary>
        ''' <param name="Index">The index of the client in the list.</param>
        Public ReadOnly Property Client(ByVal Index As Integer) As Client
            Get
                Return Clients.Item(Index)
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
