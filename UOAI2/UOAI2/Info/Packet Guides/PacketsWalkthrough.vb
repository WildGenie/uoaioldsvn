
#Region "Login Server Communications"
#Region "Client Connects to Login Server and Sends Account Login Request"
'Sent Packet #0
'80

'Username as ASCII string (30 Chars)
' m  e  g  a  m  a  n  d  o  s
'-6D-65-67-61-6D-61-6E-64-6F-73-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Password as ASCII string (30 Chars), I made them Random for obvious reasons
'-2F-6F-20-34-97-23-1D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Unknown
'-5D
#End Region

#Region "Login Server Sends Shard List"
'Recieved Packet #1
'A8

'Packet Size: 6 + (Number of Servers * 40) = 46
'-2E-00 <-Reversed Byte order!

'???
'-5D

'Number of Servers
'-00-01

#Region "Server List Loop: Servers Info * Number of servers"
'Index: Short
'-00-00

'Server Name (32 ASCII character string)
'-55-4F-47-61-6D-65-72-73-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'-00

'Time Zone: ??
'-FB

'Server IP Address:  110.139.173.209
'                   -6E -8B -AD -D1
#End Region
#End Region

#Region "Client Selects a Server"
'Sent Packet #2
'The 0 based server index of the server selected.
'A0-00-00
#End Region

#Region "The Login Server Redirects Client to the Game Server and Sends the Client an Authorization Code for Later Use."
'Recieved Packet #3
'8C

'Game Server IP Address
'-D1-AD-8B-6E

'Game Server Port
'-0A-21

'Authorization Code
'-8F-77-B9-3E

#End Region
#End Region

'Client disconnects from the login server and connects to the game server using the IP address and port from the server redirect.

#Region "Game Server Communications"

#Region "Client Connects to the Game Server and Sends: Authorization Code, Username, and Password."
'Sent Packet #4
'91

'Authorization Code
'-8F-77-B9-3E

' m  e  g  a  m  a  n  d  o  s  (EndByte)
'-6D-65-67-61-6D-61-6E-64-6F-73-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Password
'-2F-6F-20-34-97-23-1D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
#End Region

'If the login request is rejected a special packet will be sent for that, explaining why it failed, then the client is disconnected.
'But if it is accepted this continues:

#Region "The Server Sends the starting Cities and Character list"
'Recieved Packet #5
'A9

'Packet Size
'-AB-03

'Number Of Characters
'-05

#Region "Loop: Character * Number of Characters"
'On Most Servers this will loop 5 times, no matter what.
'Even if there is only 1 character, such as in my case.
'It will only have the other characters names and passwords all 0x00 bytes.

'Character Name (30 ASCII chars):
'-K  o  n  t  r  a  s  t
'-4B-6F-6E-74-72-61-73-74-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Password for that Character (I am pretty sure this it out dated, since its always 30 null bytes)
'-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
#End Region

'Number of Cities: 10
'-0A

#Region "Loop: City * Number of Cities"
'Index
'-00

'City Name (31 ASCII characters)
' Y  e  w
'-59-65-77-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Area of Town (31 ASCII Characters)
'
'-54-68-65-20-45-6D-70-61-74-68-20-41-62-62-65-79-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
#End Region

#Region "Flags"
'Flags: Enable Context Menus
'-00-00-00-08

'Sum Flags for combining
'0x2 = overwrite configuration button;
'0x4 = limit 1 character per account;
'0x8 = enable context menus;
'0x10 = limit character slots; 
'0x20 = paladin and necromancer classes; 
'0x40 = 6th character slot; 
'0x80 = samurai and ninja classes; 
'0x100 = elven race; 
'0x200 = KR support flag1; 
'0x400 = KR support flag2; 
'0x1000 = 7th character slot � only 2D client
#End Region

#End Region

#Region "The Client Selects the Character to Play"
'Sent Packet #6
'5D

'Unknown
'-ED-ED-ED-ED

'The Name of the Character (32 ASCII character string)
' K  o  n  t  r  a  s  t  (EndByte)
'-4B-6F-6E-74-72-61-73-74-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Unknown Int: 63
'-00-00-00-3F

'Unknown Int
'-00-00-00-00

'Unknown Int
'-00-00-00-00

'Unknown Int:51
'-00-00-00-33

'Unknown Int
'-00-00-00-00

'Unknown Int
'-00-00-00-00

'Unknown Int
'-00-00-00-00

'Unknown Int
'-00-00-00-00

'Client IP Address
'-C0-A8-01-7F
#End Region

#Region "The Server Requests The Version String From The Client"
'Recieved Packet #7
'BD

'Packet Size
'-03-00
#End Region

#Region "The Client Responds with the Version String"
'Sent Packet #8
'BD

'Packet Size
'-0B-00

' 7  .  0  .  4  .  5  (EndByte)
'-37-2E-30-2E-34-2E-35-00
#End Region

#Region "The Server Accepts the version number and sends basic information about the player mobile."
'Recieved Packet #9
'1B

'Player Serial
'-00-05-4E-03

'Unknown
'-00-00-00-00

'Body Type, Male-Human
'-01-90

'X
'-03-DA

'Y
'-01-C7

'Unknown Byte, always 0x00
'-00

'Z
'-1B

'Direction: West
'-06

'Unknown Byte, always 0x00
'-00

'Unknown, usualy 0x00000000... ??
'-FF-FF-FF-FF

'Unknown, usualy 0x00000000
'-00-00-00-00

'Map Width (X Axis)
'-1C-00

'Map Height (Y Axis)
'-10-00

'Unknown
'-00-00

'Unknown
'-00-00-00-00.
#End Region

#Region "Request Skills"
'Client Query - Request skills - Serial of requested target
'Sent Packet #10
'CMd  Unknown         Type         Serial (player)
'34-  ED-ED-ED-ED    - 05 -        00-05-4E-03
#End Region

#Region "Send Client Version String"
'Send client version to the server.
'Sent Packet #11
'cmd  Size    ASCII version String     7  .  0  .  4  .  5  (Endbyte)
'BD-  0B-00                            37-2E-30-2E-34-2E-35-00

#End Region

#Region "BF-0D Unknown Packet"
'Sent Packet #12
'BF-0D-00-00-05-00-00-03-20-00-18-FE-A8
#End Region

#Region "Disarm Request?"
'Disarm Request?
'Sent Packet #13
'BF-09 -00-00-  0B-45-4E-55-00
#End Region

#Region "Stun Request?"
'Sent Packet #14
'BF-0A-00-00-0F-0A-00-00-00-3F
#End Region

#Region "Some sort of part cmd, maybe request party status to rejoin a party"
'Recieved Packet #15
'BF-06-00-00-08-00
#End Region

#Region "Unknown Command"
'Recieved Packet #16
'BF-29-00-00-18-00-00-00-04-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
#End Region

#Region "Season - Sets the season to display"
'Recieved Packet #17
'BC-00-01
#End Region

#Region "Features packet"
'this one specifies -0x10 enable AOS featues, no idea what "f" is for.
'Recieved Packet #18
'B9-00-00-00-1F
#End Region

#Region "Z-Move, the character is being moved by the server"
'This is because I logged in inside my house. It moved me to the second floor.

'Recieved Packet #19
'20-00-05-4E-03-01-90-00-83-EA-00-03-DA-01-C7-00-00-06-1B

'Recieved Packet #20
'20-00-05-4E-03-01-90-00-83-EA-00-03-DA-01-C7-00-00-06-1B
#End Region

#Region "Global List Level"
'From 0 as the brightest to 1F as black. 9 is OSI's night setting.
'Recieved Packet #21
'4F-00
#End Region

#Region "Setting the personal light level"
'Same as above but this only applies to the mobile specified.
'Recieved Packet #22
'4E-00-05-4E-03-00
#End Region

#Region "Another Z-Move, the server is correcting my height."
'Recieved Packet #23
'20-00-05-4E-03-01-90-00-83-EA-00-03-DA-01-C7-00-00-06-1B
#End Region

#Region "Equipped MOB - Player"
'This is just an equipped mob, this one being my player.
'Recieved Packet #24
'78

'Size
'-6B-00 

'Serial
'-00-05-4E-03

'Character's body number
'-01-90

'X
'-03-DA

'Y
'-01-C7

'Z
'-1B

'Direction
'-06 <-West

'Skin Tone Hue
'-83-EA

'Status
'-00 <-Normal

'Reputation
'-02 <-Guild Member


'If serial is not 0 then there will be a loop for the items:
'______________________________________________________________________
'Item Serial
'-40-9D-0A-6F

'Type
'-0E-75

'Layer
'-15 <-Backpack

'Hue (Short)- if the item ID included the flag 0x8000
'______________________________________________________________________

'Item Serial
'-41-6E-A6-B6

'Type
'-10-86

'Layer <-Bracelet
'-0E

'Hue (Short)- if the item ID included the flag 0x8000
'______________________________________________________________________

'Item Serial
'-40-72-0B-F0

'Type
'-97-0D

'Layer
'-03 <- Shoes

'Hue
'-04-89 <-Fire cloth
'______________________________________________________________________

'Serial
'-41-99-C5-F1

'Type
'-97-18

'Layer
'-06 <-Head

'Hue
'-04-89 <-Fire cloth
'______________________________________________________________________

'Serial
'-41-BF-D8-F7

'Type
'-8E-FA

'Layer
'-01 <-Left Hand

'Hue
'-08-4B
'______________________________________________________________________

'Serial
'-41-3D-77-E6

'Type
'-9E-FD

'Layer
'-05 <-shirt

'Hue
'-04-89 <-Fire cloth
'______________________________________________________________________

'Serial
'-41-3D-7B-BA

'Type
'-95-39

'Layer
'-04 <- Pants

'Hue
'-04-89 <-fire cloth
'______________________________________________________________________

'Serial
'-41-80-29-E3

'Type
'-3E-B7 <-(Nightmare)

'Layer
'-19 <-Mount

'No hue, because Type didnt include the 0x8000 flag!
'______________________________________________________________________

'Serial
'-7F-EA-C3-F3

'Type
'-A0-3D

'Layer
'-0B <-Hair

'Hue
'-05-93 <-green

'Serial
'-7F-EA-C3-F2

'Type
'-A0-40

'Layer
'-10 <-Facial Hair

'Hue
'-05-93 <-green
'______________________________________________________________________

'End of Item Loop, is always 0x00000000
'-00-00-00-00
#End Region

#Region "Mobile status, this changes in size based on supported features byte!!"
'Recieved Packet #25

'Cmd
'11

'Size
'-46-00 

'Mobile Serial
'-00-05-4E-03

'Mobile name:
' K  o  n  t  r  a  s  t  (EndByte)
'-4B-6F-6E-74-72-61-73-74-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Current Hits: 94
'-00-5E

'Max Hits: 94
'-00-5E

