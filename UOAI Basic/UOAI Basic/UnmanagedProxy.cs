using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;
using RemoteObjects;
using System.Runtime.Remoting.Contexts;
using System.Collections;
using System.Security.Permissions;
using System.Collections.Generic;
using ProcessInjection;
using System.Runtime.InteropServices;

namespace UOAIBasic.Internal
{
    public class UnmanagedEvent
    {
        private bool IsForward = false;
        private UForwardAttribute fwattrib = null;
        private LocalHook m_Hook;
        //private EventInfo m_Info;
        private ParameterInfo[] parinfo;

        private BinaryTree<int, Delegate> m_Delegates = new BinaryTree<int, Delegate>();

        public UnmanagedEvent(UForwardAttribute _attrib)
        {
            IsForward = true;
            fwattrib = _attrib;
        }

        public UnmanagedEvent(EventInfo ev, uint tohook, ushort stack_cleanup_size, bool isVtblEntry, bool early)
        {
            parinfo = ev.EventHandlerType.GetMethods()[0].GetParameters();

            m_Hook = LocalHook.Hook(tohook, stack_cleanup_size, isVtblEntry);
            if(early)
                m_Hook.onCall += new LocalHook.OnCallDelegate(m_Hook_Call);
            else
                m_Hook.afterCall += new LocalHook.OnCallDelegate(m_Hook_Call);
        }

        public bool m_Hook_Call(Assembler.UnmanagedContext ctx, UnmanagedStack stck)
        {
            lock (m_Delegates)
            {
                bool retval = true;
                if (m_Delegates.Count > 0)
                {
                    object[] parameters = new object[parinfo.Length];
                    object[] attribs;
                    uint stckpos = 0;
                    uint i = 0;
                    foreach (ParameterInfo pi in parinfo)
                    {
                        //marshal parameter here
                        if ((attribs = pi.GetCustomAttributes(typeof(UThisParAttribute), true)).Length > 0)
                        {
                            if ((attribs = pi.GetCustomAttributes(typeof(UParAttribute), true)).Length > 0)
                            {
                                UParAttribute upa = (UParAttribute)attribs[0];
                                parameters[i] = UnmanagedProxy.MarshalFromUnmanaged(ctx.ECX, upa.ReturnType, upa.SizeConstant, upa.KnownType);
                            }
                            else
                                parameters[i] = null;//can't marshal
                        }
                        else if ((attribs = pi.GetCustomAttributes(typeof(UParAttribute), true)).Length > 0)
                        {
                            UParAttribute upa = (UParAttribute)attribs[0];
                            parameters[i] = UnmanagedProxy.MarshalFromUnmanaged(stck[stckpos], upa.ReturnType, upa.SizeConstant, upa.KnownType);
                            stckpos++;
                        }
                        else
                            parameters[i] = null;//can't marshal
                        i++;
                    }
                    foreach (Delegate del in m_Delegates)
                    {
                        try
                        {
                            if (Client.InvokationTarget != null)
                            {
                                if (Client.AsyncInvokation)
                                    Client.InvokationTarget.BeginInvoke(del, parameters);
                                else
                                    retval &= (bool)Client.InvokationTarget.Invoke(del, parameters);
                            }
                            else
                                retval &= (bool)del.DynamicInvoke(parameters);
                        }
                        catch
                        {
                            Remove(del);
                        }
                    }
                }
                return retval;
            }
        }

        public void Add(Delegate toadd)
        {
            if (!IsForward)
            {
                lock (m_Delegates)
                {
                    if (!m_Delegates.ContainsKey((int)toadd.Method.MethodHandle.Value))
                        m_Delegates.Add((int)toadd.Method.MethodHandle.Value, toadd);
                }
            }
            else
            {
                fwattrib.onAdd.Invoke(null, new object[] { toadd });
            }
        }

        public void Remove(Delegate toremove)
        {
            if (!IsForward)
            {
                lock (m_Delegates)
                {
                    if (m_Delegates.ContainsKey((int)toremove.Method.MethodHandle.Value))
                        m_Delegates.Remove((int)toremove.Method.MethodHandle.Value);
                }
            }
            else
            {
                fwattrib.onRemove.Invoke(null, new object[] { toremove });
            }
        }
    }

