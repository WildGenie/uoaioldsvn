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
    Public Shared Clientpath As String

    ''' <summary>Gets or sets the name of the client executable, normally "client.exe"</summary>
    Public Shared ClientExe As String

    Public Shared StrLst As StringList


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

        InitializeClientPaths()

        'Sets the default language of the string list to the language of the OS
        'used for clilocs, item types, etc. That way people with non-english can get the
        'right info.
        'Languages: enu,chs,cht,deu,esp,fra,jpn,kor
        Select Case My.Application.Culture.ThreeLetterISOLanguageName
            Case "chi" 'Chinese
                StrLst = New StringList("chs")
            Case "zho" 'Chinese Traditional
                StrLst = New StringList("cht")

            Case "eng" 'English
                StrLst = New StringList("enu")
            Case "enm" 'English
                StrLst = New StringList("enu")

            Case "fre" 'French
                StrLst = New StringList("fra")
            Case "fra" 'French
                StrLst = New StringList("fra")
            Case "frm" 'French
                StrLst = New StringList("fra")
            Case "fro" 'French
                StrLst = New StringList("fra")

            Case "ger" 'German
                StrLst = New StringList("deu")
            Case "deu" 'German
                StrLst = New StringList("deu")
            Case "gmh" 'German
                StrLst = New StringList("deu")
            Case "goh" 'German
                StrLst = New StringList("deu")

            Case "spa" 'Spanish
                StrLst = New StringList("esp")

            Case "jpn" 'Japanese
                StrLst = New StringList("jpn")

            Case "kor" 'Korean
                StrLst = New StringList("kor")

            Case Else 'Don't know what to set it to? Then English.
                StrLst = New StringList("enu")

        End Select

    End Sub

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

#Region "UOAI Events"
    Public Event onError(ByVal ErrorText As String)

    Private Sub RaiseErrorEvent(ByVal Message As String)
        RaiseEvent onError(Message)
    End Sub
#End Region

End Class
