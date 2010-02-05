using System;
using System.Collections.Generic;
using Win32API;
using ProcessInjection;
using RemoteObjects;
using System.Reflection;
using System.Threading;

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

            //- inject our assembly into the client process (where an instance of the InjectedClient class is created!
            Injection.Inject(onprocess, onthread, Assembly.GetExecutingAssembly(), typeof(InjectedClient));
                        
            //- create a proxy to the remote client object
            curclient = (Client)Server.GetObject(typeof(Client), (int)onprocess.PID);

            //- wait for server to become available
            while (!IsValid(curclient))
                Thread.Sleep(0);

            return curclient;
        }

        public bool IsValid(Client tocheck)
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
    public class InjectedClient
    {
        public InjectedClient()
        {
            Imports.AllocConsole();
            Console.WriteLine("Injected!");//should pop-up on a console from the client

            //expose all MarshalByRefObjects through Remoting
            Server.RegisterTypes();

            //callibration code is to be called here
            try
            {
                UOCallibration.Callibrate(ProcessHandler.CurrentProcess);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Callibration Dump:");
            Console.Write(UOCallibration.Callibrations.ToString());
        }
    }

    //this object is created on the client process, then marshalled back through remoting
    public class Client : MarshalByRefObject
    {
        private static Client m_Client;

        public Client()
        {
            m_Client = this;//keep us alive
        }

        public bool Validate()
        {
            return true;
        }//dummy, trying to call this when the client is unavailable will throw a remoting error, we catch this error to detect an invalid client

        public void WriteLine(string towrite)//test function, writes to the console created on the client
        {
            Console.WriteLine(towrite);
        }
    }
}
