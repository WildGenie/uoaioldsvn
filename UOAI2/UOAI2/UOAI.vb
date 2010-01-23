Imports System.Diagnostics, Microsoft.Win32, System.IO, System.Runtime.InteropServices

Public Class UOAI

#Region "UOAI Variables"
    Private _ClientList As New ClientList
    Shared UOClientDllPath As String = My.Application.Info.DirectoryPath & "\UOClientDll.dll"
#End Region

#Region "UOAI Properties"

    ''' <summary>The list of running Ultima Online 2D clients as UOAI.Client objects.</summary>
    Public ReadOnly Property Clients() As ClientList
        Get
            Return _ClientList
        End Get
    End Property

    ''' <summary>Gets or sets the path of the client executable, usualy something like 
    ''' "C:\Program Files\EA Games\Ultima Online Mondain's Legacy\". This is also the 
    ''' working directory for the clients launched using UOAI.</summary>
    ''' <returns>The path to the client executable.</returns>
    Public Property ClientPath() As String
        Get
            Return _ClientList.ClientPath
        End Get
        Set(ByVal value As String)
            _ClientList.ClientPath = value
        End Set
    End Property

    ''' <summary>Gets or sets the name of the client executable, normally "client.exe"</summary>
    ''' <returns>The name of the client executable.</returns>
    Public Property ClientExe() As String
        Get
            Return _ClientList.ClientExe
        End Get
        Set(ByVal ExeName As String)
            _ClientList.ClientExe = ExeName
        End Set
    End Property

#End Region

#Region "UOAI Constructor"
    Sub New()

        'Checks for to see if the current user is part of the administrators group, throws an exception if it fails
        If My.User.IsInRole(Microsoft.VisualBasic.ApplicationServices.BuiltInRole.Administrator) = False Then
            Dim ci As New Microsoft.VisualBasic.Devices.ComputerInfo

            'Just adding this to help people get a better idea of why they are having trouble
            'with their application not having admin privilages, even if running ad administrator
            'on a vista/7 machine. Fucking vistualized vista/7 security model....
            If Convert.ToByte(ci.OSVersion.Split(".")(0)) >= 6 Then
                Throw New ApplicationException("You need to be part of the administrators group to use UOAI2." & _
                                               " If you have User Account Control enabled, please run this application " & _
                                               "as administrator.")
            Else
                Throw New ApplicationException("You need to be part of the administrators group to use UOAI2.")
            End If
        End If

    End Sub
#End Region

#Region "UOAI Events"
    Public Event onError(ByVal ErrorText As String)

    Private Sub RaiseErrorEvent(ByVal Message As String)
        RaiseEvent onError(Message)
    End Sub
#End Region

End Class
