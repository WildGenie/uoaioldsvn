<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim ListViewItem1 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem(New String() {"UOGamers", "255.255.255.255", "52ms", "27%"}, -1)
        Dim ListViewItem2 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem(New String() {"Character Name", "Password"}, -1)
        Me.TextBox1 = New System.Windows.Forms.TextBox
        Me.TextBox2 = New System.Windows.Forms.TextBox
        Me.TextBox3 = New System.Windows.Forms.TextBox
        Me.TextBox4 = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.Button1 = New System.Windows.Forms.Button
        Me.Button2 = New System.Windows.Forms.Button
        Me.GBConnection = New System.Windows.Forms.GroupBox
        Me.GBSelectShard = New System.Windows.Forms.GroupBox
        Me.ListView1 = New System.Windows.Forms.ListView
        Me.ColumnHeader1 = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader2 = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader3 = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader4 = New System.Windows.Forms.ColumnHeader
        Me.GBSelectCharacter = New System.Windows.Forms.GroupBox
        Me.ListView2 = New System.Windows.Forms.ListView
        Me.ColumnHeader5 = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader6 = New System.Windows.Forms.ColumnHeader
        Me.Panel1 = New System.Windows.Forms.Panel
        Me.PlayerHits = New System.Windows.Forms.ProgressBar
        Me.Panel2 = New System.Windows.Forms.Panel
        Me.Label11 = New System.Windows.Forms.Label
        Me.Label8 = New System.Windows.Forms.Label
        Me.Label9 = New System.Windows.Forms.Label
        Me.Label10 = New System.Windows.Forms.Label
        Me.Label7 = New System.Windows.Forms.Label
        Me.PlayerStamina = New System.Windows.Forms.ProgressBar
        Me.Label6 = New System.Windows.Forms.Label
        Me.PlayerMana = New System.Windows.Forms.ProgressBar
        Me.Label5 = New System.Windows.Forms.Label
        Me.TextBox5 = New System.Windows.Forms.TextBox
        Me.GBConnection.SuspendLayout()
        Me.GBSelectShard.SuspendLayout()
        Me.GBSelectCharacter.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Enabled = False
        Me.TextBox1.Location = New System.Drawing.Point(102, 90)
        Me.TextBox1.MaxLength = 30
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(165, 20)
        Me.TextBox1.TabIndex = 0
        Me.TextBox1.Text = "megamandos"
        '
        'TextBox2
        '
        Me.TextBox2.Enabled = False
        Me.TextBox2.Location = New System.Drawing.Point(102, 116)
        Me.TextBox2.MaxLength = 30
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(165, 20)
        Me.TextBox2.TabIndex = 1
        '
        'TextBox3
        '
        Me.TextBox3.Location = New System.Drawing.Point(102, 38)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(207, 20)
        Me.TextBox3.TabIndex = 2
        Me.TextBox3.Text = "login.uogamers.com"
        '
        'TextBox4
        '
        Me.TextBox4.Location = New System.Drawing.Point(102, 64)
        Me.TextBox4.Name = "TextBox4"
        Me.TextBox4.Size = New System.Drawing.Size(52, 20)
        Me.TextBox4.TabIndex = 3
        Me.TextBox4.Text = "2593"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 93)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(78, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Account Name"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(43, 119)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(53, 13)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Password"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(17, 41)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(79, 13)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Server Address"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(70, 67)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(26, 13)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Port"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(160, 62)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 8
        Me.Button1.Text = "Connect"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Enabled = False
        Me.Button2.Location = New System.Drawing.Point(273, 88)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(75, 23)
        Me.Button2.TabIndex = 9
        Me.Button2.Text = "Login"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'GBConnection
        '
        Me.GBConnection.Controls.Add(Me.Label1)
        Me.GBConnection.Controls.Add(Me.Button2)
        Me.GBConnection.Controls.Add(Me.TextBox1)
        Me.GBConnection.Controls.Add(Me.TextBox3)
        Me.GBConnection.Controls.Add(Me.Button1)
        Me.GBConnection.Controls.Add(Me.TextBox4)
        Me.GBConnection.Controls.Add(Me.TextBox2)
        Me.GBConnection.Controls.Add(Me.Label4)
        Me.GBConnection.Controls.Add(Me.Label2)
        Me.GBConnection.Controls.Add(Me.Label3)
        Me.GBConnection.Location = New System.Drawing.Point(0, 0)
        Me.GBConnection.Name = "GBConnection"
        Me.GBConnection.Size = New System.Drawing.Size(436, 202)
        Me.GBConnection.TabIndex = 10
        Me.GBConnection.TabStop = False
        Me.GBConnection.Text = "Connect and Login"
        '
        'GBSelectShard
        '
        Me.GBSelectShard.Controls.Add(Me.ListView1)
        Me.GBSelectShard.Location = New System.Drawing.Point(0, 0)
        Me.GBSelectShard.Name = "GBSelectShard"
        Me.GBSelectShard.Size = New System.Drawing.Size(436, 202)
        Me.GBSelectShard.TabIndex = 11
        Me.GBSelectShard.TabStop = False
        Me.GBSelectShard.Text = "Select Shard"
        Me.GBSelectShard.Visible = False
        '
        'ListView1
        '
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4})
        Me.ListView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListView1.FullRowSelect = True
        Me.ListView1.Items.AddRange(New System.Windows.Forms.ListViewItem() {ListViewItem1})
        Me.ListView1.Location = New System.Drawing.Point(3, 16)
        Me.ListView1.MultiSelect = False
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(430, 183)
        Me.ListView1.TabIndex = 0
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Name"
        Me.ColumnHeader1.Width = 97
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Address"
        Me.ColumnHeader2.Width = 132
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Ping"
        Me.ColumnHeader3.Width = 101
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Load"
        Me.ColumnHeader4.Width = 77
        '
        'GBSelectCharacter
        '
        Me.GBSelectCharacter.Controls.Add(Me.ListView2)
        Me.GBSelectCharacter.Location = New System.Drawing.Point(0, 0)
        Me.GBSelectCharacter.Name = "GBSelectCharacter"
        Me.GBSelectCharacter.Size = New System.Drawing.Size(436, 202)
        Me.GBSelectCharacter.TabIndex = 12
        Me.GBSelectCharacter.TabStop = False
        Me.GBSelectCharacter.Text = "Select Character"
        Me.GBSelectCharacter.Visible = False
        '
        'ListView2
        '
        Me.ListView2.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader5, Me.ColumnHeader6})
        Me.ListView2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListView2.FullRowSelect = True
        Me.ListView2.Items.AddRange(New System.Windows.Forms.ListViewItem() {ListViewItem2})
        Me.ListView2.Location = New System.Drawing.Point(3, 16)
        Me.ListView2.MultiSelect = False
        Me.ListView2.Name = "ListView2"
        Me.ListView2.Size = New System.Drawing.Size(430, 183)
        Me.ListView2.TabIndex = 0
        Me.ListView2.UseCompatibleStateImageBehavior = False
        Me.ListView2.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Name"
        Me.ColumnHeader5.Width = 97
        '
        'ColumnHeader6
        '
        Me.ColumnHeader6.Text = "Password"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.GBConnection)
        Me.Panel1.Controls.Add(Me.GBSelectShard)
        Me.Panel1.Controls.Add(Me.GBSelectCharacter)
        Me.Panel1.Location = New System.Drawing.Point(12, 12)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(440, 205)
        Me.Panel1.TabIndex = 13
        '
        'PlayerHits
        '
        Me.PlayerHits.Location = New System.Drawing.Point(65, 62)
        Me.PlayerHits.MarqueeAnimationSpeed = 1
        Me.PlayerHits.Name = "PlayerHits"
        Me.PlayerHits.Size = New System.Drawing.Size(283, 23)
        Me.PlayerHits.Step = 1
        Me.PlayerHits.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.PlayerHits.TabIndex = 14
        Me.PlayerHits.Tag = ""
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.Label11)
        Me.Panel2.Controls.Add(Me.Label8)
        Me.Panel2.Controls.Add(Me.Label9)
        Me.Panel2.Controls.Add(Me.Label10)
        Me.Panel2.Controls.Add(Me.Label7)
        Me.Panel2.Controls.Add(Me.PlayerStamina)
        Me.Panel2.Controls.Add(Me.Label6)
        Me.Panel2.Controls.Add(Me.PlayerMana)
        Me.Panel2.Controls.Add(Me.Label5)
        Me.Panel2.Controls.Add(Me.PlayerHits)
        Me.Panel2.Location = New System.Drawing.Point(12, 223)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(440, 177)
        Me.Panel2.TabIndex = 15
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Font = New System.Drawing.Font("Microsoft Sans Serif", 20.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label11.Location = New System.Drawing.Point(18, 14)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(116, 31)
        Me.Label11.TabIndex = 23
        Me.Label11.Text = "Kontrast"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(354, 126)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(36, 13)
        Me.Label8.TabIndex = 22
        Me.Label8.Text = "0/100"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(354, 97)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(36, 13)
        Me.Label9.TabIndex = 21
        Me.Label9.Text = "0/100"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(354, 68)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(36, 13)
        Me.Label10.TabIndex = 20
        Me.Label10.Text = "0/100"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(14, 126)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(45, 13)
        Me.Label7.TabIndex = 19
        Me.Label7.Text = "Stamina"
        '
        'PlayerStamina
        '
        Me.PlayerStamina.Location = New System.Drawing.Point(65, 120)
        Me.PlayerStamina.Name = "PlayerStamina"
        Me.PlayerStamina.Size = New System.Drawing.Size(283, 23)
        Me.PlayerStamina.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.PlayerStamina.TabIndex = 18
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(25, 97)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(34, 13)
        Me.Label6.TabIndex = 17
        Me.Label6.Text = "Mana"
        '
        'PlayerMana
        '
        Me.PlayerMana.Location = New System.Drawing.Point(65, 91)
        Me.PlayerMana.Name = "PlayerMana"
        Me.PlayerMana.Size = New System.Drawing.Size(283, 23)
        Me.PlayerMana.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.PlayerMana.TabIndex = 16
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(21, 68)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(38, 13)
        Me.Label5.TabIndex = 15
        Me.Label5.Text = "Health"
        '
        'TextBox5
        '
        Me.TextBox5.Location = New System.Drawing.Point(15, 406)
        Me.TextBox5.Multiline = True
        Me.TextBox5.Name = "TextBox5"
        Me.TextBox5.Size = New System.Drawing.Size(435, 217)
        Me.TextBox5.TabIndex = 16
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(462, 635)
        Me.Controls.Add(Me.TextBox5)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "Form1"
        Me.Text = "UOLite"
        Me.GBConnection.ResumeLayout(False)
        Me.GBConnection.PerformLayout()
        Me.GBSelectShard.ResumeLayout(False)
        Me.GBSelectCharacter.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox4 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents GBConnection As System.Windows.Forms.GroupBox
    Friend WithEvents GBSelectShard As System.Windows.Forms.GroupBox
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents GBSelectCharacter As System.Windows.Forms.GroupBox
    Friend WithEvents ListView2 As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents PlayerHits As System.Windows.Forms.ProgressBar
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents PlayerStamina As System.Windows.Forms.ProgressBar
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents PlayerMana As System.Windows.Forms.ProgressBar
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Public WithEvents TextBox5 As System.Windows.Forms.TextBox

End Class