'Allowed Name Change: Not Allowed
'-00

'Supported Features Byte <This is very important!>
'-03

'Gender: Male
'-00

'Strength: 94
'-00-59

'Dexterity: 36
'-00-24

'Intelligence: 100
'-00-64

'Stamina: 36
'-00-24

'Max Stamina: 36
'-00-24

'Mana: 100
'-00-64

'Max Mana: 100
'-00-64

'Gold: 0
'-00-00-00-00

'Armor Rating (Physical Resistance)
'-00-00

'Weight: 128
'-00-80

'Stat Cap: 225
'-00-E1 (SF = 0x03)

'Current Followers: 2
'-02 (SF = 0x03)

'Max Followers: 5
'-05 (SF = 0x03)


#End Region

#Region "The server sends a packet telling the client whether or not its in war mode"
'Recieved Packet #26
'72

'Boolean: 0-Off / 1-On
'-00

'Unknown
'-00

'Unknown
'-32

'Unknown
'-00
#End Region

#Region "The server starts sending packets to place items on the ground."
#Region "0x1A (Show Item) Parse Example"
'Recieved Packet #27
'1A-

'Packet Size
'10-00

'Serial, Includes the 0x80000000 Flag if the amount is > 1
'-C0-69-6A-72

'Item Type (Artwork, include the 0x8000 flag is StackID > 0)
'-18-26

'Item Amount (Always > 0, only present if the Serial includes the 0x80000000 Flag)
'-00-01

'X Position (if the item's direction is not zero, include flag 0x8000)
'-03-CF

'Y Position (If the item's "status" is not zero, the include the flag 0x4000, if the item's hue is not zero then inclue the flag 0x8000)
'-01-CE

'Direction (included if X has the 0x8000 Flag)
'Byte

'The Item's Z Position
'-07

'Hue (included if Y has the 0x8000 Flag)
'Short

'Status (included if Y has the 0x4000 Flag)
'Byte
#End Region

'Recieved Packet #28
'1A-12-00-C0-B3-9B-4E-09-EB-00-01-03-CF-81-C4-08-04-7E

'Recieved Packet #29
'1A-10-00-C1-66-71-A8-0D-F1-00-01-03-CF-01-C6-37

'Recieved Packet #30
'1A-10-00-C1-66-71-A7-0D-F1-00-01-03-CF-01-C6-3B

'Recieved Packet #31
'1A-10-00-C1-66-71-AA-0D-F1-00-01-03-CF-01-C6-36

'Recieved Packet #32
'1A-10-00-C1-66-71-A9-0D-F1-00-01-03-CF-01-C6-35

'Recieved Packet #33
'1A-10-00-C0-69-6A-6E-18-22-00-01-03-CF-01-C7-07

'Recieved Packet #34
'1A-10-00-C1-2F-7F-83-09-D7-00-01-03-CF-01-C9-35

'Recieved Packet #35
'1A-10-00-C0-D2-B4-21-0E-A8-00-01-03-CF-01-C4-07

'Recieved Packet #36
'1A-10-00-C0-6C-2B-B8-10-60-00-01-03-CF-01-C9-07

'Recieved Packet #37
'1A-10-00-C1-2F-12-B9-0B-49-00-01-03-CF-01-C3-2F

'Recieved Packet #38
'1A-10-00-C1-66-71-A6-0D-F1-00-01-03-CF-01-C6-3A

'Recieved Packet #39
'1A-10-00-C1-2F-42-19-0B-34-00-01-03-CF-01-C9-2F

'Recieved Packet #40
'1A-10-00-C1-66-71-AD-0D-F1-00-01-03-CF-01-C6-34

'Recieved Packet #41
'1A-10-00-C1-66-71-AB-0D-F1-00-01-03-CF-01-C6-39

'Recieved Packet #42
'1A-12-00-C1-69-D6-D4-0E-77-00-01-03-CF-81-CE-07-03-B2

'Recieved Packet #43
'1A-10-00-C1-2F-32-46-09-D2-00-01-03-CF-01-C9-35

'Recieved Packet #44
'1A-10-00-C1-66-71-A4-0D-F1-00-01-03-CF-01-C6-38

'Recieved Packet #45
'1A-10-00-C1-66-71-A3-0D-F1-00-01-03-CF-01-C6-3C

'Recieved Packet #46
'1A-10-00-C1-66-71-AE-0D-F1-00-01-03-CF-01-C6-33

'Recieved Packet #47
'1A-10-00-C1-2F-31-92-09-D1-00-01-03-CF-01-C9-35

'Recieved Packet #48
'1A-10-00-C0-C0-5C-B3-0B-33-00-01-03-CF-01-CA-2F

'Recieved Packet #49
'1A-10-00-C0-6C-2B-BB-10-5F-00-01-03-CF-01-CA-07

'Recieved Packet #50
'1A-10-00-C1-2F-33-54-09-D2-00-01-03-CF-01-C9-36

'Recieved Packet #51
'1A-12-00-C1-2F-6F-3D-09-9A-00-01-03-CF-81-C9-39-08-A5

'Recieved Packet #52
'1A-10-00-C0-6C-2C-E3-10-19-00-01-03-CF-01-CC-07

'Recieved Packet #53
'1A-10-00-C0-6C-03-3E-0B-33-00-01-03-CF-01-CB-07

'Recieved Packet #54
'1A-10-00-C0-C9-9F-B7-0B-98-00-01-03-CE-01-D4-07

'Recieved Packet #55
'1A-10-00-C0-69-6A-17-18-1E-00-01-03-CF-01-D2-1B

'Recieved Packet #56
'1A-10-00-C0-C9-9F-B9-0B-B0-00-01-03-CE-01-D4-07

'Recieved Packet #57
'1A-10-00-C0-69-6A-57-18-1F-00-01-03-CF-01-D2-2F

'Recieved Packet #58
'1A-12-00-C1-C6-5D-15-0E-43-00-01-03-D6-81-C3-0F-00-43

'Recieved Packet #59
'1A-10-00-C0-13-6D-A8-0E-ED-27-10-03-D7-01-CC-43

'Recieved Packet #60
'1A-10-00-C0-13-92-1F-0E-ED-27-10-03-D5-01-C5-43

'Recieved Packet #61
'1A-10-00-C0-13-83-B4-0E-ED-27-10-03-D7-01-C8-43

'Recieved Packet #62
'1A-10-00-C0-C0-53-D7-0B-2D-00-01-03-DB-01-C3-2F

'Recieved Packet #63
'1A-10-00-C0-E3-25-A2-0E-7D-00-01-03-D7-01-CB-1B

'Recieved Packet #64
'1A-10-00-C0-BB-CC-58-0B-2C-00-01-03-D0-01-C8-2F

'Recieved Packet #65
'1A-12-00-C0-3C-C6-FD-23-2A-00-01-03-D6-81-C3-43-00-B8

'Recieved Packet #66
'1A-10-00-C0-6B-CC-C5-1E-43-00-01-03-D6-01-C3-07

'Recieved Packet #67
'1A-10-00-C0-69-6A-4D-08-39-00-01-03-D4-01-C8-1B

'Recieved Packet #68
'1A-10-00-C1-66-71-C1-0B-2C-00-01-03-DC-01-C4-2F

'Recieved Packet #69
'1A-10-00-C0-6B-C5-70-19-A2-00-01-03-D5-01-CE-07

'Recieved Packet #70
'1A-12-00-C1-C6-62-1B-0E-43-00-01-03-D5-81-C3-07-00-26

'Recieved Packet #71
'1A-10-00-C0-BC-00-38-0B-2C-00-01-03-DC-01-C9-2F

'Recieved Packet #72
'1A-10-00-C0-1C-08-22-0B-2D-00-01-03-DC-01-C3-07

'Recieved Packet #73
'1A-10-00-C0-11-F5-A7-0E-ED-27-10-03-D3-01-CE-43

'Recieved Packet #74
'1A-12-00-C0-DC-83-2A-0E-43-00-01-03-D7-81-C3-0B-02-57

'Recieved Packet #75
'1A-10-00-C1-66-71-D1-0B-2D-00-01-03-D3-01-C3-2F

'Recieved Packet #76
'1A-10-00-C1-66-71-B2-0B-2C-00-01-03-D0-01-C7-2F

'Recieved Packet #77
'1A-10-00-C0-69-6A-6F-18-1D-00-01-03-D3-01-C3-07

'Recieved Packet #78
'1A-10-00-C0-13-85-FA-0E-ED-27-10-03-D6-01-C7-43

'Recieved Packet #79
'1A-10-00-C0-13-82-2E-0E-ED-27-10-03-D6-01-C9-43

'Recieved Packet #80
'1A-10-00-C0-1C-76-C3-0B-2D-00-01-03-DC-01-C3-13

'Recieved Packet #81
'1A-10-00-C0-13-82-F8-0E-ED-27-10-03-D7-01-C9-43

'Recieved Packet #82
'1A-10-00-C0-6B-C6-6F-0F-AF-00-01-03-D7-01-CE-07

'Recieved Packet #83
'1A-10-00-C1-86-F1-02-0E-43-00-01-03-D8-01-C3-07

'Recieved Packet #84
'1A-10-00-C1-41-87-1F-0E-ED-27-10-03-D5-01-C9-43

'Recieved Packet #85
'1A-10-00-C0-C0-36-CE-0B-2C-00-01-03-DC-01-CD-2F

'Recieved Packet #86
'1A-10-00-C0-1F-74-B4-09-AA-00-01-03-D5-01-CB-1B

'Recieved Packet #87
'1A-12-00-C1-AD-C5-97-0E-43-00-01-03-D7-81-C3-07-03-86

'Recieved Packet #88
'1A-10-00-C0-6B-DC-F5-19-2E-00-01-03-DC-01-C4-43

'Recieved Packet #89
'1A-10-00-C0-69-6A-58-08-43-00-01-03-D1-01-CA-2F

'Recieved Packet #90
'1A-10-00-C0-69-6A-5F-08-3F-00-01-03-D8-01-C5-1B

'Recieved Packet #91
'1A-10-00-C0-BC-00-BB-0B-2C-00-01-03-DC-01-CB-2F

'Recieved Packet #92
'1A-10-00-C0-69-6A-60-08-3F-00-01-03-DC-01-C5-1B

'Recieved Packet #93
'1A-10-00-C0-13-90-A0-0E-ED-27-10-03-D7-01-C6-43

'Recieved Packet #94
'1A-10-00-C0-1F-6F-D0-09-AA-00-01-03-D4-01-CB-1B

'Recieved Packet #95
'1A-10-00-C0-69-6A-4E-08-39-00-01-03-D8-01-C8-1B

'Recieved Packet #96
'1A-10-00-C1-66-71-D8-0B-2C-00-01-03-D0-01-C5-2F

