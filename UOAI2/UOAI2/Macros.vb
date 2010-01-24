Partial Class UOAI

    Partial Class Client

        Private Function GetBytesFromUIntArray(ByVal toconvert() As UInteger) As Byte()
            Dim intbytes(toconvert.Length * 4 - 1) As Byte
            Dim buffer(3) As Byte

            For i As Integer = 0 To toconvert.Length - 1
                buffer = BitConverter.GetBytes(toconvert(i))
                intbytes((i * 4) + 0) = buffer(0)
                intbytes((i * 4) + 1) = buffer(1)
                intbytes((i * 4) + 2) = buffer(2)
                intbytes((i * 4) + 3) = buffer(3)
            Next

            Return intbytes
        End Function

        Private Sub Macro(ByVal MacroType As Enums.Macros, ByVal IntParameter As Int32, ByVal StrParameter As String)
            'TODO: Make this sub do some macro stuff.
            Dim StringParameterAddress As UInteger
            Dim MacroTableAddress As UInteger
            Dim MacroTable(9) As UInteger

            'Ask the injected dll to allocate a buffer to hold the string parameter (unicode + 0-terminated)
            'So StringParameterAddress is the address of the buffer on the client where we can write the string onto the client
            StringParameterAddress = InjectedDll.allocate((StrParameter.Length + 1) * 2)

            'Write the string to the client in the allocated buffer
            PStream.WriteUStr(StringParameterAddress, StrParameter)

            'build the macro table (parameters to the event macro function of the client come in a table)
            MacroTable(6) = MacroType
            MacroTable(7) = IntParameter
            MacroTable(8) = StringParameterAddress

            'Allocate memory on the client to hold this table:
            MacroTableAddress = InjectedDll.allocate(40)

            'Write the table onto the Client
            PStream.Write(MacroTableAddress, GetBytesFromUIntArray(MacroTable))

            'Tell the injected dll to make the call
            ' ccall EventMacro(index_to_the_macro, macrotableaddress) 
            InjectedDll.ccall(_CallibrationInfo.pEventMacro, New UInteger() {0, MacroTableAddress})

            'clean up the allocated buffers
            InjectedDll.free(StringParameterAddress)
            InjectedDll.free(MacroTableAddress)
        End Sub

        ''' <summary>
        ''' Standard UO macros
        ''' </summary>
        Public Class UOMacros
            Private m_Client As Client

            Public Sub New(ByVal onclient As Client)
                m_Client = onclient
            End Sub

            ''' <summary>
            ''' Says the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Say(ByVal message As String)
                m_Client.Macro(1, 0, message)
            End Sub

            ''' <summary>
            ''' Emotes the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Emote(ByVal message As String)
                m_Client.Macro(2, 0, message)
            End Sub

            ''' <summary>
            ''' Whispers the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Whisper(ByVal message As String)
                m_Client.Macro(3, 0, message)
            End Sub

            ''' <summary>
            ''' Yells the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Yell(ByVal message As String)
                m_Client.Macro(4, 0, message)
            End Sub

            ''' <summary>
            ''' Walks to the specified direction.
            ''' </summary>
            ''' <param name="direction">The direction.</param>
            Public Sub Walk(ByVal direction As Enums.Direction)
                m_Client.Macro(5, CInt(direction), "")
            End Sub

            ''' <summary>
            ''' Toggles the war/peace mode.
            ''' </summary>
            Public Sub ToggleWarMode()
                m_Client.Macro(6, 0, "")
            End Sub

            ''' <summary>
            ''' Pastes.
            ''' </summary>
            Public Sub Paste()
                m_Client.Macro(7, 0, "")
            End Sub

            ''' <summary>
            ''' Opens the specified element.
            ''' </summary>
            ''' <param name="element">The element to open.</param>
            Public Sub Open(ByVal element As Enums.Element)
                m_Client.Macro(8, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Closes the specified element.
            ''' </summary>
            ''' <param name="element">The element to Close.</param>
            Public Sub Close(ByVal element As Enums.Element)
                m_Client.Macro(9, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Minimizes the specified element.
            ''' </summary>
            ''' <param name="element">The element to Close.</param>
            Public Sub Minimize(ByVal element As Enums.Element)
                m_Client.Macro(10, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Maximizes the specified element.
            ''' </summary>
            ''' <param name="element">The element to Close.</param>
            Public Sub Maximize(ByVal element As Enums.Element)
                m_Client.Macro(11, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Opens the door
            ''' </summary>
            Public Sub OpenDoor()
                m_Client.Macro(12, 0, "")
            End Sub

            ''' <summary>
            ''' Uses a skill
            ''' </summary>
            Public Sub UseSkill(ByVal skill As Enums.Skills)
                m_Client.Macro(13, CInt(skill), "")
            End Sub

            ''' <summary>
            ''' Uses the last skill
            ''' </summary>
            Public Sub LastSkill()
                m_Client.Macro(14, 0, "")
            End Sub

            ''' <summary>
            ''' Casts the specified spell
            ''' </summary>
            Public Sub CastSpell(ByVal spell As Enums.Spell)
                m_Client.Macro(15, CInt(spell), "")
            End Sub

            ''' <summary>
            ''' Casts the last spell
            ''' </summary>
            Public Sub LastSpell()
                m_Client.Macro(16, 0, "")
            End Sub

            ''' <summary>
            ''' Sends the last object
            ''' </summary>
            Public Sub LastObject()
                m_Client.Macro(17, 0, "")
            End Sub

            ''' <summary>
            ''' Bows
            ''' </summary>
            Public Sub Bow()
                m_Client.Macro(18, 0, "")
            End Sub

            ''' <summary>
            ''' Salutes
            ''' </summary>
            Public Sub Salute()
                m_Client.Macro(19, 0, "")
            End Sub

            ''' <summary>
            ''' Quits the game.
            ''' </summary>
            Public Sub QuitGame()
                m_Client.Macro(20, 0, "")
            End Sub

            ''' <summary>
            ''' Display all the names.
            ''' </summary>
            Public Sub AllNames()
                m_Client.Macro(21, 0, "")
            End Sub

            ''' <summary>
            ''' Sends again the last target.
            ''' </summary>
            Public Sub LastTarget()
                m_Client.Macro(22, 0, "")
            End Sub

            ''' <summary>
            ''' Send yourself as target
            ''' </summary>
            Public Sub TargetSelf()
                m_Client.Macro(23, 0, "")
            End Sub

            ''' <summary>
            ''' Arms or disarms
            ''' </summary>
            Public Sub ArmOrDisarm()
                m_Client.Macro(24, 0, "")
            End Sub

            ''' <summary>
            ''' Waits for target.
            ''' </summary>
            Public Sub WaitForTarget()
                m_Client.Macro(25, 0, "")
            End Sub

            ''' <summary>
            ''' Targets the next (attackant?).
            ''' </summary>
            Public Sub TargetNext()
                m_Client.Macro(26, 0, "")
            End Sub

            ''' <summary>
            ''' Attacks the last target.
            ''' </summary>
            Public Sub AttackLast()
                m_Client.Macro(27, 0, "")
            End Sub

            ''' <summary>
            ''' Waits the specified seconds before send the next macro command.
            ''' </summary>
            ''' <param name="seconds">The seconds to wait.</param>
            Public Sub Delay(ByVal seconds As Integer)
                m_Client.Macro(28, seconds, "")
            End Sub

            ''' <summary>
            ''' Entables or disable the circle of transparency.
            ''' </summary>
            Public Sub CircleTrans()
                m_Client.Macro(29, 0, "")
            End Sub

            ''' <summary>
            ''' Closes all the gump.
            ''' </summary>
            Public Sub CloseAllGumps()
                m_Client.Macro(30, 0, "")
            End Sub

            ''' <summary>
            ''' Enables or disables the Always run option.
            ''' </summary>
            Public Sub AlwaysRun()
                m_Client.Macro(31, 0, "")
            End Sub

            ''' <summary>
            ''' Saves the desktop (ie your preferences and interface).
            ''' </summary>
            Public Sub SaveDesktop()
                m_Client.Macro(32, 0, "")
            End Sub

            ''' <summary>
            ''' Kills the open gump.
            ''' </summary>
            Public Sub KillGumpOpen()
                m_Client.Macro(33, 0, "")
            End Sub

            ''' <summary>
            ''' Uses the weapon primary ability.
            ''' </summary>
            Public Sub UsePrimaryAbility()
                m_Client.Macro(34, 0, "")
            End Sub

            ''' <summary>
            ''' Uses the weapon secondary ability.
            ''' </summary>
            Public Sub UseSecondaryAbility()
                m_Client.Macro(35, 0, "")
            End Sub

            ''' <summary>
            ''' Equips the last weapon.
            ''' </summary>
            Public Sub EquipLastWeapon()
                m_Client.Macro(36, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range.
            ''' </summary>
            ''' <param name="range">The range.</param>
            Public Sub SetUpdateRange(ByVal range As Integer)
                m_Client.Macro(37, range, "")
            End Sub

            ''' <summary>
            ''' Modifies the update range.
            ''' </summary>
            ''' <param name="range">The range.</param>
            Public Sub ModifyUpdateRange(ByVal range As Integer)
                m_Client.Macro(38, range, "")
            End Sub

            ''' <summary>
            ''' Increases the update range.
            ''' </summary>
            Public Sub IncreaseUpdateRange()
                m_Client.Macro(39, 0, "")
            End Sub

            ''' <summary>
            ''' Decreases the update range.
            ''' </summary>
            Public Sub DecreaseUpdateRange()
                m_Client.Macro(40, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range at maximum.
            ''' </summary>
            Public Sub MaxUpdateRange()
                m_Client.Macro(41, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range at minimum.
            ''' </summary>
            Public Sub MinUpdateRange()
                m_Client.Macro(42, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range at default value.
            ''' </summary>
            Public Sub DefaultUpdateRange()
                m_Client.Macro(43, 0, "")
            End Sub

            ''' <summary>
            ''' Updates the range info.
            ''' </summary>
            Public Sub UpdateRangeInfo()
                m_Client.Macro(44, 0, "")
            End Sub

            ''' <summary>
            ''' Enables the color of the range.
            ''' </summary>
            Public Sub EnableRangeColor()
                m_Client.Macro(45, 0, "")
            End Sub

            ''' <summary>
            ''' Disables the color of the range.
            ''' </summary>
            Public Sub DisableRangeColor()
                m_Client.Macro(46, 0, "")
            End Sub

            ''' <summary>
            ''' Enables or disables the color of the range.
            ''' </summary>
            Public Sub ToggleRangeColor()
                m_Client.Macro(47, 0, "")
            End Sub


            ''' <summary>
            ''' Invokes the specified virtue.
            ''' </summary>
            ''' <param name="virtue">The virtue to invoke.</param>
            Public Sub InvokeVirtue(ByVal virtue As Enums.Virtues)
                m_Client.Macro(48, CInt(virtue), "")
            End Sub
        End Class
    End Class

End Class