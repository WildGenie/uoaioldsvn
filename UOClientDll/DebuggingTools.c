#include "DebuggingTools.h"
#include <time.h>
#include "W2EDebug.h"

BOOL isrunning(DWORD pid)
{
	DWORD exitcode=0;
	HANDLE hProcess=OpenProcess(PROCESS_QUERY_INFORMATION,FALSE,pid);
	if(hProcess)
	{
		if(GetExitCodeProcess(hProcess,&exitcode))
		{
			if(exitcode==STILL_ACTIVE)
			{
				CloseHandle(hProcess);
				return TRUE;
			}
		}
		CloseHandle(hProcess);
	}
	return FALSE;
}

BOOL Read(unsigned int offset,void * buffer,unsigned int size)
{
	HANDLE hprocess;
	//DWORD read;
	DWORD prevprotect;
	//DWORD iostart,timediff;
	//BOOL readstat;

	hprocess=GetCurrentProcess();

	VirtualProtectEx(hprocess,(LPVOID)offset,size,PAGE_READWRITE,&prevprotect);
	
	/*readstat=FALSE;
	iostart=clock();
	while(((timediff=clock()-iostart)<REMOTE_IO_TIMEOUT)&&(!readstat))
		readstat=ReadProcessMemory(hprocess,(LPCVOID)offset,(LPVOID)buffer,size,&read)&&(read==size);*/

	wCopy((LPVOID)buffer,(LPVOID)offset,size);

	VirtualProtectEx(hprocess,(LPVOID)offset,size,prevprotect,&prevprotect);
	
	CloseHandle(hprocess);

	return TRUE;
}

int ReadInt(unsigned int offset)
{
	int toreturn;
	if(!Read(offset,(void *)&toreturn,sizeof(int)))
		toreturn=0;//error handling needed here
	return toreturn;
}

unsigned int ReadUInt(unsigned int offset)
{
	unsigned int toreturn;
	if(!Read(offset,(void *)&toreturn,sizeof(unsigned int)))
		toreturn=0;//error handling needed here
	return toreturn;
}

short ReadShort(unsigned int offset)
{
	short toreturn;
	if(!Read(offset,(void *)&toreturn,sizeof(short)))
		toreturn=0;//error handling needed here
	return toreturn;
}

unsigned short ReadUShort(unsigned int offset)
{
	unsigned short toreturn;
	if(!Read(offset,(void *)&toreturn,sizeof(unsigned short)))
		toreturn=0;//error handling needed here
	return toreturn;
}

char ReadChar(unsigned int offset)
{
	char toreturn;
	if(!Read(offset,(void *)&toreturn,sizeof(char)))
		toreturn=0;//error handling needed here
	return toreturn;
}

BYTE ReadByte(unsigned int offset)
{
	BYTE toreturn;
	if(!Read(offset,(void *)&toreturn,sizeof(BYTE)))
		toreturn=0;//error handling needed here
	return toreturn;
}

char * ReadStr(unsigned int offset)
{
	char curbyte;
	unsigned int count=0;

	if(!Read(offset+count,&curbyte,1))
		return NULL;

	count++;

	while(curbyte!='\0')
	{
		if(!Read(offset+count,&curbyte,1))
			return NULL;
		count++;
	}

	return ReadStrn(offset,count);
}

/*BSTR ReadBSTR(unsigned int offset)
{
	short * unicodestring=0;
	short curshort;
	unsigned int count=0;
	BSTR toreturn=0;

	if(!Read(offset+(2*count),&curshort,2))
		return NULL;

	count++;

	while(curshort!=0)
	{
		if(!Read(offset+(2*count),&curshort,2))
			return NULL;
		count++;
	}

	unicodestring=(short *)wAlloc(sizeof(short)*count);
	if(!Read(offset,(void *)unicodestring,sizeof(short)*count))
	{
		wFree(unicodestring);
		return NULL;
	}
	toreturn=SysAllocStringLen(unicodestring,count);
	wFree(unicodestring);
	return toreturn;
}*/

char * ReadStrn(unsigned int offset,unsigned int length)
{
	char * toreturn=(char *)wAlloc(sizeof(char)*(length+1));
	memset((void *)toreturn,0,sizeof(char)*(length+1));
	if(!Read(offset,(void *)toreturn,sizeof(char)*length))
	{
		wFree(toreturn);
		return NULL;
	}
	return toreturn;
}