    public class UnmanagedProxy : MarshalByRefObject
    {
        private Type m_Interface;
        private IntPtr m_Address;
        private List<UnmanagedBuffer> tofree=new List<UnmanagedBuffer>();
        public static RuntimeMethodHandle GetTypeMethodHandle;

        public static Dictionary<int, bool> m_Async = new Dictionary<int, bool>();

        private Dictionary<int, UnmanagedEvent> m_EventsByRemoveHandler;
        private Dictionary<int, UnmanagedEvent> m_EventsByAddHandler;

        static UnmanagedProxy()
        {
            GetTypeMethodHandle=typeof(UnmanagedObject).GetMethod("GetType").MethodHandle;
        }

        private object GetAttribute(Type AttributeType, object[] attributelist)
        {
            foreach (object curattrib in attributelist)
            {
                if (curattrib.GetType() == AttributeType)
                    return curattrib;
            }
            return null;
        }

        internal static object MarshalFromUnmanaged(uint tomarshal, UnmanagedType type, uint sizeconst, Type knowntype)
        {
            if (type == UnmanagedType.Interface)
            {
                if(knowntype==typeof(UnmanagedBuffer))
                    return (object)new UnmanagedBuffer((IntPtr)tomarshal);
                else
                    return UnmanagedObject.Create(knowntype, (IntPtr)tomarshal);
            }
            else if (type == UnmanagedType.LPStruct)
            {
                UnmanagedBuffer buff = new UnmanagedBuffer((IntPtr)tomarshal);
                return buff.ReadAt(0, knowntype);
            }
            else if (type == UnmanagedType.LPWStr)
            {
                UnmanagedBuffer buff = new UnmanagedBuffer((IntPtr)tomarshal);
                return (object)buff.ReadUnicodeStringAt(0);
            }
            else if (type == UnmanagedType.LPStr)
            {
                UnmanagedBuffer buff = new UnmanagedBuffer((IntPtr)tomarshal);
                return (object)buff.ReadStringAt(0);
            }
            else
                return (object)tomarshal;
        }

        private uint MarshalToUnmanaged(object argument, object[] attributes)
        {
            Type argtype=argument.GetType();
            if ((argument is uint) || (argument is int) || (argument is short) || (argument is ushort) || (argument is byte) || (argument is sbyte) || (argument is bool))
                return (uint)argument;
            else if (argument is string)
            {
                string strargument = (string)argument;
                UnmanagedBuffer stringbuffer;
                MarshalAsAttribute curattrib;
                bool unicode = false;
                uint length = 0;
                if ((curattrib = (MarshalAsAttribute)GetAttribute(typeof(MarshalAsAttribute),attributes)) != null)
                {
                    if (curattrib.Value == UnmanagedType.LPWStr)
                        unicode = true;
                    if (curattrib.SizeConst > 0)
                        length = (uint)curattrib.SizeConst;
                }
                if (unicode)
                {
                    if (length == 0)
                    {
                        stringbuffer = new UnmanagedBuffer((uint)((strargument.Length + 1) * 2));
                        stringbuffer.WriteUnicodeStringAt(strargument, 0);
                    }
                    else
                    {
                        stringbuffer = new UnmanagedBuffer(length * 2);
                        stringbuffer.WriteUnicodeStringAt(strargument, 0, (int)length);
                    }
                }
                else
                {
                    if (length == 0)
                    {
                        stringbuffer = new UnmanagedBuffer((uint)(strargument.Length + 1));
                        stringbuffer.WriteStringAt(strargument, 0);
                    }
                    else
                    {
                        stringbuffer = new UnmanagedBuffer(length);
                        stringbuffer.WriteStringAt(strargument, 0, (int)length);
                    }
                }
                tofree.Add(stringbuffer);
                return (uint)stringbuffer.Address;
            }
            else if (argtype.IsArray)
            {
                Array argarray = (Array)argument;
                uint size;
                if (argarray.Length == 0)
                    throw new Exception("empty arrays not supported");
                
                size = (uint)Marshal.SizeOf(argarray.GetValue(0));
                UnmanagedBuffer arrbuff = new UnmanagedBuffer((uint)(size * argarray.Length));
                foreach (object argobj in argarray)
                    arrbuff.Write(argobj);
                tofree.Add(arrbuff);
                return (uint)arrbuff.Address;
            }
            else if (argtype.IsValueType)
            {
                //assume this is a struct that can be marshalled
                UnmanagedBuffer ubuffer = new UnmanagedBuffer((uint)Marshal.SizeOf(argument));
                ubuffer.WriteAt(argument, 0);
                tofree.Add(ubuffer);
                return (uint)ubuffer.Address;
            }
            else if (argument is UnmanagedBuffer)
            {
                try
                {
                    return (uint)(((UnmanagedBuffer)argument).Address);
                }
                catch (Exception e)
                {
                    throw new Exception("Type can not be marshalled to unmanaged code: unsupported parametertype!", e);
                }
            }
            else
            {
                //is this an instance of a known unmanaged interface, if so get it's pointer and use that
                try
                {
                    return (uint)(((UnmanagedObject)argument).addr);
                }
                catch (Exception e)
                {
                    throw new Exception("Type can not be marshalled to unmanaged code: unsupported parametertype!", e);
                }
            }
        }

