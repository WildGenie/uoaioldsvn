using System;
using Assembler;
using Win32API;
using Tools;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using RemoteObjects;
using System.Reflection;

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
}