BOOL WriteBytes(unsigned int offset,BYTE * towrite,unsigned int size)
{
	HANDLE hprocess;
	//DWORD written;
	DWORD prevprotect;
	/*DWORD iostart;
	DWORD timediff;*/

	//BOOL writestat;

	hprocess=GetCurrentProcess();
	
	VirtualProtectEx(hprocess,(LPVOID)offset,size,PAGE_READWRITE,&prevprotect);
	
	/*writestat=FALSE;
	iostart=clock();
	while(((timediff=clock()-iostart)<REMOTE_IO_TIMEOUT)&&(!writestat))
		writestat=WriteProcessMemory(hprocess,(LPVOID)offset,(LPVOID)towrite,size,&written)&&(written==size);*/

	wCopy((LPVOID)offset,(LPVOID)towrite,size);

	VirtualProtectEx(hprocess,(LPVOID)offset,size,prevprotect,&prevprotect);
	
	CloseHandle(hprocess);

	return TRUE;
}

BOOL WriteInt(unsigned int offset,int towrite)
{
	return WriteBytes(offset,(BYTE *)&towrite,sizeof(int));
}

BOOL WriteUInt(unsigned int offset,unsigned int towrite)
{
	return WriteBytes(offset,(BYTE *)&towrite,sizeof(unsigned int));
}

BOOL WriteShort(unsigned int offset,short towrite)
{
	return WriteBytes(offset,(BYTE *)&towrite,sizeof(short));
}

BOOL WriteUShort(unsigned int offset,unsigned short towrite)
{
	return WriteBytes(offset,(BYTE *)&towrite,sizeof(unsigned short));
}

BOOL WriteChar(unsigned int offset,char towrite)
{
	return WriteBytes(offset,(BYTE *)&towrite,sizeof(char));
}

BOOL WriteByte(unsigned int offset,BYTE towrite)
{
	return WriteBytes(offset,&towrite,sizeof(BYTE));
}

BOOL WriteStr(unsigned int offset,char * towrite)
{
	return WriteBytes(offset,(BYTE *)towrite,strlen(towrite)+1);
}

/*BOOL WriteBSTR(unsigned int offset,BSTR towrite)
{
	return WriteBytes(offset,(BYTE *)towrite,(olestrlen((LPOLESTR)towrite)+1)*2);
}*/

BOOL WriteStrn(unsigned int offset,char * towrite,unsigned int length)
{
	return WriteBytes(offset,(BYTE *)towrite,length);
}

unsigned int olestrlen(LPOLESTR str)
{
	unsigned int i=0;
	short curchar=*str;
	while(curchar)
	{
		i++;
		curchar=*(str+i);
	}
	return i;
}

BOOL GetBaseAddress(DWORD dwPID,unsigned int * baseaddress) 
{ 
  HANDLE hModuleSnap = INVALID_HANDLE_VALUE; 
  MODULEENTRY32 me32; 
  
  hModuleSnap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwPID); 
  if(hModuleSnap==INVALID_HANDLE_VALUE) 
	  return FALSE;
 
  me32.dwSize = sizeof( MODULEENTRY32 ); 
  if(!Module32First(hModuleSnap,&me32)) 
  { 
    CloseHandle( hModuleSnap );
	return FALSE;
  } 
 
  do 
  { 
	  if((strcmp(me32.szExePath+strlen(me32.szExePath)-3,"exe")==0)||(strcmp(me32.szExePath+strlen(me32.szExePath)-3,"EXE")==0))
	  {
		  (*baseaddress)=(unsigned int)me32.modBaseAddr;
		  CloseHandle( hModuleSnap ); 
		  return TRUE;
	  }
  } while( Module32Next( hModuleSnap, &me32 ) ); 
 
  CloseHandle( hModuleSnap ); 
  return( TRUE ); 
}

unsigned int GetEntryPointOffset()
{
	IMAGE_DOS_HEADER dosheader;
	IMAGE_NT_HEADERS ntheaders;
	HANDLE hprocess;

	unsigned int baseaddress;

	DWORD read;

	if(!GetBaseAddress(GetCurrentProcessId(),&baseaddress))
		baseaddress=0x400000;//assuming a baseaddress of 0x400000 for an exe should be ok

	hprocess=GetCurrentProcess();

	if((!ReadProcessMemory(hprocess,(LPCVOID)baseaddress,&dosheader,sizeof(IMAGE_DOS_HEADER),&read))||(read!=sizeof(IMAGE_DOS_HEADER)))
	{
		CloseHandle(hprocess);
		return 0;
	}

	if(dosheader.e_magic!=IMAGE_DOS_SIGNATURE)
	{
		CloseHandle(hprocess);
		return 0;
	}

	if((!ReadProcessMemory(hprocess,(LPCVOID)(baseaddress+dosheader.e_lfanew),&ntheaders,sizeof(IMAGE_NT_HEADERS),&read))||(read!=sizeof(IMAGE_NT_HEADERS)))
	{
		CloseHandle(hprocess);
		return 0;
	}

	if(ntheaders.Signature!=IMAGE_NT_SIGNATURE)
	{
		CloseHandle(hprocess);
		return 0;
	}

	CloseHandle(hprocess);

	return (baseaddress+ntheaders.OptionalHeader.AddressOfEntryPoint);
}

