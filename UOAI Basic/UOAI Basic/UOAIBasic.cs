using System;
using System.Collections.Generic;
using Win32API;
using ProcessInjection;
using RemoteObjects;
using System.Reflection;
using System.Threading;

using Assembler;
using libdisasm;

using Microsoft.Win32;//registry stuff

namespace UOAIBasic
{
    public static class UOAI
    {
        private static string m_ClientPath;
        private static string m_ClientExe;

        public static Internal.ClientList Clients { get { return Internal.ClientList.Default; } }

        public static IClient LaunchClient()
        {
            string workingpath = System.IO.Path.GetFullPath(m_ClientPath);
            string clientpath = System.IO.Path.Combine(workingpath, m_ClientExe);

            if (System.IO.File.Exists(clientpath))
            {
                //launch client suspended
                Imports.PROCESS_INFORMATION pi = new Imports.PROCESS_INFORMATION();
                Imports.STARTUPINFO si = new Imports.STARTUPINFO();
                Imports.SECURITY_ATTRIBUTES psa = new Imports.SECURITY_ATTRIBUTES();
                Imports.SECURITY_ATTRIBUTES tsa = new Imports.SECURITY_ATTRIBUTES();
                psa.nLength = System.Runtime.InteropServices.Marshal.SizeOf(psa);
                tsa.nLength = System.Runtime.InteropServices.Marshal.SizeOf(tsa);

                if (Imports.CreateProcess(clientpath, "", ref psa, ref tsa, false, Imports.CreationFlags.CREATE_SUSPENDED | Imports.CreationFlags.CREATE_NEW_CONSOLE, IntPtr.Zero, workingpath, ref si, out pi))
                {
                    Imports.ResumeThread(pi.hThread);
                    return Clients.byPID(pi.dwProcessId);
                }
            }

            return null;
        }

        static UOAI()
        {
            try
            {
                RegistryKey hklm = Registry.LocalMachine;
                RegistryKey originkey = hklm.OpenSubKey("SOFTWARE\\Origin Worlds Online\\Ultima Online\\1.0");
                if (originkey == null)
                    originkey = hklm.OpenSubKey("SOFTWARE\\Origin Worlds Online\\Ultima Online Third Dawn\\1.0");

                if (originkey != null)
                {
                    string instcdpath = (string)originkey.GetValue("InstCDPath");
                    if (instcdpath != null)
                    {
                        m_ClientPath = instcdpath + "\\";
                        m_ClientExe = "client.exe";
                        originkey.Close();
                        return;
                    }
                    originkey.Close();
                }
            }
            catch
            {
            }

            //use default values
            m_ClientPath = "C:\\Program Files\\EA Games\\Ultima Online Mondain's Legacy";
            m_ClientExe = "client.exe";

            return;
        }
    }

    //wrapper for cross AppDomain (remoted) ISynchronizeInvoke calls
    //<- if the actual IAsyncResult object is serializable then this might be unnecessary
    //<- but since we could end up using custom ISynchronizeInvoke implementations
    //<- so there is no garantue that IAsyncResult will be serializable
    public class InvokationResult : MarshalByRefObject, IAsyncResult
    {
        private IAsyncResult m_Result;

        public InvokationResult(IAsyncResult fromresult)
        {
            m_Result = fromresult;
        }

        public IAsyncResult Result { get { return m_Result; } }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return m_Result.AsyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return m_Result.AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return m_Result.CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return m_Result.IsCompleted; }
        }

        #endregion
    }

    //wrapper for cross AppDomain (remoted) ISynchronizeInvoke calls
    public class InvokationTarget : MarshalByRefObject, System.ComponentModel.ISynchronizeInvoke
    {
        private System.ComponentModel.ISynchronizeInvoke m_Target;

        public InvokationTarget(System.ComponentModel.ISynchronizeInvoke target)
        {
            m_Target = target;
        }

        #region ISynchronizeInvoke Members

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            return new InvokationResult(m_Target.BeginInvoke(method, args));
        }

        public object EndInvoke(IAsyncResult result)
        {
            try
            {
                return m_Target.EndInvoke(result);
            }
            catch
            {
                return null;
            }
        }

        public object Invoke(Delegate method, object[] args)
        {
            try
            {
                return m_Target.Invoke(method, args);
            }
            catch
            {
                return null;
            }
        }

        public bool InvokeRequired
        {
            get { return m_Target.InvokeRequired; }
        }

        #endregion
    }
}