        public UnmanagedProxy(Type unmanagedinterface, IntPtr address)
        {
            m_Interface = unmanagedinterface;
            m_Address = address;
            EventInfo[] events=unmanagedinterface.GetEvents();

            if (events.Length > 0)
            {
                m_EventsByAddHandler = new Dictionary<int, UnmanagedEvent>(events.Length);
                m_EventsByRemoveHandler = new Dictionary<int, UnmanagedEvent>(events.Length);
            }
            
            foreach (EventInfo ev in events)
            {
                UnmanagedEvent uev;
                object[] attribs=ev.GetCustomAttributes(typeof(UHookAttribute), true);
                if (attribs.Length > 0)
                {
                    UHookAttribute uha = (UHookAttribute)attribs[0];
                    if (uha.IsVtblHook)
                    {
                        UnmanagedBuffer tempbuffer = new UnmanagedBuffer((IntPtr)address);
                        uint m_addr = tempbuffer.ReadAt<uint>(0);
                        uev = new UnmanagedEvent(ev, (uint)m_addr + 4 * uha.Address, uha.StackSize, uha.IsVtblHook, uha.IsEarly);
                    }
                    else
                    {
                        uint m_addr = (uint)UOCallibration.Callibrations[uha.Address];
                        uev = new UnmanagedEvent(ev, m_addr, uha.StackSize, uha.IsVtblHook, uha.IsEarly);
                    }
                    m_EventsByAddHandler.Add((int)ev.GetAddMethod().MethodHandle.Value, uev);
                    m_EventsByRemoveHandler.Add((int)ev.GetRemoveMethod().MethodHandle.Value, uev);
                }
                else if((attribs=ev.GetCustomAttributes(typeof(UForwardAttribute), true)).Length>0)
                {
                    uev = new UnmanagedEvent((UForwardAttribute)attribs[0]);
                    m_EventsByAddHandler.Add((int)ev.GetAddMethod().MethodHandle.Value, uev);
                    m_EventsByRemoveHandler.Add((int)ev.GetRemoveMethod().MethodHandle.Value, uev);
                }
            }
        }

        private uint PerformCall(uint addr, IMethodCallMessage methodMessage)
        {
            uint retval = 0;

            MethodBase method = methodMessage.MethodBase;

            List<uint> stack = new List<uint>(methodMessage.ArgCount);

            //a. build stack-desc
            for (uint i = 0; i < methodMessage.ArgCount; i++)
                stack.Add(MarshalToUnmanaged(methodMessage.Args[i], method.GetParameters()[i].GetCustomAttributes(false)));

            //b. perform call
            retval = UnmanagedCall.Call(addr, stack.ToArray(), (uint)m_Address);

            //c. cleanup
            foreach (UnmanagedBuffer buff in tofree)
                buff.Free();

            return retval;
        }

        public delegate IMessage InvokeDelegate(IMessage toinvoke);

