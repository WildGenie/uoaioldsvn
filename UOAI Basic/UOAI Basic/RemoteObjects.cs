using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace RemoteObjects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoteObjectAttribute : Attribute
    {
        public RemoteObjectAttribute()
        {
        }
    }

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