//remote versions:

BOOL RRead(DWORD pid,unsigned int offset,void * buffer,unsigned int size)
{
	HANDLE hprocess;
	DWORD read;
	DWORD prevprotect;
	DWORD iostart,timediff;
	BOOL readstat;

	if(pid==0)
		return Read(offset,buffer,size);
	
	hprocess=openprocess(pid);
	
	VirtualProtectEx(hprocess,(LPVOID)offset,size,PAGE_READWRITE,&prevprotect);
	
	readstat=FALSE;
	iostart=clock();
	while(((timediff=clock()-iostart)<REMOTE_IO_TIMEOUT)&&(!readstat))
		readstat=ReadProcessMemory(hprocess,(LPCVOID)offset,(LPVOID)buffer,size,&read)&&(read==size);

	VirtualProtectEx(hprocess,(LPVOID)offset,size,prevprotect,&prevprotect);
	
	CloseHandle(hprocess);

	return readstat;
}

int RReadInt(DWORD pid,unsigned int offset)
{
	int toreturn;
	if(!RRead(pid,offset,(void *)&toreturn,sizeof(int)))
		toreturn=0;//error handling needed here
	return toreturn;
}

unsigned int RReadUInt(DWORD pid,unsigned int offset)
{
	unsigned int toreturn;
	if(!RRead(pid,offset,(void *)&toreturn,sizeof(unsigned int)))
		toreturn=0;//error handling needed here
	return toreturn;
}

short RReadShort(DWORD pid,unsigned int offset)
{
	short toreturn;
	if(!RRead(pid,offset,(void *)&toreturn,sizeof(short)))
		toreturn=0;//error handling needed here
	return toreturn;
}

unsigned short RReadUShort(DWORD pid,unsigned int offset)
{
	unsigned short toreturn;
	if(!RRead(pid,offset,(void *)&toreturn,sizeof(unsigned short)))
		toreturn=0;//error handling needed here
	return toreturn;
}

char RReadChar(DWORD pid,unsigned int offset)
{
	char toreturn;
	if(!RRead(pid,offset,(void *)&toreturn,sizeof(char)))
		toreturn=0;//error handling needed here
	return toreturn;
}

BYTE RReadByte(DWORD pid,unsigned int offset)
{
	BYTE toreturn;
	if(!RRead(pid,offset,(void *)&toreturn,sizeof(BYTE)))
		toreturn=0;//error handling needed here
	return toreturn;
}

char * RReadStr(DWORD pid,unsigned int offset)
{
	char curbyte;
	unsigned int count=0;

	if(!RRead(pid,offset+count,&curbyte,1))
		return NULL;

	count++;

	while(curbyte!='\0')
	{
		if(!RRead(pid,offset+count,&curbyte,1))
			return NULL;
		count++;
	}

	return RReadStrn(pid,offset,count);
}

char * RReadStrn(DWORD pid,unsigned int offset,unsigned int length)
{
	char * toreturn=(char *)GlobalAlloc(GMEM_FIXED,sizeof(char)*(length+1));
	memset((void *)toreturn,0,sizeof(char)*(length+1));
	if(!RRead(pid,offset,(void *)toreturn,sizeof(char)*length))
	{
		GlobalFree(toreturn);
		return NULL;
	}
	return toreturn;
}

BOOL RWriteBytes(DWORD pid,unsigned int offset,BYTE * towrite,unsigned int size)
{
	HANDLE hprocess;
	DWORD written;
	DWORD prevprotect;
	DWORD iostart;
	DWORD timediff;

	BOOL writestat;

	if(pid==0)
		return WriteBytes(offset,towrite,size);
		
	hprocess=openprocess(pid);
	
	VirtualProtectEx(hprocess,(LPVOID)offset,size,PAGE_READWRITE,&prevprotect);
	
	writestat=FALSE;
	iostart=clock();
	while(((timediff=clock()-iostart)<REMOTE_IO_TIMEOUT)&&(!writestat))
		writestat=WriteProcessMemory(hprocess,(LPVOID)offset,(LPVOID)towrite,size,&written)&&(written==size);

	VirtualProtectEx(hprocess,(LPVOID)offset,size,prevprotect,&prevprotect);
	
	CloseHandle(hprocess);

	return writestat;
}

