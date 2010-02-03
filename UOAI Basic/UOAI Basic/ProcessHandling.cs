using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Tools;

namespace Win32API
{
    //WINAPI based process, thread and window stuff
    //  32bit only!!!
    //  Carefull when using this on a 64bit platform, 64bit processes are not listed in the process list!
    //  and you shouldn't use the processhandler for 64bit processes... this will be fixed eventually...
    //  but for the purpose of UOAI i only need a 32bit version. Updating means reviewing all import-structures
    //  and functions to ensure correct usage of datatypes... f.e. IntPtr is used whenever i needed a pointer,
    //  but i haven't checked if it's a 64bit pointer for 64bit systems, etc... and other things are going wrong...
    //  in summary it's a lot of work to fix, since it was all written on a 32bit platform.
    //  
    //  -   Threads : suspend/resume, terminate, get/set context
    //  -   Windows : 
    //          - finding windows (static/shared methods)
    //          - linking windows to corresponding thread/process
    //          - closing, sending/posting messages, title, classname, ...
    //  -   Process :
    //          - system.diagnostics based creation, finding/listing
    //          - module listing:
    //              Process.MainModule and Process.MainModule.Modules
    //          - Stream interface to the process's memory
    //          - Terminate, Suspend/Resume, CreateRemoteThread, ...?
    //  -   Module
    //          - PEHeader information
    //              -> imports
    //                  -> hookable (intptr address can be changed)
    //              -> exports
    //          - loaded module list (only for process modules)

    //Threads... hooking, context reading, etc.
    public class ThreadHandler
    {
        private ProcessThread m_Thread;
        private IntPtr m_ThreadHandle;

        public ThreadHandler(ProcessThread fromthread)
        {
            m_Thread = fromthread;
            m_ThreadHandle=Imports.OpenThread(Imports.ThreadAccess.GET_CONTEXT | Imports.ThreadAccess.SET_CONTEXT | Imports.ThreadAccess.SUSPEND_RESUME | Imports.ThreadAccess.TERMINATE, false, (uint)fromthread.Id);
        }

        public ProcessThread Thread { get { return m_Thread; } }
        public void Suspend()
        {
            Imports.SuspendThread(m_ThreadHandle);
        }
        public void Resume()
        {
            Imports.ResumeThread(m_ThreadHandle);
        }
        public void Terminate()
        {
            Imports.TerminateThread(m_ThreadHandle, 0);
        }
        public void Terminate(uint exitcode)
        {
            Imports.TerminateThread(m_ThreadHandle, exitcode);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CONTEXT
        {
            public CONTEXT_FLAGS ContextFlags;
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            public uint ControlWord;
            public uint StatusWord;
            public uint TagWord;
            public uint ErrorOffset;
            public uint ErrorSelector;
            public uint DataOffset;
            public uint DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] RegisterArea;//80 bytes
            public uint Cr0NpxState;
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            public uint Ebp;
            public uint Eip;
            public uint SegCs;              // MUST BE SANITIZED
            public uint EFlags;             // MUST BE SANITIZED
            public uint Esp;
            public uint SegSs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] ExtendedRegisters;//512 bytes
        }

        [Flags()]
        public enum CONTEXT_FLAGS : uint
        {
            CONTEXT_i386 = 0x10000,
            CONTEXT_i486 = 0x10000,   //  same as i386
            CONTEXT_CONTROL = CONTEXT_i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
            CONTEXT_INTEGER = CONTEXT_i386 | 0x02, // AX, BX, CX, DX, SI, DI
            CONTEXT_SEGMENTS = CONTEXT_i386 | 0x04, // DS, ES, FS, GS
            CONTEXT_FLOATING_POINT = CONTEXT_i386 | 0x08, // 387 state
            CONTEXT_DEBUG_REGISTERS = CONTEXT_i386 | 0x10, // DB 0-3,6,7
            CONTEXT_EXTENDED_REGISTERS = CONTEXT_i386 | 0x20, // cpu specific extensions
            CONTEXT_FULL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS,
            CONTEXT_ALL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS
        }

        public CONTEXT Context
        {
            get
            {
                CONTEXT toreturn = new CONTEXT();
                toreturn.ContextFlags = CONTEXT_FLAGS.CONTEXT_ALL;
                Imports.GetThreadContext(m_ThreadHandle, ref toreturn);
                return toreturn;
            }
            set
            {
                Imports.SetThreadContext(m_ThreadHandle, ref value);
            }
        }

        ~ThreadHandler()
        {
            Imports.CloseHandle((uint)m_ThreadHandle);
        }

