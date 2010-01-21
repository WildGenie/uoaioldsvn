#ifndef DEBUGGINGTOOLS_INCLUDED
#define DEBUGGINGTOOLS_INCLUDED

#define _CRT_SECURE_NO_DEPRECATE

#include "ALLOCATION.h"

#include <windows.h>

#include <Tlhelp32.h>

#define REMOTE_IO_TIMEOUT 3333

BOOL isrunning(DWORD pid);

int ReadInt(unsigned int offset);
unsigned int ReadUInt(unsigned int offset);
short ReadShort(unsigned int offset);
unsigned short ReadUShort(unsigned int offset);
char ReadChar(unsigned int offset);
BYTE ReadByte(unsigned int offset);
char * ReadStr(unsigned int offset);
char * ReadStrn(unsigned int offset,unsigned int length);
//BSTR ReadBSTR(unsigned int offset);
BOOL Read(unsigned int offset,void * buffer,unsigned int size);

BOOL WriteBytes(unsigned int offset,BYTE * towrite,unsigned int length);
BOOL WriteInt(unsigned int offset,int towrite);
BOOL WriteUInt(unsigned int offset,unsigned int towrite);
BOOL WriteShort(unsigned int offset,short towrite);
BOOL WriteUShort(unsigned int offset,unsigned short towrite);
BOOL WriteChar(unsigned int offset,char towrite);
BOOL WriteByte(unsigned int offset,BYTE towrite);
BOOL WriteStr(unsigned int offset,char * towrite);
BOOL WriteStrn(unsigned int offset,char * towrite,unsigned int length);
//BOOL WriteBSTR(unsigned int offset,BSTR towrite);

unsigned int olestrlen(LPOLESTR str);

unsigned int GetEntryPointOffset();
BOOL GetBaseAddress(DWORD dwPID,unsigned int * baseaddress);

//remote versions
int RReadInt(DWORD pid,unsigned int offset);
unsigned int RReadUInt(DWORD pid,unsigned int offset);
short RReadShort(DWORD pid,unsigned int offset);
unsigned short RReadUShort(DWORD pid,unsigned int offset);
char RReadChar(DWORD pid,unsigned int offset);
BYTE RReadByte(DWORD pid,unsigned int offset);
char * RReadStr(DWORD pid,unsigned int offset);
char * RReadStrn(DWORD pid,unsigned int offset,unsigned int length);
BOOL RRead(DWORD pid,unsigned int offset,void * buffer,unsigned int size);

BOOL RWriteBytes(DWORD pid,unsigned int offset,BYTE * towrite,unsigned int length);
BOOL RWriteInt(DWORD pid,unsigned int offset,int towrite);
BOOL RWriteUInt(DWORD pid,unsigned int offset,unsigned int towrite);
BOOL RWriteShort(DWORD pid,unsigned int offset,short towrite);
BOOL RWriteUShort(DWORD pid,unsigned int offset,unsigned short towrite);
BOOL RWriteChar(DWORD pid,unsigned int offset,char towrite);
BOOL RWriteByte(DWORD pid,unsigned int offset,BYTE towrite);
BOOL RWriteStr(DWORD pid,unsigned int offset,char * towrite);
BOOL RWriteStrn(DWORD pid,unsigned int offset,char * towrite,unsigned int length);

HANDLE openprocess(DWORD pid);
unsigned int RGetEntryPointOffset(DWORD pid);

char * GetExeModuleFilePath(DWORD dwPID);

#endif