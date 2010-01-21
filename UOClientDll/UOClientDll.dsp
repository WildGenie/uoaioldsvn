# Microsoft Developer Studio Project File - Name="UOClientDll" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Dynamic-Link Library" 0x0102

CFG=UOClientDll - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "UOClientDll.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "UOClientDll.mak" CFG="UOClientDll - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "UOClientDll - Win32 Release" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE "UOClientDll - Win32 Debug" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "UOClientDll - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release"
# PROP Intermediate_Dir "Release"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MT /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "UOCLIENTDLL_EXPORTS" /YX /FD /c
# ADD CPP /nologo /MT /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "UOCLIENTDLL_EXPORTS" /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x813 /d "NDEBUG"
# ADD RSC /l 0x813 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /dll /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib uuid.lib odbc32.lib odbccp32.lib wsock32.lib ole32.lib oleaut32.lib Version.lib /nologo /dll /machine:I386

!ELSEIF  "$(CFG)" == "UOClientDll - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "Debug"
# PROP Intermediate_Dir "Debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "UOCLIENTDLL_EXPORTS" /YX /FD /GZ /c
# ADD CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "UOCLIENTDLL_EXPORTS" /YX /FD /GZ /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x813 /d "_DEBUG"
# ADD RSC /l 0x813 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /dll /debug /machine:I386 /pdbtype:sept
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib wsock32.lib  Version.lib /nologo /dll /debug /machine:I386 /pdbtype:sept
# SUBTRACT LINK32 /pdb:none

!ENDIF 

# Begin Target

# Name "UOClientDll - Win32 Release"
# Name "UOClientDll - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Group "libdisasmsource"

# PROP Default_Filter ""
# Begin Source File

SOURCE=.\libdisasm\ia32_implicit.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_insn.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_invariant.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_modrm.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_opcode_tables.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_operand.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_reg.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_settings.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_disasm.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_format.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_imm.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_insn.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_misc.c
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_operand_list.c
# End Source File
# End Group
# Begin Source File

SOURCE=.\ASMParser.c
# End Source File
# Begin Source File

SOURCE=.\BinaryTree.c
# End Source File
# Begin Source File

SOURCE=.\Bufferhandler.c
# End Source File
# Begin Source File

SOURCE=.\Collection.c
# End Source File
# Begin Source File

SOURCE=.\DebuggingTools.c
# End Source File
# Begin Source File

SOURCE=.\Sockets.c
# End Source File
# Begin Source File

SOURCE=.\Synchronization.c
# End Source File
# Begin Source File

SOURCE=.\SyncQueue.c
# End Source File
# Begin Source File

SOURCE=.\UOClientDll.c
# End Source File
# Begin Source File

SOURCE=.\UOClientDll.def
# End Source File
# Begin Source File

SOURCE=.\UOMLCallibrations.c
# End Source File
# Begin Source File

SOURCE=.\UOMLVersionInfo.c
# End Source File
# Begin Source File

SOURCE=.\W2EDebug.c
# End Source File
# Begin Source File

SOURCE=.\wError.c
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl"
# Begin Group "libdisasmheaders"

# PROP Default_Filter ""
# Begin Source File

SOURCE=.\libdisasm\ia32_implicit.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_insn.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_invariant.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_modrm.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_opcode_tables.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_operand.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_reg.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\ia32_settings.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\libdis.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\qword.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_imm.h
# End Source File
# Begin Source File

SOURCE=.\libdisasm\x86_operand_list.h
# End Source File
# End Group
# Begin Source File

SOURCE=.\ALLOCATION.h
# End Source File
# Begin Source File

SOURCE=.\ASMParser.h
# End Source File
# Begin Source File

SOURCE=.\BinaryTree.h
# End Source File
# Begin Source File

SOURCE=.\Bufferhandler.h
# End Source File
# Begin Source File

SOURCE=.\Collection.h
# End Source File
# Begin Source File

SOURCE=.\DebuggingTools.h
# End Source File
# Begin Source File

SOURCE=.\Sockets.h
# End Source File
# Begin Source File

SOURCE=.\Synchronization.h
# End Source File
# Begin Source File

SOURCE=.\SyncQueue.h
# End Source File
# Begin Source File

SOURCE=.\UOClientDll.h
# End Source File
# Begin Source File

SOURCE=.\UOMLCallibrations.h
# End Source File
# Begin Source File

SOURCE=.\UOMLVersionInfo.h
# End Source File
# Begin Source File

SOURCE=.\W2EDebug.h
# End Source File
# Begin Source File

SOURCE=.\wError.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe"
# End Group
# End Target
# End Project