        public IMessage Invoke(IMessage message)
        {
            IMessage returnMessage;
            object[] attribs;
            uint addr;
            uint retval;

            IMethodCallMessage methodMessage =
               new MethodCallMessageWrapper((IMethodCallMessage)message);

            MethodBase method = methodMessage.MethodBase;

            object returnValue = null;

            if (method.MethodHandle == GetTypeMethodHandle)
            {
                returnValue = m_Interface;
            }
            else if (((attribs = method.GetCustomAttributes(typeof(SyncAttribute), true)).Length > 0)&&(Client.Default.InvokeRequired))
            {
                //m_Async value overrides Sync.Async
                if (m_Async.ContainsKey((int)method.MethodHandle.Value))
                {
                    if ((m_Async[(int)method.MethodHandle.Value]))//async override
                    {
                        Client.Default.BeginInvoke(new InvokeDelegate(Invoke), new object[] { message });
                        return new ReturnMessage(null, methodMessage.Args,
                          methodMessage.ArgCount, methodMessage.LogicalCallContext,
                          methodMessage);
                    }
                    else//sync override
                        return (IMessage)Client.Default.Invoke(new InvokeDelegate(Invoke), new object[] { message });
                }
                else if(((SyncAttribute)attribs[0]).Async)//preconfigured async
                {
                    Client.Default.BeginInvoke(new InvokeDelegate(Invoke), new object[] { message });
                    return new ReturnMessage(null, methodMessage.Args,
                      methodMessage.ArgCount, methodMessage.LogicalCallContext,
                      methodMessage);
                }
                else//preconfigured sync
                {
                    return (IMessage)Client.Default.Invoke(new InvokeDelegate(Invoke), new object[] { message });
                }
            }
            else
            {
                if ((attribs = method.GetCustomAttributes(typeof(UMethodAttribute), true)).Length > 0)
                {
                    UMethodAttribute mattrib = (UMethodAttribute)attribs[0];
                    addr = (uint)UOCallibration.Callibrations[(uint)mattrib.CallibratedOffsetFeature];
                    retval = PerformCall(addr, methodMessage);
                    if (mattrib.HasReturnValue)
                        returnValue = MarshalFromUnmanaged(retval, mattrib.ReturnType, mattrib.SizeConstant, mattrib.KnownType);
                    else
                        returnValue = null;
                }
                else if ((attribs = method.GetCustomAttributes(typeof(UFieldAttribute), true)).Length > 0)
                {
                    UFieldAttribute fattrib = (UFieldAttribute)attribs[0];
                    UnmanagedBuffer buff = new UnmanagedBuffer((IntPtr)((uint)m_Address + (uint)UOCallibration.Callibrations[(uint)fattrib.CallibratedOffsetFeature]));
                    retval = buff.Read<uint>();
                    returnValue = MarshalFromUnmanaged(retval, fattrib.ReturnType, fattrib.SizeConstant, fattrib.KnownType);
                }
                else if ((attribs = method.GetCustomAttributes(typeof(UGlobalAttribute), true)).Length > 0)
                {
                    UGlobalAttribute gattrib = (UGlobalAttribute)attribs[0];
                    UnmanagedBuffer buff = new UnmanagedBuffer((IntPtr)UOCallibration.Callibrations[(uint)gattrib.CallibratedOffsetFeature]);
                    retval = buff.Read<uint>();
                    returnValue = MarshalFromUnmanaged(retval, gattrib.ReturnType, gattrib.SizeConstant, gattrib.KnownType);
                }
                else if ((attribs = method.GetCustomAttributes(typeof(UVtblMethodAttribute), true)).Length > 0)
                {
                    UVtblMethodAttribute mattrib = (UVtblMethodAttribute)attribs[0];
                    UnmanagedBuffer m_This = new UnmanagedBuffer(m_Address);
                    addr = m_This.ReadAt<uint>(0);
                    if (mattrib.hasidx)
                        addr += 4 * mattrib.index;
                    else
                        addr += (uint)UOCallibration.Callibrations[(uint)mattrib.CallibratedOffsetFeature];
                    retval = PerformCall(addr, methodMessage);
                    if (mattrib.HasReturnValue)
                        returnValue = MarshalFromUnmanaged(retval, mattrib.ReturnType, mattrib.SizeConstant, mattrib.KnownType);
                    else
                        returnValue = null;
                }
                else if ((attribs = method.GetCustomAttributes(typeof(UForwardAttribute), true)).Length > 0)
                {
                    UForwardAttribute fattrib = (UForwardAttribute)attribs[0];
                    /*if (fattrib.Target == null)
                    {
                        if (m_EventsByAddHandler.ContainsKey((int)method.MethodHandle.Value))
                            returnValue = fattrib.onAdd.Invoke(null, methodMessage.Args);
                        else if (m_EventsByRemoveHandler.ContainsKey((int)method.MethodHandle.Value))
                            returnValue = fattrib.onRemove.Invoke(null, methodMessage.Args);
                        else
                            returnValue = null;
                    }
                    else*/
                    returnValue = fattrib.Target.Invoke(null, methodMessage.Args);
                }
                else if ((methodMessage.ArgCount == 1) && (methodMessage.Args[0] is Delegate))
                {
                    returnValue = null;
                    if (m_EventsByAddHandler.ContainsKey((int)method.MethodHandle.Value))
                    {
                        m_EventsByAddHandler[(int)method.MethodHandle.Value].Add((Delegate)methodMessage.Args[0]);
                    }
                    else if (m_EventsByRemoveHandler.ContainsKey((int)method.MethodHandle.Value))
                    {
                        m_EventsByRemoveHandler[(int)method.MethodHandle.Value].Remove((Delegate)methodMessage.Args[0]);
                    }
                }
            }
            returnMessage =
               new ReturnMessage(returnValue, methodMessage.Args,
                  methodMessage.ArgCount, methodMessage.LogicalCallContext,
                  methodMessage);

            return returnMessage;
        }

