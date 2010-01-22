Partial Class UOAI

    Partial Class Client

        Private Shared Sub Macro(ByVal MacroType As Enums.Macros, ByVal IntParameter As Int32, ByVal StrParameter As String)
            'TODO: Make this sub do some macro stuff.
        End Sub

        ''' <summary>
        ''' Standard UO macros
        ''' </summary>
        Public Class Macros

            ''' <summary>
            ''' Says the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Say(ByVal message As String)
                Macro(1, 0, message)
            End Sub

            ''' <summary>
            ''' Emotes the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Emote(ByVal message As String)
                Macro(2, 0, message)
            End Sub

            ''' <summary>
            ''' Whispers the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Whisper(ByVal message As String)
                Macro(3, 0, message)
            End Sub

            ''' <summary>
            ''' Yells the specified message.
            ''' </summary>
            ''' <param name="message">The message.</param>
            Public Sub Yell(ByVal message As String)
                Macro(4, 0, message)
            End Sub

            ''' <summary>
            ''' Walks to the specified direction.
            ''' </summary>
            ''' <param name="direction">The direction.</param>
            Public Sub Walk(ByVal direction As Enums.Direction)
                Macro(5, CInt(direction), "")
            End Sub

            ''' <summary>
            ''' Toggles the war/peace mode.
            ''' </summary>
            Public Sub ToggleWarMode()
                Macro(6, 0, "")
            End Sub

            ''' <summary>
            ''' Pastes.
            ''' </summary>
            Public Sub Paste()
                Macro(7, 0, "")
            End Sub

            ''' <summary>
            ''' Opens the specified element.
            ''' </summary>
            ''' <param name="element">The element to open.</param>
            Public Sub Open(ByVal element As Enums.Element)
                Macro(8, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Closes the specified element.
            ''' </summary>
            ''' <param name="element">The element to Close.</param>
            Public Sub Close(ByVal element As Enums.Element)
                Macro(9, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Minimizes the specified element.
            ''' </summary>
            ''' <param name="element">The element to Close.</param>
            Public Sub Minimize(ByVal element As Enums.Element)
                Macro(10, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Maximizes the specified element.
            ''' </summary>
            ''' <param name="element">The element to Close.</param>
            Public Sub Maximize(ByVal element As Enums.Element)
                Macro(11, CInt(element), "")
            End Sub

            ''' <summary>
            ''' Opens the door
            ''' </summary>
            Public Sub OpenDoor()
                Macro(12, 0, "")
            End Sub

            ''' <summary>
            ''' Uses a skill
            ''' </summary>
            Public Sub UseSkill(ByVal skill As Enums.Skills)
                Macro(13, CInt(skill), "")
            End Sub

            ''' <summary>
            ''' Uses the last skill
            ''' </summary>
            Public Sub LastSkill()
                Macro(14, 0, "")
            End Sub

            ''' <summary>
            ''' Casts the specified spell
            ''' </summary>
            Public Sub CastSpell(ByVal spell As Enums.Spell)
                Macro(15, CInt(spell), "")
            End Sub

            ''' <summary>
            ''' Casts the last spell
            ''' </summary>
            Public Sub LastSpell()
                Macro(16, 0, "")
            End Sub

            ''' <summary>
            ''' Sends the last object
            ''' </summary>
            Public Sub LastObject()
                Macro(17, 0, "")
            End Sub

            ''' <summary>
            ''' Bows
            ''' </summary>
            Public Sub Bow()
                Macro(18, 0, "")
            End Sub

            ''' <summary>
            ''' Salutes
            ''' </summary>
            Public Sub Salute()
                Macro(19, 0, "")
            End Sub

            ''' <summary>
            ''' Quits the game.
            ''' </summary>
            Public Sub QuitGame()
                Macro(20, 0, "")
            End Sub

            ''' <summary>
            ''' Display all the names.
            ''' </summary>
            Public Sub AllNames()
                Macro(21, 0, "")
            End Sub

            ''' <summary>
            ''' Sends again the last target.
            ''' </summary>
            Public Sub LastTarget()
                Macro(22, 0, "")
            End Sub

            ''' <summary>
            ''' Send yourself as target
            ''' </summary>
            Public Sub TargetSelf()
                Macro(23, 0, "")
            End Sub

            ''' <summary>
            ''' Arms or disarms
            ''' </summary>
            Public Sub ArmOrDisarm()
                Macro(24, 0, "")
            End Sub

            ''' <summary>
            ''' Waits for target.
            ''' </summary>
            Public Sub WaitForTarget()
                Macro(25, 0, "")
            End Sub

            ''' <summary>
            ''' Targets the next (attackant?).
            ''' </summary>
            Public Sub TargetNext()
                Macro(26, 0, "")
            End Sub

            ''' <summary>
            ''' Attacks the last target.
            ''' </summary>
            Public Sub AttackLast()
                Macro(27, 0, "")
            End Sub

            ''' <summary>
            ''' Waits the specified seconds before send the next macro command.
            ''' </summary>
            ''' <param name="seconds">The seconds to wait.</param>
            Public Sub Delay(ByVal seconds As Integer)
                Macro(28, seconds, "")
            End Sub

            ''' <summary>
            ''' Entables or disable the circle of transparency.
            ''' </summary>
            Public Sub CircleTrans()
                Macro(29, 0, "")
            End Sub

            ''' <summary>
            ''' Closes all the gump.
            ''' </summary>
            Public Sub CloseAllGumps()
                Macro(30, 0, "")
            End Sub

            ''' <summary>
            ''' Enables or disables the Always run option.
            ''' </summary>
            Public Sub AlwaysRun()
                Macro(31, 0, "")
            End Sub

            ''' <summary>
            ''' Saves the desktop (ie your preferences and interface).
            ''' </summary>
            Public Sub SaveDesktop()
                Macro(32, 0, "")
            End Sub

            ''' <summary>
            ''' Kills the open gump.
            ''' </summary>
            Public Sub KillGumpOpen()
                Macro(33, 0, "")
            End Sub

            ''' <summary>
            ''' Uses the weapon primary ability.
            ''' </summary>
            Public Sub UsePrimaryAbility()
                Macro(34, 0, "")
            End Sub

            ''' <summary>
            ''' Uses the weapon secondary ability.
            ''' </summary>
            Public Sub UseSecondaryAbility()
                Macro(35, 0, "")
            End Sub

            ''' <summary>
            ''' Equips the last weapon.
            ''' </summary>
            Public Sub EquipLastWeapon()
                Macro(36, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range.
            ''' </summary>
            ''' <param name="range">The range.</param>
            Public Sub SetUpdateRange(ByVal range As Integer)
                Macro(37, range, "")
            End Sub

            ''' <summary>
            ''' Modifies the update range.
            ''' </summary>
            ''' <param name="range">The range.</param>
            Public Sub ModifyUpdateRange(ByVal range As Integer)
                Macro(38, range, "")
            End Sub

            ''' <summary>
            ''' Increases the update range.
            ''' </summary>
            Public Sub IncreaseUpdateRange()
                Macro(39, 0, "")
            End Sub

            ''' <summary>
            ''' Decreases the update range.
            ''' </summary>
            Public Sub DecreaseUpdateRange()
                Macro(40, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range at maximum.
            ''' </summary>
            Public Sub MaxUpdateRange()
                Macro(41, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range at minimum.
            ''' </summary>
            Public Sub MinUpdateRange()
                Macro(42, 0, "")
            End Sub

            ''' <summary>
            ''' Sets the update range at default value.
            ''' </summary>
            Public Sub DefaultUpdateRange()
                Macro(43, 0, "")
            End Sub

            ''' <summary>
            ''' Updates the range info.
            ''' </summary>
            Public Sub UpdateRangeInfo()
                Macro(44, 0, "")
            End Sub

            ''' <summary>
            ''' Enables the color of the range.
            ''' </summary>
            Public Sub EnableRangeColor()
                Macro(45, 0, "")
            End Sub

            ''' <summary>
            ''' Disables the color of the range.
            ''' </summary>
            Public Sub DisableRangeColor()
                Macro(46, 0, "")
            End Sub

            ''' <summary>
            ''' Enables or disables the color of the range.
            ''' </summary>
            Public Sub ToggleRangeColor()
                Macro(47, 0, "")
            End Sub


            ''' <summary>
            ''' Invokes the specified virtue.
            ''' </summary>
            ''' <param name="virtue">The virtue to invoke.</param>
            Public Sub InvokeVirtue(ByVal virtue As Enums.Virtues)
                Macro(48, CInt(virtue), "")
            End Sub
        End Class
    End Class

End Class