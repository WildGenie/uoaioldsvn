#ifndef UOCLIENTDLL_INCLUDED
#define UOCLIENTDLL_INCLUDED

#define _CRT_SECURE_NO_DEPRECATE

#ifdef BUILDING_DLL
# define UOCLIENTDLLAPI __declspec (dllexport)
#else /* Not BUILDING_DLL */
# define UOCLIENTDLLAPI __declspec (dllimport)
#endif /* Not BUILDING_DLL */


#include "ALLOCATION.h"

#include <windows.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <winnt.h>
#include <winsock.h>
#include "Collection.h"

UOCLIENTDLLAPI void injectself(DWORD dwThreadId);
UOCLIENTDLLAPI void MultiClientPatch(DWORD pid);
LRESULT CALLBACK GetMsgHook(int code,WPARAM wParam,LPARAM lParam);

typedef struct StackElementStruct
{
	unsigned int element;
	struct StackElementStruct * next;
} StackElement;

typedef struct
{
	unsigned int itemcount;
	StackElement * top;
} Stack;

unsigned int getpacketsize(BYTE * packet);

Stack * newstack();
unsigned int stackpush(Stack * onstack,unsigned int topush);
unsigned int stackpop(Stack * fromstack);
void deletestack(Stack * todelete);
unsigned int dostdcall(unsigned int offset,Stack *parameterstack);
unsigned int doccall(unsigned int offset,Stack * parameterstack);
unsigned int dothiscall(unsigned int offset,Stack * parameterstack);
unsigned int dostdthiscall(unsigned int offset,Stack * parameterstack);
unsigned int domemcpy(Stack * memcpystack);

unsigned int SendHook(BYTE * tosend);
void __stdcall HandlePacketHook(BYTE * received);

void CreateEvents();
void CreateLocalIPCWindow(DWORD id);
LRESULT CALLBACK IPCWndProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam);

void Cleanup();

void _stdcall SendHookB(unsigned int para,unsigned int parb);
void _stdcall SendHookC(unsigned int para);

typedef struct VtblInfoStruct
{
	unsigned int vtbloffset;
	unsigned int functioncount;
	unsigned int * originalvtbl;
} VtblInfo;

//item defs:
/*#define oID 0x80
#define oContents 0xC0
#define oContentsNext 0x44//loops through contents
#define oNextItem 0x88//loops through main itemlist
#define oGumpNext 0x58//loops through any gump-list
#define oSubGumps 0x60*/

unsigned int FindGump(unsigned int tofind, unsigned int parent);
unsigned int FindItem(unsigned int tofind,unsigned int parent);

void SetVtblHook(unsigned int vtbloffset,unsigned int functioncount,unsigned int index, unsigned int newfunction);
unsigned int GetOriginalVtblEntry(unsigned int vtbloffset,unsigned int index);

void __stdcall ItemDestructorHook();
void __stdcall GumpDestructorHook(unsigned int argument);

BOOL AddGumpInstance(unsigned int toadd,unsigned int parent);
BOOL AddItemInstance(unsigned int toadd,unsigned int parent);

void DestructGump(unsigned int todestruct);
void DestructItem(unsigned int todestruct);

typedef void (__stdcall * pGumpDestructor)(unsigned int);
typedef void (__stdcall * pItemDestructor)();

DWORD WINAPI safe_stdthiscall(void * stack);

#endif

unsigned int docustomcall(Stack * parameterstack);

void __stdcall OnConnectionLossHook(unsigned int par);

typedef unsigned int (*MessageHandlerFunc)(unsigned int wParam,unsigned int lParam);

typedef struct
{
	unsigned int message;
	MessageHandlerFunc handler;
	char * name;
} MessageHandler;

int MessageHandlerCompare(void * a,void * b);