'Recieved Packet #97
'1A-10-00-C1-66-71-EE-0B-2D-00-01-03-D4-01-C3-2F

'Recieved Packet #98
'1A-10-00-C0-13-75-63-0E-ED-27-10-03-D7-01-CB-43

'Recieved Packet #99
'1A-10-00-C0-6B-CC-CB-1E-45-00-01-03-D6-01-C5-07

'Recieved Packet #100
'1A-10-00-C1-66-71-F6-0B-2D-00-01-03-D9-01-C3-2F

'Recieved Packet #101
'1A-10-00-C0-1C-08-C5-0B-2D-00-01-03-DC-01-C3-15

'Recieved Packet #102
'1A-10-00-C1-A8-A9-80-0E-43-00-01-03-D8-01-C3-0F

'Recieved Packet #103
'1A-10-00-C0-C9-9F-B4-54-6F-00-01-03-D6-01-CA-00
#End Region


'Recieved Packet #104
'BF-0D-00-00-1D-40-C9-9F-B4-00-00-46-83

'Sent Packet #105
'BF-09-00-00-1E-40-C9-9F-B4

#Region "Most packets for items"

'Recieved Packet #106
'1A-10-00-C1-C6-0A-86-1E-BD-00-01-03-D7-01-CC-1B

'Recieved Packet #107
'1A-10-00-C1-66-71-D4-0B-2D-00-01-03-D7-01-C3-2F

'Recieved Packet #108
'1A-10-00-C0-6B-CC-CA-1E-44-00-01-03-D6-01-C4-07

'Recieved Packet #109
'1A-10-00-C0-6B-CC-D0-1E-48-00-01-03-D5-01-C3-07

'Recieved Packet #110
'1A-12-00-C1-8D-3A-6F-0E-FA-00-01-03-DC-81-C3-10-08-46

'Recieved Packet #111
'1A-10-00-C0-13-44-93-0E-ED-27-10-03-D5-01-CE-43

'Recieved Packet #112
'1A-10-00-C0-6B-DE-A2-09-31-00-01-03-D9-01-C3-43

'Recieved Packet #113
'1A-10-00-C0-13-48-FD-0E-ED-27-10-03-D7-01-CE-43

'Recieved Packet #114
'1A-10-00-C0-69-6A-12-18-20-00-01-03-D3-01-CE-43

'Recieved Packet #115
'1A-12-00-C1-C6-5C-2F-0E-43-00-01-03-D6-81-C3-07-00-43

'Recieved Packet #116
'1A-10-00-C0-6B-EF-48-12-0F-00-01-03-D6-01-C9-2F

'Recieved Packet #117
'1A-10-00-C0-29-82-69-0E-43-00-01-03-D8-01-C3-0B

'Recieved Packet #118
'1A-10-00-C0-69-6A-50-06-F3-00-01-03-D0-01-CF-1B

'Recieved Packet #119
'1A-10-00-C0-6B-EF-52-12-15-00-01-03-D5-01-CA-2F

'Recieved Packet #120
'1A-12-00-C0-CE-A2-00-0E-FA-00-01-03-DC-81-C3-0A-08-A5

'Recieved Packet #121
'1A-10-00-C0-69-6A-5A-08-47-00-01-03-DA-01-CA-2F

'Recieved Packet #122
'1A-10-00-C0-74-BE-E7-0E-ED-27-10-03-D5-01-C7-43

'Recieved Packet #123
'1A-10-00-C0-BC-00-6A-0B-2C-00-01-03-DC-01-C6-2F

'Recieved Packet #124
'1A-10-00-C0-6B-EF-49-12-10-00-01-03-D7-01-C9-2F

'Recieved Packet #125
'1A-10-00-C0-13-84-C2-0E-ED-27-10-03-D7-01-C7-43

'Recieved Packet #126
'1A-12-00-C1-C6-62-51-0E-43-00-01-03-D6-81-C3-0B-00-43

'Recieved Packet #127
'1A-10-00-C0-13-4B-3F-0E-ED-27-10-03-D7-01-CF-43

'Recieved Packet #128
'1A-12-00-C1-C6-5D-60-0E-43-00-01-03-D6-81-C3-13-00-43

'Recieved Packet #129
'1A-10-00-C1-66-71-BF-0B-2C-00-01-03-D0-01-C4-2F

'Recieved Packet #130
'1A-10-00-C0-E3-25-A3-0E-7D-00-01-03-D7-01-C3-1B

'Recieved Packet #131
'1A-10-00-C1-05-7E-83-0E-D4-00-01-03-D2-01-C9-43

'Recieved Packet #132
'1A-10-00-C0-23-BF-E2-0E-ED-27-10-03-D3-01-CB-43

'Recieved Packet #133
'1A-12-00-C1-C6-5C-94-0E-43-00-01-03-D7-81-C3-0F-00-43

'Recieved Packet #134
'1A-10-00-C1-66-71-F2-0B-2C-00-01-03-DC-01-C5-2F

'Recieved Packet #135
'1A-10-00-C0-6B-EF-67-12-12-00-01-03-D7-01-CB-2F

'Recieved Packet #136
'1A-10-00-C0-69-6A-56-08-43-00-01-03-D6-01-CF-1B

'Recieved Packet #137
'1A-10-00-C1-0A-E2-A8-0E-43-00-01-03-D8-01-C3-13

'Recieved Packet #138
'1A-10-00-C0-13-61-E3-0E-ED-27-10-03-D5-01-CB-43

'Recieved Packet #139
'1A-10-00-C1-66-71-F4-0B-2D-00-01-03-D1-01-C3-2F

'Recieved Packet #140
'1A-12-00-C0-94-69-D7-0E-FA-00-01-03-DC-81-C3-12-09-67

'Recieved Packet #141
'1A-12-00-C0-1A-C6-CE-0E-FA-00-01-03-DC-81-C3-0C-04-61

'Recieved Packet #142
'1A-10-00-C1-C3-EE-79-11-EA-00-01-03-D8-01-C3-1B

'Recieved Packet #143
'1A-10-00-C0-1C-07-A4-0B-2D-00-01-03-DC-01-C3-0D

'Recieved Packet #144
'1A-10-00-C1-66-71-E4-0B-2C-00-01-03-D0-01-C6-2F

'Recieved Packet #145
'1A-12-00-C1-AC-AC-A5-0E-FA-00-01-03-DC-81-C3-08-08-4C

'Recieved Packet #146
'1A-10-00-C0-6B-EF-47-12-0E-00-01-03-D5-01-C9-2F

'Recieved Packet #147
'1A-10-00-C1-66-71-AF-0B-2D-00-01-03-D5-01-C3-2F

'Recieved Packet #148
'1A-10-00-C0-69-6A-4A-08-45-00-01-03-D0-01-C9-1B

'Recieved Packet #149
'1A-10-00-C0-1C-02-AA-0B-2D-00-01-03-DC-01-C3-09

'Recieved Packet #150
'1A-10-00-C1-66-71-C0-0B-2D-00-01-03-D6-01-C3-2F

'Recieved Packet #151
'1A-10-00-C1-66-71-ED-0B-2D-00-01-03-DA-01-C3-2F

'Recieved Packet #152
'1A-10-00-C0-4F-BF-90-0E-D4-00-01-03-D2-01-C5-43

'Recieved Packet #153
'1A-10-00-C0-1C-03-39-0B-2D-00-01-03-DC-01-C3-11

'Recieved Packet #154
'1A-10-00-C1-66-71-C6-0B-2D-00-01-03-D8-01-C3-2F

'Recieved Packet #155
'1A-10-00-C1-66-71-CD-0B-2D-00-01-03-D2-01-C3-2F

'Recieved Packet #156
'1A-10-00-C0-6B-E1-E3-0B-44-00-01-03-D8-01-C3-43

'Recieved Packet #157
'1A-12-00-C1-AD-B2-12-0E-FA-00-01-03-DC-81-C3-0E-08-50

'Recieved Packet #158
'1A-12-00-C1-04-2F-BB-0E-FA-00-01-03-DC-81-C3-14-09-79

'Recieved Packet #159
'1A-12-00-C1-C6-5D-A6-0E-43-00-01-03-D5-81-C3-0B-00-26

'Recieved Packet #160
'1A-10-00-C0-69-6A-65-18-21-00-01-03-DD-01-C7-07

'Recieved Packet #161
'1A-10-00-C0-69-6A-4F-08-3F-00-01-03-D4-01-CD-1B

'Recieved Packet #162
'1A-10-00-C0-BB-C7-A5-0B-2C-00-01-03-D0-01-CE-2F

'Recieved Packet #163
'1A-10-00-C0-23-BD-D4-0E-ED-27-10-03-D5-01-CC-43

'Recieved Packet #164
'1A-10-00-C0-C0-37-4C-0B-2C-00-01-03-DC-01-CE-2F

'Recieved Packet #165
'1A-10-00-C1-C0-58-5D-0B-2D-00-01-03-DC-01-C3-0F

'Recieved Packet #166
'1A-10-00-C0-69-6A-5C-18-28-00-01-03-D1-01-C5-1B

'Recieved Packet #167
'1A-10-00-C0-6B-EF-62-12-16-00-01-03-D6-01-CA-2F

'Recieved Packet #168
'1A-10-00-C0-69-6A-5E-08-3F-00-01-03-D4-01-C5-1B

'Recieved Packet #169
'1A-10-00-C0-13-71-FE-0E-ED-27-10-03-D3-01-C9-43

'Recieved Packet #170
'1A-10-00-C1-C0-59-75-0B-2D-00-01-03-DC-01-C3-0B

'Recieved Packet #171
'1A-10-00-C0-13-7F-0B-0E-ED-27-10-03-D4-01-C9-43

'Recieved Packet #172
'1A-10-00-C0-6B-CC-C1-1E-41-00-01-03-D7-01-C4-07

'Recieved Packet #173
'1A-12-00-C1-C6-5C-CE-0E-43-00-01-03-D7-81-C3-13-00-43

'Recieved Packet #174
'1A-10-00-C0-13-6C-A6-0E-ED-27-10-03-D6-01-CC-43

'Recieved Packet #175
'1A-10-00-C0-6B-C5-6E-19-7A-00-01-03-D3-01-CE-07

'Recieved Packet #176
'1A-10-00-C0-13-95-6A-0E-ED-27-10-03-D6-01-C5-43

'Recieved Packet #177
'1A-10-00-C0-69-6A-69-18-21-00-01-03-DB-01-C5-07

'Recieved Packet #178
'1A-10-00-C0-13-91-80-0E-ED-27-10-03-D7-01-C5-43

'Recieved Packet #179
'1A-10-00-C0-BB-BA-B4-0B-2C-00-01-03-D0-01-CD-2F

'Recieved Packet #180
'1A-10-00-C0-69-6A-6C-18-22-00-01-03-D1-01-C5-07

