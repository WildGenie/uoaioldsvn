Partial Class UOAI

    Partial Class Client
        Private _WaitingForHue As Boolean = False

        ''' <summary>
        ''' Displays a hue picker.
        ''' </summary>
        ''' <param name="Artwork">The artwork of the item or w/e you want to hue, this is only for the GUI.</param>
        ''' <param name="UID">A UID to keep track of this, the response will be handled by the event "onHueResponse".</param>
        Public Sub PromptForHue(ByVal Artwork As UShort, ByVal UID As UInteger)
            _WaitingForHue = True

            Dim j As New Packets.HuePicker(New Serial(UID), Artwork, CUShort(0))

            Send(j, Enums.PacketDestination.CLIENT)
        End Sub

        Public Sub PromptForHue(ByVal UID As UInteger)
            _WaitingForHue = True

            Dim j As New Packets.HuePicker(New Serial(UID), CUShort(&H191), CUShort(0))

            Send(j, Enums.PacketDestination.CLIENT)
        End Sub

        Public Event onHueResponse(ByVal UID As UInteger, ByVal Hue As UShort)

    End Class

End Class