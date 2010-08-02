Imports UOLite2

Public Class frmMain
    Public WithEvents Client As New LiteClient

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim ConnectResponse As String = Client.GetServerList(TextBox1.Text, TextBox2.Text, TextBox3.Text, TextBox4.Text)
        If ConnectResponse = "SUCCESS" Then
            Log("Connected to server: " & Client.LoginServerAddress & ":" & Client.LoginPort)
            Me.AcceptButton = SendButton
            CmdBox.Select()
        Else
            Log(ConnectResponse)
        End If

    End Sub

    Private Sub Client_LoginDenied(ByRef Reason As String) Handles Client.LoginDenied
        MsgBox(Reason)
    End Sub

    Private Sub Client_onCharacterListReceive(ByRef Client As LiteClient, ByVal CharacterList As System.Collections.ArrayList) Handles Client.onCharacterListReceive
        Client.ChooseCharacter(DirectCast(CharacterList.Item(0), CharListEntry).Name, DirectCast(CharacterList.Item(0), CharListEntry).Password, DirectCast(CharacterList.Item(0), CharListEntry).Slot)
    End Sub

    Private Sub Client_onCliLocSpeech(ByRef Client As UOLite2.LiteClient, ByVal Serial As UOLite2.LiteClient.Serial, ByVal BodyType As UShort, ByVal SpeechType As UOLite2.LiteClient.Enums.SpeechTypes, ByVal Hue As UShort, ByVal Font As UOLite2.LiteClient.Enums.Fonts, ByVal CliLocNumber As UInteger, ByVal Name As String, ByVal ArgsString As String) Handles Client.onCliLocSpeech
        'Log("SPEECH: " & Name & " : " & Text)
    End Sub

    Private Sub Client_onError(ByRef Description As String) Handles Client.onError
        Log(Description)
    End Sub

    Private Sub Client_onLoginComplete() Handles Client.onLoginComplete

    End Sub

    Private Sub Client_onLoginConfirm(ByRef Player As UOLite2.LiteClient.Mobile) Handles Client.onLoginConfirm

    End Sub

    Private Sub Client_onSpeech(ByRef Client As UOLite2.LiteClient, ByVal Serial As UOLite2.LiteClient.Serial, ByVal BodyType As UShort, ByVal SpeechType As UOLite2.LiteClient.Enums.SpeechTypes, ByVal Hue As UShort, ByVal Font As UOLite2.LiteClient.Enums.Fonts, ByVal Text As String, ByVal Name As String) Handles Client.onSpeech
        Debug.WriteLine(Text)
        Log("SPEECH: " & Name & " : " & Text)
    End Sub

#Region "Logging Code"

    Public Delegate Sub ConsoleWrite(ByRef Text As String)

    Private Sub Client_RecievedServerList() Handles Client.RecievedServerList
        Client.ChooseServer(0)
    End Sub

    Private Sub ConsoleLog(ByRef Text As String)
        ConsoleBox.SuspendLayout()
        ConsoleBox.AppendText(Text)
        ConsoleBox.ResumeLayout()
    End Sub

    Private Sub Log(ByRef Text As String)
        Dim args(0) As Object
        args(0) = Text & vbNewLine
        Me.Invoke(New ConsoleWrite(AddressOf ConsoleLog), args)
    End Sub

#End Region
End Class