'Recieved Packet #181
'1A-10-00-C0-6B-CC-CF-1E-47-00-01-03-D5-01-C4-07

'Recieved Packet #182
'1A-10-00-C0-13-45-F4-0E-ED-27-10-03-D6-01-CE-43

'Recieved Packet #183
'1A-10-00-C0-BB-FF-C6-0B-2C-00-01-03-DC-01-C7-2F

'Recieved Packet #184
'1A-10-00-C1-A6-97-BA-20-F3-00-01-03-D0-01-C3-07

'Recieved Packet #185
'1A-10-00-C0-6B-C5-71-19-9E-00-01-03-D6-01-CE-07

'Recieved Packet #186
'1A-10-00-C0-BB-AB-1E-0B-2C-00-01-03-D0-01-CB-2F

'Recieved Packet #187
'1A-11-00-C0-6B-C5-6F-19-7E-00-01-83-D4-01-CE-1D-07

'Recieved Packet #188
'1A-10-00-C0-23-C4-55-0E-ED-27-10-03-D4-01-CC-43

'Recieved Packet #189
'1A-10-00-C0-6B-DC-F6-19-30-00-01-03-DC-01-C5-43

'Recieved Packet #190
'1A-10-00-C0-6B-CC-C3-1E-42-00-01-03-D7-01-C3-07

'Recieved Packet #191
'1A-10-00-C0-6B-EF-66-12-13-00-01-03-D6-01-CB-2F

'Recieved Packet #192
'1A-10-00-C0-6B-E1-E0-0B-43-00-01-03-D7-01-C3-43

'Recieved Packet #193
'1A-10-00-C0-23-C2-8C-0E-ED-27-10-03-D3-01-CC-43

'Recieved Packet #194
'1A-10-00-C0-1F-71-66-0E-7D-00-01-03-D3-01-CC-1B

'Recieved Packet #195
'1A-10-00-C0-C0-27-93-0B-2C-00-01-03-DC-01-CF-2F

'Recieved Packet #196
'1A-10-00-C0-BB-B9-10-0B-2C-00-01-03-D0-01-CC-2F

'Recieved Packet #197
'1A-10-00-C0-13-43-2D-0E-ED-27-10-03-D4-01-CE-43

'Recieved Packet #198
'1A-10-00-C0-6B-EF-65-12-14-00-01-03-D5-01-CB-2F

'Recieved Packet #199
'1A-10-00-C0-13-2E-8D-0E-ED-27-10-03-D3-01-C5-43

'Recieved Packet #200
'1A-10-00-C0-BB-FF-66-0B-2C-00-01-03-DC-01-C8-2F

'Recieved Packet #201
'1A-10-00-C0-6B-CC-CE-1E-46-00-01-03-D5-01-C5-07

'Recieved Packet #202
'1A-10-00-C0-13-9E-EB-0E-ED-27-10-03-D4-01-C5-43

'Recieved Packet #203
'1A-10-00-C0-BB-CF-3E-0B-2C-00-01-03-D0-01-C9-2F

'Recieved Packet #204
'1A-10-00-C0-BB-C9-89-0B-2C-00-01-03-D0-01-CF-2F

'Recieved Packet #205
'1A-10-00-C0-BB-FF-F9-0B-2C-00-01-03-DC-01-CC-2F

'Recieved Packet #206
'1A-10-00-C0-6B-CC-BF-1E-40-00-01-03-D7-01-C5-07

'Recieved Packet #207
'1A-11-00-C0-6B-DE-B7-09-30-00-01-83-DA-01-C3-0B-43

'Recieved Packet #208
'1A-10-00-C0-6B-DC-F3-19-2C-00-01-03-DC-01-C3-43

'Recieved Packet #209
'1A-10-00-C0-6B-EF-64-12-11-00-01-03-D7-01-CA-2F

'Recieved Packet #210
'1A-10-00-C0-81-2D-75-1E-6C-00-01-03-D3-01-C4-07

'Recieved Packet #211
'1A-10-00-C0-C0-27-3A-0B-2D-00-01-03-DA-01-D1-2F

'Recieved Packet #212
'1A-10-00-C0-C0-27-C0-0B-2D-00-01-03-D7-01-D1-2F

'Recieved Packet #213
'1A-10-00-C0-69-6A-0E-18-1D-00-01-03-D3-01-D0-07

'Recieved Packet #214
'1A-10-00-C0-69-6A-13-18-28-00-01-03-DD-01-D3-07

'Recieved Packet #215
'1A-10-00-C0-C0-51-FF-0B-2C-00-01-03-D0-01-D0-2F

'Recieved Packet #216
'1A-10-00-C0-C0-28-F7-0B-2D-00-01-03-D1-01-D1-2F

'Recieved Packet #217
'1A-10-00-C0-69-6A-70-18-26-00-01-03-D1-01-D0-07

'Recieved Packet #218
'1A-10-00-C0-74-BA-32-0E-ED-27-10-03-D6-01-D0-43

'Recieved Packet #219
'1A-10-00-C0-C0-29-64-0B-2D-00-01-03-D4-01-D1-2F

'Recieved Packet #220
'1A-10-00-C0-69-6A-52-06-F1-00-01-03-D0-01-D0-1B

'Recieved Packet #221
'1A-10-00-C0-C0-4A-C5-0B-2D-00-01-03-DB-01-D1-2F

'Recieved Packet #222
'1A-10-00-C0-C0-28-52-0B-2D-00-01-03-D9-01-D1-2F

'Recieved Packet #223
'1A-10-00-C0-6D-BA-70-1D-A0-00-01-03-D5-01-D4-02

'Recieved Packet #224
'1A-10-00-C0-69-6A-0F-18-1E-00-01-03-D4-01-D0-07

'Recieved Packet #225
'1A-12-00-C0-68-45-4B-14-F0-00-01-03-D5-81-D2-43-06-72

'Recieved Packet #226
'1A-10-00-C1-AD-BF-38-09-AA-00-01-03-DE-01-D4-02

'Recieved Packet #227
'1A-10-00-C0-C0-28-17-0B-2D-00-01-03-D5-01-D1-2F

'Recieved Packet #228
'1A-10-00-C1-48-AD-60-09-A9-00-01-03-D7-01-D4-07

'Recieved Packet #229
'1A-10-00-C0-C0-27-10-0B-2D-00-01-03-D8-01-D1-2F

'Recieved Packet #230
'1A-10-00-C0-8E-2D-13-0E-43-00-01-03-D4-01-D4-06

'Recieved Packet #231
'1A-10-00-C1-88-40-6C-0E-43-00-01-03-D7-01-D4-03

'Recieved Packet #232
'1A-10-00-C0-6B-D6-12-0B-42-00-01-03-DA-01-D1-07

'Recieved Packet #233
'1A-10-00-C1-86-F1-03-0E-43-00-01-03-D4-01-D4-02

'Recieved Packet #234
'1A-10-00-C0-C0-29-C8-0B-2D-00-01-03-D2-01-D1-2F

'Recieved Packet #235
'1A-12-00-C0-68-45-4A-14-F0-00-01-03-D6-81-D2-43-06-72

'Recieved Packet #236
'1A-10-00-C0-69-6A-62-06-E7-00-01-03-D6-01-D3-07

'Recieved Packet #237
'1A-10-00-C0-69-6A-10-18-1F-00-01-03-D5-01-D0-07

'Recieved Packet #238
'1A-10-00-C0-69-6A-61-06-E5-00-01-03-D5-01-D3-07

'Recieved Packet #239
'1A-10-00-C0-69-6A-15-18-24-00-01-03-D7-01-D0-07

'Recieved Packet #240
'1A-10-00-C0-69-6A-11-18-20-00-01-03-D6-01-D0-07

'Recieved Packet #241
'1A-10-00-C0-6B-D6-11-0B-41-00-01-03-DA-01-D0-07

'Recieved Packet #242
'1A-10-00-C0-13-55-25-0E-ED-27-10-03-D7-01-D0-43

'Recieved Packet #243
'1A-10-00-C0-C0-28-B6-0B-2D-00-01-03-D3-01-D1-2F

'Recieved Packet #244
'1A-10-00-C0-69-6A-16-18-24-00-01-03-DD-01-D1-07

'Recieved Packet #245
'1A-10-00-C0-C0-4B-FF-0B-2C-00-01-03-DC-01-D0-2F

'Recieved Packet #246
'1A-10-00-C0-C0-27-E0-0B-2D-00-01-03-D6-01-D1-2F

'Recieved Packet #247
'1A-10-00-C0-86-BF-B2-54-04-00-01-03-D6-01-DD-00

#End Region

'Recieved Packet #248
'BF-0D-00-00-1D-40-86-BF-B2-00-00-07-90

'Sent Packet #249
'BF-09-00-00-1E-40-86-BF-B2

#Region "Even more packets for items on the ground"
'Recieved Packet #250
'1A-12-00-C0-3C-67-BE-1F-03-00-01-03-E4-81-C9-0C-08-A5

'Recieved Packet #251
'1A-12-00-C0-04-E7-2B-17-66-00-01-03-E6-81-CB-1B-04-7E

'Recieved Packet #252
'1A-10-00-C1-33-59-99-10-0F-00-01-03-E3-01-CB-07

'Recieved Packet #253
'1A-12-00-C1-76-6D-B4-15-3B-00-01-03-E1-81-C9-07-06-B4

'Recieved Packet #254
'1A-12-00-C1-84-8A-E0-1F-03-00-01-03-E4-81-C9-08-05-29

'Recieved Packet #255
'1A-10-00-C0-9A-3D-50-0E-42-00-01-03-E1-01-CC-07

'Recieved Packet #256
'1A-10-00-C1-8B-0E-02-0E-43-00-01-03-E5-01-C8-0F

'Recieved Packet #257
'1A-12-00-C0-70-80-99-0E-43-00-01-03-E6-81-C8-33-00-56

'Recieved Packet #258
'1A-10-00-C0-A0-FE-95-0A-9E-00-01-03-E7-01-CF-1B

'Recieved Packet #259
'1A-12-00-C0-4B-20-9E-23-2A-00-01-03-E6-81-C8-07-00-2B

'Recieved Packet #260
'1A-10-00-C1-EA-F8-B6-0E-7F-00-01-03-E1-01-CB-07

'Recieved Packet #261
'1A-12-00-C0-F8-7B-E5-0E-43-00-01-03-E5-81-C8-3F-00-44

'Recieved Packet #262
'1A-13-00-C1-13-83-06-0A-22-00-01-83-E5-81-CE-1D-0A-08-AB

'Recieved Packet #263
'1A-10-00-C0-22-93-65-0F-F2-00-01-03-E4-01-CF-07

'Recieved Packet #264
'1A-12-00-C0-01-5D-CB-12-24-00-01-03-E9-81-C9-07-04-7E

