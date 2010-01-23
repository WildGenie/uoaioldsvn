Imports System.Text
Imports System.Threading
Imports System.Runtime.InteropServices
Imports System.Net
Imports System.Net.Sockets
Imports System.IO


Partial Class UOAI
    'CallibrationInfo structure to hold info read from the injected dll
    <StructLayout(LayoutKind.Sequential, Size:=640)> _
    Public Structure CallibrationInfo
        Public pSendHook As UInt32
        'address of the send-hook function in the injected dll
        Public pHandlePacketHook As UInt32
        'address of the handlepacket(recv)-hook function in the injected dll
        Public pOriginalSend As UInt32
        'address of the client's original send function (which was hooked)
        Public pOriginalHandlePacket As UInt32
        'address of the client's original 
        Public pPacketInfo As UInt32
        'packet sizes
        Public pOriginalSendb As UInt32
        'part of how send-hooks are set
        Public pSendbHook As UInt32
        'part of how send-hooks are set
        Public pOriginalSendc As UInt32
        'part of how send hooks are set
        Public pSendcHook As UInt32
        'part of how send hooks are set
        Public pOriginalConnLossHandler As UInt32
        'Connection-Loss handler is hooked too, this is the original client's handler
        Public pConnLossHook As UInt32
        'Connection-loss hook address
        Public pPlayer As UInt32
        'memory address of a variable on the client containing  a pointer to the player-mobile object
        Public pPlayerStatus As UInt32
        'memory address of a variable on the client containing a pointer to the player's status gump object
        Public pItemList As UInt32
        'memory address of a variable on the client containing a pointer to the first Item in a linked list of all items seen by the client
        Public pEventMacro As UInt32
        'memory address of the event macro function
        Public pBLoggedIn As UInt32
        'memory address of a boolean indicating whether this client is logged in to a gameserver or not
        Public pLastSkill As UInt32
        'memory address of the lastskill variable
        Public pLastSpell As UInt32
        'memory address of the last spell variable
        Public pLastObject As UInt32
        'memory address of the last object (id) variable
        Public pLTargetID As UInt32
        'memory address of the last target id variable
        Public pLTargetKind As UInt32
        'memory address of the last target kind variable
        Public pLTargetX As UInt32
        'memory address of the last target x variable
        Public pLTargetY As UInt32
        'memory address of the last target y variable
        Public pLTargetZ As UInt32
        'memory address of the last target z variable
        Public pLTargetTile As UInt32
        'memory address of the last target tile type variable
        Public pSockObject As UInt32
        'memory address of the client's network object (has the socket, send, recv, handlepacket functions as members)
        Public pAlwaysRun As UInt32
        'memory address of the always run boolean variable
        Public pBTargCurs As UInt32
        'memory address of a boolean variable indicating whether a target cursor is shown or not
        Public pFindItemByID As UInt32
        'memory address of the client's finditem-function (find's items by id)
        Public pLastObjectType As UInt32
        'memory address of the last object type variable
        Public pLastObjectFunc As UInt32
        'memory address of the "last object function" (does the same thing as the last object macro)
        Public pLoginCrypt As UInt32
        'badly named: this function performs sending packets: it encryptes/encodes them and then queues then in a to-send buffer
        Public pSend As UInt32
        Public pRecv As UInt32
        Public pCalculatePath As UInt32
        'part of the pathfind functionality
        Public pStartOfPath As UInt32
        'part of the pathfind functionality
        Public pBDoPathfind As UInt32
        'part of the pathfind functionality
        Public pWalkPath As UInt32
        'part of the pathfind functionality
        Public pInvertPath As UInt32
        'part of the pathfind functionality
        Public pGumpList As UInt32
        'memory address of a variable containing a pointer to the first gump object in the gump list
        Public pFindStatusGump As UInt32
        '
        Public pTextOut As UInt32
        'Show message function address
        Public pJournalStart As UInt32
        'journal start pointer
        Public pGumpOpener As UInt32
        'ShowGump-function to create custom gumps
        Public pOnTarget As UInt32
        'function called when a target is selected
        Public pDefaultOnTarget As UInt32
        'default value of pOnTarget (to be set if pBShowTargetCursor is set to true)
        Public pSocketObjectVtbl As UInt32
        'pointer to the virtual function table on the network object
        Public pWinMain As UInt32
        'address of the client's windows main function
        Public pPatchPos2 As UInt32
        'used in encryption patching
        Public pPacketHandlerFunc As UInt32
        'client's packet handler function read from the SocketObjectVtbl
        Public pFacet As UInt32
        'address where the current facet is stored
        Public pPatchPos3 As UInt32
        'used in encryption patching
        Public pPatchPos4 As UInt32
        'used in encryption patching
        Public pHoldingID As UInt32
        'id of the item currently being dragged
        Public pShowItemPropertyGump As UInt32
        'function related to AOS Item Properties and Names
        Public PortTable As UInt32
        'IPIndex is an index into IPTable/PortTable to specify which server to connect to
        Public IPTable As UInt32
        'by modifying those 3 variables you don't have to use login.cfg
        Public IPIndex As UInt32
        Public pSkillCaps As UInt32
        'start of skill caps list
        Public pSkillLocks As UInt32
        'start of skill locks list
        Public pSkillRealValues As UInt32
        'start of skill real values list
        Public pSkillModifiedValues As UInt32
        'start of modified skill values list
        Public pCharName As UInt32
        'pointer to a string representing the current player character's name
        Public pServerName As UInt32
        'pointer to a string representing the current server's name
        Public pAllocate As UInt32
        'do we need this?, [ccall, 1 parameter: size]
        Public pStatusGumpConstructor As UInt32
        '[stdthiscall, pThis=allocated unconstructed gump, 2 parameters : mobile * pMobile, boolean IsPlayer]
        Public pShowGump As UInt32
        '[aka AddToGumpList(pParent,boolean) ... the boolean is ignored, client always uses 1 ... stdthiscall... pThis=gump]
        Public pPartyMembers As UInt32
        'array of 10 x struct partymember{ID,char[32] name} (i think... even though name should typically be only 30 bytes)
        Public pLoadFromIndexedFile As UInt32
        'indexed mul file access
        Public pStaticTileData As UInt32
        'lookup pStaticTileData + 40*type {int flags at offset 0, byte weight at offset 4, short animationtype at offset 0xC, byte height at offset 0x12, char[20] name at offset 0x13
        Public pLandTileData As UInt32
        'lookup pLandTileData + 28*type {int flags at offset 0, short tiletype (see art) at offset 4, char[20] name at offset 6}
        Public pLoadFromMulFile As UInt32
        'non-indexed mul file access
        Public logincryptpatchpos As UInt32
        'used in encrpytion patching
        Public logincryptpatchtarget As UInt32
        'used in encryption patching
        Public recvcryptpatchpos As UInt32
        'used in encryption patching
        Public recvcryptpatchtarget As UInt32
        'used in encryption patching
        Public sendcryptpatchpos As UInt32
        'used in encryption patching
        Public sendcryptpatchtarget As UInt32
        'used in encryption patching
        Public pPropGumpID As UInt32
        'ID of item currently shown on the AOS Item Name/Properties gump
        Public pPropsAndNameGet As UInt32
        'address of a function related to Item Name/Property parsing
        Public pGetName As UInt32
        'low level item property/name accesss
        Public pGetProperties As UInt32
        'low level item property/name acess
        Public pPropInitA As UInt32
        '
        Public pPropInitB As UInt32
        '
        Public itempropgumpduplicate As UInt32
        '
        Public pDoGetName As UInt32
        '
        Public pDoGetProperties As UInt32
        '
        Public pStringObjectInit As UInt32
        '
        Public pDefaultNameString As UInt32
        '
        Public pDefaultPropertiesString As UInt32
        '
        Public pMapInfo As UInt32

        'item object fields
        Public oItemX As UInt32
        Public oItemY As UInt32
        Public oItemZ As UInt32
        Public oItemType As UInt32
        Public oItemTypeIncrement As UInt32

        Public oItemDirection As UInt32
        Public oItemID As UInt32
        Public oItemContainer As UInt32
        Public oItemNext As UInt32
        Public oItemContentsNext As UInt32

        Public oItemContentsPrevious As UInt32
        Public oItemContents As UInt32
        Public oItemGump As UInt32
        Public oItemColor As UInt32
        Public oItemHighlightColor As UInt32

        Public oItemReputation As UInt32
        Public oItemStackCount As UInt32

        Public oItemFlags As UInt32

        'ItemVtbl entries
        Public oItemIsMobile As UInt32
        'multi
        Public oMultiType As UInt32
        Public oMultiContents As UInt32

        'mobile object fields
        Public oMobileStatus As UInt32
        Public oMobileLayers As UInt32

        Public oMobileWarMode As UInt32
        Public oMobileEnemy As UInt32
        Public oMobileRunning As UInt32

        'status gump fields
        Public oStatusName As UInt32

        'journal fields
        Public oJournalNext As UInt32
        Public oJournalPrevious As UInt32
        Public oJournalText As UInt32
        Public oJournalColor As UInt32
        Public oPaperdollTitle As UInt32

        'gump vtbl entries
        Public oGumpClosable As UInt32
        Public oGumpClose As UInt32
        Public oGumpClick As UInt32
        Public oGumpFocus As UInt32
        Public oGumpWriteChar As UInt32

        'gump fields
        Public oGumpID As UInt32
        '0x50
        Public oGumpType As UInt32
        Public oGumpNext As UInt32
        Public oGumpPrevious As UInt32
        Public oGumpSubGumpFirst As UInt32
        Public oGumpSubGumpLast As UInt32
        Public oGumpX As UInt32
        Public oGumpY As UInt32
        Public oGumpWidth As UInt32
        Public oGumpHeight As UInt32
        Public oGumpName As UInt32
        Public oGumpItem As UInt32

        'button gump
        Public oButtonGumpOnClick As UInt32

        'input control gump
        Public oGumpText As UInt32

        'unicode input control gump
        Public oGumpUnicodeText As UInt32

        'generic gump
        Public oGenericGumpID As UInt32
        Public GenericGumpType As UInt32
        Public oGumpElements As UInt32
        Public oGumpSubElements As UInt32
        'generic gump element
        Public oGumpElementType As UInt32
        Public oGumpElementNext As UInt32
        Public oGumpElementID As UInt32
        Public oGumpElementSelected As UInt32
        Public oGumpElementText As UInt32
        Public oGumpElementTooltip As UInt32
        Public oGumpElementX As UInt32
        Public oGumpElementY As UInt32
        Public oGumpElementWidth As UInt32
        Public oGumpElementHeight As UInt32
        Public oGumpElementReleasedType As UInt32
        Public oGumpElementPressedType As UInt32
        'generic gump element vtbl
        Public oGumpElementClick As UInt32
    End Structure

    '''<summary>IPCConstants = windows messages send to the injected dll to perform synchronized actions</summary>
    Friend Class IPCConstants
        ''' <summary></summary>
        Friend Shared customcallmessage As UInteger = 0

        '2
        ''' <summary></summary>
        Friend Shared allocmessage As UInteger = 0

        '5
        ''' <summary></summary>
        Friend Shared freemessage As UInteger = 0

        '6
        ''' <summary></summary>
        Friend Shared newstackmessage As UInteger = 0

        '7
        ''' <summary></summary>
        Friend Shared pushmessage As UInteger = 0

        '8
        ''' <summary></summary>
        Friend Shared stdcallmessage As UInteger = 0

        '9
        ''' <summary></summary>
        Friend Shared ccallmessage As UInteger = 0

        '10
        ''' <summary></summary>
        Friend Shared thiscallmessage As UInteger = 0

        '11
        ''' <summary></summary>
        Friend Shared stdthiscallmessage As UInteger = 0

        '12
        ''' <summary></summary>
        Friend Shared memcpymessage As UInteger = 0

        '13
        ''' <summary></summary>
        Friend Shared memsetzeromessage As UInteger = 0

        '14
        ''' <summary></summary>
        Friend Shared getcallibrationinfomessage As UInteger = 0

        '15
        ''' <summary></summary>
        Friend Shared clientlockmessage As UInteger = 0

        '16
        ''' <summary></summary>
        Friend Shared clientunlockmessage As UInteger = 0

        '17
        ''' <summary></summary>
        Friend Shared safestdthiscallmessage As UInteger = 0

        '24
        ''' <summary></summary>
        Friend Shared setpacketfiltermessage As UInteger = 0

        '25
        ''' <summary></summary>
        Friend Shared removepacketfiltermessage As UInteger = 0

        '26
        ''' <summary></summary>
        Friend Shared querypacketfiltermessage As UInteger = 0

        '27
        ''' <summary></summary>
        Friend Shared skipuoaistdthiscallmessage As UInteger = 0

        '28
        ''' <summary></summary>
        Friend Shared geteventportmessage As UInteger = 0

        ''' <summary></summary>
        Friend Shared patchencryptionmessage As UInteger = 0

        ''' <summary></summary>
        Friend Shared hookitemdestructormessage As UInteger = 0

        ''' <summary></summary>
        Friend Shared setupeventtimermessage As UInteger = 0

        Shared Sub New()
            setupeventtimermessage = [Imports].RegisterWindowMessage("setupeventtimermessage")
            hookitemdestructormessage = [Imports].RegisterWindowMessage("hookitemdestructormessage")
            customcallmessage = [Imports].RegisterWindowMessage("customcallmessage")
            allocmessage = [Imports].RegisterWindowMessage("allocmessage")
            freemessage = [Imports].RegisterWindowMessage("freemessage")
            newstackmessage = [Imports].RegisterWindowMessage("newstackmessage")
            pushmessage = [Imports].RegisterWindowMessage("pushmessage")
            stdcallmessage = [Imports].RegisterWindowMessage("stdcallmessage")
            ccallmessage = [Imports].RegisterWindowMessage("ccallmessage")
            thiscallmessage = [Imports].RegisterWindowMessage("thiscallmessage")
            memcpymessage = [Imports].RegisterWindowMessage("memcpymessage")
            getcallibrationinfomessage = [Imports].RegisterWindowMessage("getcallibrationinfomessage")
            stdthiscallmessage = [Imports].RegisterWindowMessage("stdthiscallmessage")
            clientlockmessage = [Imports].RegisterWindowMessage("clientlockmessage")
            memsetzeromessage = [Imports].RegisterWindowMessage("memsetzeromessage")
            setpacketfiltermessage = [Imports].RegisterWindowMessage("setpacketfiltermessage")
            removepacketfiltermessage = [Imports].RegisterWindowMessage("removepacketfiltermessage")
            querypacketfiltermessage = [Imports].RegisterWindowMessage("querypacketfiltermessage")
            safestdthiscallmessage = [Imports].RegisterWindowMessage("safestdthiscallmessage")
            skipuoaistdthiscallmessage = [Imports].RegisterWindowMessage("skipuoaistdthiscallmessage")
            clientunlockmessage = [Imports].RegisterWindowMessage("clientunlockmessage")
            geteventportmessage = [Imports].RegisterWindowMessage("geteventportmessage")
            patchencryptionmessage = [Imports].RegisterWindowMessage("patchencryptionmessage")
        End Sub
    End Class

    'To perform synchronized calls, a stack datastructure is set up in injected dll (remotely)
    'and all parameters are pushed onto that through the message based IPC.
    'This RStack class hides the details of that.
    Public Class RStack
        Private m_Stack As UInteger
        Private m_Client As Client

        Public Sub New(ByVal onclient As Client)
            m_Client = onclient
            m_Stack = onclient.InjectedDll.IPCSend(IPCConstants.newstackmessage, 0, 0)
        End Sub

        Public Sub Push(ByVal topush As UInteger)
            m_Client.InjectedDll.IPCSend(IPCConstants.pushmessage, m_Stack, topush)
        End Sub
        Public ReadOnly Property offset() As UInteger
            Get
                Return m_Stack
            End Get
        End Property
    End Class

    'Wraps up functionality of UOClientDll
    Friend Class UOClientDll
        'private members
        Private m_ClientProcess As ProcessStream
        Private m_tid As UInt32
        Private m_ipchwnd As UInt32
        Private m_ClientLockEvent As EventWaitHandle
        Private Shared pUOClientDll As IntPtr
        Private m_IPCSocket As Socket

        Private Shared Function RawDataToObject(ByRef rawData As Byte(), ByVal overlayType As Type) As Object
            Dim result As Object = Nothing

            Dim pinnedRawData As GCHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned)
            Try
                ' Get the address of the data array
                Dim pinnedRawDataPtr As IntPtr = pinnedRawData.AddrOfPinnedObject()

                ' overlay the data type on top of the raw data
                result = Marshal.PtrToStructure(pinnedRawDataPtr, overlayType)
            Finally
                ' must explicitly release
                pinnedRawData.Free()
            End Try

            Return result
        End Function

        'constructors : dynamically load UOClientDll.dll and import the required functions
        Friend Sub New(ByVal clientprocess As UOAI.ProcessStream, ByVal tid As UInt32)
            Dim IPCPort As UInt32
            Dim ipcwndname As String

            m_ClientProcess = clientprocess
            m_tid = tid

            ipcwndname = "IPCWIN_" & clientprocess.PID.ToString("x")

            If (InlineAssignHelper(m_ipchwnd, [Imports].FindWindowEx([Imports].HWND_MESSAGE, 0, ipcwndname, ipcwndname))) = 0 Then
                '- inject UOClientDll
                InjectSelf(tid)
                'UOClientDll.dll will inject itself onto the main window thread (with id = tid) of the client here
                '- wait for the IPC window to be created by the injected dll
                While (InlineAssignHelper(m_ipchwnd, [Imports].FindWindowEx([Imports].HWND_MESSAGE, 0, ipcwndname, ipcwndname))) = 0
                    Thread.Sleep(100)
                End While
            End If

            'make sure packet based IPC is available
            IPCPort = [Imports].SendMessage(m_ipchwnd, IPCConstants.setupeventtimermessage, 0, 0)
            m_IPCSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            m_IPCSocket.Connect(IPAddress.Loopback, CInt(IPCPort))
            If m_IPCSocket.Connected = False Then
                Throw New Exception("Could not connect to the client's IPC server!")
            End If

            'get client lock event
            m_ClientLockEvent = AutoResetEvent.OpenExisting("Global\CLIENTLOCKEVENT_" & m_ClientProcess.PID.ToString("x"))
        End Sub
        Shared Sub New()
            'dynamically load library
            pUOClientDll = [Imports].LoadLibrary(UOAI.UOClientDllPath)

            If pUOClientDll <> IntPtr.Zero Then
                'get address for injectself method
                Dim pInjectSelf As IntPtr = [Imports].GetProcAddress(pUOClientDll, "injectself")
                If pInjectSelf <> IntPtr.Zero Then
                    InjectSelf = DirectCast(Marshal.GetDelegateForFunctionPointer(pInjectSelf, GetType(InjectSelfDelegate)), InjectSelfDelegate)
                    Dim pMultiClientPatch As IntPtr = [Imports].GetProcAddress(pUOClientDll, "MultiClientPatch")
                    If pMultiClientPatch <> IntPtr.Zero Then
                        MultiClientPatch = DirectCast(Marshal.GetDelegateForFunctionPointer(pMultiClientPatch, GetType(MultiClientPatchDelegate)), MultiClientPatchDelegate)
                        Dim pGetClientVersion As IntPtr = [Imports].GetProcAddress(pUOClientDll, "GetClientVersion")
                        If pGetClientVersion <> IntPtr.Zero Then
                            GetClientVersion = DirectCast(Marshal.GetDelegateForFunctionPointer(pGetClientVersion, GetType(GetClientVersionDelegate)), GetClientVersionDelegate)
                        Else
                            Throw New Exception("Could not get the address of the GetClientVersion procedure in UOClientDll.dll!")
                        End If
                    Else
                        Throw New Exception("Could not get the address of the MultiClientPatch procedure in UOClientDll.dll!")
                    End If
                Else
                    Throw New Exception("Could not get the address of the injectself procedure in UOClientDll.dll!")
                End If
            Else
                Throw New Exception("Could not find UOClientDll.dll!")
            End If
        End Sub

        'public members : Injection and MultiClientPatching
        Public Delegate Sub InjectSelfDelegate(ByVal threadid As UInt32)
        Public Delegate Sub MultiClientPatchDelegate(ByVal processid As UInt32)
        Public Delegate Function GetClientVersionDelegate(ByVal pid As UInt32) As IntPtr
        Public Shared InjectSelf As InjectSelfDelegate
        Public Shared MultiClientPatch As MultiClientPatchDelegate
        Public Shared GetClientVersion As GetClientVersionDelegate

        'These functions Send windows messages to the injected UOClientDll to peform synchronized actions
        Public Function IPCSend(ByVal message As UInt32, ByVal wParam As UInt32, ByVal lParam As UInt32) As UInt32
            Dim bh As New BufferHandler(4 * 4, False)
            bh.writeuint(1)
            bh.writeuint(message)
            bh.writeuint(wParam)
            bh.writeuint(lParam)
            m_IPCSocket.Send(bh.buffer)
            m_IPCSocket.Receive(bh.buffer)
            bh.Position = 0
            Return bh.readuint()
        End Function
        Public Function IPCPost(ByVal message As UInt32, ByVal wParam As UInt32, ByVal lParam As UInt32) As Boolean
            Dim bh As New BufferHandler(4 * 4, False)
            bh.writeuint(0)
            bh.writeuint(message)
            bh.writeuint(wParam)
            bh.writeuint(lParam)
            m_IPCSocket.Send(bh.buffer)
            Return True
        End Function

        ' - allocation/deallocation of memory on the client process
        Public Function allocate(ByVal size As UInt32) As UInt32
            Return IPCSend(IPCConstants.allocmessage, size, 0)
        End Function
        Public Sub free(ByVal tofree As UInt32)
            IPCPost(IPCConstants.freemessage, tofree, 0)
        End Sub
        Public Sub HookDestructor(ByVal objectaddress As UInt32)
            IPCSend(IPCConstants.hookitemdestructormessage, objectaddress, 0)
        End Sub
        ' - different calling sequences, parameters are passed on a RStack type datastructure
        Public Function stdcall(ByVal functionaddress As UInt32, ByVal parameters As UInt32()) As UInt32
            Dim remotestack As UInt32 = 0
            Dim toreturn As UInt32 = 0

            If (InlineAssignHelper(remotestack, IPCSend(IPCConstants.newstackmessage, 0, 0))) <> 0 Then
                'build stack
                For i As Integer = (parameters.Count() - 1) To 0 Step -1
                    IPCSend(IPCConstants.pushmessage, remotestack, parameters(i))
                Next

                'make call
                toreturn = IPCSend(IPCConstants.stdcallmessage, functionaddress, remotestack)
            End If

            Return toreturn
        End Function
        Public Function ccall(ByVal functionaddress As UInt32, ByVal parameters As UInt32()) As UInt32
            Dim remotestack As UInt32 = 0
            Dim toreturn As UInt32 = 0

            If (InlineAssignHelper(remotestack, IPCSend(IPCConstants.newstackmessage, 0, 0))) <> 0 Then
                'build stack
                For i As Integer = (parameters.Count() - 1) To 0 Step -1
                    IPCSend(IPCConstants.pushmessage, remotestack, parameters(i))
                Next

                'make call
                toreturn = IPCSend(IPCConstants.ccallmessage, functionaddress, remotestack)
            End If

            Return toreturn
        End Function
        Public Function stdthiscall(ByVal functionaddress As UInt32, ByVal thispar As UInt32, ByVal parameters As UInt32()) As UInt32
            Dim remotestack As UInt32 = 0
            Dim toreturn As UInt32 = 0

            If (InlineAssignHelper(remotestack, IPCSend(IPCConstants.newstackmessage, 0, 0))) <> 0 Then
                'build stack
                For i As Integer = (parameters.Count() - 1) To 0 Step -1
                    IPCSend(IPCConstants.pushmessage, remotestack, parameters(i))
                Next

                'push thispar
                IPCSend(IPCConstants.pushmessage, remotestack, thispar)

                'make call
                toreturn = IPCSend(IPCConstants.stdthiscallmessage, functionaddress, remotestack)
            End If

            Return toreturn
        End Function
        Public Function thiscall(ByVal functionaddress As UInt32, ByVal thispar As UInt32, ByVal parameters As UInt32()) As UInt32
            Dim remotestack As UInt32 = 0
            Dim toreturn As UInt32 = 0

            If (InlineAssignHelper(remotestack, IPCSend(IPCConstants.newstackmessage, 0, 0))) <> 0 Then
                'build stack
                For i As Integer = (parameters.Count() - 1) To 0 Step -1
                    IPCSend(IPCConstants.pushmessage, remotestack, parameters(i))
                Next

                'push thispar
                IPCSend(IPCConstants.pushmessage, remotestack, thispar)

                'make call
                toreturn = IPCSend(IPCConstants.thiscallmessage, functionaddress, remotestack)
            End If

            Return toreturn
        End Function
        Public Function skipuoai_stdthiscall(ByVal functionaddress As UInt32, ByVal thispar As UInt32, ByVal parameters As UInt32()) As UInt32
            Dim remotestack As UInt32 = 0
            Dim toreturn As UInt32 = 0

            If (InlineAssignHelper(remotestack, IPCSend(IPCConstants.newstackmessage, 0, 0))) <> 0 Then
                'build stack
                For i As Integer = (parameters.Count() - 1) To 0 Step -1
                    IPCSend(IPCConstants.pushmessage, remotestack, parameters(i))
                Next

                'push thispar
                IPCSend(IPCConstants.pushmessage, remotestack, thispar)

                'make call
                toreturn = IPCSend(IPCConstants.skipuoaistdthiscallmessage, functionaddress, remotestack)
            End If

            Return toreturn
        End Function
        'Lock opens up an embedded message loop, which means the client's main message loop gets stuck,
        'but the injecteddll takes over the message loop. This means socket functions are no longer called,
        'but GUI messages are still handled correctly. The state of the client is therefore locked, though the
        'client continues to work.
        Public Function Lock() As Boolean
            If m_ClientProcess.IsRunning Then
                IPCPost(IPCConstants.clientlockmessage, 0, 0)
                'effectively locks the whole client process except for the injected dll
                Return m_ClientLockEvent.WaitOne()
            End If
            Return False
        End Function

        Public Sub Unlock()
            IPCPost(IPCConstants.clientunlockmessage, 0, 0)
        End Sub
        'Get offset of CallibrationInfo from the injected dll, copy the data and parse it into its structure.
        Public Function GetCallibrations() As CallibrationInfo
            Dim coffset As UInt32
            Dim cbytes As Byte()

            coffset = IPCSend(IPCConstants.getcallibrationinfomessage, 0, 0)
            If coffset <> 0 Then
                cbytes = m_ClientProcess.Read(coffset, 640)
                If cbytes IsNot Nothing Then
                    Return DirectCast(UOClientDll.RawDataToObject(cbytes, GetType(CallibrationInfo)), CallibrationInfo)
                End If
            End If

            Return New CallibrationInfo()
        End Function

        'Get the Event Server Port (a random free port is used to provide multi-client support)
        Public Function GetEventPort() As Integer
            Return CInt(IPCSend(IPCConstants.geteventportmessage, 0, 0))
        End Function
        'Encryption patching is done remotely by sending an IPC message, as it was easiest to implement this in UOClientDll
        Public Sub PatchEncryption()
            IPCSend(IPCConstants.patchencryptionmessage, 0, 0)
        End Sub
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
            target = value
            Return value
        End Function
    End Class

End Class
