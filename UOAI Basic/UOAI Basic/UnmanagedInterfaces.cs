using System;
using System.Runtime.InteropServices;
using ProcessInjection;
using UOAIBasic.Internal;

namespace UOAIBasic
{
    [StructLayout(LayoutKind.Sequential), Serializable()]
    public struct MacroEntry
    {
        public uint macronumber;
        public uint integerparameter;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string stringparameter;
        public MacroEntry(uint _macronumber, uint _uintparameter, string _stringparameter)
        {
            macronumber = _macronumber;
            integerparameter = _uintparameter;
            stringparameter = _stringparameter;
        }
    }

    [StructLayout(LayoutKind.Sequential), Serializable()]
    public struct MacroTable
    {
        [MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]
        public uint[] Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public MacroEntry[] macros;
        public MacroTable(MacroEntry[] Entries)
        {
            Header = new uint[] { 0, 0, 0, 0, 0, 0 };
            if (Entries.Length > 10)
                throw new Exception("Maximum 10 Macro Table Entries Allowed");
            macros = new MacroEntry[10];
            uint i=0;
            foreach (MacroEntry me in Entries)
            {
                macros[i] = me;
                i++;
            }
        }
    }

    public delegate bool OnKeyUpDelegate(uint VirtualKeyCode);
    public delegate bool OnKeyDownDelegate(uint VirtualKeyCode, bool repeated);

    public delegate bool SimpleDelegate();

    public delegate bool OnWindowsMessageDelegate(ref Win32API.Imports.MSG msg);

    public interface IClient
    {      
        //Unmanaged Methods and Globals

        [UMethod(UOCallibration.CallibratedFeatures.MacroFunction), Sync()]
        void Macro([MarshalAs(UnmanagedType.LPStruct)] MacroTable macro, uint index);

        [UMethod(UOCallibration.CallibratedFeatures.pTextOut), Sync()]
        void SysMessage(uint color, uint font, string message);

        INetworkObject NetworkObject {
            [UGlobal(UOCallibration.CallibratedFeatures.NetworkObject, UnmanagedType.Interface, typeof(INetworkObject))]
            get;
        }

        //Forwards
        [UForward(typeof(Client), "SetAsync")]
        void SetAsync(string typename, string methodname, bool value);

        [UForward(typeof(Client), "SetInvokationTarget")]
        void SetInvokationTarget(InvokationTarget target, bool bInvokeAsynchronously);

        [UForward(typeof(Client), "Add_OnKeyUp", "Remove_OnKeyUp")]
        event OnKeyUpDelegate OnKeyUp;

        [UForward(typeof(Client), "Add_OnKeyDown", "Remove_OnKeyDown")]
        event OnKeyDownDelegate OnKeyDown;

        [UForward(typeof(Client), "Add_OnQuit", "Remove_OnQuit")]
        event SimpleDelegate OnQuit;

        [UForward(typeof(Client), "Add_OnWindowsMessage", "Remove_OnWindowsMessage")]
        event OnWindowsMessageDelegate OnWindowsMessage;

        [UForward(typeof(Client), "PatchEncryption")]
        void PatchEncryption();

        [UForward(typeof(Client), "Quit")]
        void Quit();

        [UForward(typeof(Client), "AllocateBuffer")]
        UnmanagedBuffer AllocateBuffer(uint size);
    }

    public delegate bool OnPacketDelegate([UPar(UnmanagedType.Interface,typeof(UnmanagedBuffer))] UnmanagedBuffer packet);

    public interface INetworkObject
    {
        [UMethod(UOCallibration.CallibratedFeatures.pSendPacket), Sync()]
        void SendPacket(UnmanagedBuffer buffer);

        [UVtblMethod(8), Sync()]
        void HandlePacket(UnmanagedBuffer buffer);

        [UHook((uint)8,4,true,true)]
        event OnPacketDelegate onPacketRecieve;

        [UHook((uint)8, 4, true, false)]
        event SimpleDelegate OnPacketHandled;

        [UHook(UOCallibration.CallibratedFeatures.pSendPacket, 0, true)]
        event OnPacketDelegate onPacketSend;

        //to be addded: connection loss/close connection here
        //info on connection : server address, port, etc.
    }
}