'Recieved Packet #265
'1A-10-00-C0-C5-2B-B0-18-76-00-02-03-E4-01-CE-08

'Recieved Packet #266
'1A-10-00-C1-BF-F5-A7-0F-AF-00-01-03-E1-01-CF-07

'Recieved Packet #267
'1A-11-00-C0-FB-82-D2-1E-D1-00-01-83-E3-01-CD-02-2F

'Recieved Packet #268
'1A-10-00-C1-66-44-9D-0E-79-00-01-03-E4-01-CA-07

'Recieved Packet #269
'1A-12-00-C0-8F-BC-C0-0B-2D-00-01-03-E4-81-CE-0B-00-D8

'Recieved Packet #270
'1A-10-00-C1-BF-EE-0F-19-96-00-01-03-E2-01-CE-1B

'Recieved Packet #271
'1A-10-00-C1-BF-F1-40-0F-B1-00-01-03-E1-01-CE-07

'Recieved Packet #272
'1A-10-00-C1-45-76-76-0F-B6-00-01-03-E4-01-CE-15

'Recieved Packet #273
'1A-10-00-C0-F8-7B-FA-0E-42-00-01-03-E1-01-CA-37

'Recieved Packet #274
'1A-10-00-C0-FE-E6-3E-18-77-00-02-03-E4-01-CE-0D

'Recieved Packet #275
'1A-10-00-C1-69-66-4A-18-23-00-01-03-EA-01-C8-2F

'Recieved Packet #276
'1A-12-00-C1-F3-CA-5C-0E-75-00-01-03-E2-81-C9-2F-07-D1

'Recieved Packet #277
'1A-12-00-C1-9C-AC-A1-1F-03-00-01-03-E4-81-C9-07-05-48

'Recieved Packet #278
'1A-12-00-C1-8A-42-45-0E-43-00-01-03-E5-81-C8-0B-00-58

'Recieved Packet #279
'1A-12-00-C0-F8-8A-B4-0E-43-00-01-03-E2-81-C8-3B-00-63

'Recieved Packet #280
'1A-10-00-C0-97-E2-B6-1E-BD-00-0B-03-E7-01-CD-07

'Recieved Packet #281
'1A-10-00-C1-35-9F-EA-0D-25-00-01-03-E1-01-CC-2F

'Recieved Packet #282
'1A-10-00-C1-B6-49-BE-0E-B1-00-01-03-EA-01-CA-2F

'Recieved Packet #283
'1A-10-00-C0-C9-A7-87-54-3F-00-01-03-E6-01-CF-00

#End Region


'Recieved Packet #284
'BF-0D-00-00-1D-40-C9-A7-87-00-00-2E-05

'Sent Packet #285
'BF-09-00-00-1E-40-C9-A7-87

#Region "And still, more packets for items on the ground"

'Recieved Packet #286
'1A-10-00-C1-BF-F2-0F-10-15-00-01-03-E1-01-C8-07

'Recieved Packet #287
'1A-10-00-C0-F8-8A-BE-0E-43-00-01-03-E4-01-C8-2F

'Recieved Packet #288
'1A-10-00-C1-69-66-4B-18-23-00-01-03-EB-01-C9-43

'Recieved Packet #289
'1A-10-00-C0-B6-25-E8-2B-DC-00-01-03-E1-01-CC-0D

'Recieved Packet #290
'1A-10-00-C1-1D-61-95-18-78-00-02-03-E4-01-CE-10

'Recieved Packet #291
'1A-10-00-C1-12-30-C3-2B-DB-00-01-03-E1-01-CE-07

'Recieved Packet #292
'1A-10-00-C1-1D-61-94-18-79-00-02-03-E4-01-CE-0A

'Recieved Packet #293
'1A-10-00-C0-D1-62-C1-0F-81-00-1F-03-E2-01-CF-07

'Recieved Packet #294
'1A-10-00-C1-7A-C6-58-0F-81-00-0C-03-E2-01-CB-33

'Recieved Packet #295
'1A-12-00-C1-73-6F-AB-0E-42-00-01-03-E1-81-CA-2F-00-1A

'Recieved Packet #296
'1A-12-00-C1-10-55-3A-0E-42-00-01-03-E1-81-CD-07-00-65

'Recieved Packet #297
'1A-12-00-C1-79-EE-10-0E-75-00-01-03-E2-81-CE-07-00-20

'Recieved Packet #298
'1A-10-00-C1-BF-F0-19-0B-44-00-01-03-E9-01-CA-07

'Recieved Packet #299
'1A-12-00-C0-F8-7B-E9-0E-42-00-01-03-E1-81-CA-33-00-3C

'Recieved Packet #300
'1A-10-00-C1-69-66-43-18-22-00-01-03-E5-01-CD-2F

'Recieved Packet #301
'1A-12-00-C0-A9-2E-C3-0E-75-00-01-03-E7-81-C8-2F-07-D1

'Recieved Packet #302
'1A-10-00-C0-FF-11-24-0E-79-00-01-03-E3-01-CD-07

'Recieved Packet #303
'1A-10-00-C1-BF-F6-65-10-61-00-01-03-E2-01-C8-07

'Recieved Packet #304
'1A-10-00-C0-E5-0E-42-0F-81-00-AF-03-E8-01-C8-2F

'Recieved Packet #305
'1A-10-00-C1-B8-7D-2D-1D-A0-00-01-03-E5-01-CC-07

'Recieved Packet #306
'1A-12-00-C0-F8-8A-C1-0E-43-00-01-03-E3-81-C8-3B-00-26

'Recieved Packet #307
'1A-10-00-C0-55-6F-F4-09-A9-00-01-03-E5-01-CE-1B

'Recieved Packet #308
'1A-12-00-C0-8F-BC-94-0B-2D-00-01-03-E4-81-CE-07-00-D8

'Recieved Packet #309
'1A-11-00-C1-BF-EE-0E-19-8A-00-01-83-E2-01-CD-1D-1B

'Recieved Packet #310
'1A-12-00-C0-8F-BC-A1-0B-2D-00-01-03-E4-81-CE-11-00-D8

'Recieved Packet #311
'1A-10-00-C1-BF-F0-15-0B-43-00-01-03-E8-01-CA-07

'Recieved Packet #312
'1A-12-00-C0-F8-8A-B8-0E-43-00-01-03-E3-81-C8-37-01-D3

'Recieved Packet #313
'1A-12-00-C0-8F-BC-A9-0B-2D-00-01-03-E4-81-CE-0E-00-D8

'Recieved Packet #314
'1A-12-00-C0-8F-BC-9C-0B-2D-00-01-03-E4-81-CE-09-00-D8

'Recieved Packet #315
'1A-10-00-C0-F8-8A-BA-0E-43-00-01-03-E3-01-C8-2F

'Recieved Packet #316
'1A-12-00-C0-F8-8A-BF-0E-43-00-01-03-E2-81-C8-33-00-36

'Recieved Packet #317
'1A-10-00-C0-F8-7B-F4-0E-43-00-01-03-E6-01-C8-37

'Recieved Packet #318
'1A-10-00-C1-35-A0-11-0C-FE-00-01-03-E1-01-CD-2F

'Recieved Packet #319
'1A-12-00-C0-F8-8A-B7-0E-43-00-01-03-E2-81-C8-3F-00-CA

'Recieved Packet #320
'1A-12-00-C0-F8-8A-C2-0E-43-00-01-03-E4-81-C8-37-01-3D

'Recieved Packet #321
'1A-12-00-C0-F8-8A-C3-0E-43-00-01-03-E5-81-C8-33-00-44

'Recieved Packet #322
'1A-10-00-C1-35-9F-68-0C-B7-00-01-03-E1-01-CF-2F

'Recieved Packet #323
'1A-12-00-C0-F8-7B-E2-0E-43-00-01-03-E5-81-C8-3B-00-44

'Recieved Packet #324
'1A-12-00-C0-F8-8A-C5-0E-43-00-01-03-E2-81-C8-37-00-FC

'Recieved Packet #325
'1A-12-00-C0-F8-8A-C6-0E-43-00-01-03-E3-81-C8-33-00-44

'Recieved Packet #326
'1A-10-00-C0-F8-7B-FD-0E-42-00-01-03-E9-01-C8-2F

'Recieved Packet #327
'1A-12-00-C0-F8-7B-E6-0E-43-00-01-03-E5-81-C8-37-00-44

'Recieved Packet #328
'1A-12-00-C0-F8-8A-D4-0E-43-00-01-03-E2-81-C8-2F-03-AD

'Recieved Packet #329
'1A-10-00-C0-F8-8A-D2-0E-43-00-01-03-E6-01-C8-2F

'Recieved Packet #330
'1A-12-00-C0-F8-8A-D0-0E-43-00-01-03-E4-81-C8-3F-00-8E

'Recieved Packet #331
'1A-12-00-C0-F8-8A-CE-0E-43-00-01-03-E4-81-C8-3B-00-2B

'Recieved Packet #332
'1A-12-00-C0-F8-8A-CD-0E-43-00-01-03-E5-81-C8-2F-00-2B

'Recieved Packet #333
'1A-12-00-C0-F8-8A-CC-0E-43-00-01-03-E3-81-C8-3F-00-ED

'Recieved Packet #334
'1A-12-00-C0-F8-8A-C8-0E-43-00-01-03-E4-81-C8-33-00-13

'Recieved Packet #335
'1A-10-00-C1-35-9F-70-1A-9A-00-01-03-E1-01-CE-2F

'Recieved Packet #336
'1A-10-00-C1-BF-EE-10-19-92-00-01-03-E2-01-CF-1B

'Recieved Packet #337
'1A-10-00-C1-BF-F6-66-10-62-00-01-03-E3-01-C8-07

'Recieved Packet #338
'1A-12-00-C0-C1-F7-46-0F-AB-00-01-03-E5-81-CB-07-00-01

'Recieved Packet #339
'1A-12-00-C1-76-C7-79-15-3B-00-01-03-E1-81-C9-08-06-BC

'Recieved Packet #340
'1A-10-00-C1-BF-EE-0D-19-86-00-01-03-E2-01-CC-1B

'Recieved Packet #341
'1A-12-00-C0-09-91-28-0E-42-00-01-03-E1-81-D1-07-00-21

'Recieved Packet #342
'1A-10-00-C1-35-A0-AA-0C-BE-00-01-03-E4-01-D2-2F

'Recieved Packet #343
'1A-13-00-C1-B8-10-6A-0B-1D-00-01-83-E1-81-D1-01-16-09-66

'Recieved Packet #344
'1A-12-00-C1-63-0F-6C-0E-D0-00-01-03-E5-81-D0-07-04-F2

'Recieved Packet #345
'1A-12-00-C0-4E-18-C1-0E-42-00-01-03-E1-81-D1-0F-00-0C

'Recieved Packet #346
'1A-12-00-C0-DD-45-97-14-3E-00-01-03-E5-81-D2-09-09-66