        public bool CanCastTo(Type fromType)
        {
            return true;
        }
    }

    [UnmanagedObject()]
    public class UnmanagedObject : ContextBoundObject, IClient
    {
        public UnmanagedProxy LocalProxy;
        public IntPtr addr;

        public static BinaryTree<uint, WeakReference> KnownObjects=new BinaryTree<uint, WeakReference>();

        public static UnmanagedObject Create(Type unmanagedinterface, IntPtr address)
        {
            UnmanagedObject toreturn;
            WeakReference wr;
            if (KnownObjects.ContainsKey((uint)address))
            {
                wr = KnownObjects[(uint)address];
                if (wr.IsAlive)
                    return (UnmanagedObject)wr.Target;
                else
                {
                    toreturn = new UnmanagedObject(unmanagedinterface, address);
                    KnownObjects.Remove((uint)address);
                    KnownObjects.Add((uint)address, new WeakReference(toreturn));
                }
            }
            else
            {
                toreturn = new UnmanagedObject(unmanagedinterface, address);
                KnownObjects.Add((uint)address, new WeakReference(toreturn));
            }
            
            //early marshal, we need the URI
            ObjRef test = RemotingServices.Marshal(toreturn);
            
            //marshal local proxy with related URI, so we can always find it from a reference to this object
            RemotingServices.SetObjectUriForMarshal(toreturn.LocalProxy, test.URI + "_LOCALPROXY");
            RemotingServices.Marshal(toreturn.LocalProxy);

            return toreturn;
        }

        private UnmanagedObject(Type unmanagedinterface, IntPtr address)
        {
            addr = address;
            LocalProxy = new UnmanagedProxy(unmanagedinterface, address);
        }

        ~UnmanagedObject()
        {
            if (KnownObjects.ContainsKey((uint)addr))
                KnownObjects.Remove((uint)addr);
        }

        #region Dummy IClient Members
        //This just solves a casting problem.
        //All calls on a unamangedobject are intercepted, so none of this ever gets called.
        //We can cast a proxy to an UnmanagedObject to any interface, even if it doesn't support it.
        //This because calls get intercepted and called from the interception code... if an unknown call is encountered null is returned.
        //However a direct reference (no proxy) to the remote object can't be cast.
        //Internally, immediately after the injection, we create the first remoteobject for the IClient interface.
        //At that point we have a direct reference and no proxy (we are still on the injected code), so we can't cast to IClient...
        //To allow us to return an object as "IClient" rather than RemoteObject to the user app...
        //we need a dummy implementation, so that we can cast before we return.
        //Once we return the object (across remoting boundaries.. Client.exe->user app) a proxy is created on the user app (since this class has a proxy attribute),
        //so all IClient calls get intercepted by our proxy and these funtions here become meaningless.

        public void SysMessage(uint color, uint font, string message)
        {
            //dummy event executions to get rid of the annoying 'is never used' messages
            OnKeyDown(0, true);
            OnKeyUp(0);
            OnQuit();
            Win32API.Imports.MSG msg=new Win32API.Imports.MSG();
            OnWindowsMessage(ref msg);

            throw new NotImplementedException();
        }