unsigned int handle_getitemnamemessage(unsigned int wParam, unsigned int lParam); //1
unsigned int handle_customcallmessage(unsigned int wParam, unsigned int lParam); //2
unsigned int handle_addreadermessage(unsigned int wParam, unsigned int lParam); //3
unsigned int handle_removereadermessage(unsigned int wParam, unsigned int lParam); //4
unsigned int handle_allocmessage(unsigned int wParam, unsigned int lParam); //5
unsigned int handle_freemessage(unsigned int wParam, unsigned int lParam); //6
unsigned int handle_newstackmessage(unsigned int wParam, unsigned int lParam); //7
unsigned int handle_pushmessage(unsigned int wParam, unsigned int lParam); //8
unsigned int handle_stdcallmessage(unsigned int wParam, unsigned int lParam); //9
unsigned int handle_ccallmessage(unsigned int wParam, unsigned int lParam); //10
unsigned int handle_thiscallmessage(unsigned int wParam, unsigned int lParam); //11
unsigned int handle_stdthiscallmessage(unsigned int wParam, unsigned int lParam); //12
unsigned int handle_memcpymessage(unsigned int wParam, unsigned int lParam); //13
unsigned int handle_memsetzeromessage(unsigned int wParam, unsigned int lParam); //14
//unsigned int handle_gethookinfomessage(unsigned int wParam, unsigned int lParam); //15
unsigned int handle_getcallibrationinfomessage(unsigned int wParam,unsigned int lParam);//15
//unsigned int handle_setcallibrationinfomessage(unsigned int wParam,unsigned int lParam);
unsigned int handle_clientlockmessage(unsigned int wParam, unsigned int lParam); //16
unsigned int handle_clientunlockmessage(unsigned int wParam, unsigned int lParam); //17
unsigned int handle_safestdthiscallmessage(unsigned int wParam, unsigned int lParam); //24
unsigned int handle_setpacketfiltermessage(unsigned int wParam, unsigned int lParam); //25
unsigned int handle_removepacketfiltermessage(unsigned int wParam, unsigned int lParam); //26
unsigned int handle_querypacketfiltermessage(unsigned int wParam, unsigned int lParam); //27
unsigned int handle_skipuoaistdthiscallmessage(unsigned int wParam, unsigned int lParam); //28
unsigned int handle_geteventportmessage(unsigned int wParam, unsigned int lParam);
unsigned int handle_finditembyidmessage(unsigned int wParam, unsigned int lParam);
unsigned int handle_finditembytypemessage(unsigned int wParam, unsigned int lParam);
unsigned int handle_finditembypositionmessage(unsigned int wParam,unsigned int lParam);
unsigned int handle_findmobilebyidmessage(unsigned int wParam, unsigned int lParam);
unsigned int handle_findmobilebytypemessage(unsigned int wParam, unsigned int lParam);
unsigned int handle_findmobilebypositionmessage(unsigned int wParam, unsigned int lParam);
unsigned int handle_patchencryptionmessage(unsigned int wParam, unsigned int lParam);

void PatchHooks();

int UOCUIntCompare(void * a,void * b);

typedef void (*UOCPacketHandler)(BYTE * packet,unsigned int size);

#define NEWITEM_PACKET 0x1A
#define ADDITEMTOCONTAINER_PACKET 0x25
#define CONTAINERCONTENTS_PACKET 0x3C

#define NEWMOBILE_PACKET 0x78
#define UPDATEMOBILE_PACKET 0x77

void NewItemHandler(BYTE * packet,unsigned int size);
void NewMobileHandler(BYTE * packet,unsigned int size);
void UpdateMobileHandler(BYTE * packet,unsigned int size);
void AddItemToContainerHandler(BYTE * packet,unsigned int size);
void ContainerContentsHandler(BYTE * packet,unsigned int size);

void FillCaches();

unsigned int ValidateAndLockItem(unsigned int itemid,unsigned int lparam);
unsigned int UnlockItem(unsigned int itemid, unsigned int lParam);

unsigned int GetItemList(unsigned int wParam, unsigned int lParam);

BOOL SetPrivilege(HANDLE hToken,LPCTSTR Privilege,BOOL bEnablePrivilege);
BOOL GetBackupPrivileges();
UOCLIENTDLLAPI BOOL GetDebugPrivilege();
void handle_message_if_available();

typedef void (__stdcall * pItemDestructor)();
unsigned int handle_hookitemdestructormessage(unsigned int para, unsigned int parb);
unsigned int handle_setupeventtimermessage(unsigned int para, unsigned int parb);

typedef struct IPCMessageStruct
{
	unsigned int responsecount;
	unsigned int message;
	unsigned int wParam;
	unsigned int lParam;
} IPCMessage;

int HandlePacketIPC(BOOL nested);

void windowmessagehandler();