'Recieved Packet #347
'1A-12-00-C0-5A-5C-E9-0E-D0-00-01-03-E6-81-D0-07-04-8F

'Recieved Packet #348
'1A-12-00-C1-E8-95-ED-0E-42-00-01-03-E1-81-D0-07-00-35

'Recieved Packet #349
'1A-12-00-C1-76-35-9E-0E-42-00-01-03-E1-81-D1-0B-00-26

'Recieved Packet #350
'1A-10-00-C1-1B-2D-16-03-82-00-01-03-E0-01-D8-07

'Recieved Packet #351
'1A-12-00-C0-A0-05-DC-0E-77-00-01-03-EC-81-D7-07-03-B2

'Recieved Packet #352
'1A-10-00-C1-24-13-99-0E-43-00-01-03-E6-01-D1-1B

'Recieved Packet #353
'1A-12-00-C0-84-7A-5B-0E-42-00-01-03-E1-81-D0-0B-01-F6

'Recieved Packet #354
'1A-10-00-C0-53-52-E8-0D-F0-00-01-03-E2-01-D8-16

'Recieved Packet #355
'1A-10-00-C0-2B-0A-64-14-F6-00-01-03-E2-01-D9-02

'Recieved Packet #356
'1A-10-00-C1-01-D2-C6-0F-95-00-68-03-E2-01-D3-07

'Recieved Packet #357
'1A-10-00-C1-69-66-49-18-26-00-01-03-E2-01-D3-1B

'Recieved Packet #358
'1A-11-00-C1-D5-F5-CD-0A-0F-00-01-83-E1-01-D0-02-19

'Recieved Packet #359
'1A-10-00-C0-34-CA-CF-0E-79-00-01-03-E3-01-D3-0B

'Recieved Packet #360
'1A-10-00-C1-69-66-46-18-24-00-01-03-E1-01-D3-07

'Recieved Packet #361
'1A-12-00-C0-ED-1A-69-0E-43-00-01-03-E5-81-D3-07-03-86

'Recieved Packet #362
'1A-11-00-C1-9F-3C-2F-0A-22-00-01-83-E2-01-D8-1D-14

'Recieved Packet #363
'1A-10-00-C0-C9-A7-8C-0B-D2-00-01-03-E0-01-D9-07

'Recieved Packet #364
'1A-12-00-C1-06-F2-31-0E-75-00-01-03-E1-81-D9-02-00-62

'Recieved Packet #365
'1A-10-00-C0-C9-A7-88-0B-9A-00-01-03-E0-01-D9-07

'Recieved Packet #366
'1A-10-00-C1-35-9F-B2-0C-C8-00-01-03-E1-01-D2-2F

'Recieved Packet #367
'1A-12-00-C1-11-65-5A-0E-42-00-01-03-E3-81-D3-07-03-C1

'Recieved Packet #368
'1A-10-00-C0-83-20-27-0F-81-01-65-03-E6-01-D1-07

'Recieved Packet #369
'1A-12-00-C1-63-6D-A6-0B-35-00-01-03-E7-81-D3-07-01-5F

'Recieved Packet #370
'1A-10-00-C1-89-6A-68-0E-76-00-01-03-E5-01-D5-07

'Recieved Packet #371
'1A-12-00-C1-64-2D-9E-09-AA-00-01-03-E7-81-D3-0D-01-5F

'Recieved Packet #372
'1A-10-00-C1-69-66-48-18-26-00-01-03-E1-01-D4-2F

'Recieved Packet #373
'1A-10-00-C1-10-2F-CB-1E-B5-00-01-03-E2-01-D8-07

'Recieved Packet #374
'1A-12-00-C1-9F-7F-33-23-2A-00-01-03-E4-81-D2-07-00-56

'Recieved Packet #375
'1A-10-00-C1-69-66-44-18-1F-00-01-03-E1-01-D2-07

'Recieved Packet #376
'1A-10-00-C1-59-18-0A-0A-51-00-01-03-E4-01-D9-02

'Recieved Packet #377
'1A-10-00-C0-12-7B-35-09-BB-00-04-03-E3-01-D0-07

'Recieved Packet #378
'1A-10-00-C1-69-66-42-06-D7-00-01-03-E5-01-D8-07

'Recieved Packet #379
'1A-10-00-C1-69-66-40-06-D5-00-01-03-E4-01-D8-07

'Recieved Packet #380
'1A-10-00-C1-69-66-3F-06-75-00-01-03-EA-01-D0-1B

'Recieved Packet #381
'1A-10-00-C1-69-66-3B-06-77-00-01-03-EB-01-D0-1B

'Recieved Packet #382
'1A-10-00-C1-69-66-3A-18-22-00-01-03-E2-01-D8-07

'Recieved Packet #383
'1A-10-00-C1-69-66-47-18-24-00-01-03-E1-01-D5-2F

'Recieved Packet #384
'1A-10-00-C1-69-66-45-18-1F-00-01-03-E1-01-D3-1B

'Recieved Packet #385
'1A-10-00-C0-55-AC-15-0E-43-00-01-03-EC-01-D6-09

'Recieved Packet #386
'1A-12-00-C0-14-03-8C-23-2B-00-01-03-E3-81-D9-02-00-2B

'Recieved Packet #387
'1A-10-00-C1-35-A0-C5-0C-C8-00-01-03-E1-01-D3-2F

'Recieved Packet #388
'1A-10-00-C1-32-9F-43-0C-86-00-01-03-E2-01-D3-2F

'Recieved Packet #389
'1A-10-00-C0-FC-6F-0A-0C-84-00-01-03-E5-01-D0-30
#End Region

'Recieved Packet #390
'78-6B-00-00-05-4E-03-01-90-03-DA-01-C7-1B-06-83-EA-00-02-40-9D-0A-6F-0E-75-15-41-6E-A6-B6-10-86-0E-40-72-0B-F0-97-0D-03-04-89-41-99-C5-F1-97-18-06-04-89-41-BF-D8-F7-8E-FA-01-08-4B-41-3D-77-E6-9E-FD-05-04-89-41-3D-7B-BA-95-39-04-04-89-41-80-29-E3-3E-B7-19-7F-EA-C3-F3-A0-3D-0B-05-93-7F-EA-C3-F2-A0-40-10-05-93-00-00-00-00

'Recieved Packet #391
'78-54-00-00-04-76-BE-01-90-03-D2-01-D4-05-04-84-1C-00-01-40-69-6E-C9-0E-75-15-40-6A-1D-3D-9E-FD-05-04-89-40-6A-23-F6-97-18-06-04-89-40-6A-1E-D7-95-39-04-04-89-40-85-A9-FC-97-0D-03-04-89-7F-EE-21-07-A0-3D-0B-03-04-7F-EE-21-06-A0-40-10-02-3C-00-00-00-00

'Sent Packet #392
'09-00-04-76-BE

'Recieved Packet #393
'78-42-00-00-01-51-AD-01-90-03-D1-01-D1-1B-02-83-F4-00-01-40-1B-1C-E5-9E-FD-05-02-1F-40-1B-1C-E8-97-11-03-07-4C-40-1B-1C-E9-95-2E-04-07-72-40-1B-3E-18-13-F8-01-7F-FA-B5-4B-A0-3C-0B-04-78-00-00-00-00

'Sent Packet #394
'09-00-01-51-AD

'Recieved Packet #395
'B9-00-00-00-1F

'Recieved Packet #396
'20-00-05-4E-03-01-90-00-83-EA-00-03-DA-01-C7-00-00-06-1B

'Recieved Packet #397
'11-46-00-00-05-4E-03-4B-6F-6E-74-72-61-73-74-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-5E-00-5E-00-03-00-00-59-00-24-00-64-00-24-00-24-00-64-00-64-00-00-00-00-00-00-00-80-00-E1-02-05

'Recieved Packet #398
'72-00-00-32-00

'Recieved Packet #399
'78-6B-00-00-05-4E-03-01-90-03-DA-01-C7-1B-06-83-EA-00-02-40-9D-0A-6F-0E-75-15-41-6E-A6-B6-10-86-0E-40-72-0B-F0-97-0D-03-04-89-41-99-C5-F1-97-18-06-04-89-41-BF-D8-F7-8E-FA-01-08-4B-41-3D-77-E6-9E-FD-05-04-89-41-3D-7B-BA-95-39-04-04-89-41-80-29-E3-3E-B7-19-7F-EA-C3-F3-A0-3D-0B-05-93-7F-EA-C3-F2-A0-40-10-05-93-00-00-00-00

#Region "The server sends the message that he login is complete!"
'Recieved Packet #400
'55
#End Region

#End Region

#Region "Backpack Contents 'ContainerContents' Packet"
'3C-

'Size
'5D-02-

'Item Count
'00-1E-

'-------------------------------------------------------------
'Item Serial
'40-FE-32-90-

'Artwork
'0E-FA-

'StackID?
'00-

'Amount
'00-01-

'X
'00-4C-

'Y
'00-4D-

'Grid-Index, since 6.0.1.7 (2D)
'00-

'Container Serial
'40-9D-0A-6F-

'Hue
'08-4E-
'------------------------------------------------------------
'Item Serial
'40-5E-49-6F-

'Artwork
'0D-F2-

'StackID
'00-

'Amount
'00-01-

'X
'00-8F-

'Y
'00-62-

'Grid Index
'00-

'Container Serial
'40-9D-0A-6F-

'Hue
'00-00-

'Item Serial
'41-E8-4F-7A-

