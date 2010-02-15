using System;
using Assembler;
using Win32API;
using Tools;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using RemoteObjects;
using System.Reflection;
using libdisasm;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProcessInjection
{
    //dll injection
    public class Injection
    {
        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct UINTARG//Variant holding a UInt
        {
            public UInt16 vt;
            public UInt16 wReserved1;
            public UInt16 wReserved2;
            public UInt16 wReserved3;
            public UInt32 value;
            public UInt32 valueb;
            public UINTARG(UInt32 uint_value)
            {
                vt = 0x13;
                wReserved1 = 0;
                wReserved2 = 0;
                wReserved3 = 0;
                value = uint_value;
                valueb = 0;
            }
        }

        //GUIDs required to load the CLR and setup a new AppDomain to load the injected assembly
        private static Guid CLSID_CorRuntimeHost = new Guid(0xcb2f6723, 0xab3a, 0x11d2, 0x9c, 0x40, 0x00, 0xc0, 0x4f, 0xa3, 0x0a, 0x3e);
        private static Guid IID_ICorRuntimeHost = new Guid(0xcb2f6722, 0xab3a, 0x11d2, 0x9c, 0x40, 0x00, 0xc0, 0x4f, 0xa3, 0x0a, 0x3e);
        private static Guid IID_IAppSetup = new Guid("{27fff232-a7a8-40dd-8d4a-734ad59fcd41}");
        private static Guid IID_IAppDomain = new Guid("{05f696dc-2b29-3663-ad8b-c4389cf2a713}");

        //injection: loads the CLR on the specified thread of the specified process
        //it thereafter basically does the following:
        //      appdomain=CLR->CreateDomain(setupparameters={..., AppBase-path, ...})
        //      asm=appdomain->load(specified assembly)
        //      asm->CreateInstance(specified type)
        //... i could have used the default appdomain, but when i was testing remoting from the injected code
        // i noticed that the defautl appdomain gets a basepath equal to that of the injected process, which 
        // causes Remoting to fail as client/server are referring different assembly paths, and the injected remoting
        // code can't find the assembly when trying to read type info to setup remoting proxies.
        // Changing the basepath is impossible, so we just create a new appdomain with the right basepath and
        // run code there.
        public static void Inject(ProcessHandler onprocess, ThreadHandler onthread, Assembly toinject, Type tocreate)
        {            
            StreamHandler sh = new StreamHandler(onprocess);

            IntPtr RemotePage = onprocess.Allocate(4096);//allocate one page of readable, writable and executable memory

            //inject data
            sh.Position=(long)RemotePage;

            //write GUIDs

            //- write CLSID_CorRuntimeHost
            IntPtr pCLSIDCorRuntimeHost = (IntPtr)sh.Position;
            sh.Write(CLSID_CorRuntimeHost.ToByteArray(), 0, 16);            
            
            //- write IID_ICorRuntimeHost
            IntPtr pIID_ICorRuntimeHost = (IntPtr)sh.Position;
            sh.Write(IID_ICorRuntimeHost.ToByteArray(), 0, 16);

            //- write IID_IAppSetup (not the actualy name i think)
            IntPtr pIID_IAppSetup = (IntPtr)sh.Position;
            sh.Write(IID_IAppSetup.ToByteArray(), 0, 16);

            //- write IID_IAppDomain (not the actualy name i think)
            IntPtr pIID_IAppDomain = (IntPtr)sh.Position;
            sh.Write(IID_IAppDomain.ToByteArray(), 0, 16);            
            
            //- write VARIANT (result var for AppDomain.CreateInstance)
            UINTARG VariantArg = new UINTARG(0);
            IntPtr pVariant = (IntPtr)sh.Position;
            sh.Write<UINTARG>(VariantArg);

            //reserve space for variables (<- this could be done better by using the stack?)

            //- reserve uint stackbackup
            IntPtr pStackBackup = (IntPtr)sh.Position;
            sh.Write<uint>(0);

            //- reserve RuntimeObject pointer
            IntPtr pRuntimeObject = (IntPtr)sh.Position;
            sh.Write<uint>(0);

            //- reserve Module Handle pointer
            IntPtr pHandle = (IntPtr)sh.Position;
            sh.Write<uint>(0);
            
            //- reserve Setup Object pointer (IUnknown version)
            IntPtr pSetupObject = (IntPtr)sh.Position;
            sh.Write<uint>(0);

            //- reserve AppSetup Object pointer (after QueryInterface, so IAppSetup version)
            IntPtr pAppSetupObject = (IntPtr)sh.Position;
            sh.Write<uint>(0);

            //- reserve AppDomain object pointer (IUnknown version)
            IntPtr pDomainObject = (IntPtr)sh.Position;
            sh.Write<uint>(0);

            //- reserve AppDomain object pointer (IAppDomain version, after QueryInterface)
            IntPtr pAppDomainObject = (IntPtr)sh.Position;
            sh.Write<uint>(0);

            //- reserve Assembly object pointer
            IntPtr pAsm = (IntPtr)sh.Position;
            sh.Write<uint>(0);

            //- reserve uint function pointers
            IntPtr pCorBindToRuntimeEx = (IntPtr)sh.Position;
            sh.Write<uint>(0);          

            //- write strings (for loadlibrary calls)
            sh.EnsureAlignment(2);
            IntPtr pStrCorBindToRuntimeEx = (IntPtr)sh.Position;
            sh.WriteString("CorBindToRuntimeEx");

            sh.EnsureAlignment(2);
            IntPtr pStrMSCorEE=(IntPtr)sh.Position;
            sh.WriteString("MSCorEE.dll");

            sh.EnsureAlignment(2);
            IntPtr pStrwks=(IntPtr)sh.Position;
            sh.WriteUnicodeString("wks");//workstation mode

            sh.EnsureAlignment(2);
            IntPtr pStrDomainName=(IntPtr)sh.Position;
            sh.WriteUnicodeString("injected_domain_" + onprocess.PID.ToString("X"));

            sh.EnsureAlignment(2);
            IntPtr pStrAppBase = (IntPtr)(sh.Position + 4);//+4 to skip pre-pendend length
            sh.WriteBSTR(System.IO.Path.GetDirectoryName(toinject.Location));

            sh.EnsureAlignment(2);
            IntPtr pAssemblyName = (IntPtr)(sh.Position + 4);//+4 to skip pre-pendend length
            sh.WriteBSTR(toinject.GetName().Name);

            sh.EnsureAlignment(2);
            IntPtr pTypeName = (IntPtr)(sh.Position + 4);//+4 to skip pre-pendend length
            sh.WriteBSTR(tocreate.FullName);

            //get required function addresses
            //- LoadLibraryA, GetProcAddress from KERNEL32.dll
            uint pLoadLibraryA = onprocess.MainModule.PEHeader.ImportedLibraries["KERNEL32.dll"].ImportedSymbols["LoadLibraryA"].Address;
            uint pGetProcAddress = onprocess.MainModule.PEHeader.ImportedLibraries["KERNEL32.dll"].ImportedSymbols["GetProcAddress"].Address;
            
            //build code
            AsmBuilder _asm = new AsmBuilder();
            
            //safety measures : push all registers, save the stack pointer
            _asm.Instructions.Add(new PushAll());
            _asm.Instructions.Add(new BackupEsp((uint)pStackBackup));

            //build loadlibrary, getprocaddress calls to get the pCorBindToRuntimeEx from MSCorEE.dll
            _asm.Instructions.Add(new PushImmediate((uint)pStrMSCorEE));
            _asm.Instructions.Add(new CallRelative((int)pLoadLibraryA));
            _asm.Instructions.Add(new MovMemoryEax((uint)pHandle));
            _asm.Instructions.Add(new PushImmediate((uint)pStrCorBindToRuntimeEx));
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new CallRelative((int)pGetProcAddress));
            _asm.Instructions.Add(new MovMemoryEax((uint)pCorBindToRuntimeEx));            

            //build CorBindToRuntimeEx call
            _asm.Instructions.Add(new PushImmediate((uint)pRuntimeObject));
            _asm.Instructions.Add(new PushImmediate((uint)pIID_ICorRuntimeHost));
            _asm.Instructions.Add(new PushImmediate((uint)pCLSIDCorRuntimeHost));
            _asm.Instructions.Add(new PushImmediate(2));
            _asm.Instructions.Add(new PushImmediate((uint)pStrwks));
            _asm.Instructions.Add(new PushImmediate((uint)0));
            _asm.Instructions.Add(new CallFunctionPointer((uint)pCorBindToRuntimeEx));

            //pRuntimeObject->Start()
            _asm.Instructions.Add(new MovEaxMemory((uint)pRuntimeObject));
            _asm.Instructions.Add(new MovEcxMemory((uint)pRuntimeObject));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0x28));//Start() is at 0x28 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //pRuntimeObject->CreateDomainSetup(IUnkown ** pSetup)
            _asm.Instructions.Add(new PushImmediate((uint)pSetupObject));
            _asm.Instructions.Add(new MovEaxMemory((uint)pRuntimeObject));
            _asm.Instructions.Add(new MovEcxMemory((uint)pRuntimeObject));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0x48));//CreateDomainSetup() is at 0x48 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //pSetupObject->QueryInterface(&IID_IAppSetup, IID_IAppSetup ** pAppsetup)
            _asm.Instructions.Add(new PushImmediate((uint)pAppSetupObject));
            _asm.Instructions.Add(new PushImmediate((uint)pIID_IAppSetup));
            _asm.Instructions.Add(new MovEaxMemory((uint)pSetupObject));
            _asm.Instructions.Add(new MovEcxMemory((uint)pSetupObject));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0x0));//QueryInterface() is at 0x0 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //AppSetup->put_ApplicationBase(BSTR pStrAppBase)
            _asm.Instructions.Add(new PushImmediate((uint)pStrAppBase));
            _asm.Instructions.Add(new MovEaxMemory((uint)pAppSetupObject));
            _asm.Instructions.Add(new MovEcxMemory((uint)pAppSetupObject));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0x10));//put_ApplicationBase is at 0x10 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //pRuntimeObject->CreateDomainEx(unicode string pStrDomainName,pAppSetup,0,&pDomain);
            _asm.Instructions.Add(new PushImmediate((uint)pDomainObject));
            _asm.Instructions.Add(new PushImmediate(0));
            _asm.Instructions.Add(new MovEaxMemory((uint)pAppSetupObject));
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new PushImmediate((uint)pStrDomainName));
            _asm.Instructions.Add(new MovEaxMemory((uint)pRuntimeObject));
            _asm.Instructions.Add(new MovEcxMemory((uint)pRuntimeObject));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0x44));//CreateDomainEx is at 0x44 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //pDomainObject->QueryInterface(IID_IDomain, IAppDomain * pAppDomain)
            _asm.Instructions.Add(new PushImmediate((uint)pAppDomainObject));
            _asm.Instructions.Add(new PushImmediate((uint)pIID_IAppDomain));
            _asm.Instructions.Add(new MovEaxMemory((uint)pDomainObject));
            _asm.Instructions.Add(new MovEcxMemory((uint)pDomainObject));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0x0));//QueryInterface is at 0x0 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //pAppDomain->Load_2(pAssemblyName, &pAsm)
            _asm.Instructions.Add(new PushImmediate((uint)pAsm));
            _asm.Instructions.Add(new PushImmediate((uint)pAssemblyName));
            _asm.Instructions.Add(new MovEaxMemory((uint)pAppDomainObject));
            _asm.Instructions.Add(new MovEcxMemory((uint)pAppDomainObject));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0xB0));//Load_2 is at 0xB0 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //pAsm->CreateInstance(pTypeName,&pVariant)
            _asm.Instructions.Add(new PushImmediate((uint)pVariant));
            _asm.Instructions.Add(new PushImmediate((uint)pTypeName));
            _asm.Instructions.Add(new MovEaxMemory((uint)pAsm));
            _asm.Instructions.Add(new MovEcxMemory((uint)pAsm));//<- shouldn't be necessary, it's not a C++ object, but a COM object, so this doesn't have to be stored in ecx
            _asm.Instructions.Add(new PushEax());
            _asm.Instructions.Add(new DereferEax());
            _asm.Instructions.Add(new DereferEaxTable(0xA4));//CreateInstance is at 0xA4 in the vtbl
            _asm.Instructions.Add(new CallEax());

            //freelibrary?

            //safety measures : restore stack pointer, pop all registers
            _asm.Instructions.Add(new RestoreEsp((uint)pStackBackup));
            _asm.Instructions.Add(new PopAll());

            //write code
            //- suspend thread
            onthread.Suspend();
            //- read the context
            ThreadHandler.CONTEXT ctx=onthread.Context;
            //- add a jmp to the original eip
            _asm.Instructions.Add(new JmpRelative((int)ctx.Eip));
            //- code will be written at half the allocated page
            ctx.Eip=((uint)RemotePage+(4096/2));
            //- write code
            _asm.Write(onprocess,(int)ctx.Eip);
            //- set the thread's context (EIP now points to our code)
            onthread.Context=ctx;
            //- resume the thread (our code executes and jumps back to the original eip)
            onthread.Resume();
        }
    }

    //Unmanaged Allocator; by using our own heap we can ensure allocated code has execution permissions (needed for code injection)
    public class Allocator
    {
        public static IntPtr m_Heap;

        static Allocator()
        {
            m_Heap = Imports.HeapCreate(Imports.HeapCreationOptions.HEAP_CREATE_ENABLE_EXECUTE, 0, 0);
        }

        public static UnmanagedBuffer AllocateBuffer(uint size)
        {
            UnmanagedBuffer toreturn = new UnmanagedBuffer(Imports.HeapAlloc(m_Heap, 0, size), (long)size);
            toreturn.FreeOnDestruction = true;
            return toreturn;
        }

        public static IntPtr Allocate(uint size)
        {
            return Imports.HeapAlloc(m_Heap, 0, size);
        }

        public static IntPtr ReAllocate(IntPtr torealloc, uint newsize)
        {
            return Imports.HeapReAlloc(m_Heap, 0, torealloc, newsize);
        }

        public static void Free(IntPtr tofree)
        {
            Imports.HeapFree(m_Heap, 0, tofree);
        }
    }

    //faster then process stream for local unmanaged buffers
    //... but with little error-checks, so be carefull
    //... also, don't use the streamhandler for this stream, 
    //... but use the included generic read/write instead... 
    //... they are slightly faster then the streamhandler's generic fucntions
    public class UnmanagedBuffer : Stream
    {
        private IntPtr m_Address;
        private long m_Position;
        private long m_Size;
        private bool m_SwapByteOrder = false;
        private bool m_AutoFree = false;
        private long m_SwapBreakPoint=0;

        private static T SwapBytes<T>(T toswap)
        {
            T toreturn;

            int size = Marshal.SizeOf(toswap);

            //-> copy to unmanaged memory
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(toswap, unmanagedPointer, true);

            //-> copy unmanaged memory to a byte[] buffer
            byte[] tempbuffer = new byte[size];
            Marshal.Copy(unmanagedPointer, tempbuffer, 0, size);

            //-> swap the byte buffer
            Array.Reverse(tempbuffer);

            //-> copy back to unmanaged
            Marshal.Copy(tempbuffer, 0, unmanagedPointer, size);

            //-> copy to managed, it should now be swapped
            toreturn = (T)Marshal.PtrToStructure(unmanagedPointer, toswap.GetType());

            //free unmanaged memory
            Marshal.FreeHGlobal(unmanagedPointer);

            return toreturn;
        }

        ~UnmanagedBuffer()
        {
            if (m_AutoFree)
                Allocator.Free(m_Address);
        }

        public bool FreeOnDestruction
        {
            get { return m_AutoFree; }
            set { m_AutoFree = value; }
        }

        public UnmanagedBuffer(IntPtr Address)
        {
            m_Address = Address;
            m_Size = 0;//unknown size
            m_Position = 0;
        }
        public UnmanagedBuffer(IntPtr Address, long size)
        {
            m_Address = Address;
            m_Size = size;
            m_Position = 0;
        }

        public bool SwapByteOrder
        {
            get { return m_SwapByteOrder; }
            set { m_SwapByteOrder = value; }
        }
        public IntPtr Address
        {
            get { return m_Address; }
        }

        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanSeek
        {
            get { return true; }
        }
        public override bool CanWrite
        {
            get { return true; }
        }
        public override void Flush()
        {
            return;
        }
        public override long Length
        {
            get {
                if (m_Size == 0)
                    throw new NotImplementedException();
                else
                    return m_Size;
            }
        }
        public override long Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                lock (this)
                {
                    m_Position = value;
                }
            }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                Marshal.Copy((IntPtr)((long)m_Address + m_Position), buffer, offset, count);
                if (m_SwapByteOrder && (m_Position >= m_SwapBreakPoint))
                    Array.Reverse(buffer, offset, count);
                m_Position += count;
                return count;
            }
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (this)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        m_Position = offset;
                        break;
                    case SeekOrigin.Current:
                        m_Position += offset;
                        break;
                    case SeekOrigin.End:
                        m_Position = Length - offset;
                        break;
                    default:
                        break;
                }
                return m_Position;
            }
        }
        public override void SetLength(long value)
        {
            lock (this)
            {
                if(m_AutoFree)
                    m_Address = Allocator.ReAllocate(m_Address, (uint)value);
            }
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                if ((m_SwapByteOrder) && (m_Position >= m_SwapBreakPoint))
                    Array.Reverse(buffer, offset, count);
                Marshal.Copy(buffer, offset, (IntPtr)((long)m_Address + m_Position), count);
                m_Position += count;
            }
        }

        public T Read<T>()
        {
            lock (this)
            {
                T toreturn = (T)Marshal.PtrToStructure((IntPtr)((long)m_Address + m_Position), typeof(T));
                if ((m_SwapByteOrder) && (m_Position >= m_SwapBreakPoint))
                    toreturn = SwapBytes<T>(toreturn);//relatively expensive operation :(
                m_Position += Marshal.SizeOf(toreturn);
                return toreturn;
            }
        }
        public void Write<T>(T towrite)
        {
            lock (this)
            {
                if ((m_SwapByteOrder) && (m_Position >= m_SwapBreakPoint))
                    towrite = SwapBytes<T>(towrite);
                Marshal.StructureToPtr(towrite, (IntPtr)((long)m_Address + m_Position), false);
                m_Position += Marshal.SizeOf(towrite);
            }
        }
        public T ReadAt<T>(long Position)
        {
            lock (this)
            {
                m_Position = Position;
                T toreturn = (T)Marshal.PtrToStructure((IntPtr)((long)m_Address + m_Position), typeof(T));
                if ((m_SwapByteOrder) && (m_Position >= m_SwapBreakPoint))
                    toreturn = SwapBytes<T>(toreturn);
                m_Position += Marshal.SizeOf(toreturn);
                return toreturn;
            }
        }
        public void WriteAt<T>(T towrite, long Position)
        {
            lock (this)
            {
                m_Position = Position;
                if ((m_SwapByteOrder) && (m_Position >= m_SwapBreakPoint))
                    towrite = SwapBytes<T>(towrite);
                Marshal.StructureToPtr(towrite, (IntPtr)((long)m_Address + m_Position), false);
                m_Position += Marshal.SizeOf(towrite);
            }
        }
        public int ReadAt(byte[] buffer, long Position, int offset, int count)
        {
            lock (this)
            {
                m_Position = Position;
                Marshal.Copy((IntPtr)((long)m_Address + m_Position), buffer, offset, count);
                if ((m_SwapByteOrder) && (m_Position >= m_SwapBreakPoint))
                    Array.Reverse(buffer, offset, count);
                m_Position += count;
                return count;
            }
        }
        public void WriteAt(byte[] buffer, long Position, int offset, int count)
        {
            lock (this)
            {
                m_Position = Position;
                if ((m_SwapByteOrder) && (m_Position >= m_SwapBreakPoint))
                    Array.Reverse(buffer, offset, count);
                Marshal.Copy(buffer, offset, (IntPtr)((long)m_Address + m_Position), count);
                m_Position += count;
            }
        }

        public string ReadString()
        {
            lock (this)
            {
                string toreturn = Marshal.PtrToStringAnsi((IntPtr)((long)m_Address + m_Position));
                m_Position += toreturn.Length + 1;
                return toreturn;
            }
        }
        public string ReadStringAt(long Position)
        {
            lock (this)
            {
                m_Position = Position;
                return ReadString();
            }
        }

        public string ReadString(int length)
        {
            lock (this)
            {
                string toreturn = Marshal.PtrToStringAnsi((IntPtr)((long)m_Address + m_Position), length);
                m_Position += length;
                return toreturn;
            }
        }
        public string ReadStringAt(long Position, int length)
        {
            lock (this)
            {
                m_Position = Position;
                return ReadString(length);
            }
        }

        public string ReadUnicodeString()
        {
            lock (this)
            {
                string toreturn = Marshal.PtrToStringUni((IntPtr)((long)m_Address + m_Position));
                m_Position += (toreturn.Length + 1) * 2;
                return toreturn;
            }
        }
        public string ReadUnicodeStringAt(long Position)
        {
            lock (this)
            {
                m_Position = Position;
                return ReadUnicodeString();
            }
        }

        public string ReadUnicodeString(int length)
        {
            lock (this)
            {
                string toreturn = Marshal.PtrToStringUni((IntPtr)((long)m_Address + m_Position), length);
                m_Position += (toreturn.Length + 1) * 2;
                return toreturn;
            }
        }
        public string ReadUnicodeStringAt(long Position, int length)
        {
            lock (this)
            {
                m_Position = Position;
                return ReadUnicodeString(length);
            }
        }

        public string ReadBSTR()
        {
            lock (this)
            {
                string toreturn = Marshal.PtrToStringBSTR((IntPtr)((long)m_Address + m_Position));
                m_Position += toreturn.Length + 1;
                return toreturn;
            }
        }
        public string ReadBSTRAt(long Position)
        {
            lock (this)
            {
                m_Position = Position;
                return ReadBSTR();
            }
        }

        public void WriteString(string towrite)
        {
            lock (this)
            {
                byte[] asciibytes = ASCIIEncoding.ASCII.GetBytes(towrite);
                Write(asciibytes, 0, asciibytes.Length);
                if (asciibytes[asciibytes.Length - 1] != 0)
                    Write<byte>(0);
            }
        }
        public void WriteStringAt(string towrite, long Position)
        {
            lock (this)
            {
                m_Position = Position;
                WriteString(towrite);
            }
        }
        public void WriteString(string towrite, int length)
        {
            lock (this)
            {
                byte[] asciibytes = ASCIIEncoding.ASCII.GetBytes(towrite);
                Write(asciibytes, 0, Math.Min(length,asciibytes.Length));
                for (int i = asciibytes.Length; i < length; i++)
                    Write<byte>(0);
            }
        }
        public void WriteStringAt(string towrite, long Position, int length)
        {
            lock (this)
            {
                m_Position = Position;
                WriteString(towrite, length);
            }
        }

        public void WriteUnicodeString(string towrite)
        {
            lock (this)
            {
                byte[] unicodebytes = UnicodeEncoding.Unicode.GetBytes(towrite);
                Write(unicodebytes, 0, unicodebytes.Length);
                if (!((unicodebytes[unicodebytes.Length - 2] == 0) && (unicodebytes[unicodebytes.Length - 1] == 0)))
                    Write<ushort>(0);
            }
        }
        public void WriteUnicodeStringAt(string towrite, long Position)
        {
            lock (this)
            {
                m_Position = Position;
                WriteUnicodeString(towrite);
            }
        }
        public void WriteUnicodeString(string towrite, int length)
        {
            lock (this)
            {
                byte[] unicodebytes = UnicodeEncoding.Unicode.GetBytes(towrite);
                Write(unicodebytes, 0, Math.Min(unicodebytes.Length, length * 2));
                for(int i=unicodebytes.Length;i<(length*2);i++)
                    Write<byte>(0);
            }
        }
        public void WriteUnicodeStringAt(string towrite, long Position, int length)
        {
            lock (this)
            {
                m_Position = Position;
                WriteUnicodeString(towrite, length);
            }
        }

        public void WriteBSTR(string towrite)
        {
            lock (this)
            {
                Write<uint>((uint)(towrite.Length * 2));
                WriteUnicodeString(towrite);
            }
        }
        public void WriteBSTRAt(string towrite, long Position)
        {
            lock (this)
            {
                m_Position = Position;
                WriteBSTR(towrite);
            }
        }

        public long SwapBreakPoint
        {
            get { return m_SwapBreakPoint; }
            set { m_SwapBreakPoint = value; }
        }
    }

    public class UnmanagedStack
    {
        private UnmanagedBuffer m_stack;
        private uint m_address;

        internal UnmanagedStack(uint address)
        {
            m_address = address;
            m_stack = new UnmanagedBuffer((IntPtr)address);
        }

        public uint this[uint index]
        {
            get
            {
                return m_stack.ReadAt<uint>((long)index * 4);
            }
            set
            {
                m_stack.WriteAt<uint>(value, (long)index * 4);
            }
        }

        public UnmanagedBuffer AsBuffer
        {
            get { return m_stack; }
        }
    }

    //hooks :: only on the local process
    //vtbl, random, call hook
    public class LocalHook : MarshalByRefObject
    {
        private bool m_Hooked = true;
        private uint m_Address = 0;
        private uint original_address = 0;
        private bool m_VtblHook = false;
        private byte[] copied_instructions=null;
        private UnmanagedBuffer m_HookMemory;//base address of the hooks allocated memory
        private UnmanagedBuffer m_EspBackup;//for stack reading purposes
        private InternalHookDelegate m_Delegate;//need to keep this alive while the hook is!
        private InternalHookDelegate m_LateDelegate;//need to keep this alive while the hook is!
        internal uint m_LateHookAddress;
        internal Stack<uint> m_ReturnAddresses=new Stack<uint>();//stack needed to allow reentrant hooks (hook might be called from the original hooked function before we are able to return to the stored address; stacking return address's solves this)

        private static BinaryTree<uint, LocalHook> m_Hooks = new BinaryTree<uint, LocalHook>();//hooks by ret addr : by checking the stack we see what hook we got called from; also keeps hooks alive

        public static LocalHook Hook(uint address, ushort stack_cleanup_size, bool IsVtblEntry)
        {
            if (m_Hooks.ContainsKey(address))
                return m_Hooks[address];
            else
                return new LocalHook(address, stack_cleanup_size, IsVtblEntry);
        }

        public void UnHook()
        {
            if (m_Hooked)
            {
                ProcessHandler curproc = ProcessHandler.CurrentProcess;
                StreamHandler sh = new StreamHandler(curproc);
                if (original_address == 0)
                {
                    sh.Position = m_Address;
                    sh.Write(copied_instructions, 0, copied_instructions.Length);
                }
                else if (m_VtblHook)
                {
                    sh.Position = m_Address;
                    sh.Write<uint>(original_address);
                }
                else
                {
                    CallRelative cr = new CallRelative((int)original_address);
                    cr.Write(sh, (int)m_Address);
                }
                m_Hooks.Remove(m_Address);
                m_Hooked = false;
            }
        }

        internal LocalHook(uint address, ushort stack_cleanup_size, bool IsVtblEntry)
        {
            uint skip_call_address;
            asmInstruction curinsn = null;
            ProcessHandler curprocess = ProcessHandler.CurrentProcess;
            StreamHandler sh = new StreamHandler(curprocess);
            CallRelative crel;
            JmpRelative jrel;
            int readsize=0;

            m_Address = address;
            m_VtblHook = IsVtblEntry;

            //read original target address

            curprocess.Position = address;
            if (IsVtblEntry)
                original_address = sh.Read<uint>();
            else
            {
                curinsn = disassembler.disassemble(curprocess);
                if (curinsn.Instruction.type == x86_insn_type.insn_call)
                    original_address = (uint)curinsn.ReadAddressOperand();
                else
                {
                    original_address = 0;//not hooking a call
                    readsize=curinsn.Instruction.size;
                    while (readsize < 5)
                    {
                        curinsn=disassembler.disassemble(curprocess);
                        readsize += curinsn.Instruction.size;
                    }
                    copied_instructions = new byte[readsize];
                    curprocess.Position = address;
                    curprocess.Read(copied_instructions, 0, readsize);
                }
            }

            //allocate required space (60 bytes)
            m_HookMemory = Allocator.AllocateBuffer((uint)(46 + 32 + readsize));
            m_EspBackup = Allocator.AllocateBuffer(4);
            skip_call_address = (uint)(m_HookMemory.Address.ToInt32() + 35 + readsize);

            //build unmanaged function pointer       
            m_Delegate = new InternalHookDelegate(ActualHook);
            m_LateDelegate = new InternalHookDelegate(ActualLateHook);
            IntPtr unmanaged_hook_pointer=Marshal.GetFunctionPointerForDelegate(m_Delegate);
            IntPtr unmanaged_hook_pointerb = Marshal.GetFunctionPointerForDelegate(m_LateDelegate);

            //build hook code
            AsmBuilder hookcode = new AsmBuilder();
            hookcode.Instructions.Add(new PushAll());
            hookcode.Instructions.Add(new BackupEsp((uint)m_EspBackup.Address.ToInt32()));
            hookcode.Instructions.Add(new PushImmediate(address));
            hookcode.Instructions.Add(new CallRelative(unmanaged_hook_pointer.ToInt32()));
            hookcode.Instructions.Add(new TestEaxEax());
            hookcode.Instructions.Add(new JzRelativeShort((int)skip_call_address));
            hookcode.Instructions.Add(new RestoreEsp((uint)m_EspBackup.Address.ToInt32()));
            hookcode.Instructions.Add(new PopAll());
            //switch vtbl: return, non-call : jmp address+readsize, call, jmp original
            hookcode.Write(curprocess, m_HookMemory.Address.ToInt32());
            if (original_address == 0)
            {
                curprocess.Position = m_HookMemory.Address.ToInt32() + 30;
                curprocess.Write(copied_instructions, 0, readsize);
                hookcode = new AsmBuilder();
                hookcode.Instructions.Add(new JmpRelative((int)address + readsize));
            }
            else
            {
                hookcode = new AsmBuilder();
                hookcode.Instructions.Add(new JmpRelative((int)original_address));
            }
            //end_code
            hookcode.Instructions.Add(new RestoreEsp((uint)m_EspBackup.Address.ToInt32()));
            hookcode.Instructions.Add(new PopAll());
            if (stack_cleanup_size == 0)
                hookcode.Instructions.Add(new Rtn());
            else
                hookcode.Instructions.Add(new RtnStackSize(stack_cleanup_size));
            hookcode.Write(curprocess, (int)m_HookMemory.Address.ToInt32() + 30 + readsize);

            hookcode = new AsmBuilder();
            hookcode.Instructions.Add(new PushImmediate(0));
            hookcode.Instructions.Add(new PushAll());
            hookcode.Instructions.Add(new BackupEsp((uint)m_EspBackup.Address.ToInt32()));
            hookcode.Instructions.Add(new PushImmediate(address));
            hookcode.Instructions.Add(new CallRelative(unmanaged_hook_pointerb.ToInt32()));
            hookcode.Instructions.Add(new RestoreEsp((uint)m_EspBackup.Address.ToInt32()));
            hookcode.Instructions.Add(new PopAll());
            hookcode.Instructions.Add(new Rtn());
            hookcode.Write(curprocess, (int)m_HookMemory.Address.ToInt32() + 46 + readsize);

            m_LateHookAddress = (uint)(m_HookMemory.Address.ToInt32() + 46 + readsize);

            //install hook
            if (original_address != 0)
            {
                if (IsVtblEntry)
                {
                    //vtbl_hook
                    sh.Position = address;
                    sh.Write<int>(m_HookMemory.Address.ToInt32());
                }
                else
                {
                    crel = new CallRelative((int)m_HookMemory.Address.ToInt32());
                    if (crel.Size != curinsn.Instruction.size)
                        throw new Exception("Can only hook call instructions with size equal to " + crel.Size.ToString());
                    crel.Write(curprocess, (int)address);
                    curprocess.Position = address + crel.Size;
                }
            }
            else
            {
                //random hook
                jrel = new JmpRelative((int)m_HookMemory.Address.ToInt32());
                jrel.Write(curprocess, (int)address);
                curprocess.Position = address + jrel.Size;
                for (uint i = 0; i < (readsize - jrel.Size); i++)
                    curprocess.WriteByte(0x90);//NOP
            }

            m_Hooks.Add(address, this);
        }

        ~LocalHook()
        {
            UnHook();
        }

        private delegate bool InternalHookDelegate(uint pAddress);
        private static bool ActualHook(uint pAddress)
        {
            LocalHook curhook;
            UnmanagedBuffer buff;
            UnmanagedContext ctx;
            UnmanagedStack stck;
            uint esp;
            if (m_Hooks.ContainsKey(pAddress))
            {
                curhook = m_Hooks[pAddress];
                esp = curhook.m_EspBackup.ReadAt<uint>(0);
                buff = new UnmanagedBuffer((IntPtr)esp);
                ctx = buff.ReadAt<UnmanagedContext>(0);
                stck = new UnmanagedStack(esp + 40);
                curhook.m_ReturnAddresses.Push(buff.ReadAt<uint>(36));
                buff.WriteAt<uint>(curhook.m_LateHookAddress, 36);
                return curhook.InvokeOnCall(ctx, stck);
            }
            return true;
        }
        private static bool ActualLateHook(uint pAddress)
        {
            LocalHook curhook;
            UnmanagedBuffer buff;
            UnmanagedContext ctx;
            UnmanagedStack stck;
            uint esp;
            if (m_Hooks.ContainsKey(pAddress))
            {
                curhook = m_Hooks[pAddress];
                esp = curhook.m_EspBackup.ReadAt<uint>(0);
                buff = new UnmanagedBuffer((IntPtr)esp);
                ctx = buff.ReadAt<UnmanagedContext>(0);
                stck = new UnmanagedStack(esp + 40);
                buff.WriteAt<uint>(curhook.m_ReturnAddresses.Pop(), 36);
                return curhook.InvokeAfterCall(ctx, stck);
            }
            return true;
        }

        public bool InvokeOnCall(UnmanagedContext ctx, UnmanagedStack stck)
        {
            if (onCall != null)
                return onCall(ctx, stck);
            return true;
        }
        public bool InvokeAfterCall(UnmanagedContext ctx, UnmanagedStack stck)
        {
            if (afterCall != null)
                return afterCall(ctx, stck);
            return true;
        }

        public delegate bool OnCallDelegate(UnmanagedContext ctx, UnmanagedStack stck);
        public event OnCallDelegate onCall;
        public event OnCallDelegate afterCall;
    }
}
