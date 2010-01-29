Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim j As New ListViewItem(ComboBox3.Text)

        j.SubItems.Add(ComboBox2.Text)
        j.SubItems.Add(ComboBox1.Text)
        j.SubItems.Add(TextBox1.Text)
        j.SubItems.Add(ComboBox5.Text)
        j.SubItems.Add(ComboBox6.Text)

        ListView1.Items.Add(j)

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim buffPosition As Integer = Convert.ToInt32(ComboBox4.Text)

        TextBox3.SuspendLayout()
        TextBox3.Text &= "Public Class " & TextBox2.Text
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & "Inherits Packet"
        TextBox3.Text &= vbNewLine

        For Each j As ListViewItem In ListView1.Items
            TextBox3.Text &= vbTab & j.Text & " " & j.SubItems(1).Text & " As " & j.SubItems(2).Text
            If TextBox1.Text <> "" Then TextBox3.Text &= " = " & TextBox1.Text
            TextBox3.Text &= vbNewLine
        Next

        TextBox3.Text &= vbNewLine

        TextBox3.Text &= vbTab & "Friend Sub New(ByVal bytes() As byte)"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "MyBase.New(Enums.PacketType." & TextBox2.Text & ")"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "_Data = bytes"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "_Size = bytes.Length"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "buff = New BufferHandler(bytes)"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "With buff"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & vbTab & ".Position = " & ComboBox4.Text
        TextBox3.Text &= vbNewLine

        For Each j As ListViewItem In ListView1.Items
            If j.SubItems(5).Text = "0" Then Continue For
            TextBox3.Text &= vbTab & vbTab & vbTab & "'" & buffPosition & "-" & (Convert.ToInt32(j.SubItems(5).Text) + buffPosition - 1)
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & j.SubItems(1).Text & " = " & GetReadString(j.SubItems(5).Text)
            TextBox3.Text &= vbNewLine
            buffPosition += Convert.ToInt32(j.SubItems(5).Text)
        Next

        TextBox3.Text &= vbTab & vbTab & "End With"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "End Sub"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbNewLine

        buffPosition = Convert.ToInt32(ComboBox4.Text)

        For Each j As ListViewItem In ListView1.Items
            If j.SubItems(5).Text = "0" Then Continue For
            TextBox3.Text &= vbTab & vbTab & "Public Property " & j.SubItems(4).Text & " As " & j.SubItems(2).Text
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & "Get"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & vbTab & "Return " & j.SubItems(1).Text
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & "End Get"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & "Set(ByVal Value As " & j.SubItems(2).Text & ")"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & vbTab & j.SubItems(1).Text & " = Value"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & vbTab & "buff.Position = " & buffPosition
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & vbTab & "buff" & GetWriteString(j.SubItems(5).Text) & "(Value)"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & "End Set"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & "End Property "
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbNewLine
            buffPosition += Convert.ToInt32(j.SubItems(5).Text)
        Next

        TextBox3.Text &= "End Class"
        TextBox3.Text &= vbNewLine
        TextBox3.ResumeLayout()
    End Sub


    Public Function GetReadString(ByVal text As String) As String

        Select Case text
            Case "4"
                Return ".readuint"
            Case "2"
                Return ".readushort"
            Case "1"
                Return ".readbyte"
            Case Else
                Return "1"
        End Select

    End Function

    Public Function GetWriteString(ByVal text As String) As String

        Select Case text
            Case "4"
                Return ".writeuint"
            Case "2"
                Return ".writeushort"
            Case "1"
                Return ".writebyte"
            Case Else
                Return "1"
        End Select

    End Function

    Private Sub ListView1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListView1.DoubleClick
        ListView1.Items.RemoveAt(ListView1.SelectedIndices(0))
    End Sub

End Class