'
'0E-FA-00-00-01-00-36-00-4F-00-40-9D-0A-6F-08-AB-40-69-6A-4B-10-0F-00-00-01-00-83-00-41-00-40-9D-0A-6F-00-00-41-DE-26-50-13-B6-00-00-01-00-7C-00-53-00-40-9D-0A-6F-00-00-40-6F-6E-D0-0F-0C-00-00-01-00-8E-00-41-00-40-9D-0A-6F-00-00-40-6F-70-52-0F-0C-00-00-01-00-8E-00-41-00-40-9D-0A-6F-00-00-40-6F-6E-B8-0F-0C-00-00-01-00-8E-00-41-00-40-9D-0A-6F-00-00-40-6F-70-67-0F-0C-00-00-01-00-8E-00-41-00-40-9D-0A-6F-00-00-41-57-EA-E5-0E-FA-00-00-01-00-2C-00-41-00-40-9D-0A-6F-08-A5-41-07-DE-56-0E-B3-00-00-01-00-65-00-5C-00-40-9D-0A-6F-00-00-41-E4-92-C3-0E-FA-00-00-01-00-41-00-41-00-40-9D-0A-6F-08-46-41-34-36-5D-0F-9F-00-00-01-00-59-00-60-00-40-9D-0A-6F-00-00-40-19-09-71-09-13-00-00-01-00-8E-00-81-00-40-9D-0A-6F-04-80-41-8D-E1-86-0E-21-00-00-32-00-8C-00-7A-00-40-9D-0A-6F-00-00-41-8D-DF-AE-0F-85-00-00-3C-00-89-00-80-00-40-9D-0A-6F-00-00-41-8D-E0-09-0F-84-00-00-3C-00-89-00-7D-00-40-9D-0A-6F-00-00-41-8D-DF-78-0F-7A-00-00-1A-00-89-00-86-00-40-9D-0A-6F-00-00-41-8D-E0-46-0F-8C-00-00-3C-00-89-00-79-00-40-9D-0A-6F-00-00-41-8D-DF-ED-0F-88-00-00-39-00-89-00-78-00-40-9D-0A-6F-00-00-41-8D-E0-18-0F-8D-00-00-3C-00-89-00-7A-00-40-9D-0A-6F-00-00-41-8D-DF-C6-0F-86-00-00-1A-00-89-00-82-00-40-9D-0A-6F-00-00-41-8D-DF-6A-0F-7B-00-00-1A-00-89-00-7C-00-40-9D-0A-6F-00-00-41-CA-E8-38-09-F1-00-00-0A-00-66-00-7A-00-40-9D-0A-6F-00-00-40-60-54-84-0F-07-00-00-01-00-97-00-5B-00-40-9D-0A-6F-00-00-40-60-61-93-0F-0E-00-00-03-00-30-00-83-00-40-9D-0A-6F-00-00-40-60-55-C0-0F-07-00-00-01-00-2C-00-83-00-40-9D-0A-6F-00-00-40-60-80-B4-12-62-00-00-01-00-52-00-7A-00-40-9D-0A-6F-00-00-40-E7-69-09-0E-FA-00-00-01-00-41-00-61-00-40-9D-0A-6F-00-00-40-1D-F2-8E-20-F6-00-00-01-00-67-00-41-00-40-9D-0A-6F-00-00


#End Region

'Sent Packet #401
'34-ED-ED-ED-ED-04-00-05-4E-03

'Sent Packet #402
'B5-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00

'Recieved Packet #403
'5B-15-24-0A

'Recieved Packet #404
'BC-00-01

'Recieved Packet #405
'BF-06-00-00-08-00

'Recieved Packet #406
'AE-0C-01-FF-FF-FF-FF-FF-FF-00-03-B2-00-03-45-4E-55-00-53-79-73-74-65-6D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-57-00-65-00-6C-00-63-00-6F-00-6D-00-65-00-2C-00-20-00-4B-00-6F-00-6E-00-74-00-72-00-61-00-73-00-74-00-21-00-20-00-54-00-68-00-65-00-72-00-65-00-20-00-61-00-72-00-65-00-20-00-63-00-75-00-72-00-72-00-65-00-6E-00-74-00-6C-00-79-00-20-00-38-00-39-00-33-00-20-00-75-00-73-00-65-00-72-00-73-00-20-00-6F-00-6E-00-6C-00-69-00-6E-00-65-00-2C-00-20-00-77-00-69-00-74-00-68-00-20-00-31-00-38-00-31-00-33-00-34-00-36-00-35-00-31-00-20-00-69-00-74-00-65-00-6D-00-73-00-20-00-61-00-6E-00-64-00-20-00-36-00-35-00-39-00-32-00-37-00-37-00-20-00-6D-00-6F-00-62-00-69-00-6C-00-65-00-73-00-20-00-69-00-6E-00-20-00-74-00-68-00-65-00-20-00-77-00-6F-00-72-00-6C-00-64-00-2E-00-00

'Recieved Packet #407
'4F-00

'Recieved Packet #408
'4E-00-05-4E-03-00

'Recieved Packet #409
'DD-A8-00-00-10-D3-8E-93-A5-64-C3-00-00-00-00-00-00-00-00-00-00-00-89-00-00-00-E5-78-9C-5D-8E-51-0E-C2-20-10-44-3D-CA-1E-61-A0-2C-95-E3-A8-C1-DA-A4-54-A2-34-31-36-DE-DD-A1-FC-34-CD-C2-7C-30-8F-97-5D-25-5F-86-28-90-DF-2A-AF-F8-1E-BF-31-8F-37-31-A8-C7-7A-40-94-D7-74-5A-FB-4F-BA-3F-4A-9A-86-25-65-51-2B-7C-74-96-80-92-85-87-F3-62-38-E4-AE-4B-29-CF-59-3C-24-10-01-B6-E8-D9-A1-F5-7B-0F-89-C0-8F-8A-AA-6B-1E-47-0E-3B-8F-3D-6B-13-99-BE-46-D8-44-38-8A-3A-AE-72-34-69-03-4F-7F-69-20-3A-7F-00-00-00-00-00-00-00-00

'Recieved Packet #410
'AE-96-00-FF-FF-FF-FF-FF-FF-00-03-B2-00-03-45-4E-55-00-53-79-73-74-65-6D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-54-00-68-00-69-00-73-00-20-00-61-00-63-00-63-00-6F-00-75-00-6E-00-74-00-20-00-69-00-73-00-20-00-6F-00-77-00-6E-00-65-00-64-00-20-00-62-00-79-00-20-00-27-00-6D-00-65-00-67-00-61-00-6D-00-61-00-6E-00-64-00-6F-00-73-00-40-00-68-00-6F-00-74-00-6D-00-61-00-69-00-6C-00-2E-00-63-00-6F-00-6D-00-27-00-2E-00-00

'Recieved Packet #411
'3A-F5-01-02-00-01-00-44-00-00-01-03-E8-00-02-00-00-00-00-01-03-E8-00-03-03-E8-03-E8-00-03-E8-00-04-00-00-00-00-01-03-E8-00-05-00-00-00-00-01-03-E8-00-06-00-4B-00-00-01-03-E8-00-07-00-00-00-00-01-03-E8-00-08-00-59-00-00-01-03-E8-00-09-00-6F-00-00-01-03-E8-00-0A-03-E8-03-E8-00-03-E8-00-0B-01-7E-00-00-01-03-E8-00-0C-00-C4-00-00-01-03-E8-00-0D-00-66-00-00-01-03-E8-00-0E-01-74-00-00-01-03-E8-00-0F-00-00-00-00-01-03-E8-00-10-00-22-00-00-01-03-E8-00-11-00-00-00-00-01-03-E8-00-12-00-9B-00-00-01-03-E8-00-13-00-00-00-00-01-03-E8-00-14-00-00-00-00-01-03-E8-00-15-00-C0-00-00-01-03-E8-00-16-00-00-00-00-01-03-E8-00-17-00-15-00-00-01-03-E8-00-18-00-57-00-00-01-03-E8-00-19-00-5A-00-00-01-03-E8-00-1A-03-E8-03-E8-00-03-E8-00-1B-00-00-00-00-01-03-E8-00-1C-00-00-00-00-01-03-E8-00-1D-00-5A-00-00-01-03-E8-00-1E-03-E8-03-E8-00-03-E8-00-1F-00-AE-00-00-01-03-E8-00-20-00-31-00-00-01-03-E8-00-21-00-00-00-00-01-03-E8-00-22-00-24-00-00-01-03-E8-00-23-00-8D-00-00-01-03-E8-00-24-03-E8-03-E8-00-03-E8-00-25-00-00-00-00-01-03-E8-00-26-00-51-00-00-01-03-E8-00-27-00-AA-00-00-01-03-E8-00-28-03-E8-03-E8-00-03-E8-00-29-00-4B-00-00-01-03-E8-00-2A-00-53-00-00-01-03-E8-00-2B-00-3B-00-00-01-03-E8-00-2C-00-53-00-00-01-03-E8-00-2D-00-B2-00-00-01-03-E8-00-2E-00-B2-00-00-01-03-E8-00-2F-03-E8-03-E8-00-03-E8-00-30-00-00-00-00-01-03-E8-00-31-00-00-00-00-01-03-E8-00-32-00-00-00-00-00-03-E8-00-33-00-00-00-00-00-03-E8-00-34-00-00-00-00-00-03-E8-00-35-00-00-00-00-00-03-E8-00-36-00-00-00-00-00-03-E8-00-37-00-00-00-00-01-03-E8-00-00

