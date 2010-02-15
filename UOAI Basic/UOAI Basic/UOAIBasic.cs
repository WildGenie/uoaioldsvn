using System;
using System.Collections.Generic;
using Win32API;
using ProcessInjection;
using RemoteObjects;
using System.Reflection;
using System.Threading;

using Assembler;
using libdisasm;

namespace UOAIBasic
{
    public class ClientList:IEnumerable<Client>
    {
        private static ClientList m_DefaultInstance=new ClientList();
        
        public static ClientList Default
        {
            get { return m_DefaultInstance; }
        }
        
        public Client this[int idx]
        {
            get
            {
                return Clients[idx];
            }
        }
        
        public int Count { get { return Clients.Count; } }

        private Client BuildClient(ProcessHandler onprocess, ThreadHandler onthread)
        {
            Client curclient;

            //- inject our assembly into the client process (where an instance of the Client class is created!
            Injection.Inject(onprocess, onthread, Assembly.GetExecutingAssembly(), typeof(InjectedProcess));
                        
            //- create a proxy to the remote client object
            curclient = (Client)Server.GetObject(typeof(Client), (int)onprocess.PID);

            //- wait for server to become available
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while((!IsValid(curclient)&&(sw.ElapsedMilliseconds<1000)))
                Thread.Sleep(0);
            sw.Stop();

            if (IsValid(curclient))
                return curclient;
            else
                return null;
        }

        public static bool IsValid(Client tocheck)
        {
            if (tocheck == null)
                return false;
            else
            {
                try
                {
                    tocheck.Validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public List<Client> Clients
        {
            get
            {
                Client curclient;
                List<Client> Toreturn = new List<Client>();
                List<WindowHandler> UOWINs = WindowHandler.FindWindow("Ultima Online", -1, -1);
                foreach (WindowHandler UOWIN in UOWINs)
                {
                    curclient = (Client)Server.GetObject(typeof(Client), (int)UOWIN.onProcess.PID);
                    if(!IsValid(curclient))//new client
                        curclient = BuildClient(UOWIN.onProcess, UOWIN.onThread);
                    if(IsValid(curclient))
                        Toreturn.Add(curclient);
                }
                //- return
                return Toreturn;
            }
        }

        #region IEnumerable<Client> Members

        public IEnumerator<Client> GetEnumerator()
        {
            return Clients.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Clients.GetEnumerator();
        }

        #endregion
    }

    //don't use this class, it is supposed to be created only on the client's address space
    public class InjectedProcess
    {
        public InjectedProcess()
        {
            //expose all MarshalByRefObjects through Remoting
            Server.RegisterTypes();
        }
    }

    //We need an extra (remoteable) class as an event proxy
    //the problem is that the events need to be passed across process-boundaries
    //attaching to an event accross process boundaries only works if the class
    //containing the handler function(s) is known to both processes.
    //An intermediate eventhandler class (initiated at the handler site) solves this.
    public class ClientEvents : MarshalByRefObject
    {
        private Client m_Client;
        private System.ComponentModel.ISynchronizeInvoke m_Invoker = null;

        public System.ComponentModel.ISynchronizeInvoke SynchronizationObject
        {
            get { return m_Invoker; }
            set { m_Invoker = value; }
        }

        public delegate bool OnPacketReceiveDelegate(UnmanagedBuffer packet);
        public event OnPacketReceiveDelegate OnPacketReceive;
        public bool InvokeOnPacketReceive(UnmanagedBuffer packet)
        {
            if (OnPacketReceive != null)
            {
                if (m_Invoker != null)
                {
                    bool toreturn = true;
                    foreach (Delegate d in OnPacketReceive.GetInvocationList())
                    {
                        if (!(bool)m_Invoker.Invoke(d, new object[] { packet }))
                        {
                            toreturn = false;
                            break;
                        }
                    }
                    return toreturn;
                }
                else
                    return OnPacketReceive(packet);
            }
            return true;
        }

        public delegate void OnPacketHandledDelegate();
        public event OnPacketHandledDelegate OnPacketHandled;
        public void InvokeOnPacketHandled()
        {
            if (OnPacketHandled != null)
            {
                if (m_Invoker != null)
                {
                    foreach (Delegate d in OnPacketHandled.GetInvocationList())
                        m_Invoker.Invoke(d, new object[] { });
                }
                else
                    OnPacketHandled();
            }
            return;
        }

        public delegate bool OnPacketSendDelegate(UnmanagedBuffer packet);
        public event OnPacketSendDelegate OnPacketSend;
        public bool InvokeOnPacketSend(UnmanagedBuffer packet)
        {
            if (OnPacketSend != null)
            {
                if (m_Invoker != null)
                {
                    bool result = true;
                    foreach (Delegate d in OnPacketSend.GetInvocationList())
                    {
                        if (!(bool)m_Invoker.Invoke(d, new object[] { packet }))
                        {
                            result = false;
                            break;
                        }
                    }
                    return result;
                }
                else
                    return OnPacketSend(packet);
            }
            return true;
        }

        public delegate void SimpleDelegate();
        
        public event SimpleDelegate OnTick;
        public void InvokeOnTick()
        {
            if (OnTick != null)
            {
                if (m_Invoker != null)
                {
                    foreach (Delegate d in OnTick.GetInvocationList())
                        m_Invoker.BeginInvoke(d, new object[] { });
                }
                else
                    OnTick();
            }
        }

        public event SimpleDelegate OnQuit;
        public void InvokeOnQuit()
        {
            if (OnQuit != null)
            {
                if (m_Invoker != null)
                {
                    foreach (Delegate d in OnQuit.GetInvocationList())
                        m_Invoker.Invoke(d, new object[] { });
                }
                else
                    OnQuit();
            }
        }

        public void Release()
        {
            m_Client.RemoveEventSink(this);
            m_Client = null;
        }

        public ClientEvents(Client onclient)
        {
            m_Client = onclient;
            m_Client.AddEventSink(this);
        }

        ~ClientEvents()
        {
            try
            {
                if (ClientList.IsValid(m_Client))
                    m_Client.RemoveEventSink(this);
            }
            catch
            {
            }
        }
    }

    //this object is created on the client process, then marshalled back through remoting
    [RemoteObject()]//the remote object attribute on a marshalbyref class will cause the server to share this object through remoting as a singleton, you will need at least one singleton to use remoting
    public class Client : MarshalByRefObject
    {
        private static Client m_Client;
        private Exception m_CallibrationException;
        private List<LocalHook> m_Hooks = new List<LocalHook>();
        private UnmanagedBuffer m_PacketInfo;
        private List<ClientEvents> m_EventSinks=new List<ClientEvents>();
        private Imports.HookProc m_HookProc;
        private bool m_ShuttingDown = false;

        public void UnHookTest()
        {
            m_Hooks[1].UnHook();
        }

        public Client()
        {
            m_Client = this;//keep us alive

            //Imports.AllocConsole();

            //callibrations:
            try
            {
                UOCallibration.Callibrate(ProcessHandler.CurrentProcess);
            }
            catch (Exception e)
            {
                m_CallibrationException = e;
            }

            m_PacketInfo = new UnmanagedBuffer((IntPtr)UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.pPacketInfo]);

            m_Hooks.Add(LocalHook.Hook((uint)UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.pSendPacket], 4, false));
            m_Hooks.Add(LocalHook.Hook((uint)UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.NetworkObjectVtbl] + 4 * 8, 4, true));
            m_Hooks.Add(LocalHook.Hook((uint)UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.GeneralPurposeHookAddress],0,false));

