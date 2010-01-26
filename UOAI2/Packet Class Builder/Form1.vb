Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim j As New ListViewItem(ComboBox3.Text)

        j.SubItems.Add(ComboBox2.Text)
        j.SubItems.Add(ComboBox1.Text)
        j.SubItems.Add(TextBox1.Text)
        j.SubItems.Add(ComboBox5.Text)

        ListView1.Items.Add(j)

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        TextBox3.SuspendLayout()
        TextBox3.Text &= "Public Class " & TextBox2.Text
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & "Inherits Packet"
        TextBox3.Text &= vbNewLine
        'TextBox3.Text &= 
        For Each j As ListViewItem In ListView1.Items
            TextBox3.Text &= vbTab & j.Text & " " & j.SubItems(0).Text & " As " & j.SubItems(1).Text
            If TextBox1.Text <> "" Then TextBox3.Text &= " = " & TextBox1.Text
            TextBox3.Text &= vbNewLine
        Next

        TextBox3.Text &= vbNewLine

        TextBox3.Text &= vbTab & "Friend Sub New(ByVal bytes() As byte)"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "MyBase.New(Enums.PacketType." & TextBox2.Text
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "_Data = bytes()"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "_Size = bytes.Length"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "buff = BufferHandler(bytes)"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & ""
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "With buff"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & vbTab & ".Position = " & Convert.ToInt32(ComboBox4.Text)
        TextBox3.Text &= vbNewLine

        For Each j As ListViewItem In ListView1.Items
            TextBox3.Text &= vbTab & vbTab & vbTab & j.SubItems(1).Text & " = " & GetReadString(j.SubItems(2).Text)
            TextBox3.Text &= vbNewLine
        Next

        TextBox3.Text &= vbTab & vbTab & "End With"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbTab & vbTab & "End Sub"
        TextBox3.Text &= vbNewLine
        TextBox3.Text &= vbNewLine

        For Each j As ListViewItem In ListView1.Items
            TextBox3.Text &= vbTab & vbTab & "Public ReadOnly Property " & j.SubItems(4).Text
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & "Get"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & vbTab & "Return " & j.SubItems(1).Text
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & vbTab & "End Get"
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbTab & vbTab & "End Property "
            TextBox3.Text &= vbNewLine
            TextBox3.Text &= vbNewLine
        Next

        TextBox3.Text &= "End Class"
        TextBox3.Text &= vbNewLine
        TextBox3.ResumeLayout()
    End Sub


    Public Function GetReadString(ByVal text As String)
        Select Case text
            Case "UInt32"
                Return ".readuint"
            Case "Serial"
                Return ".readuint"
            Case "UShort"
                Return ".readushort"
            Case "Byte"
                Return ".readbyte"
            Case Else
                Return ""
        End Select
    End Function

End Class
