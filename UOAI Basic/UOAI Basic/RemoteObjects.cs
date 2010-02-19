using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Proxies;

namespace RemoteObjects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoteObjectAttribute : Attribute
    {
        public RemoteObjectAttribute()
        {
        }
    }

    /*
    public class KeepAliveObject : MarshalByRefObject
    {
        private _MarshalByRefObject m_tokeepalive;
        public KeepAliveObject(_MarshalByRefObject tokeepalive)
        {
            m_tokeepalive = tokeepalive;
        }
    }

    [KeepAlive()]
    public class _MarshalByRefObject : ContextBoundObject
    {
        private KeepAliveObject m_KeepAliveObject;

        public _MarshalByRefObject()
            : base()
        {
            m_KeepAliveObject = new KeepAliveObject(this);
            RemotingServices.SetObjectUriForMarshal(m_KeepAliveObject, RemotingServices.GetObjectUri(this) + "_KEEP_ALIVE_OBJECT");
            RemotingServices.Marshal(m_KeepAliveObject);
        }
    }

    public class KeepAliveSponsor : ISponsor
    {
        public KeepAliveSponsor()
        {
        }

        #region ISponsor Members

        public TimeSpan Renewal(ILease lease)
        {
            return TimeSpan.FromMinutes(2);
        }

        #endregion
    }

    public class KeepAliveProxy : RealProxy
    {
        private KeepAliveObject m_KAO;
        private RealProxy m_ActualProxy;
        private KeepAliveSponsor m_ActualSponsor;

        public KeepAliveProxy(RealProxy fromproxy, ObjRef objRef)
        {
            m_ActualProxy = fromproxy;
            m_ActualSponsor = new KeepAliveSponsor();
            
            //install sponsor based on the fromref uri
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

            m_KAO=(KeepAliveObject)Activator.GetObject(typeof(KeepAliveObject), chURI + "/" + objRef.URI + "_KEEP_ALIVE_OBJECT");

            ILease lease=(ILease)RemotingServices.GetLifetimeService(m_KAO);
            lease.Register(m_ActualSponsor);
        }

        ~KeepAliveProxy()
        {
            ILease lease = (ILease)RemotingServices.GetLifetimeService(m_KAO);
            lease.Unregister(m_ActualSponsor);
        }

        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
        {
            return m_ActualProxy.Invoke(msg);
        }
    }

    public class KeepAliveAttribute : ProxyAttribute
    {
        public KeepAliveAttribute()
        {
        }

        public override RealProxy CreateProxy(ObjRef objRef, Type serverType, object serverObject, System.Runtime.Remoting.Contexts.Context serverContext)
        {
            return new KeepAliveProxy(base.CreateProxy(objRef, serverType, serverObject, serverContext), objRef);
        }
    }*/

    public class Server
    {
        private static bool m_TypesRegistered=false;
        private static System.Runtime.Remoting.Channels.Ipc.IpcChannel m_Channel;
        private static bool InheritsFromMarshalByRef(Type tocheck)
        {
            Type mbrtype = typeof(MarshalByRefObject);
            tocheck = tocheck.BaseType;
            while (tocheck != null)
            {
                if (tocheck==mbrtype)
                    return true;
                tocheck = tocheck.BaseType;
            }
            return false;
        }

        private static bool HasRemoteObjectAttribute(Type tocheck)
        {
            if (tocheck.GetCustomAttributes(typeof(RemoteObjectAttribute), false).Length > 0)
                return true;
            else
                return false;
        }

        static Server()
        {
            Startup();
        }

        private static void Startup()
        {
            //setup IPC channel
            Hashtable props = new Hashtable();

            System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider clientsink = new System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider();
            System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider serversink = new System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider();

            serversink.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            props["name"] = "";
            props["portName"] = "REMOTING_IPC_CHANNEL_0x" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString("X");
            props["typeFilterLevel"] = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            m_Channel = new System.Runtime.Remoting.Channels.Ipc.IpcChannel(props, clientsink, serversink);

            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(m_Channel, false);
        }

        public static void RegisterTypes()
        {
            if (!m_TypesRegistered)
            {
                //find all classes that inherit from MarshalByRef
                Assembly curassembly = Assembly.GetCallingAssembly();
                foreach (Type curtype in curassembly.GetTypes())
                {
                    if ((InheritsFromMarshalByRef(curtype))&&(HasRemoteObjectAttribute(curtype)))
                        RemotingConfiguration.RegisterWellKnownServiceType(curtype, curtype.Name, WellKnownObjectMode.Singleton);
                }
                m_TypesRegistered = true;
            }

            return;
        }

        public static object GetObject(Type objecttype, int processid)
        {
            return Activator.GetObject(objecttype, "ipc://REMOTING_IPC_CHANNEL_0x"+processid.ToString("X")+"/" + objecttype.Name);
        }
        public static object GetObject(Type objecttype)
        {
            return GetObject(objecttype, (int)Win32API.ProcessHandler.CurrentProcess.PID);
        }

        public static bool TypesRegistered { get { return m_TypesRegistered; } }
    }
}