            m_Hooks[0].onCall+=new LocalHook.OnCallDelegate(Client_onSend);
            m_Hooks[1].onCall += new LocalHook.OnCallDelegate(Client_onReceive);
            m_Hooks[1].afterCall+=new LocalHook.OnCallDelegate(Client_packetHandled);
            m_Hooks[2].onCall+=new LocalHook.OnCallDelegate(Client_onTick);
        }

        ~Client()
        {
        }

        public void Quit()
        {
            
            List<WindowHandler> wins = WindowHandler.FindWindow("Ultima Online", (int)ProcessHandler.CurrentProcess.PID, -1);
            if (wins.Count == 1)
                wins[0].PostMessage((uint)WindowHandler.Messages.WM_QUIT, 0, 0);

            //ProcessHandler.CurrentProcess.MainWindow.PostMessage((uint)WindowHandler.Messages.WM_QUIT, 0, 0);//<-simpler version?
        }

        private bool Client_onTick(UnmanagedContext ctx, UnmanagedStack stck)
        {
            if (m_HookProc == null)
            {
                //install message hook if not installed yet (must be done here as we need the correct threadid, and this tick event gets handled on the client's main thread)
                m_HookProc = new Imports.HookProc(MessageHook);
                Imports.SetWindowsHookEx(Imports.HookType.WH_GETMESSAGE, System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(m_HookProc), IntPtr.Zero, Imports.GetCurrentThreadId());
            }

            if (!m_ShuttingDown)
            {
                //invoke on Tick
                foreach (ClientEvents ce in m_EventSinks)
                    ce.InvokeOnTick();

                //sync's go here
            }
         
            return true;
        }

