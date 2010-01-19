Imports System.Diagnostics, Microsoft.Win32

Public Class UOAI
    Public Clients As New UOClientList

    Sub New()
        'Taken right out of your C# version, Wim.
        InitializeClientPaths()

    End Sub

    'Get the working directory and location of client.exe and all that stuff.
    Private Sub InitializeClientPaths()
        Dim hklm As RegistryKey = Registry.LocalMachine
        Dim originkey As RegistryKey = hklm.OpenSubKey("SOFTWARE\Origin Worlds Online\Ultima Online\1.0")

        If originkey Is Nothing Then
            originkey = hklm.OpenSubKey("SOFTWARE\Origin Worlds Online\Ultima Online Third Dawn\1.0")
        End If

        If originkey IsNot Nothing Then
            Dim instcdpath As String = DirectCast(originkey.GetValue("InstCDPath"), String)
            If instcdpath IsNot Nothing Then
                Clients.ClientPath = instcdpath & "\"
                Clients.ClientExe = "client.exe"
                originkey.Close()
                Exit Sub
            End If
            originkey.Close()
        End If

        'use default values
        Clients.ClientPath = "C:\Program Files\EA Games\Ultima Online Mondain's Legacy\"
        Clients.ClientExe = "client.exe"

        Exit Sub
    End Sub

    'Just throwing some things in here to get an idea of structure.
    Public Class UOClient
        Public PID As Integer

        Sub New()
            'TODO: add injection code here.
        End Sub

        Public Event onClientExit()

        Friend Sub CallEvent_onClientExit()
            RaiseEvent onClientExit()
        End Sub

        Public Sub Close()
            Process.GetProcessById(PID).Kill()
        End Sub

    End Class

    Public Class UOClientList

        Private proclist() As Process
        Private Clients As New ArrayList
        Friend ClientExe As String
        Friend ClientPath As String

        Public Sub LaunchClient()
            'Create the process object.
            Dim p As New Process
            p.StartInfo.FileName = ClientPath & ClientExe
            p.StartInfo.WorkingDirectory = ClientPath

            'Start the client.
            p.Start()

            'TODO: add multi-clienting patch here

            'Add the new client to the client array list.
            Add(p.Id)

            'Enable event handlers.
            p.EnableRaisingEvents = True
            AddHandler p.Exited, AddressOf ClientProcessExit
        End Sub

        Friend Sub Add(ByVal PID As Integer)
            Dim c As New UOAI.UOClient
            c.PID = PID
            Clients.Add(c)
        End Sub

        Friend Sub Remove(ByVal PID As Integer)
            For Each c As UOAI.UOClient In Clients
                If c.PID = PID Then
                    Clients.Remove(c)
                    c.CallEvent_onClientExit()
                    Exit Sub
                End If
            Next
        End Sub

        Public Function Count()
            ForceUpdateClientList()
            Return Clients.Count
        End Function

        Public Function Client(ByVal Index As Integer) As UOClient
            Return Clients.Item(Index)
        End Function

        'TODO: only for testing, should be private!
        Public Sub ForceUpdateClientList()

            'Update process list
            proclist = Process.GetProcessesByName(ClientExe.Split(".")(0))

            Dim ProcChk As Boolean = False

            'Check for clients
            For Each proc As Process In proclist
                'Just ensures it is a uo client
                If proc.MainWindowTitle.Contains("Ultima Online") Then
                    'See If the client is already in the list, if not then add it.
                    For Each c As UOClient In Clients
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

        Sub ClientProcessExit(ByVal sender As Object, ByVal e As System.EventArgs)
            Remove(sender.ID)
        End Sub

    End Class

End Class