        public override string ToString()
        {
            return m_Thread.Id.ToString();
        }
    }

    //To find Windows and communicate with them
    public class WindowHandler
    {
        public enum Messages : uint
        {
            WM_NULL = 0x00,
            WM_CREATE = 0x01,
            WM_DESTROY = 0x02,
            WM_MOVE = 0x03,
            WM_SIZE = 0x05,
            WM_ACTIVATE = 0x06,
            WM_SETFOCUS = 0x07,
            WM_KILLFOCUS = 0x08,
            WM_ENABLE = 0x0A,
            WM_SETREDRAW = 0x0B,
            WM_SETTEXT = 0x0C,
            WM_GETTEXT = 0x0D,
            WM_GETTEXTLENGTH = 0x0E,
            WM_PAINT = 0x0F,
            WM_CLOSE = 0x10,
            WM_QUERYENDSESSION = 0x11,
            WM_QUIT = 0x12,
            WM_QUERYOPEN = 0x13,
            WM_ERASEBKGND = 0x14,
            WM_SYSCOLORCHANGE = 0x15,
            WM_ENDSESSION = 0x16,
            WM_SYSTEMERROR = 0x17,
            WM_SHOWWINDOW = 0x18,
            WM_CTLCOLOR = 0x19,
            WM_WININICHANGE = 0x1A,
            WM_SETTINGCHANGE = 0x1A,
            WM_DEVMODECHANGE = 0x1B,
            WM_ACTIVATEAPP = 0x1C,
            WM_FONTCHANGE = 0x1D,
            WM_TIMECHANGE = 0x1E,
            WM_CANCELMODE = 0x1F,
            WM_SETCURSOR = 0x20,
            WM_MOUSEACTIVATE = 0x21,
            WM_CHILDACTIVATE = 0x22,
            WM_QUEUESYNC = 0x23,
            WM_GETMINMAXINFO = 0x24,
            WM_PAINTICON = 0x26,
            WM_ICONERASEBKGND = 0x27,
            WM_NEXTDLGCTL = 0x28,
            WM_SPOOLERSTATUS = 0x2A,
            WM_DRAWITEM = 0x2B,
            WM_MEASUREITEM = 0x2C,
            WM_DELETEITEM = 0x2D,
            WM_VKEYTOITEM = 0x2E,
            WM_CHARTOITEM = 0x2F,
            WM_SETFONT = 0x30,
            WM_GETFONT = 0x31,
            WM_SETHOTKEY = 0x32,
            WM_GETHOTKEY = 0x33,
            WM_QUERYDRAGICON = 0x37,
            WM_COMPAREITEM = 0x39,
            WM_COMPACTING = 0x41,
            WM_WINDOWPOSCHANGING = 0x46,
            WM_WINDOWPOSCHANGED = 0x47,
            WM_POWER = 0x48,
            WM_COPYDATA = 0x4A,
            WM_CANCELJOURNAL = 0x4B,
            WM_NOTIFY = 0x4E,
            WM_INPUTLANGCHANGEREQUEST = 0x50,
            WM_INPUTLANGCHANGE = 0x51,
            WM_TCARD = 0x52,
            WM_HELP = 0x53,
            WM_USERCHANGED = 0x54,
            WM_NOTIFYFORMAT = 0x55,
            WM_CONTEXTMENU = 0x7B,
            WM_STYLECHANGING = 0x7C,
            WM_STYLECHANGED = 0x7D,
            WM_DISPLAYCHANGE = 0x7E,
            WM_GETICON = 0x7F,
            WM_SETICON = 0x80,
            WM_NCCREATE = 0x81,
            WM_NCDESTROY = 0x82,
            WM_NCCALCSIZE = 0x83,
            WM_NCHITTEST = 0x84,
            WM_NCPAINT = 0x85,
            WM_NCACTIVATE = 0x86,
            WM_GETDLGCODE = 0x87,
            WM_NCMOUSEMOVE = 0xA0,
            WM_NCLBUTTONDOWN = 0xA1,
            WM_NCLBUTTONUP = 0xA2,
            WM_NCLBUTTONDBLCLK = 0xA3,
            WM_NCRBUTTONDOWN = 0xA4,
            WM_NCRBUTTONUP = 0xA5,
            WM_NCRBUTTONDBLCLK = 0xA6,
            WM_NCMBUTTONDOWN = 0xA7,
            WM_NCMBUTTONUP = 0xA8,
            WM_NCMBUTTONDBLCLK = 0xA9,
            WM_KEYFIRST = 0x100,
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WM_CHAR = 0x102,
            WM_DEADCHAR = 0x103,
            WM_SYSKEYDOWN = 0x104,
            WM_SYSKEYUP = 0x105,
            WM_SYSCHAR = 0x106,
            WM_SYSDEADCHAR = 0x107,
            WM_KEYLAST = 0x108,
            WM_IME_STARTCOMPOSITION = 0x10D,
            WM_IME_ENDCOMPOSITION = 0x10E,
            WM_IME_COMPOSITION = 0x10F,
            WM_IME_KEYLAST = 0x10F,
            WM_INITDIALOG = 0x110,
            WM_COMMAND = 0x111,
            WM_SYSCOMMAND = 0x112,
            WM_TIMER = 0x113,
            WM_HSCROLL = 0x114,
            WM_VSCROLL = 0x115,
            WM_INITMENU = 0x116,
            WM_INITMENUPOPUP = 0x117,
            WM_MENUSELECT = 0x11F,
            WM_MENUCHAR = 0x120,
            WM_ENTERIDLE = 0x121,
            WM_CTLCOLORMSGBOX = 0x132,
            WM_CTLCOLOREDIT = 0x133,
            WM_CTLCOLORLISTBOX = 0x134,
            WM_CTLCOLORBTN = 0x135,
            WM_CTLCOLORDLG = 0x136,
            WM_CTLCOLORSCROLLBAR = 0x137,
            WM_CTLCOLORSTATIC = 0x138,
            WM_MOUSEFIRST = 0x200,
            WM_MOUSEMOVE = 0x200,
            WM_LBUTTONDOWN = 0x201,
            WM_LBUTTONUP = 0x202,
            WM_LBUTTONDBLCLK = 0x203,
            WM_RBUTTONDOWN = 0x204,
            WM_RBUTTONUP = 0x205,
            WM_RBUTTONDBLCLK = 0x206,
            WM_MBUTTONDOWN = 0x207,
            WM_MBUTTONUP = 0x208,
            WM_MBUTTONDBLCLK = 0x209,
            WM_MOUSEWHEEL = 0x20A,
            WM_MOUSEHWHEEL = 0x20E,
            WM_PARENTNOTIFY = 0x210,
            WM_ENTERMENULOOP = 0x211,
            WM_EXITMENULOOP = 0x212,
            WM_NEXTMENU = 0x213,
            WM_SIZING = 0x214,
            WM_CAPTURECHANGED = 0x215,
            WM_MOVING = 0x216,
            WM_POWERBROADCAST = 0x218,
            WM_DEVICECHANGE = 0x219,
            WM_MDICREATE = 0x220,
            WM_MDIDESTROY = 0x221,
            WM_MDIACTIVATE = 0x222,
            WM_MDIRESTORE = 0x223,
            WM_MDINEXT = 0x224,
            WM_MDIMAXIMIZE = 0x225,
            WM_MDITILE = 0x226,
            WM_MDICASCADE = 0x227,
            WM_MDIICONARRANGE = 0x228,
            WM_MDIGETACTIVE = 0x229,
            WM_MDISETMENU = 0x230,
            WM_ENTERSIZEMOVE = 0x231,
            WM_EXITSIZEMOVE = 0x232,
            WM_DROPFILES = 0x233,
            WM_MDIREFRESHMENU = 0x234,
            WM_IME_SETCONTEXT = 0x281,
            WM_IME_NOTIFY = 0x282,
            WM_IME_CONTROL = 0x283,
            WM_IME_COMPOSITIONFULL = 0x284,
            WM_IME_SELECT = 0x285,
            WM_IME_CHAR = 0x286,
            WM_IME_KEYDOWN = 0x290,
            WM_IME_KEYUP = 0x291,
            WM_MOUSEHOVER = 0x2A1,
            WM_NCMOUSELEAVE = 0x2A2,
            WM_MOUSELEAVE = 0x2A3,
            WM_CUT = 0x300,
            WM_COPY = 0x301,
            WM_PASTE = 0x302,
            WM_CLEAR = 0x303,
            WM_UNDO = 0x304,
            WM_RENDERFORMAT = 0x305,
            WM_RENDERALLFORMATS = 0x306,
            WM_DESTROYCLIPBOARD = 0x307,
            WM_DRAWCLIPBOARD = 0x308,
            WM_PAINTCLIPBOARD = 0x309,
            WM_VSCROLLCLIPBOARD = 0x30A,
            WM_SIZECLIPBOARD = 0x30B,
            WM_ASKCBFORMATNAME = 0x30C,
            WM_CHANGECBCHAIN = 0x30D,
            WM_HSCROLLCLIPBOARD = 0x30E,
            WM_QUERYNEWPALETTE = 0x30F,
            WM_PALETTEISCHANGING = 0x310,
            WM_PALETTECHANGED = 0x311,
            WM_HOTKEY = 0x312,
            WM_PRINT = 0x317,
            WM_PRINTCLIENT = 0x318,
            WM_HANDHELDFIRST = 0x358,
            WM_HANDHELDLAST = 0x35F,
            WM_PENWINFIRST = 0x380,
            WM_PENWINLAST = 0x38F,
            WM_COALESCE_FIRST = 0x390,
            WM_COALESCE_LAST = 0x39F,
            WM_DDE_FIRST = 0x3E0,
            WM_DDE_INITIATE = 0x3E0,
            WM_DDE_TERMINATE = 0x3E1,
            WM_DDE_ADVISE = 0x3E2,
            WM_DDE_UNADVISE = 0x3E3,
            WM_DDE_ACK = 0x3E4,
            WM_DDE_DATA = 0x3E5,
            WM_DDE_REQUEST = 0x3E6,
            WM_DDE_POKE = 0x3E7,
            WM_DDE_EXECUTE = 0x3E8,
            WM_DDE_LAST = 0x3E8,
            WM_USER = 0x400,
            WM_APP = 0x8000
        }

        private uint m_hwnd;//window handle
        private uint m_pid;//process id of process that created this window
        private uint m_tid;//thread id of thread from which it was created
        private static List<WindowHandler> m_toreturn;
        
        private static string classname_tocheck;
        private static int pid_tocheck;
        private static int tid_tocheck;

        public WindowHandler(uint hwnd)
        {
            m_hwnd = hwnd;
            m_tid = Imports.GetWindowThreadProcessId(hwnd, out m_pid);
        }
        public WindowHandler(IntPtr hwnd)
        {
            m_hwnd = (uint)hwnd;
            m_tid = Imports.GetWindowThreadProcessId((uint)hwnd, out m_pid);
        }
        public WindowHandler(uint hWnd, uint pid, uint tid)
        {
            m_hwnd = hWnd;
            m_pid = pid;
            m_tid = tid;
        }

        public ProcessHandler onProcess { get { return ProcessHandler.GetProcessById((int)m_pid); } }
        public ThreadHandler onThread
        {
            get
            {
                foreach (ProcessThread pt in Process.GetProcessById((int)m_pid).Threads)
                {
                    if (pt.Id == m_tid)
                        return new ThreadHandler(pt);
                }
                return null;
            }
        }
        public string ClassName
        {
            get
            {
                StringBuilder newstring = new StringBuilder(256);
                Imports.GetClassName(m_hwnd, newstring, 256);
                return newstring.ToString();
            }
        }
        public string Text
        {
            get
            {
                StringBuilder newstring=new StringBuilder(256);
                Imports.GetWindowText(m_hwnd, newstring, 256);
                return newstring.ToString();
            }
            set
            {
                Imports.SetWindowText(m_hwnd, value);
            }
        }

        public uint SendMessage(uint msg, uint wParam, uint lParam)
        {
            return Imports.SendMessage(m_hwnd, msg, wParam, lParam);
        }
        public uint SendMessage(WindowHandler.Messages msg, uint wParam, uint lParam)
        {
            return Imports.SendMessage(m_hwnd, (uint)msg, wParam, lParam);
        }
        public void PostMessage(uint msg, uint wParam, uint lParam)
        {
            Imports.PostMessage(m_hwnd, msg, wParam, lParam);
        }
        public void PostMessage(WindowHandler.Messages msg, uint wParam, uint lParam)
        {
            Imports.PostMessage(m_hwnd, (uint)msg, wParam, lParam);
        }

        public void Close()
        {
            PostMessage(Messages.WM_CLOSE, 0, 0);
        }

        public static List<WindowHandler> Windows
        {
            get { return FindWindow(null, -1, -1); }
        }

        public static List<WindowHandler> FindWindow(string classname, int pid, int tid)
        {
            List<WindowHandler> toreturn = new List<WindowHandler>();
            m_toreturn = toreturn;
            classname_tocheck = classname;
            pid_tocheck = pid;
            tid_tocheck = tid;
            Imports.EnumWindows(new Imports.EnumWindowsProc(CheckWindow), 0);
            m_toreturn = null;
            classname_tocheck = null;
            pid = -1;
            tid = -1;
            return toreturn;
        }

        private static bool CheckWindow(UInt32 hWnd, UInt32 lParam)
        {
            UInt32 pid, tid;
            
            if (classname_tocheck != null)
            {
                StringBuilder wname = new StringBuilder(256);
                if (Imports.GetClassName(hWnd, wname, 256)==false)
                    return true;
                if (string.Compare(wname.ToString(), classname_tocheck) != 0)
                    return true;
            }

            tid = Imports.GetWindowThreadProcessId(hWnd, out pid);

            if (((tid_tocheck != -1) && (tid != tid_tocheck)) || ((pid_tocheck != -1) && (pid != pid_tocheck)))
                return true;

            m_toreturn.Add(new WindowHandler(hWnd,pid,tid));
            
            return true;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public struct ExportedSymbol
    {
        public string Name;
        public uint Ordinal;
        public uint Address;

        public ExportedSymbol(string name, uint ordinal, uint address)
        {
            Name = name;
            Ordinal = ordinal;
            Address = address;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ImportedLibrary
    {
        private string m_Name;
        private Dictionary<string, ImportedSymbol> m_ByName;

        public Dictionary<string, ImportedSymbol> ImportedSymbols
        {
            get { return m_ByName; }
        }

        public string Name { get { return m_Name; } }

        public override string ToString()
        {
            return Name;
        }

        internal ImportedLibrary(string name, Dictionary<string, ImportedSymbol> syms)
        {
            m_ByName = syms;
            m_Name = name;
        }
    }

    public class ImportedSymbol
    {
        private string m_Name;
        private ushort m_Ordinal;
        private UInt32 m_Address;
        private uint m_IATAddress;
        private StreamHandler m_Stream;
        private bool IsX86;

        public override string ToString()
        {
            return m_Name;
        }

        internal ImportedSymbol(string name, ushort ordinal, UInt32 address, uint IAT_Address, StreamHandler processstream, bool x86)
        {
            m_Name = name;
            m_Ordinal = ordinal;
            m_Address = address;
            m_IATAddress = IAT_Address;
            m_Stream = processstream;
            IsX86 = x86;
        }

        public IntPtr testval()
        {
            //IntPtr loadedlib = Imports.LoadLibrary("Advapi32.dll");
            return (IntPtr)m_IATAddress;// Imports.GetProcAddress(loadedlib, "RegQueryValueExW");
        }

        public string Name { get { return m_Name; } }
        public ushort Ordinal { get { return m_Ordinal; } }
        public UInt32 Address 
        { 
            get { 
                return m_Address; 
            }
            set
            {
                m_Stream.Position = m_IATAddress;
                if(IsX86)                    
                    m_Stream.Write<uint>((uint)value);
                else
                    m_Stream.Write<UInt64>((UInt64)value);                    
                m_Address = value;
            }
        }
    }

    public class ModuleHandler
    {      
        private ProcessHandler m_Process;
        private Imports.MODULEENTRY32 m_ME;
        private Dictionary<string, ModuleHandler> m_Modules;

        private PEHeaders m_PEHeaders;

        public PEHeaders PEHeader 
        { 
            get 
            {
                if(m_PEHeaders==null)
                    m_PEHeaders = new PEHeaders(m_Process, m_ME.modBaseAddr);
                return m_PEHeaders; 
            } 
        }
        public Dictionary<string,ModuleHandler> Modules { get { return m_Modules; } }

        public override string ToString()
        {
            return Name;
        }

        private void BuildModule()
        {
            Imports.MODULEENTRY32 me=new Imports.MODULEENTRY32();
            m_Modules=new Dictionary<string, ModuleHandler>();
            IntPtr hModule = Imports.CreateToolhelp32Snapshot(Imports.CreateToolhelp32SnapshotFlags.TH32CS_SNAPMODULE, m_Process.PID);
            me.dwSize = (uint)Marshal.SizeOf(me);
            if (hModule != IntPtr.Zero)
            {
                if (Imports.Module32First(hModule, ref me))
                {
                    m_ME = me;
                    me = new Imports.MODULEENTRY32();
                    me.dwSize = (uint)Marshal.SizeOf(me);
                    while (Imports.Module32Next(hModule, ref me))
                    {
                        m_Modules.Add(me.szModule, new ModuleHandler(me, m_Process));
                        me = new Imports.MODULEENTRY32();
                        me.dwSize = (uint)Marshal.SizeOf(me);
                    }
                    Imports.CloseHandle((uint)hModule);
                    return;
                }
                else
                    Imports.CloseHandle((uint)hModule);
            }
            throw new Exception("Failed to open first module on this process (PID=" + m_Process.PID.ToString() + ") (Are you trying to access a 64bit process from 32bit?!??)");
        }

        internal ModuleHandler(Imports.MODULEENTRY32 me, ProcessHandler onprocess)
        {
            m_ME = me;
            m_Process = onprocess;
            m_Modules = new Dictionary<string, ModuleHandler>(0);
            m_PEHeaders = new PEHeaders(m_Process, m_ME.modBaseAddr);//not delayed, as for some processes we might fail to parse the PE headers, we want to exclude those from our process-list, by catching the exception(s) thrown during PE-parsing
        }

        internal ModuleHandler(ProcessHandler onprocess)//from pid
        {
            m_Process = onprocess;
            BuildModule();
            //m_PEHeaders = new PEHeaders(m_Process, m_ME.modBaseAddr);//delayed cause parsing all pe headers is not required until you need them
        }

        ~ModuleHandler()
        {
        }

        public uint BaseAddress { get { return m_ME.modBaseAddr ; } }
        public uint EntryPointAddress { get { return BaseAddress+m_PEHeaders.OptionalHeader.AddressOfEntryPoint; } }
        public string Name { get { return m_ME.szModule; } }
        public string FileName { get { return m_ME.szExePath; } }
    }

    public class ProcessHandler : Stream
    {
        private uint m_PID;        
        private Process m_Process;
        private uint m_Handle;
        private long m_CurrentPosition = 0;
        private ModuleHandler m_MainModule;

        public ModuleHandler MainModule { 
            get { 
                return m_MainModule; 
            } 
        }

        public override string ToString()
        {
            return this.MainModule.Name;
        }

        private static List<ProcessHandler> BuildProcessList()
        {
            List<ProcessHandler> toreturn = new List<ProcessHandler>();
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    ProcessHandler ph = new ProcessHandler(p);
                    toreturn.Add(new ProcessHandler(p));
                }
                catch
                {
                    //we drop 64bit processes
                }
            }
            return toreturn;
        }

        public static List<ProcessHandler> Processes
        {
            get
            {
                return BuildProcessList();
            }
        }

        public static ProcessHandler CurrentProcess { get { return new ProcessHandler(Process.GetCurrentProcess()); } }
        public static ProcessHandler GetProcessById(int pid)
        {
            return new ProcessHandler(Process.GetProcessById(pid));
        }

        ~ProcessHandler()
        {
            Imports.CloseHandle(m_Handle);
        }

        public ProcessHandler(Process ofprocess)
        {
            m_Process = ofprocess;
            m_PID = (uint)ofprocess.Id;
            if (m_Process.Id == Process.GetCurrentProcess().Id)
                m_Handle = 0;
            else
            {
                m_Handle = Imports.OpenProcess(Imports.ProcessAccess.VMRead | Imports.ProcessAccess.VMWrite | Imports.ProcessAccess.QueryInformation | Imports.ProcessAccess.CreateThread | Imports.ProcessAccess.VMOperation, false, PID);
                if (m_Handle == 0)
                    throw new Exception("Failed to open a Client Process (pid = " + PID.ToString() + ")\nTry running this application with Administrator privileges!");
            }
            m_CurrentPosition = 0;
            m_MainModule = new ModuleHandler(this);
        }

        public uint PID { get { return m_PID; } }
        public uint Handle { get { return m_Handle; } }
        public Process Process { get { return m_Process; } }

        public bool IsRunning
        {
            get
            {
                UInt32 exitcode;

                if (Handle != 0)
                {
                    if (Imports.GetExitCodeProcess(Handle, out exitcode))
                    {
                        if (exitcode == 259)//STILL_ACTIVE
                            return true;
                    }
                }

                return false;
            }
        }

        public WindowHandler MainWindow { get { return new WindowHandler(m_Process.MainWindowHandle); } }

        public List<WindowHandler> Windows { get { return WindowHandler.FindWindow(null, (int)m_PID, -1); } }
        public List<ThreadHandler> Threads
        {
            get
            {
                List<ThreadHandler> toreturn = new List<ThreadHandler>();
                foreach (ProcessThread pt in m_Process.Threads)
                    toreturn.Add(new ThreadHandler(pt));
                return toreturn;
            }
        }

        public void Suspend()
        {
            foreach (ThreadHandler th in Threads)
                th.Suspend();
        }
        public void Resume()
        {
            foreach (ThreadHandler th in Threads)
                th.Resume();
        }
        public void Terminate()
        {
            Imports.TerminateProcess((IntPtr)m_Handle, 0);
        }
        public void Terminate(uint exitcode)
        {
            Imports.TerminateProcess((IntPtr)m_Handle, exitcode);
        }

        public ThreadHandler CreateThread(uint startaddress, uint parameter, bool suspended)
        {
            uint tid;
            IntPtr threadhandle = Imports.CreateRemoteThread((IntPtr)m_Handle, IntPtr.Zero, 0, startaddress, (IntPtr)parameter, suspended ? Imports.ThreadCreationFlags.CREATE_SUSPENDED : Imports.ThreadCreationFlags.RUN_IMMEDIATELY, out tid);
            Imports.CloseHandle((uint)threadhandle);
            foreach (ProcessThread pt in m_Process.Threads)
            {
                if (pt.Id == tid)
                    return new ThreadHandler(pt);
            }
            return null;
        }

        //allocate/deallocate
        public IntPtr Allocate(uint size)
        {
            if (m_Handle != 0)
                return Imports.VirtualAllocEx((IntPtr)m_Handle, IntPtr.Zero, size, Imports.AllocationType.Commit | Imports.AllocationType.Reserve, Imports.MemoryProtection.EXECUTE_READWRITE);
            else
                return Imports.VirtualAlloc(IntPtr.Zero, size, Imports.AllocationType.Commit | Imports.AllocationType.Reserve, Imports.MemoryProtection.EXECUTE_READWRITE);
        }

        public void Free(IntPtr tofree)
        {
            if (m_Handle != 0)
                Imports.VirtualFreeEx((IntPtr)m_Handle, tofree, 0, Imports.AllocationType.Release);
            else
                Imports.VirtualFree(tofree, 0, Imports.AllocationType.Release);
        }

        //stream implementation, usefull mainly as process-stream on the processhandler

        private byte[] _Read(long address, int bytecount)
        {
            byte[] toreturn = null;
            UInt32 prevprotect;
            UInt32 bytesread = 0;

            if (m_Handle != 0)
            {
                try
                {
                    if (Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, Imports.PAGE_READWRITE, out prevprotect))
                    {
                        toreturn = new byte[bytecount];

                        Imports.ReadProcessMemory(m_Handle, (uint)address, toreturn, (uint)bytecount, out bytesread);

                        Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, prevprotect, out prevprotect);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to read from the remote process!", e);
                }

                if (bytesread == 0)
                    return null;
                else if (bytesread < bytecount)
                    Array.Resize<byte>(ref toreturn, (int)bytesread);

                m_CurrentPosition += toreturn.Length;
            }
            else
            {
                bytesread=0;
                toreturn = new byte[bytecount];
                try
                {
                    if (Imports.VirtualProtect((IntPtr)address, (uint)bytecount, Imports.PAGE_READWRITE, out prevprotect))
                    {
                        Marshal.Copy((IntPtr)m_CurrentPosition, toreturn, 0, bytecount);
                        
                        Imports.VirtualProtect((IntPtr)address, (uint)bytecount, prevprotect, out prevprotect);
                        
                        bytesread = (uint)bytecount;
                        m_CurrentPosition += bytecount;
                    }
                }
                catch
                {
                    bytesread = 0;
                }
                if (bytesread == 0)
                    return null;
                else if (bytesread < bytecount)
                    Array.Resize<byte>(ref toreturn, (int)bytesread);
            }

            return toreturn;
        }
        private bool _Write(long address, byte[] towrite)
        {
            UInt32 prevprotect;
            UInt32 byteswritten;
            UInt32 bytecount = (UInt32)towrite.Length;

            if (m_Handle != 0)
            {
                try
                {
                    if (Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, Imports.PAGE_READWRITE, out prevprotect))
                    {
                        if (Imports.WriteProcessMemory(m_Handle, (uint)address, towrite, (uint)bytecount, out byteswritten))
                        {
                            m_CurrentPosition += byteswritten;
                            if (byteswritten == bytecount)
                            {
                                Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, prevprotect, out prevprotect);
                                return true;
                            }
                        }

                        Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, prevprotect, out prevprotect);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to write to the remote process!", e);
                }
            }
            else
            {
                try
                {
                    if (Imports.VirtualProtect((IntPtr)address, (uint)bytecount, Imports.PAGE_READWRITE, out prevprotect))
                    {
                        Marshal.Copy(towrite, 0, (IntPtr)address, (int)bytecount);
                        Imports.VirtualProtect((IntPtr)address, (uint)bytecount, prevprotect, out prevprotect);
                        m_CurrentPosition += bytecount;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to write to the unmanged memory of this process!", e);
                }
            }

            return false;
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
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                return m_CurrentPosition;
            }
            set
            {
                m_CurrentPosition = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] readbytes = _Read(m_CurrentPosition, count);
            if (readbytes != null)
            {
                readbytes.CopyTo(buffer, offset);
                return readbytes.Length;
            }
            else
                return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                m_CurrentPosition = offset;
            else if (origin == SeekOrigin.Current)
                m_CurrentPosition += offset;
            else
                throw new Exception("SeekOrigin.End not allowed for process streams!");

            return m_CurrentPosition;

        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] m_Temp = new byte[count];
            Buffer.BlockCopy(buffer, offset, m_Temp, 0, count);
            _Write(m_CurrentPosition, m_Temp);
        }
    }

    public class PEHeaders
    {
        #region PE_STUFF
        public enum PESignatures : ushort
        {
            IMAGE_DOS_SIGNATURE = 0x5A4D,     // MZ
            IMAGE_OS2_SIGNATURE = 0x454E,     // NE
            IMAGE_OS2_SIGNATURE_LE = 0x454C,  // LE
            IMAGE_VXD_SIGNATURE = 0x454C,     // LE
            IMAGE_NT_SIGNATURE = 0x00004550,  // PE00
        }
        public enum Characteristics : ushort
        {
            IMAGE_FILE_RELOCS_STRIPPED = 0x0001,  // Relocation info stripped from file.
            IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002,  // File is executable  (i.e. no unresolved externel references).
            IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004,  // Line nunbers stripped from file.
            IMAGE_FILE_LOCAL_SYMS_STRIPPED = 0x0008,  // Local symbols stripped from file.
            IMAGE_FILE_AGGRESIVE_WS_TRIM = 0x0010,  // Agressively trim working set
            IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020,  // App can handle >2gb addresses
            IMAGE_FILE_BYTES_REVERSED_LO = 0x0080,  // Bytes of machine word are reversed.
            IMAGE_FILE_32BIT_MACHINE = 0x0100,  // 32 bit word machine.
            IMAGE_FILE_DEBUG_STRIPPED = 0x0200,  // Debugging info stripped from file in .DBG file
            IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = 0x0400,  // If Image is on removable media, copy and run from the swap file.
            IMAGE_FILE_NET_RUN_FROM_SWAP = 0x0800,  // If Image is on Net, copy and run from the swap file.
            IMAGE_FILE_SYSTEM = 0x1000,  // System File.
            IMAGE_FILE_DLL = 0x2000,  // File is a DLL.
            IMAGE_FILE_UP_SYSTEM_ONLY = 0x4000,  // File should only be run on a UP machine
            IMAGE_FILE_BYTES_REVERSED_HI = 0x8000  // Bytes of machine word are reversed.
        }
        public enum Machine : ushort
        {
            IMAGE_FILE_MACHINE_UNKNOWN = 0,
            IMAGE_FILE_MACHINE_I386 = 0x014c,  // Intel 386.
            IMAGE_FILE_MACHINE_R3000 = 0x0162,  // MIPS little-endian, 0x160 big-endian
            IMAGE_FILE_MACHINE_R4000 = 0x0166,  // MIPS little-endian
            IMAGE_FILE_MACHINE_R10000 = 0x0168,  // MIPS little-endian
            IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x0169,  // MIPS little-endian WCE v2
            IMAGE_FILE_MACHINE_ALPHA = 0x0184,  // Alpha_AXP
            IMAGE_FILE_MACHINE_POWERPC = 0x01F0,  // IBM PowerPC Little-Endian
            IMAGE_FILE_MACHINE_SH3 = 0x01a2,  // SH3 little-endian
            IMAGE_FILE_MACHINE_SH3E = 0x01a4,  // SH3E little-endian
            IMAGE_FILE_MACHINE_SH4 = 0x01a6,  // SH4 little-endian
            IMAGE_FILE_MACHINE_ARM = 0x01c0,  // ARM Little-Endian
            IMAGE_FILE_MACHINE_THUMB = 0x01c2,
            IMAGE_FILE_MACHINE_IA64 = 0x0200,  // Intel 64
            IMAGE_FILE_MACHINE_MIPS16 = 0x0266,  // MIPS
            IMAGE_FILE_MACHINE_MIPSFPU = 0x0366,  // MIPS
            IMAGE_FILE_MACHINE_MIPSFPU16 = 0x0466,  // MIPS
            IMAGE_FILE_MACHINE_ALPHA64 = 0x0284,  // ALPHA64
            IMAGE_FILE_MACHINE_AXP64 = IMAGE_FILE_MACHINE_ALPHA64
        }
        public enum MagicValues : ushort
        {
            IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
            IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b,
            IMAGE_ROM_OPTIONAL_HDR_MAGIC = 0x107
        }
        public enum SectionCharacteristics : uint
        {
            // Section characteristics.
            //
            //      IMAGE_SCN_TYPE_REG                   0x00000000  // Reserved.
            //      IMAGE_SCN_TYPE_DSECT                 0x00000001  // Reserved.
            //      IMAGE_SCN_TYPE_NOLOAD                0x00000002  // Reserved.
            //      IMAGE_SCN_TYPE_GROUP                 0x00000004  // Reserved.
            IMAGE_SCN_TYPE_NO_PAD = 0x00000008,  // Reserved.
            //      IMAGE_SCN_TYPE_COPY                  0x00000010  // Reserved.

            IMAGE_SCN_CNT_CODE = 0x00000020,  // Section contains code.
            IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040, // Section contains initialized data.
            IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080,  // Section contains uninitialized data.

            IMAGE_SCN_LNK_OTHER = 0x00000100,  // Reserved.
            IMAGE_SCN_LNK_INFO = 0x00000200, // Section contains comments or some other type of information.
            //      IMAGE_SCN_TYPE_OVER                  0x00000400  // Reserved.
            IMAGE_SCN_LNK_REMOVE = 0x00000800,  // Section contents will not become part of image.
            IMAGE_SCN_LNK_COMDAT = 0x00001000, // Section contents comdat.
            //                                           0x00002000  // Reserved.
            //      IMAGE_SCN_MEM_PROTECTED - Obsolete   0x00004000
            IMAGE_SCN_NO_DEFER_SPEC_EXC = 0x00004000, // Reset speculative exceptions handling bits in the TLB entries for this section.
            IMAGE_SCN_GPREL = 0x00008000, // Section content can be accessed relative to GP
            IMAGE_SCN_MEM_FARDATA = 0x00008000,
            //      IMAGE_SCN_MEM_SYSHEAP  - Obsolete    0x00010000
            IMAGE_SCN_MEM_PURGEABLE = 0x00020000,
            IMAGE_SCN_MEM_16BIT = 0x00020000,
            IMAGE_SCN_MEM_LOCKED = 0x00040000,
            IMAGE_SCN_MEM_PRELOAD = 0x00080000,

            IMAGE_SCN_ALIGN_1BYTES = 0x00100000,  //
            IMAGE_SCN_ALIGN_2BYTES = 0x00200000, //
            IMAGE_SCN_ALIGN_4BYTES = 0x00300000,  //
            IMAGE_SCN_ALIGN_8BYTES = 0x00400000,  //
            IMAGE_SCN_ALIGN_16BYTES = 0x00500000, // Default alignment if no others are specified.
            IMAGE_SCN_ALIGN_32BYTES = 0x00600000,  //
            IMAGE_SCN_ALIGN_64BYTES = 0x00700000, //
            IMAGE_SCN_ALIGN_128BYTES = 0x00800000,  //
            IMAGE_SCN_ALIGN_256BYTES = 0x00900000,  //
            IMAGE_SCN_ALIGN_512BYTES = 0x00A00000, //
            IMAGE_SCN_ALIGN_1024BYTES = 0x00B00000, //
            IMAGE_SCN_ALIGN_2048BYTES = 0x00C00000, //
            IMAGE_SCN_ALIGN_4096BYTES = 0x00D00000, //
            IMAGE_SCN_ALIGN_8192BYTES = 0x00E00000, //
            // Unused                                    0x00F00000

            IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000,  // Section contains extended relocations.
            IMAGE_SCN_MEM_DISCARDABLE = 0x02000000,  // Section can be discarded.
            IMAGE_SCN_MEM_NOT_CACHED = 0x04000000,  // Section is not cachable.
            IMAGE_SCN_MEM_NOT_PAGED = 0x08000000, // Section is not pageable.
            IMAGE_SCN_MEM_SHARED = 0x10000000,  // Section is shareable.
            IMAGE_SCN_MEM_EXECUTE = 0x20000000,  // Section is executable.
            IMAGE_SCN_MEM_READ = 0x40000000,  // Section is readable.
            IMAGE_SCN_MEM_WRITE = 0x80000000  // Section is writeable.
        }
        public enum SubsystemValues : ushort
        {
            IMAGE_SUBSYSTEM_UNKNOWN = 0,   // Unknown subsystem.
            IMAGE_SUBSYSTEM_NATIVE = 1,   // Image doesn't require a subsystem.
            IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,   // Image runs in the Windows GUI subsystem.
            IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,   // Image runs in the Windows character subsystem.
            IMAGE_SUBSYSTEM_OS2_CUI = 5,   // image runs in the OS/2 character subsystem.
            IMAGE_SUBSYSTEM_POSIX_CUI = 7,   // image runs in the Posix character subsystem.
            IMAGE_SUBSYSTEM_NATIVE_WINDOWS = 8,   // image is a native Win9x driver.
            IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9   // Image runs in the Windows CE subsystem.
        }
        public enum DllCharacteristicsEntries : ushort
        {
            //      IMAGE_LIBRARY_PROCESS_INIT           0x0001     // Reserved.
            //      IMAGE_LIBRARY_PROCESS_TERM           0x0002     // Reserved.
            //      IMAGE_LIBRARY_THREAD_INIT            0x0004     // Reserved.
            //      IMAGE_LIBRARY_THREAD_TERM            0x0008     // Reserved.
            IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000     // Driver uses WDM model
        }
        public enum DirectoryEntries : int
        {
            IMAGE_DIRECTORY_ENTRY_EXPORT = 0,   // Export Directory
            IMAGE_DIRECTORY_ENTRY_IMPORT = 1,   // Import Directory
            IMAGE_DIRECTORY_ENTRY_RESOURCE = 2,   // Resource Directory
            IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3,   // Exception Directory
            IMAGE_DIRECTORY_ENTRY_SECURITY = 4,   // Security Directory
            IMAGE_DIRECTORY_ENTRY_BASERELOC = 5,   // Base Relocation Table
            IMAGE_DIRECTORY_ENTRY_DEBUG = 6,   // Debug Directory
            //      IMAGE_DIRECTORY_ENTRY_COPYRIGHT=       7,   // (X86 usage)
            IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7,   // Architecture Specific Data
            IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8,   // RVA of GP
            IMAGE_DIRECTORY_ENTRY_TLS = 9,   // TLS Directory
            IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10,   // Load Configuration Directory
            IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11,   // Bound Import Directory in headers
            IMAGE_DIRECTORY_ENTRY_IAT = 12,   // Import Address Table
            IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13,   // Delay Load Import Descriptors
            IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14   // COM Runtime descriptor
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DOS_HEADER
        {      // DOS .EXE header
            public PESignatures e_magic;                     // Magic number
            public UInt16 e_cblp;                      // Bytes on last page of file
            public UInt16 e_cp;                        // Pages in file
            public UInt16 e_crlc;                      // Relocations
            public UInt16 e_cparhdr;                   // Size of header in paragraphs
            public UInt16 e_minalloc;                  // Minimum extra paragraphs needed
            public UInt16 e_maxalloc;                  // Maximum extra paragraphs needed
            public UInt16 e_ss;                        // Initial (relative) SS value
            public UInt16 e_sp;                        // Initial SP value
            public UInt16 e_csum;                      // Checksum
            public UInt16 e_ip;                        // Initial IP value
            public UInt16 e_cs;                        // Initial (relative) CS value
            public UInt16 e_lfarlc;                    // File address of relocation table
            public UInt16 e_ovno;                      // Overlay number
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt16[] e_res;                    // Reserved words
            public UInt16 e_oemid;                     // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo;                   // OEM information; e_oemid specific
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public UInt16[] e_res2;                  // Reserved words
            public UInt32 e_lfanew;                    // File address of new exe header
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_FILE_HEADER
        {
            public Machine Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public Characteristics Characteristics;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            //
            // Standard fields.
            //

            public MagicValues Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;

            //
            // NT additional fields.
            //

            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public SubsystemValues Subsystem;
            public DllCharacteristicsEntries DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public MagicValues Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;

            public UInt64 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public SubsystemValues Subsystem;
            public DllCharacteristicsEntries DllCharacteristics;
            public UInt64 SizeOfStackReserve;
            public UInt64 SizeOfStackCommit;
            public UInt64 SizeOfHeapReserve;
            public UInt64 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_NT_HEADERS64
        {
            public UInt32 Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_NT_HEADERS
        {
            public UInt32 Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_SECTION_HEADER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string Name;
            public UInt32 Misc;//PhysicalAddress or VirtualSize;
            public UInt32 VirtualAddress;
            public UInt32 SizeOfRawData;
            public UInt32 PointerToRawData;
            public UInt32 PointerToRelocations;
            public UInt32 PointerToLinenumbers;
            public UInt16 NumberOfRelocations;
            public UInt16 NumberOfLinenumbers;
            public SectionCharacteristics Characteristics;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_EXPORT_DIRECTORY
        {
            public UInt32 Characteristics;
            public UInt32 TimeDateStamp;
            public UInt16 MajorVersion;
            public UInt16 MinorVersion;
            public UInt32 Name;
            public UInt32 Base;
            public UInt32 NumberOfFunctions;
            public UInt32 NumberOfNames;
            public UInt32 AddressOfFunctions;     // RVA from base of image
            public UInt32 AddressOfNames;     // RVA from base of image
            public UInt32 AddressOfNameOrdinals;  // RVA from base of image
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_IMPORT_DESCRIPTOR
        {
            public UInt32 OriginalFirstThunk;
            public UInt32 TimeDateStamp;
            public UInt32 ForwarderChain;
            public UInt32 Name;
            public UInt32 FirstThunk;
        }

        public struct IMAGE_OPTIONAL_HEADER
        {
            public MagicValues Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;

            public UInt64 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public SubsystemValues Subsystem;
            public DllCharacteristicsEntries DllCharacteristics;
            public UInt64 SizeOfStackReserve;
            public UInt64 SizeOfStackCommit;
            public UInt64 SizeOfHeapReserve;
            public UInt64 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;

            public IMAGE_OPTIONAL_HEADER(IMAGE_OPTIONAL_HEADER32 fromheader)
            {
                Magic = fromheader.Magic;
                MajorLinkerVersion = fromheader.MajorLinkerVersion;
                MinorLinkerVersion = fromheader.MinorLinkerVersion;
                SizeOfCode = fromheader.SizeOfCode;
                SizeOfInitializedData = fromheader.SizeOfInitializedData;
                SizeOfUninitializedData = fromheader.SizeOfUninitializedData;
                AddressOfEntryPoint = fromheader.AddressOfEntryPoint;
                BaseOfCode = fromheader.BaseOfCode;

                ImageBase = fromheader.ImageBase;
                SectionAlignment = fromheader.SectionAlignment;
                FileAlignment = fromheader.FileAlignment;
                MajorOperatingSystemVersion = fromheader.MajorOperatingSystemVersion;
                MinorOperatingSystemVersion = fromheader.MinorOperatingSystemVersion;
                MajorImageVersion = fromheader.MajorImageVersion;
                MinorImageVersion = fromheader.MinorImageVersion;
                MajorSubsystemVersion = fromheader.MajorSubsystemVersion;
                MinorSubsystemVersion = fromheader.MinorSubsystemVersion;
                Win32VersionValue = fromheader.Win32VersionValue;
                SizeOfImage = fromheader.SizeOfImage;
                SizeOfHeaders = fromheader.SizeOfHeaders;
                CheckSum = fromheader.CheckSum;
                Subsystem = fromheader.Subsystem;
                DllCharacteristics = fromheader.DllCharacteristics;
                SizeOfStackReserve = fromheader.SizeOfStackReserve;
                SizeOfStackCommit = fromheader.SizeOfStackCommit;
                SizeOfHeapReserve = fromheader.SizeOfHeapReserve;
                SizeOfHeapCommit = fromheader.SizeOfHeapCommit;
                LoaderFlags = fromheader.LoaderFlags;
                NumberOfRvaAndSizes = fromheader.NumberOfRvaAndSizes;
                DataDirectory = fromheader.DataDirectory;

            }

            public IMAGE_OPTIONAL_HEADER(IMAGE_OPTIONAL_HEADER64 fromheader)
            {
                Magic = fromheader.Magic;
                MajorLinkerVersion = fromheader.MajorLinkerVersion;
                MinorLinkerVersion = fromheader.MinorLinkerVersion;
                SizeOfCode = fromheader.SizeOfCode;
                SizeOfInitializedData = fromheader.SizeOfInitializedData;
                SizeOfUninitializedData = fromheader.SizeOfUninitializedData;
                AddressOfEntryPoint = fromheader.AddressOfEntryPoint;
                BaseOfCode = fromheader.BaseOfCode;

                ImageBase = fromheader.ImageBase;
                SectionAlignment = fromheader.SectionAlignment;
                FileAlignment = fromheader.FileAlignment;
                MajorOperatingSystemVersion = fromheader.MajorOperatingSystemVersion;
                MinorOperatingSystemVersion = fromheader.MinorOperatingSystemVersion;
                MajorImageVersion = fromheader.MajorImageVersion;
                MinorImageVersion = fromheader.MinorImageVersion;
                MajorSubsystemVersion = fromheader.MajorSubsystemVersion;
                MinorSubsystemVersion = fromheader.MinorSubsystemVersion;
                Win32VersionValue = fromheader.Win32VersionValue;
                SizeOfImage = fromheader.SizeOfImage;
                SizeOfHeaders = fromheader.SizeOfHeaders;
                CheckSum = fromheader.CheckSum;
                Subsystem = fromheader.Subsystem;
                DllCharacteristics = fromheader.DllCharacteristics;
                SizeOfStackReserve = fromheader.SizeOfStackReserve;
                SizeOfStackCommit = fromheader.SizeOfStackCommit;
                SizeOfHeapReserve = fromheader.SizeOfHeapReserve;
                SizeOfHeapCommit = fromheader.SizeOfHeapCommit;
                LoaderFlags = fromheader.LoaderFlags;
                NumberOfRvaAndSizes = fromheader.NumberOfRvaAndSizes;
                DataDirectory = fromheader.DataDirectory;

            }
        }

        #endregion        

        private IMAGE_DOS_HEADER m_dosheader;
        private IMAGE_FILE_HEADER m_fileheader;
        private IMAGE_OPTIONAL_HEADER m_optionalheader;
        private IMAGE_EXPORT_DIRECTORY m_exportsdirectory;
        private List<IMAGE_IMPORT_DESCRIPTOR> m_importsdirectory=new List<IMAGE_IMPORT_DESCRIPTOR>();
        private Dictionary<string, ExportedSymbol> m_ExportsByName;
        private Dictionary<ushort, ExportedSymbol> m_ExportsByOrdinal;
        private Dictionary<string, ImportedLibrary> m_ImportedLibraries;
        private StreamHandler sh;
        private uint BaseAddress;
        private ProcessHandler m_OnProcess;//if null, we are parsing a file

        public IMAGE_DOS_HEADER DosHeader { get { return m_dosheader; } }
        public IMAGE_FILE_HEADER FileHeader { get { return m_fileheader; } }
        public IMAGE_OPTIONAL_HEADER OptionalHeader { get { return m_optionalheader; } }
        public IMAGE_EXPORT_DIRECTORY ExportsDirectory { get { return m_exportsdirectory; } }      

        public Dictionary<string, ExportedSymbol> ExportsByName
        { 
            get 
            {
                if (m_ExportsByName == null)
                    InitExports();
                return m_ExportsByName; 
            } 
        }
        public Dictionary<ushort, ExportedSymbol> ExportsByOrdinal
        {
            get
            {
                if (m_ExportsByOrdinal == null)
                    InitExports();
                return m_ExportsByOrdinal;
            }
        }

        private void InitExports()
        {
            ExportedSymbol newexport;
                //read exports directory
            if (m_optionalheader.DataDirectory[(int)DirectoryEntries.IMAGE_DIRECTORY_ENTRY_EXPORT].Size > 0)
            {
                //read exports
                sh.Position = BaseAddress + m_optionalheader.DataDirectory[(int)DirectoryEntries.IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress;

                try
                {
                    m_exportsdirectory = sh.Read<IMAGE_EXPORT_DIRECTORY>();//read exports directory

                    m_ExportsByName = new Dictionary<string, ExportedSymbol>((int)m_exportsdirectory.NumberOfNames);
                    m_ExportsByOrdinal = new Dictionary<ushort, ExportedSymbol>((int)m_exportsdirectory.NumberOfNames);

                    string[] Names = new String[m_exportsdirectory.NumberOfNames];
                    uint[] NameAddresses = new uint[m_exportsdirectory.NumberOfNames];
                    ushort[] Ordinals = new ushort[m_exportsdirectory.NumberOfNames];

                    sh.Position = BaseAddress + m_exportsdirectory.AddressOfNames;

                    for (uint i = 0; i < m_exportsdirectory.NumberOfNames; i++)//read addresses of name strings
                        NameAddresses[i] = BaseAddress + sh.Read<uint>();

                    for (uint i = 0; i < m_exportsdirectory.NumberOfNames; i++)//read the actual names
                    {
                        sh.Position = NameAddresses[i];
                        Names[i] = sh.ReadString();
                    }

                    sh.Position = BaseAddress + m_exportsdirectory.AddressOfNameOrdinals;//read ordinals
                    for (uint i = 0; i < m_exportsdirectory.NumberOfNames; i++)
                        Ordinals[i] = sh.Read<ushort>();

                    uint curaddress;
                    for (uint i = 0; i < m_exportsdirectory.NumberOfNames; i++)//read symbol addresses
                    {
                        sh.Position = BaseAddress + m_exportsdirectory.AddressOfFunctions + ((Ordinals[i] - m_exportsdirectory.Base) * 4);
                        curaddress = BaseAddress + sh.Read<uint>();
                        newexport = new ExportedSymbol(Names[i], Ordinals[i], curaddress);
                        m_ExportsByName.Add(Names[i], newexport);
                        m_ExportsByOrdinal.Add(Ordinals[i], newexport);
                    }
                }
                catch// (Exception e)
                {
                    m_ExportsByName = new Dictionary<string, ExportedSymbol>(0);
                    m_ExportsByOrdinal = new Dictionary<ushort, ExportedSymbol>(0);
                }
            }
            else
            {
                m_ExportsByName = new Dictionary<string, ExportedSymbol>(0);
                m_ExportsByOrdinal = new Dictionary<ushort, ExportedSymbol>(0);
            }
        }

        private struct ImportByName
        {
            public ushort Ordinal;
            public string Name;
            public ImportByName(ushort ordinal, string name)
            {
                Ordinal = ordinal;
                Name = name;
            }
        }

        private void InitImports()
        {
            string libname;
            List<ImportByName> importsbyname;
            IMAGE_IMPORT_DESCRIPTOR curdesc;
            UInt64 curvar;
            long backup;
            string curname;
            ushort curordinal;
            Dictionary<string, ImportedSymbol> curlib_imports;
            m_ImportedLibraries = new Dictionary<string, ImportedLibrary>();
            if (m_optionalheader.DataDirectory[(int)DirectoryEntries.IMAGE_DIRECTORY_ENTRY_IMPORT].Size > 0)
            {
                sh.Position = BaseAddress+m_optionalheader.DataDirectory[(int)DirectoryEntries.IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
                while ((curdesc = sh.Read<IMAGE_IMPORT_DESCRIPTOR>()).OriginalFirstThunk != 0)
                    m_importsdirectory.Add(curdesc);

                foreach (IMAGE_IMPORT_DESCRIPTOR desc in m_importsdirectory)
                {
                    sh.Position = BaseAddress+desc.Name;
                    libname = sh.ReadString();
                    importsbyname=new List<ImportByName>();
                    //read Import Name Table (ordinals or (ordinal_hint, name) pairs)
                    sh.Position = BaseAddress+desc.OriginalFirstThunk;
                    //build imports
                    if (m_optionalheader.Magic == MagicValues.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                    {
                        while ((curvar = sh.Read<uint>())!=0)
                        {
                            if ((curvar & 0x80000000) != 0)//import by ordinal
                            {
                                backup = sh.Position;
                                curordinal=(ushort)(curvar-0x80000000);
                                if (m_OnProcess == null)
                                    curname = libname + "_ordinal_0x" + curordinal.ToString("X");
                                else
                                {
                                    try
                                    {
                                        curname = m_OnProcess.MainModule.Modules[libname].PEHeader.ExportsByOrdinal[curordinal].Name;
                                    }
                                    catch
                                    {
                                        curname = libname + "_ordinal_0x" + curordinal.ToString("X");
                                    }
                                }
                               
                                importsbyname.Add(new ImportByName(curordinal, curname));
                                sh.Position = backup;
                            }
                            else
                            {
                                backup = sh.Position;
                                sh.Position = BaseAddress + (long)curvar;
                                //read ordinal hint
                                curvar = sh.Read<ushort>();
                                //read name
                                importsbyname.Add(new ImportByName((ushort)curvar, sh.ReadString()));
                                //jump back
                                sh.Position = backup;
                            }
                        }
                        sh.Position = BaseAddress + desc.FirstThunk;
                        curlib_imports = new Dictionary<string, ImportedSymbol>(importsbyname.Count);
                        foreach(ImportByName curimport in importsbyname)
                        {
                            backup=sh.Position;
                            curvar = (ulong)(/*BaseAddress+*/sh.Read<uint>());
                            curlib_imports.Add(curimport.Name, new ImportedSymbol(curimport.Name, curimport.Ordinal, (UInt32)curvar, (uint)backup, sh, true));
                        }
                        m_ImportedLibraries.Add(libname, new ImportedLibrary(libname, curlib_imports));
                    }
                    else if (m_optionalheader.Magic == MagicValues.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                    {
                        while ((curvar = sh.Read<UInt64>())!=0)
                        {
                            if ((curvar & 0x8000000000000000) != 0)//import by ordinal
                                importsbyname.Add(new ImportByName((ushort)(curvar - 0x8000000000000000), "ordinal_" + (curvar - 0x8000000000000000).ToString()));
                            else
                            {
                                backup = sh.Position;
                                sh.Position = BaseAddress + (long)curvar;
                                //read ordinal hint
                                curvar = sh.Read<ushort>();//NOT SURE ARE ORDINALS STILL 16BIT FOR 64BIT COMPUTERS??? <- yes (winnt.h)
                                //read name
                                importsbyname.Add(new ImportByName((ushort)curvar, sh.ReadString()));
                                //jump back
                                sh.Position = backup;
                            }
                        }
                        sh.Position = BaseAddress + desc.FirstThunk;
                        curlib_imports = new Dictionary<string, ImportedSymbol>(importsbyname.Count);
                        foreach (ImportByName curimport in importsbyname)
                        {
                            backup = sh.Position;
                            curvar = sh.Read<UInt64>();
                            curlib_imports.Add(curimport.Name, new ImportedSymbol(curimport.Name, curimport.Ordinal, (UInt32)curvar, (uint)backup, sh, false));
                        }
                        m_ImportedLibraries.Add(libname, new ImportedLibrary(libname, curlib_imports));
                    }
                }
            }
        }

        public Dictionary<string, ImportedLibrary> ImportedLibraries
        {
            get
            {
                if (m_ImportedLibraries == null)
                    InitImports();
                return m_ImportedLibraries;
            }
        }

        public PEHeaders(ProcessHandler from_process, uint m_BaseAddress)
        {
            m_OnProcess = from_process;
            sh = new StreamHandler(from_process);
            BaseAddress = m_BaseAddress;

            sh.Position = BaseAddress;

            try
            {
                m_dosheader = sh.Read<IMAGE_DOS_HEADER>();

                if (m_dosheader.e_magic != PESignatures.IMAGE_DOS_SIGNATURE)
                    throw new Exception("Failed to read the PE Header of this module!", new Exception("Unexpected magic number for the DOS Header!"));

                sh.Position = BaseAddress + m_dosheader.e_lfanew + 4;//move to IMAGE_NT_HEADER and skip it's signature

                m_fileheader = sh.Read<IMAGE_FILE_HEADER>();//read IMAGE_NT_HEADER.FileHeader

                MagicValues m_Magic = (MagicValues)sh.Read<UInt16>();//read IMAGE_NT_HEADER.OptionalHeader.Magic to determine if we're a 32bit or 64bit module

                if (m_Magic == MagicValues.IMAGE_ROM_OPTIONAL_HDR_MAGIC)
                    throw new Exception("Failed to read the PE Header of this module!", new Exception("Can't handle ROM Modules!"));
                else if (m_Magic == MagicValues.IMAGE_NT_OPTIONAL_HDR32_MAGIC)//32 bit
                {
                    sh.Position -= 2;//magic number is considered part of the optional header

                    m_optionalheader = new IMAGE_OPTIONAL_HEADER(sh.Read<IMAGE_OPTIONAL_HEADER32>());//read complete 32bit optional header
                }
                else if (m_Magic == MagicValues.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    sh.Position -= 2;//magic number is considered part of the optional header

                    m_optionalheader = new IMAGE_OPTIONAL_HEADER(sh.Read<IMAGE_OPTIONAL_HEADER64>());//read complete 64bit optional header
                }
                else
                    throw new Exception("Failed to read the PE Header of this module!", new Exception("Unexpected magic number for the Optional Header!"));
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read the PE Header of this module!", new Exception("Failed to read PE Header!", e));
            }
        }
        /*public PEHeaders(Stream from_stream, uint m_BaseAddress)
        {
            m_OnProcess = null;//PE file parsing
            sh = new StreamHandler(from_stream);
            BaseAddress = m_BaseAddress;

            sh.Position = BaseAddress;

            try
            {
                m_dosheader = sh.Read<IMAGE_DOS_HEADER>();

                if (m_dosheader.e_magic != PESignatures.IMAGE_DOS_SIGNATURE)
                    throw new Exception("Failed to read the PE Header of this module!", new Exception("Unexpected magic number for the DOS Header!"));

                sh.Position = BaseAddress + m_dosheader.e_lfanew + 4;//move to IMAGE_NT_HEADER and skip it's signature

                m_fileheader = sh.Read<IMAGE_FILE_HEADER>();//read IMAGE_NT_HEADER.FileHeader

                MagicValues m_Magic = (MagicValues)sh.Read<UInt16>();//read IMAGE_NT_HEADER.OptionalHeader.Magic to determine if we're a 32bit or 64bit module

                if (m_Magic == MagicValues.IMAGE_ROM_OPTIONAL_HDR_MAGIC)
                    throw new Exception("Failed to read the PE Header of this module!", new Exception("Can't handle ROM Modules!"));
                else if (m_Magic == MagicValues.IMAGE_NT_OPTIONAL_HDR32_MAGIC)//32 bit
                {
                    sh.Position -= 2;//magic number is considered part of the optional header

                    m_optionalheader = new IMAGE_OPTIONAL_HEADER(sh.Read<IMAGE_OPTIONAL_HEADER32>());//read complete 32bit optional header
                }
                else if (m_Magic == MagicValues.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    sh.Position -= 2;//magic number is considered part of the optional header

                    m_optionalheader = new IMAGE_OPTIONAL_HEADER(sh.Read<IMAGE_OPTIONAL_HEADER64>());//read complete 64bit optional header
                }
                else
                    throw new Exception("Failed to read the PE Header of this module!", new Exception("Unexpected magic number for the Optional Header!"));
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read the PE Header of this module!", new Exception("Failed to read PE Header!",e));
            }
        }*/
    }
}