BOOL RWriteInt(DWORD pid,unsigned int offset,int towrite)
{
	return RWriteBytes(pid,offset,(BYTE *)&towrite,sizeof(int));
}

BOOL RWriteUInt(DWORD pid,unsigned int offset,unsigned int towrite)
{
	return RWriteBytes(pid,offset,(BYTE *)&towrite,sizeof(unsigned int));
}

BOOL RWriteShort(DWORD pid,unsigned int offset,short towrite)
{
	return RWriteBytes(pid,offset,(BYTE *)&towrite,sizeof(short));
}

BOOL RWriteUShort(DWORD pid,unsigned int offset,unsigned short towrite)
{
	return RWriteBytes(pid,offset,(BYTE *)&towrite,sizeof(unsigned short));
}

BOOL RWriteChar(DWORD pid,unsigned int offset,char towrite)
{
	return RWriteBytes(pid,offset,(BYTE *)&towrite,sizeof(char));
}

BOOL RWriteByte(DWORD pid,unsigned int offset,BYTE towrite)
{
	return RWriteBytes(pid,offset,&towrite,sizeof(BYTE));
}

BOOL RWriteStr(DWORD pid,unsigned int offset,char * towrite)
{
	return RWriteBytes(pid,offset,(BYTE *)towrite,strlen(towrite)+1);
}

BOOL RWriteStrn(DWORD pid,unsigned int offset,char * towrite,unsigned int length)
{
	return RWriteBytes(pid,offset,(BYTE *)towrite,length);
}

HANDLE openprocess(DWORD pid)
{
	return OpenProcess(PROCESS_CREATE_THREAD|PROCESS_VM_READ|PROCESS_VM_WRITE|PROCESS_VM_OPERATION|PROCESS_QUERY_INFORMATION,0,pid);
}

unsigned int RGetEntryPointOffset(DWORD pid)
{
	IMAGE_DOS_HEADER dosheader;
	IMAGE_NT_HEADERS ntheaders;
	HANDLE hprocess;

	unsigned int baseaddress;

	DWORD read;

	if(!GetBaseAddress(pid,&baseaddress))
		baseaddress=0x400000;//assuming a baseaddress of 0x400000 for an exe should be ok

	hprocess=openprocess(pid);

	if((!ReadProcessMemory(hprocess,(LPCVOID)baseaddress,&dosheader,sizeof(IMAGE_DOS_HEADER),&read))||(read!=sizeof(IMAGE_DOS_HEADER)))
	{
		CloseHandle(hprocess);
		return 0;
	}

	if(dosheader.e_magic!=IMAGE_DOS_SIGNATURE)
	{
		CloseHandle(hprocess);
		return 0;
	}

	if((!ReadProcessMemory(hprocess,(LPCVOID)(baseaddress+dosheader.e_lfanew),&ntheaders,sizeof(IMAGE_NT_HEADERS),&read))||(read!=sizeof(IMAGE_NT_HEADERS)))
	{
		CloseHandle(hprocess);
		return 0;
	}

	if(ntheaders.Signature!=IMAGE_NT_SIGNATURE)
	{
		CloseHandle(hprocess);
		return 0;
	}

	CloseHandle(hprocess);

	return (baseaddress+ntheaders.OptionalHeader.AddressOfEntryPoint);
}

char * GetExeModuleFilePath(DWORD dwPID)
{ 
  HANDLE hModuleSnap = INVALID_HANDLE_VALUE; 
  MODULEENTRY32 me32;
  
  char * toreturn;
  
  hModuleSnap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwPID); 
  if(hModuleSnap==INVALID_HANDLE_VALUE) 
	  return FALSE;
 
  me32.dwSize = sizeof( MODULEENTRY32 ); 
  if(!Module32First(hModuleSnap,&me32)) 
  { 
    CloseHandle( hModuleSnap );
	return 0;
  } 
 
  do 
  { 
	  if((strcmp(me32.szExePath+strlen(me32.szExePath)-3,"exe")==0)||(strcmp(me32.szExePath+strlen(me32.szExePath)-3,"EXE")==0))
	  {
		  toreturn=(char *)wAlloc(strlen(me32.szExePath)+1);
		  toreturn[0]='\0';
		  strcat(toreturn,me32.szExePath);		  
		  CloseHandle( hModuleSnap ); 
		  return toreturn;
	  }
  } while( Module32Next( hModuleSnap, &me32 ) ); 
 
  CloseHandle( hModuleSnap ); 
  return 0; 
}