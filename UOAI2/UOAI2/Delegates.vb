Partial Class UOAI
    Friend Delegate Sub SimpleDelegate()
    Friend Delegate Sub HandleClientDelegate(ByVal client As Client)
    Friend Delegate Sub HandlePacketDelegate(ByVal packet As Packet)
    Friend Delegate Sub HandleKeyDelegate(ByVal virtual_key_code As UInteger)
    Friend Delegate Sub HandlePartyInvitationDelegate(ByVal invitation As PartyInvitation)
    Friend Delegate Sub HandleItemDelegate(ByVal tohandle As Item)
    Friend Delegate Sub HandleMobileDelegate(ByVal tohandle As Mobile)
    Friend Delegate Sub HandleDragDelegate(ByVal Amount As UShort)
    Friend Delegate Sub HandleDropDelegate(ByVal droppeditem As Item, ByVal location As Point3D, ByVal container As Item)

    Friend Delegate Function PacketConstructor(ByVal buffer As Byte(), ByVal origin As Enums.PacketOrigin) As Packet
End Class