'Recieved Packet #412
'D8-1E-04-03-00-40-C9-9F-B4-00-00-46-83-06-0B-04-0D-09-20-86-3D-20-78-9C-63-48-63-48-21-08-19-18-92-19-08-81-64-FA-AB-62-0F-20-4A-55-34-51-AA-02-89-52-B5-1B-43-15-23-51-AA-C8-B7-31-8A-7A-21-C1-40-CD-B0-27-2A-54-C1-98-50-EA-4A-65-8F-02-00-41-5A-1E-16-21-20-A7-20-78-9C-65-91-6B-0E-84-30-08-84-FD-81-27-DA-D4-2B-7B-02-D2-5A-1F-EB-D1-76-5A-1E-D2-35-13-43-5A-BE-0E-08-74-D3-06-5D-D3-D4-A3-0A-A7-8B-34-43-07-15-A8-F6-98-A1-A2-AA-31-83-17-76-2E-9D-E3-96-C1-AD-10-5F-DA-A1-33-10-3C-66-BC-FE-EA-84-50-F0-98-3F-4D-FE-36-AB-BB-F7-01-62-01-B1-38-C1-21-B2-7A-A4-C6-84-FA-E6-F5-78-C0-61-4E-83-07-C7-4E-3B-91-5E-1E-CF-44-AA-38-84-3E-C6-7F-15-42-3C-E4-AF-77-CD-E4-50-6B-D3-A9-DF-BA-85-23-F4-C2-5D-71-FA-1C-A6-CE-7F-55-7D-3F-BA-85-41-ED-0E-9F-ED-C5-FA-31-15-8D-E7-0F-B8-D8-B2-78-22-20-CA-20-78-9C-6D-90-4D-12-82-30-0C-85-BB-78-AC-D0-22-8C-A3-E7-60-AF-47-6B-67-7A-1A-3D-81-0A-E2-CF-A5-44-97-A6-49-64-A8-30-6F-1A-D2-CE-47-5E-12-14-58-CC-A8-C4-13-0D-1A-63-70-87-C5-1A-CB-89-56-E8-70-21-9D-D0-12-91-1B-93-ED-B7-8E-E5-49-F4-A5-0A-07-1C-71-25-DD-22-91-ED-98-F0-03-E5-B8-42-AB-B4-C5-06-25-65-81-F2-82-FC-0B-EE-A2-C3-99-88-F8-EA-61-EB-1E-79-CC-62-8E-9C-EE-2F-8A-42-78-FE-CF-D6-1F-22-9C-D6-4B-09-F6-D4-1A-E2-5E-D1-0C-15-4F-F2-88-3D-26-2E-69-1F-BA-13-99-6D-E4-F2-EB-A3-17-69-E5-B1-CB-1C-21-9D-4E-FB-D0-BD-FE-11-81-9D-A6-31-BA-BC-79-1F-6E-50-48-4E-32-0B-9D-30-A2-54-32-FF-F0-CE-5B-D4-28-77-F7-05-08-89-7B-14-23-20-6E-20-78-9C-63-39-C0-B2-13-37-64-60-60-D9-C7-B2-87-65-1B-5E-B8-0B-A8-62-05-CB-6E-AC-10-A1-62-17-AB-00-10-F2-A3-93-50-D3-61-2A-F8-A0-72-7C-60-12-4C-A3-A9-40-87-FC-58-55-F0-43-CD-C1-6E-8B-00-AA-2C-56-15-02-28-2A-70-D9-82-C0-40-12-C3-2F-70-3F-80-D5-10-F6-0B-AA-3B-B6-E1-88-95-E5-48-2A-08-C5-0B-21-15-FB-71-C4-1B-2C-EE-F6-02-00-BA-1C-86-A9-24-20-86-20-78-9C-7D-90-DD-0D-C0-20-08-84-7D-60-FF-0D-1C-83-40-ED-EF-68-6D-F1-44-6C-13-73-11-89-7C-9C-01-BA-68-99-EA-A0-8D-E4-11-5B-54-93-40-35-2F-46-B0-13-8D-52-E4-95-60-CB-D9-14-AB-D1-23-83-C8-76-9E-98-92-FF-F5-25-DE-BE-8C-33-12-82-7E-35-8F-19-C1-70-CC-78-ED-44-13-87-18-09-F1-69-DA-4C-8C-59-FB-3E-2A-A1-98-B7-6F-67-F4-50-F7-91-1F-D1-5C-15-F5-B8-FB-4E-D4-1A-7B-65-24-66-2A-74-D2-6A-52-DC-51-2F-B1-DF-30-C4-C3-AE-25-64-5E-20-78-9C-63-3E-C5-7C-92-F9-20-10-9E-C4-0A-21-32-27-18-C0-00-46-23-00-42-86-F9-00-41-35-07-88-52-73-02-6A-F3-71-4C-35-30-19-4C-FD-58-DC-43-15-35-D8-DC-4A-8E-1A-CA-DD-83-F0-3B-2C-6E-F0-A8-C5-19-5F-48-6A-0E-E0-57-C3-7C-0A-68-17-21-35-27-A0-2E-03-A5-92-E3-38-52-D0-71-E6-E3-00-AC-43-4B-EA-26-64-8A-20-78-9C-8D-92-DB-0D-80-20-0C-45-49-BA-FF-4A-86-FA-43-65-08-5C-C5-4B-79-1B-40-21-A9-A5-1E-0E-22-90-A7-0B-FD-D6-B8-7A-8A-99-B6-56-27-A1-80-E8-C1-DF-C4-C4-C8-39-77-A7-EF-A3-C5-45-5E-B9-30-F7-A9-47-19-58-30-8F-CE-6A-E1-81-D1-F9-64-4B-6C-D9-E8-19-CC-D6-98-77-25-31-5B-0F-18-5D-FF-C8-23-64-69-9C-F6-B8-F3-F4-EB-82-79-ED-79-CA-7C-7B-64-F1-3D-DC-55-A5-F7-CC-1B-FE-F0-F2-0C-2A-F3-CF-23-F9-6C-CB-8D-69-BD-54-DC-03-D7-04-6A-1B-27-64-3A-20-78-9C-63-3E-C5-7C-92-20-3C-C1-40-00-30-9F-20-56-0D-C7-4E-8E-1D-30-08-E4-C1-D8-BB-51-D4-6C-C7-A6-1F-26-3A-AA-86-63-17-D6-30-DC-86-AC-06-3F-A0-A6-1A-82-A9-E7-38-00-06-0F-50-ED-28-64-24-20-78-9C-63-BE-C4-7C-91-F9-20-01-7C-81-81-00-60-3E-C0-7C-80-A0-9A-0B-A3-E6-0C-47-73-08-A6-9F-F3-00-E8-FE-34-89

'Recieved Packet #413
'D8-D5-00-03-00-40-86-BF-B2-00-00-07-90-00-FE-00-C4-07-20-90-22-00-78-9C-63-48-63-48-41-82-0C-0C-C9-0C-08-90-CC-CE-45-24-9F-91-44-F5-C8-7C-84-ED-A9-EC-5C-00-3B-0A-0B-6E-21-60-14-00-78-9C-63-E5-63-E5-63-60-60-E5-63-45-A2-A9-09-01-9F-5C-03-6B-22-60-10-00-78-9C-63-E5-63-00-02-56-3E-5A-41-00-9E-9E-03-6B-23-60-10-00-78-9C-63-E5-63-00-02-56-3E-5A-41-00-9E-9E-03-6B-25-7E-21-00-78-9C-63-60-60-64-43-82-AC-0C-08-F0-13-0F-EF-0F-0A-8F-01-0F-EF-0F-86-29-BF-E0-F0-07-00-66-BC-0B-18-26-7E-18-00-78-9C-63-60-60-64-43-82-AC-0C-08-F0-93-46-BC-5F-70-F8-03-00-C6-D4-0D-04-27-7E-18-00-78-9C-63-60-60-64-43-82-AC-0C-08-F0-93-46-BC-5F-70-F8-03-00-C6-D4-0D-04

'Recieved Packet #414
'D8-3F-03-03-00-40-C9-A7-87-00-00-2E-05-04-19-03-2E-0A-20-EE-32-10-78-9C-63-48-63-48-21-08-19-18-92-19-08-81-64-E6-6F-C4-A8-22-D2-AC-2F-44-A9-7A-4F-A6-2A-46-72-DD-35-58-55-31-FF-04-AA-22-14-8B-A9-CC-3F-00-0C-49-1C-61-21-98-89-10-78-9C-5D-90-6D-0E-80-20-08-86-FB-C1-1D-BC-6D-AB-BB-74-24-A1-B9-72-AD-9B-84-7C-08-AB-77-9A-C2-03-BE-0A-0F-F4-9F-68-E1-6F-FC-65-77-41-83-CA-22-9B-C9-56-1E-43-66-9B-AD-29-C5-AB-91-3C-CA-CA-84-D7-21-4B-73-98-D8-4B-08-AD-EE-C2-8C-7C-4F-FD-9C-A0-D4-A9-0A-E7-51-27-F0-E7-32-7C-FB-5D-C6-FE-10-F2-9C-C4-3D-7D-84-AB-10-9A-9B-78-0F-9C-B7-D4-A1-6F-A7-5D-5A-3A-35-7A-8C-7A-2C-7B-D9-CA-0E-6F-3A-73-3A-D0-9C-EA-03-82-D5-A8-92-22-98-69-10-78-9C-85-90-5D-0A-C0-20-0C-83-7D-88-67-12-76-2E-BD-F0-1E-F7-EF-4E-B0-40-BB-59-66-C7-0C-F2-D1-1A-48-10-1B-16-D4-98-21-2C-C2-10-EE-99-9B-15-27-66-3A-84-3A-D3-A1-33-37-3B-26-1C-74-08-CB-9B-54-56-95-2F-C1-ED-D1-F8-D5-A3-B1-CF-75-7A-FC-EA-C9-AD-36-1F-A3-51-DF-43-5F-62-92-EB-FC-07-5F-83-39-4E-F2-10-93-75-5C-D0-D4-8C-01-23-98-36-10-78-9C-63-99-C3-32-9B-05-0B-66-00-02-6C-E2-94-62-61-17-02-F2-D6-0C-0C-C2-46-04-D4-28-53-DF-5D-F8-31-03-0A-C0-16-56-F8-E4-D1-F5-03-55-60-0D-6F-64-00-00-2C-03-68-8D-24-98-50-10-78-9C-85-8F-0B-0A-40-21-08-04-07-C1-93-74-FF-3B-06-D9-CB-2F-BC-16-51-D9-61-2B-15-FD-D7-3A-12-E5-CC-DC-9D-5B-46-98-C3-EB-34-62-25-C7-B3-9C-20-DC-22-63-06-C9-99-32-EA-0B-63-FF-FE-C3-30-75-E1-3E-E1-34-26-B9-46-14-6A-20-AA-2A-B1-01-0E-E3-0A-3B-25-D4-58-10-78-9C-8D-91-6D-0A-00-21-08-44-F3-59-DD-FF-A4-ED-11-DA-BE-A0-85-A5-54-11-FB-F1-70-86-09-C8-C4-D6-B2-F6-7E-ED-9D-C2-B5-40-2A-6A-30-7D-2C-46-DB-9D-3B-32-39-C3-4F-70-68-F5-2B-0E-46-6D-3F-38-FC-9C-B4-98-E9-8F-A4-4F-19-A2-5A-46-3F-4B-2D-FF-FF-48-EA-97-79-01-07-B0-0C-AE-26-D4-58-10-78-9C-8D-91-4B-12-C0-20-08-43-4D-94-DE-A4-FB-DE-FF-72-9A-FE-16-4E-EB-04-1C-61-61-86-17-84-64-23-B8-29-AF-2A-58-8B-09-EA-58-4D-30-12-7D-2C-6B-3F-3C-4B-1A-CF-8A-C4-5C-F5-5F-43-EA-67-70-5F-F4-AF-1F-BD-37-F4-99-E6-58-25-31-FB-E5-E8-DD-CE-B3-9F-99-73-C6-00-E8-C0-04-B8-27-D4-4E-10-78-9C-8D-91-01-0A-00-20-08-03-73-65-FF-FF-71-2D-A8-20-0C-9C-A2-82-8A-27-0A-A0-C3-68-8D-FA-8F-86-5A-12-01-35-ED-71-B8-30-27-67-B9-C0-2A-02-CB-05-56-FD-F7-E0-5C-8D-66-23-EE-C3-7A-B3-F1-D2-22-3F-D2-94-CC-FE-C9-F5-2F-67-C9-04-45-C9-04-27-28-D4-2C-10-78-9C-63-94-62-94-24-08-25-18-08-00-46-89-91-A5-86-11-39-D4-C4-31-D5-80-E5-C5-51-4D-C2-34-9B-3C-11-8C-D8-11-47-57-03-00-C5-DF-05-CD-09-3C-31-00-78-9C-15-C9-A9-11-00-40-08-04-41-B1-8F-C2-60-C8-3F-C2-23-85-63-DD-4C-35-56-2E-3C-A9-B0-52-5F-31-CB-BE-E5-60-1D-75-D4-51-47-1D-35-E7-03-86-98-0C-85

'Recieved Packet #415
'1C-35-00-00-04-76-BE-01-90-06-00-35-00-03-4B-6F-6E-74-72-61-73-74-00-00-00-00-00-00-