namespace UOAIBasic.Internal
{
    public class ClientList:IEnumerable<IClient>
    {
        private static ClientList m_DefaultInstance=new ClientList();
        
        public static ClientList Default
        {
            get { return m_DefaultInstance; }
        }
        
        public IClient this[int idx]
        {
            get
            {
                return Clients[idx];
            }
        }
        
        public int Count { get { return Clients.Count; } }

        private IClient BuildClient(ProcessHandler onprocess, ThreadHandler onthread)
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
                return curclient.ActualClient;
            else
                return null;
        }

        public IClient byPID(int pid)
        {
            Client curclient = (Client)Server.GetObject(typeof(Client), pid);
            if (IsValid(curclient))
                return curclient.ActualClient;
            else
                return null;
        }

        private static bool IsValid(Client tocheck)
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

        public List<IClient> Clients
        {
            get
            {
                Client curclient;
                IClient curiclient;
                List<IClient> Toreturn = new List<IClient>();
                List<WindowHandler> UOWINs = WindowHandler.FindWindow("Ultima Online", -1, -1);
                foreach (WindowHandler UOWIN in UOWINs)
                {
                    curclient = (Client)Server.GetObject(typeof(Client), (int)UOWIN.onProcess.PID);
                    if (!IsValid(curclient))//new client
                        curiclient = BuildClient(UOWIN.onProcess, UOWIN.onThread);
                    else
                        curiclient = curclient.ActualClient;
                    if(curiclient!=null)
                        Toreturn.Add(curiclient);
                }
                //- return
                return Toreturn;
            }
        }

        #region IEnumerable<IClient> Members

        public IEnumerator<IClient> GetEnumerator()
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
    //This serves as an entrypoint for the injected code.
    //On injection an instance of this class is created.
    //All it will do is register the Client class (or all RemoteObject() classes)
    //with the remoting server, making it accessible through Server.GetObject from the user app.
    public class InjectedProcess
    {
        public InjectedProcess()
        {
            //expose all MarshalByRefObjects through Remoting
            Server.RegisterTypes();
        }
    }

    //class used for the client's internal SyncInvoke implementation
    //... maybe it should inherit from MarshalByRefObject...
    //... cause right now ISynchronizeInvoke on the Client class will work only
    //... if used form the injected code, simply because this DelegateRequest can't be remoted
    public class DelegateRequest : IAsyncResult
    {
        private object[] m_Parameters;
        private Delegate m_Delegate;
        private object m_Response;
        private ManualResetEvent m_Event=new ManualResetEvent(false);
        private bool m_CompletedSynchronously = false;
        private int m_CreationThread;

        public object Parameters { get { return m_Parameters; } }
        public Delegate fromDelegate { get { return m_Delegate; } }
        public DelegateRequest(Delegate _fromdelegate, object[] _parameters)
        {
            m_Parameters = _parameters;
            m_Delegate = _fromdelegate;
            m_CreationThread = Thread.CurrentThread.ManagedThreadId;
        }

        public void DoInvoke()
        {
            m_Response = m_Delegate.DynamicInvoke(m_Parameters);
            Complete();
        }

        public void Complete()
        {
            if (m_CreationThread == Thread.CurrentThread.ManagedThreadId)
                m_CompletedSynchronously = true;
            m_Event.Set();
        }

