

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Imports System.Runtime.InteropServices

Partial Class UOAI
    ''' <summary>A class to encapsulate imported functions from User32.dll and Kernel32.dll</summary>
    Friend Class [Imports]
        Private Sub New()
        End Sub
#Region "user32_imports"
        <DllImport("user32.dll")> _
        Shared Function EnumWindows(ByVal lpEnumFunc As EnumWindowsProc, ByVal lParam As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("user32.dll")> _
        Shared Function GetWindowThreadProcessId(ByVal hWnd As UInt32, ByRef ProcessId As UInt32) As UInteger
        End Function

        Friend Delegate Function EnumWindowsProc(ByVal hWnd As UInt32, ByVal lParam As UInt32) As Boolean

        <DllImport("user32.dll", CharSet:=CharSet.Auto)> _
        Shared Function GetClassName(ByVal hWnd As UInt32, ByVal lpClassName As StringBuilder, ByVal nMaxCount As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("user32.dll", SetLastError:=True)> _
        Shared Function FindWindowEx(ByVal hwndParent As UInt32, ByVal hwndChildAfter As UInt32, ByVal lpszClass As String, ByVal lpszWindow As String) As UInt32
        End Function

        'TODO: ensure this works. Used to be: Friend Const HWND_MESSAGE As UInt32 = &HFFFFFFFD
        Friend Const HWND_MESSAGE As UInt32 = 4294967293 'FFFFFFFD

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
        Shared Function RegisterWindowMessage(ByVal lpString As String) As UInt32
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=False)> _
        Shared Function SendMessage(ByVal hWnd As UInt32, ByVal Msg As UInt32, ByVal wParam As UInt32, ByVal lParam As UInt32) As UInt32
        End Function

        <DllImport("user32.dll", SetLastError:=True)> _
        Shared Function PostMessage(ByVal hWnd As UInt32, ByVal Msg As UInt32, ByVal wParam As UInt32, ByVal lParam As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function
#End Region

#Region "kernel32_imports"

        <Flags()> _
        Friend Enum CreationFlags As UInteger
            CREATE_BREAKAWAY_FROM_JOB = &H1000000
            CREATE_DEFAULT_ERROR_MODE = &H4000000
            CREATE_NEW_CONSOLE = &H10
            CREATE_NEW_PROCESS_GROUP = &H200
            CREATE_NO_WINDOW = &H8000000
            CREATE_PROTECTED_PROCESS = &H40000
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = &H2000000
            CREATE_SEPARATE_WOW_VDM = &H1000
            CREATE_SUSPENDED = &H4
            CREATE_UNICODE_ENVIRONMENT = &H400
            DEBUG_ONLY_THIS_PROCESS = &H2
            DEBUG_PROCESS = &H1
            DETACHED_PROCESS = &H8
            EXTENDED_STARTUPINFO_PRESENT = &H80000
        End Enum

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
        Friend Structure STARTUPINFO
            Public cb As Int32
            Public lpReserved As String
            Public lpDesktop As String
            Public lpTitle As String
            Public dwX As Int32
            Public dwY As Int32
            Public dwXSize As Int32
            Public dwYSize As Int32
            Public dwXCountChars As Int32
            Public dwYCountChars As Int32
            Public dwFillAttribute As Int32
            Public dwFlags As Int32
            Public wShowWindow As Int16
            Public cbReserved2 As Int16
            Public lpReserved2 As IntPtr
            Public hStdInput As IntPtr
            Public hStdOutput As IntPtr
            Public hStdError As IntPtr
        End Structure

        <StructLayout(LayoutKind.Sequential)> _
        Friend Structure PROCESS_INFORMATION
            Public hProcess As IntPtr
            Public hThread As IntPtr
            Public dwProcessId As Integer
            Public dwThreadId As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential)> _
        Friend Structure SECURITY_ATTRIBUTES
            Public nLength As Integer
            Public lpSecurityDescriptor As IntPtr
            Public bInheritHandle As Integer
        End Structure

        <DllImport("kernel32.dll")> _
        Shared Function CreateProcess(ByVal lpApplicationName As String, ByVal lpCommandLine As String, ByRef lpProcessAttributes As SECURITY_ATTRIBUTES, ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, ByVal bInheritHandles As Boolean, ByVal dwCreationFlags As CreationFlags, _
         ByVal lpEnvironment As IntPtr, ByVal lpCurrentDirectory As String, <[In]()> ByRef lpStartupInfo As STARTUPINFO, ByRef lpProcessInformation As PROCESS_INFORMATION) As Boolean
        End Function

        <Flags()> _
        Friend Enum ProcessAccess As Integer
            ''' <summary>Specifies all possible access flags for the process object.</summary>
            AllAccess = CreateThread Or DuplicateHandle Or QueryInformation Or SetInformation Or Terminate Or VMOperation Or VMRead Or VMWrite Or Synchronize
            ''' <summary>Enables usage of the process handle in the CreateRemoteThread function to create a thread in the process.</summary>
            CreateThread = &H2
            ''' <summary>Enables usage of the process handle as either the source or target process in the DuplicateHandle function to duplicate a handle.</summary>
            DuplicateHandle = &H40
            ''' <summary>Enables usage of the process handle in the GetExitCodeProcess and GetPriorityClass functions to read information from the process object.</summary>
            QueryInformation = &H400
            ''' <summary>Enables usage of the process handle in the SetPriorityClass function to set the priority class of the process.</summary>
            SetInformation = &H200
            ''' <summary>Enables usage of the process handle in the TerminateProcess function to terminate the process.</summary>
            Terminate = &H1
            ''' <summary>Enables usage of the process handle in the VirtualProtectEx and WriteProcessMemory functions to modify the virtual memory of the process.</summary>
            VMOperation = &H8
            ''' <summary>Enables usage of the process handle in the ReadProcessMemory function to' read from the virtual memory of the process.</summary>
            VMRead = &H10
            ''' <summary>Enables usage of the process handle in the WriteProcessMemory function to write to the virtual memory of the process.</summary>
            VMWrite = &H20
            ''' <summary>Enables usage of the process handle in any of the wait functions to wait for the process to terminate.</summary>
            Synchronize = &H100000
        End Enum

        <DllImport("kernel32.dll")> _
        Shared Function ResumeThread(ByVal hThread As IntPtr) As UInteger
        End Function

        <DllImport("kernel32.dll")> _
        Shared Function OpenProcess(ByVal dwDesiredAccess As ProcessAccess, <MarshalAs(UnmanagedType.Bool)> ByVal bInheritHandle As Boolean, ByVal dwProcessId As UInt32) As UInt32
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Shared Function GetExitCodeProcess(ByVal hProcess As UInt32, ByRef lpExitCode As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Shared Function CloseHandle(ByVal hObject As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("kernel32.dll")> _
        Shared Function LoadLibrary(ByVal dllToLoad As String) As IntPtr
        End Function

        <DllImport("kernel32.dll")> _
        Shared Function GetProcAddress(ByVal hModule As IntPtr, ByVal procedureName As String) As IntPtr
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Shared Function ReadProcessMemory(ByVal hProcess As UInt32, ByVal lpBaseAddress As UInt32, <Out()> ByVal lpBuffer As Byte(), ByVal dwSize As UInt32, ByRef lpNumberOfBytesRead As UInt32) As Boolean
        End Function

        Friend Const PAGE_READWRITE As UInt32 = 4

        <DllImport("kernel32.dll")> _
        Shared Function VirtualProtectEx(ByVal hProcess As UInt32, ByVal lpAddress As UInt32, ByVal dwSize As UInt32, ByVal flNewProtect As UInt32, ByRef lpflOldProtect As UInt32) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)> _
        Shared Function WriteProcessMemory(ByVal hProcess As UInt32, ByVal lpBaseAddress As UInt32, ByVal lpBuffer As Byte(), ByVal nSize As UInt32, ByRef lpNumberOfBytesWritten As UInt32) As Boolean
        End Function
#End Region
    End Class

End Class