        public void Macro(MacroTable macro, uint index)
        {
            throw new NotImplementedException();
        }

        public INetworkObject NetworkObject
        {
            get { throw new NotImplementedException(); }
        }

        public event OnKeyUpDelegate OnKeyUp;

        public event OnKeyDownDelegate OnKeyDown;

        public void PatchEncryption()
        {
            throw new NotImplementedException();
        }

        public UnmanagedBuffer AllocateBuffer(uint size)
        {
            throw new NotImplementedException();
        }

        public void Quit()
        {
            throw new NotImplementedException();
        }

        public event SimpleDelegate OnQuit;

        public event OnWindowsMessageDelegate OnWindowsMessage;

        public void SetInvokationTarget(InvokationTarget target, bool bInvokeAsynchronously)
        {
            throw new NotImplementedException();
        }

        public void SetAsync(string typename, string methodname, bool value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class UnmanagedObjectAttribute : ProxyAttribute
    {
        public UnmanagedObjectAttribute()
        {
        }

        public override RealProxy CreateProxy(ObjRef objRef, Type serverType, object serverObject, Context serverContext)
        {
            System.Runtime.Remoting.Channels.ChannelDataStore chds = null;
            foreach (object obj in objRef.ChannelInfo.ChannelData)
            {
                if (obj is System.Runtime.Remoting.Channels.ChannelDataStore)
                {
                    chds = (System.Runtime.Remoting.Channels.ChannelDataStore)obj;
                    break;
                }
            }
            string chURI = chds.ChannelUris[0];
            return new UnmanagedObjectProxy(serverType, chURI, objRef.URI);
        }
    }

    public class UnmanagedObjectProxy : RealProxy, IRemotingTypeInfo
    {
        public UnmanagedProxy m_Proxy;

        public UnmanagedObjectProxy(Type oftype, string chURI, string objURI) : base(oftype)
        {
            m_Proxy = (UnmanagedProxy)Activator.GetObject(typeof(UnmanagedProxy), chURI + "/" + objURI + "_LOCALPROXY");
        }

        public override IMessage Invoke(IMessage msg)
        {
            return m_Proxy.Invoke(msg);
        }

        #region IRemotingTypeInfo Members

        public bool CanCastTo(Type fromType, object o)
        {
            return m_Proxy.CanCastTo(fromType);
        }

        public string TypeName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UMethodAttribute : Attribute
    {
        public UOCallibration.CallibratedFeatures CallibratedOffsetFeature;
        public UnmanagedType ReturnType;
        public bool HasReturnValue;
        public uint SizeConstant;
        public Type KnownType = null;

        public UMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = false;
        }

        public UMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = true;
            ReturnType = returntype;
            SizeConstant = 0;
        }

        public UMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, Type knowntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = true;
            ReturnType = UnmanagedType.Interface;
            SizeConstant = 0;
            KnownType = knowntype;
        }

        public UMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype, uint sizeconstant)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = true;
            ReturnType = returntype;
            SizeConstant = sizeconstant;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UVtblMethodAttribute : Attribute
    {
        public bool hasidx = false;
        public uint index;
        public UOCallibration.CallibratedFeatures CallibratedOffsetFeature;
        public UnmanagedType ReturnType;
        public bool HasReturnValue;
        public uint SizeConstant;
        public Type KnownType = null;

        public UVtblMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = false;
        }