        public object Response { get { return m_Response; } set { m_Response = value; } }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return Response; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return m_Event; }
        }

        public bool CompletedSynchronously
        {
            get { return m_CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return m_Event.WaitOne(0); }
        }

        #endregion
    }

    //this object is created on the client process, then marshalled back through remoting
    //the remote object attribute on a marshalbyref class will cause the server to share this object through remoting as a singleton, you will need at least one singleton to use remoting
    //from the user app we get hold of a single instance of this object
    //through Server.GetObject specifying the client's pid and type info for the client class
    //the first call to Server.GetObject will cause an instance of this Client class to be created on the injected code
    //subsequent calls will just return that same instance. (since this is remoted as a singleton)
    [RemoteObject()]
    public class Client : MarshalByRefObject, System.ComponentModel.ISynchronizeInvoke
    {
        private static Client m_Client;
        private Exception m_CallibrationException;
        private List<LocalHook> m_Hooks = new List<LocalHook>();
        private UnmanagedBuffer m_PacketInfo;
        private Imports.HookProc m_HookProc;
        private bool m_ShuttingDown = false;
        private uint drop_msg_message = 0;
        private IClient m_ActualClient;
        private Queue<DelegateRequest> DelegateQueue = new Queue<DelegateRequest>();
        private int m_ClientThread;

        private static bool m_AsyncInvokation = false;
        private static InvokationTarget m_InvokationTarget = null;

        public static bool AsyncInvokation { get { return m_AsyncInvokation; } }
        public static InvokationTarget InvokationTarget { get { return m_InvokationTarget; } }

        //dynamic event implementation
        //events don't play nicely across remoting boundaries
        //so instead we use a custom implementation here...
        //there is an event on the IClient interface (or other interface)... but without implementation
        //the AddHandler/RemoveHandler calls are intercepted on our proxy objects
        //and forwarded to fake add/remove handler functions here that add the
        //specified handlers to these lists.
        //We then just invoke anything in theses lists whenever the event occurs.
        private static List<OnKeyUpDelegate> keyup_handlers = new List<OnKeyUpDelegate>();
        private static List<OnKeyDownDelegate> keydown_handlers = new List<OnKeyDownDelegate>();
        private static List<OnWindowsMessageDelegate> windowsmessage_handlers = new List<OnWindowsMessageDelegate>();
        private static List<SimpleDelegate> quit_handlers = new List<SimpleDelegate>();

        //returned to the user app
        //the proxy that dynamically implements IClient will forward some of it's functionality to static (shared) functions on this class though
        public IClient ActualClient { get { return m_ActualClient; } }

        //required for Synchronized Invokation of unmanaged functions
        //we need to be able to find the Client instance in use
        //from the proxy that forwards IClient (or other) calls to unamanged code
        public static Client Default { get { return m_Client; } }

        public static void SetInvokationTarget(InvokationTarget target, bool bInvokeAsynchronously)
        {
            m_InvokationTarget = target;
            m_AsyncInvokation = bInvokeAsynchronously;
        }

        //only one instance is ever created; when requesting a reference (proxy)
        //through remoting from the user app
        public Client()
        {
            m_Client = this;//keep us alive

            /*
            
            //for debugging purposes, this object is on the client app (injected code)
            //the VS debugger has no access to it...
            //...so a console is our only source of debugging info (or attaching IDA to the client would work too ofc)

             Imports.AllocConsole();
            
             */

            //create IClient proxy
            //this is what we will return to the user app
            //the UnmanagedObject has all calls intercepted on a proxy
            //which allows us to forward calls to unmanaged functions (actual client functions)
            m_ActualClient = (IClient)UnmanagedObject.Create(typeof(IClient), IntPtr.Zero);

            drop_msg_message = Imports.RegisterWindowMessage("drop_msg_message");//dummy message.. changing an msg.message to this will make sure the client doesn't handle it

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

            m_Hooks.Add(LocalHook.Hook((uint)UOCallibration.Callibrations[(uint)UOCallibration.CallibratedFeatures.GeneralPurposeHookAddress],0,false));
            m_Hooks[0].onCall+=new LocalHook.OnCallDelegate(Client_onTick);
        }

        ~Client()
        {
        }

        //new UnmanagedBuffer on a user app would fail as the buffer would be
        //allocated in the unmanaged part of that app (so not on the client app).
        //Instead this function will cause a buffer to be allocated on the client.
        //(forwarded form IClient)
        public static UnmanagedBuffer AllocateBuffer(uint size)
        {
            return Allocator.DefAllocateBuffer(size);
        }

        //Causes the client to Quit (forwarded form IClient)
        public static void Quit()
        {
            
            List<WindowHandler> wins = WindowHandler.FindWindow("Ultima Online", (int)ProcessHandler.CurrentProcess.PID, -1);
            if (wins.Count == 1)
                wins[0].PostMessage((uint)WindowHandler.Messages.WM_QUIT, 0, 0);

            //ProcessHandler.CurrentProcess.MainWindow.PostMessage((uint)WindowHandler.Messages.WM_QUIT, 0, 0);//<-simpler version?
        }

        //Internal Event Handler
        //-> onTick is just a name, it's not really a timer and ticks are not necessarily well spaced
        //-> This is basically a function called from a hook of the client's "general-purpose" function
        //-> this is a function called by the client whenever it has no windows messages to handle.
        //-> from this function it does everything else it needs to do: send packets that are in the packet queue
        //-> receive/handle packets, perform pathfinding/following/.... etc.
        //-> In this hook we can do our own things at that time... completely synchronized with the client.
        //-> So onTick = synchronized stuff.
        //-> The invokation for the ISynchronizeInvoke of the client also goes here.
        private bool Client_onTick(UnmanagedContext ctx, UnmanagedStack stck)
        {
            if (m_HookProc == null)
            {
                //install message hook if not installed yet (must be done here as we need the correct threadid, and this tick event gets handled on the client's main thread)
                m_ClientThread = Thread.CurrentThread.ManagedThreadId;
                m_HookProc = new Imports.HookProc(MessageHook);
                Imports.SetWindowsHookEx(Imports.HookType.WH_GETMESSAGE, System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(m_HookProc), IntPtr.Zero, Imports.GetCurrentThreadId());
            }

            if (!m_ShuttingDown)
            {
                while (DelegateQueue.Count > 0)
                    DelegateQueue.Dequeue().DoInvoke();
            }
         
            return true;
        }

        //Hooks for Windows Messages
        //-> currently for the WM_QUIT, WM_KEYDOWN, WM_KEYUP messages
        //-> i could add a general onMessage(MSG msg) hook
        //-> but that would have the potential of slowing the client down enormously
        private int MessageHook(int code, IntPtr wParam, IntPtr lParam)
        {
            UnmanagedBuffer msgbuff=new UnmanagedBuffer(lParam);
            Imports.MSG msg = msgbuff.Read<Imports.MSG>();
            bool drop = false;

            lock (windowsmessage_handlers)
            {
                //invoke general message handlers (don't use these unless really necessary.. the WILL slow down the client)
                foreach (OnWindowsMessageDelegate del in Client.windowsmessage_handlers.ToArray())//toarray is needed to ensure we can remove handlers while enumerating (list can't change while enumerating it, so we enumerate a copy (the array) of it.
                {
                    try
                    {
                        if (m_InvokationTarget != null)
                        {
                            if (m_AsyncInvokation)
                                m_InvokationTarget.BeginInvoke(del, new object[] { msg });
                            else if (!(bool)m_InvokationTarget.Invoke(del, new object[] { msg }))
                                drop = true;
                        }
                        else if (!del.Invoke(ref msg))
                            drop = true;
                    }
                    catch
                    {
                        windowsmessage_handlers.Remove(del);
                    }
                }
                if (drop)
                    msg.message = drop_msg_message;
            }

            if (msg.message == (uint)WindowHandler.Messages.WM_QUIT)
            {
                m_ShuttingDown = true;

                lock (quit_handlers)
                {
                    //invoke on quit
                    foreach (SimpleDelegate del in Client.quit_handlers.ToArray())//toarray is needed to ensure we can remove handlers while enumerating (list can't change while enumerating it, so we enumerate a copy (the array) of it.
                    {
                        try
                        {
                            if (m_InvokationTarget != null)
                            {
                                if (m_AsyncInvokation)
                                    m_InvokationTarget.BeginInvoke(del, new object[] { });
                                else
                                    m_InvokationTarget.Invoke(del, new object[] { });
                            }
                            else
                                del.Invoke();
                        }
                        catch
                        {
                            quit_handlers.Remove(del);
                        }
                    }
                }

                foreach (LocalHook lh in m_Hooks)
                    lh.UnHook();
            }
            else if (msg.message == (uint)WindowHandler.Messages.WM_KEYDOWN)
            {
                bool dropkey = false;
                bool repeated = (msg.lParam & (2 ^ 30)) != 0;

                lock (keydown_handlers)
                {
                    //invoke onkeydown
                    foreach (OnKeyDownDelegate del in Client.keydown_handlers.ToArray())//toarray is needed to ensure we can remove handlers while enumerating (list can't change while enumerating it, so we enumerate a copy (the array) of it.
                    {
                        try
                        {
                            if (m_InvokationTarget != null)
                            {
                                if (m_AsyncInvokation)
                                    m_InvokationTarget.BeginInvoke(del, new object[] { msg.wParam, repeated });
                                else if (!(bool)m_InvokationTarget.Invoke(del, new object[] { msg.wParam, repeated }))
                                {
                                    dropkey = true;
                                    break;
                                }
                            }
                            else if (!del.Invoke(msg.wParam, repeated))
                            {
                                dropkey = true;
                                break;
                            }
                        }
                        catch
                        {
                            keydown_handlers.Remove(del);
                        }
                    }
                }

                if (dropkey)
                    msg.message = drop_msg_message;
            }
            else if (msg.message == (uint)WindowHandler.Messages.WM_KEYUP)
            {
                bool dropkey = false;

                lock (keyup_handlers)
                {
                    //invoke onkeyup
                    foreach (OnKeyUpDelegate del in Client.keyup_handlers.ToArray())//use of toarray is needed to be able to remove handlers while enumerating
                    {
                        try
                        {
                            if (m_InvokationTarget != null)
                            {
                                if (m_AsyncInvokation)
                                    m_InvokationTarget.BeginInvoke(del, new object[] { msg.wParam });
                                else if (!(bool)m_InvokationTarget.Invoke(del, new object[] { msg.wParam }))
                                {
                                    dropkey = true;
                                    break;
                                }
                            }
                            else if (!del.Invoke(msg.wParam))
                            {
                                dropkey = true;
                                break;
                            }
                        }
                        catch
                        {
                            keyup_handlers.Remove(del);
                        }
                    }
                }

                if (dropkey)
                    msg.message = drop_msg_message;
            }
            msgbuff.WriteAt<Imports.MSG>(msg, 0);
            return Imports.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        //currently unused
        //looks up the packet size in the client's internal packetinfo table... 
        //if it's fixed size true is returned and size is the size; 
        //for dynamic size packets false is returned
        private bool GetPacketSize(byte packetnumber, out ushort size)
        {
            size=m_PacketInfo.ReadAt<ushort>(packetnumber * 3 * 4);
            if ((size == 0x8000) || (size == 0))
                return false;
            else
                return true;
        }

        //Feature check, looksup the named offset in the callibration tree.
        //If all specified features are present (callibrated) it returns true.
        //Otherwise false.
        //I will add fixed arrays of features for specific functionality...
        //... like: the NetworkFeatures array will contain all named offsets required
        //... for network functions like send/receive/packet events, etc.
        //... so an app that needs network features can check HasFeatures(NetworkFeatures)
        public bool HasFeatures(UOCallibration.CallibratedFeatures[] tocheck)
        {
            foreach (UOCallibration.CallibratedFeatures feature in tocheck)
            {
                if (!HasFeature(feature))
                    return false;
            }
            return true;
        }

        //Feature check, looksup the named offset in the callibration tree.
        //If not present it returns false, otherwise true.
        public bool HasFeature(UOCallibration.CallibratedFeatures tocheck)
        {
            return UOCallibration.Callibrations.ContainsKey((uint)tocheck);
        }

        //Feature check, looksup the named offset in the callibration tree.
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

        //Dummy, trying to call this when the client is unavailable will throw a remoting error (so actually you can't call it).
        //We catch this error to detect an invalid client.
        //If you can call it then remoting is ok and it will return true.
        //So it's a dummy because it only serves it's purpose when you can't actually call it. :)
        public bool Validate()
        {
            return true;
        }

        //set the SyncAttribute.Async boolean for the specified method
        //if SyncAttribute.Async = true the method gets executed asynchronously relative to the caller, but synchronously relative to the client
        //...
        //this means if f.e. SendPacket has Async=true on its SyncAttribute then it
        //will return immediately, not waiting until the packet is actually sent.
        //Still, the sendpacket call gets posted to a queue on the client's main thread, so it's executed synchronised with the client's main thread.
        //...
        //This is important in the following situation:
        // - if you use SetInvokationTarget to ensure events get synchronized with your windows form main thread (or other main thread)
        // - and then also handle packet events
        // - and also send packets (directly through SendPacket or indirectly: Macro, SysMessage, ... all send packets)
        // -> then there would be a problem if you couldn't set all those packet-sending functions to async
        // -> cause while you are sending a packet from your main thread, the packet event can't be invoked cause your main thread is blocked
        // -> so basically without setting async on all methods that might send a packet everything would get blocked
        // -> you send a packet -> in the packet send the onPacketSend event is invoked, but never returns, since it waits until your main thread invokes it, but your main thread is blocked waiting for the packet send to return
        // -> with async set, the packet send returns immediately and just 'posts' the action to a queue; as a result the main thread block is lifted
        //
        // so in summary: SetInvokationTarget + direct/indirect packet sending + packet events = SetAsync on all methods that might send a packet!
        public static void SetAsync(string typename, string methodname, bool value)
        {
            object[] attribs;
            MethodInfo mi;

            Type type = Assembly.GetExecutingAssembly().GetType(typename);
            if (type == null)
                type = Assembly.GetExecutingAssembly().GetType("UOAIBasic." + typename);

            if (type != null)
            {
                if ((mi = type.GetMethod(methodname)) != null)
                {
                    if ((attribs = mi.GetCustomAttributes(typeof(SyncAttribute), true)).Length > 0)
                    {
                        if (UnmanagedProxy.m_Async.ContainsKey((int)mi.MethodHandle.Value))
                            UnmanagedProxy.m_Async.Remove((int)mi.MethodHandle.Value);
                        UnmanagedProxy.m_Async.Add((int)mi.MethodHandle.Value, value);
                    }
                }
            }
        }

        //encryption patch
        public static void PatchEncryption()
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

        #region Add/Remove Handler for forwarded events
        public static void Add_OnKeyUp(OnKeyUpDelegate toadd)
        {
            lock (keyup_handlers)
            {
                keyup_handlers.Add(toadd);
            }
        }

        public static void Remove_OnKeyUp(OnKeyUpDelegate toremove)
        {
            lock (keyup_handlers)
            {
                keyup_handlers.Remove(toremove);
            }
        }

        public static void Add_OnKeyDown(OnKeyDownDelegate toadd)
        {
            lock (keydown_handlers)
            {
                keydown_handlers.Add(toadd);
            }
        }

        public static void Remove_OnKeyDown(OnKeyDownDelegate toremove)
        {
            lock (keydown_handlers)
            {
                keydown_handlers.Remove(toremove);
            }
        }

        public static void Add_OnWindowsMessage(OnWindowsMessageDelegate toadd)
        {
            lock (windowsmessage_handlers)
            {
                windowsmessage_handlers.Add(toadd);
            }
        }

        public static void Remove_OnWindowsMessage(OnWindowsMessageDelegate toremove)
        {
            lock (windowsmessage_handlers)
            {
                windowsmessage_handlers.Remove(toremove);
            }
        }

        public static void Add_OnQuit(SimpleDelegate toadd)
        {
            lock (quit_handlers)
            {
                quit_handlers.Add(toadd);
            }
        }

        public static void Remove_OnQuit(SimpleDelegate toremove)
        {
            lock (quit_handlers)
            {
                quit_handlers.Remove(toremove);
            }
        }
        #endregion

        #region ISynchronizeInvoke Members : invokation occurs on the Client's main thread, which is often necessary to ensure correct synchronization

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            DelegateRequest request = new DelegateRequest(method, args);
            DelegateQueue.Enqueue(request);
            return (IAsyncResult)request;
        }

        public object EndInvoke(IAsyncResult result)
        {
            DelegateRequest request = (DelegateRequest)result;
            request.AsyncWaitHandle.WaitOne();
            return request.Response;
        }

        public object Invoke(Delegate method, object[] args)
        {
            return EndInvoke(BeginInvoke(method, args));
        }

        public bool InvokeRequired
        {
            get { return (m_ClientThread != Thread.CurrentThread.ManagedThreadId); }
        }

        #endregion
    }
}