        private int MessageHook(int code, IntPtr wParam, IntPtr lParam)
        {
            Imports.MSG msg = (Imports.MSG)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(Imports.MSG));
            if (msg.message == (uint)WindowHandler.Messages.WM_CLOSE)
            {
                m_ShuttingDown = true;
                m_EventSinks.Clear();
                foreach (LocalHook lh in m_Hooks)
                    lh.UnHook();
                for (uint i = 0; i < 50000; i++)
                    ;
            }
            return Imports.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        public void AddEventSink(ClientEvents toadd)
        {
            m_EventSinks.Add(toadd);
        }

        public void RemoveEventSink(ClientEvents toremove)
        {
            m_EventSinks.Remove(toremove);
        }

        private bool GetPacketSize(byte packetnumber, out ushort size)
        {
            size=m_PacketInfo.ReadAt<ushort>(packetnumber * 3 * 4);
            if ((size == 0x8000) || (size == 0))
                return false;
            else
                return true;
        }

        private bool Client_packetHandled(UnmanagedContext ctx, UnmanagedStack stck)
        {
            if (!m_ShuttingDown)
            {
                foreach (ClientEvents ce in m_EventSinks)
                    ce.InvokeOnPacketHandled();
            }
            return true;
        }
        private bool Client_onSend(UnmanagedContext ctx, UnmanagedStack stck)
        {
            ushort size;
            UnmanagedBuffer packet = new UnmanagedBuffer((IntPtr)stck[0]);
            if (!GetPacketSize(packet.ReadAt<byte>(0), out size))
                packet.SwapBreakPoint = 3;
            packet.SwapByteOrder = true;

            bool toreturn = true;
            if (!m_ShuttingDown)
            {
                foreach (ClientEvents ce in m_EventSinks)
                {
                    if (!ce.InvokeOnPacketSend(packet))
                    {
                        toreturn = false;
                        break;
                    }
                }
            }
            return toreturn;
        }
        private bool Client_onReceive(UnmanagedContext ctx, UnmanagedStack stck)
        {
            ushort size;
            UnmanagedBuffer packet = new UnmanagedBuffer((IntPtr)stck[0]);
            
            if (!GetPacketSize(packet.ReadAt<byte>(0), out size))
                packet.SwapBreakPoint = 3;
            packet.SwapByteOrder = true;

            bool toreturn = true;
            if (!m_ShuttingDown)
            {
                foreach (ClientEvents ce in m_EventSinks)
                {
                    if (!ce.InvokeOnPacketReceive(packet))
                    {
                        toreturn = false;
                        break;
                    }
                }
            }

            return toreturn;
        }

        public bool HasFeatures(UOCallibration.CallibratedFeatures[] tocheck)
        {
            foreach (UOCallibration.CallibratedFeatures feature in tocheck)
            {
                if (!HasFeature(feature))
                    return false;
            }
            return true;
        }
        public bool HasFeature(UOCallibration.CallibratedFeatures tocheck)
        {
            return UOCallibration.Callibrations.ContainsKey((uint)tocheck);
        }
        public long this[UOCallibration.CallibratedFeatures feature]
        {
            get
            {
                if (HasFeature(feature))
                    return UOCallibration.Callibrations[(uint)feature];
                else
                    return 0;
            }
            set
            {
                if (HasFeature(feature))
                    UOCallibration.Callibrations.Remove((uint)feature);
                UOCallibration.Callibrations.Add((uint)feature, value);
            }
        }

        public bool Validate()//dummy, trying to call this when the client is unavailable will throw a remoting error, we catch this error to detect an invalid client
        {
            return true;
        }

        public void PatchEncryption()
        {
            ProcessHandler us = ProcessHandler.CurrentProcess;
            int curtarget;
            JmpRelative jrel;
            asmInstruction curins;

            us.Position = UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.sendcryptpatchpos];

            curins = disassembler.disassemble(us);
            curtarget = (int)curins.ReadAddressOperand();

            jrel = new JmpRelative(curtarget);
            jrel.Write(us, (int)curins.Address);

            us.Position = UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.recvcryptpatchpos];

            curins = disassembler.disassemble(us);
            curtarget = (int)curins.ReadAddressOperand();

            jrel = new JmpRelative(curtarget);
            jrel.Write(us, (int)curins.Address);

            us.Position = (int)UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.SendCryptPatchPos2];

            curins = disassembler.disassemble(us);
            curtarget = (int)curins.ReadAddressOperand();

            jrel = new JmpRelative(curtarget);
            jrel.Write(us, (int)curins.Address);
        }
    }
}