        public UVtblMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = true;
            ReturnType = returntype;
            SizeConstant = 0;
        }

        public UVtblMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, Type knowntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = true;
            ReturnType = UnmanagedType.Interface;
            SizeConstant = 0;
            KnownType = knowntype;
        }

        public UVtblMethodAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype, uint sizeconstant)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            HasReturnValue = true;
            ReturnType = returntype;
            SizeConstant = sizeconstant;
        }

        public UVtblMethodAttribute(uint idx)
        {
            hasidx = true;
            index = idx;
            HasReturnValue = false;
        }

        public UVtblMethodAttribute(uint idx, UnmanagedType returntype)
        {
            hasidx = true;
            index = idx;
            HasReturnValue = true;
            ReturnType = returntype;
            SizeConstant = 0;
        }

        public UVtblMethodAttribute(uint idx, Type knowntype)
        {
            hasidx = true;
            index = idx;
            HasReturnValue = true;
            ReturnType = UnmanagedType.Interface;
            SizeConstant = 0;
            KnownType = knowntype;
        }

        public UVtblMethodAttribute(uint idx, UnmanagedType returntype, uint sizeconstant)
        {
            hasidx = true;
            index = idx;
            HasReturnValue = true;
            ReturnType = returntype;
            SizeConstant = sizeconstant;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UFieldAttribute : Attribute
    {
        public UOCallibration.CallibratedFeatures CallibratedOffsetFeature;
        public UnmanagedType ReturnType;
        public uint SizeConstant;
        public Type KnownType = null;

        public UFieldAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            ReturnType = returntype;
            SizeConstant = 0;
        }

        public UFieldAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype, Type knowntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            ReturnType = returntype;
            SizeConstant = 0;
            KnownType = knowntype;
        }

        public UFieldAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype, uint sizeconstant)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            ReturnType = returntype;
            SizeConstant = sizeconstant;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UGlobalAttribute : Attribute
    {
        public UOCallibration.CallibratedFeatures CallibratedOffsetFeature;
        public UnmanagedType ReturnType;
        public uint SizeConstant;
        public Type KnownType = null;

        public UGlobalAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            ReturnType = returntype;
            SizeConstant = 0;
        }

        public UGlobalAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype, Type knowntype)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            ReturnType = returntype;
            SizeConstant = 0;
            KnownType = knowntype;
        }

        public UGlobalAttribute(UOCallibration.CallibratedFeatures CallibratedOffset, UnmanagedType returntype, uint sizeconstant)
        {
            CallibratedOffsetFeature = CallibratedOffset;
            ReturnType = returntype;
            SizeConstant = sizeconstant;
        }
    }

    //Synchronize with the client's main thread
    [AttributeUsage(AttributeTargets.Method)]
    public class SyncAttribute : Attribute
    {
        public bool Async = false;//tricky: Async=true still means the method this attribute applies to
                                  // gets executed synchronously with the client's main thread...
                                  // however BeginInvoke is used, without waiting for a result.
                                  // So from the caller's point of view this method gets
                                  // executed synchronously if async=false and asynchronously if async=true
                                  // but in both cases the call is syncrhonized with the client's main thread.

        public SyncAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class UThisParAttribute : Attribute
    {
        public UThisParAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class UParAttribute : Attribute
    {
        public UnmanagedType ReturnType;
        public uint SizeConstant;
        public Type KnownType = null;

        public UParAttribute(UnmanagedType returntype)
        {
            ReturnType = returntype;
            SizeConstant = 0;
        }

        public UParAttribute(UnmanagedType returntype, Type knowntype)
        {
            ReturnType = returntype;
            SizeConstant = 0;
            KnownType = knowntype;
        }

        public UParAttribute(UnmanagedType returntype, uint sizeconstant)
        {
            ReturnType = returntype;
            SizeConstant = sizeconstant;
        }
    }

    [AttributeUsage(AttributeTargets.Event)]
    public class UHookAttribute : Attribute
    {
        public uint Address;
        public ushort StackSize;
        public bool IsVtblHook;
        public bool IsEarly;

        public UHookAttribute(uint address, ushort stacksize, bool isvtblhook, bool early)
        {
            Address = address;
            StackSize = stacksize;
            IsVtblHook = isvtblhook;
            IsEarly = early;
        }

        public UHookAttribute(UOCallibration.CallibratedFeatures address, ushort stacksize, bool early)
        {
            Address = (uint)address;
            StackSize = stacksize;
            IsVtblHook = false;
            IsEarly = early;
        }
    }

    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Event)]
    public class UForwardAttribute : Attribute
    {
        public MethodInfo Target;
        public MethodInfo onAdd;
        public MethodInfo onRemove;

        public UForwardAttribute(Type ontype, string target)
        {
            Target = ontype.GetMethod(target);
            onAdd = null;
            onRemove = null;
        }

        public UForwardAttribute(Type ontype, string _onadd, string _onremove)
        {
            Target = null;
            onAdd = ontype.GetMethod(_onadd);
            onRemove = ontype.GetMethod(_onremove);
        }
    }
}
