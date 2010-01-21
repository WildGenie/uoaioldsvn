#if WINVER < 0x0500
#define  WINVER  0x0500
#endif

#define BUILDING_DLL

#include "UOClientDll.h"
#include "W2EDebug.h"
#include <winsock.h>
#include <stddef.h>
#include <windows.h>
#include "DebuggingTools.h"
#include "GeneralTools.h"
#include "BinaryTree.h"
#include "ASMParser.h"
#include "UOMLCallibrations.h"
#include "Sockets.h"
#include "Collection.h"

unsigned int initialized=0;

HANDLE clientlockevent;

unsigned int customcallmessage=0;//2
unsigned int allocmessage=0;//5
unsigned int freemessage=0;//6
unsigned int newstackmessage=0;//7
unsigned int pushmessage=0;//8
unsigned int stdcallmessage=0;//9
unsigned int ccallmessage=0;//10
unsigned int thiscallmessage=0;//11
unsigned int stdthiscallmessage=0;//12
unsigned int memcpymessage=0;//13
unsigned int memsetzeromessage=0;//14
//unsigned int gethookinfomessage=0;//15
unsigned int getcallibrationinfomessage=0;//15
unsigned int clientlockmessage=0;//16
unsigned int clientunlockmessage=0;//17
unsigned int additeminstancemessage=0;//18
unsigned int addgumpinstancemessage=0;//19
unsigned int lockiteminstancemessage=0;//20
unsigned int lockgumpinstancemessage=0;//21
unsigned int unlockiteminstancemessage=0;//22
unsigned int unlockgumpinstancemessage=0;//23
unsigned int safestdthiscallmessage=0;//24
unsigned int setpacketfiltermessage=0;//25
unsigned int removepacketfiltermessage=0;//26
unsigned int querypacketfiltermessage=0;//27
unsigned int skipuoaithiscallmessage=0;//28

BOOL skipuoai=FALSE;

BOOL filtered[256];

HWND LocalIPCHWnd=0;

//HookInfo hooks={SendHook,HandlePacketHook,0,0,0,0,SendHookB,0,SendHookC, 0, 0, 0,OnConnectionLossHook};

CallibrationInfo * callibrations=0;

//MMQueue * eventqueue=0;//receives packets & keys
ServerSocket * eventserver=0;

BOOL droppacket;

#define TYPE_RECEIVED_PACKET 0
#define TYPE_SENT_PACKET 1
#define TYPE_KEY_DOWN 2
#define TYPE_KEY_UP 3
#define TYPE_FILTERED_RECEIVED_PACKET 4
#define TYPE_FILTERED_SENT_PACKET 5
#define TYPE_DESTRUCTOR 6
#define TYPE_GUMP_DESTRUCTOR 7
#define TYPE_CONN_LOSS 8

unsigned int iskeydown=0;

SyncObject * ItemInstanceCacheLock=0;
BinaryTree * ItemInstanceCache=0;
SyncObject * GumpInstanceCacheLock=0;
BinaryTree * GumpInstanceCache=0;
SyncObject * VtblInfoCacheLock=0;
BinaryTree * VtblInfoCache=0;

BinaryTree * messagehandlers=0;

static HHOOK curhook=0;

BYTE * eventbuffer=0;
unsigned int eventbuffersize=0;

BOOL APIENTRY DllMain (HINSTANCE hInst     /* Library instance handle. */ ,
                       DWORD reason        /* Reason this function is being called. */ ,
                       LPVOID reserved     /* Not used. */ )
{
    switch (reason)
    {
      case DLL_PROCESS_ATTACH:
        break;

      case DLL_PROCESS_DETACH:
		  if(initialized)
			Cleanup();
        break;

      case DLL_THREAD_ATTACH:
        break;

      case DLL_THREAD_DETACH:
        break;
    }

    return TRUE;
}

void BroadcastEvent(unsigned int type,unsigned int size,BYTE * data)
{
	while(eventbuffersize<(size+8))
	{
		eventbuffer=wReAlloc(eventbuffer,eventbuffersize*2);
		eventbuffersize*=2;
	}
	(*((unsigned int *)eventbuffer))=type;
	(*((unsigned int *)(eventbuffer+4)))=size;
	wCopy(eventbuffer+8,data,size);
	CBroadcast(eventserver,eventbuffer,size+8);
}

int MessageHandlerCompare(void * a,void * b)
{
		unsigned int * ma=(unsigned int *)a;
		unsigned int * mb=(unsigned int *)b;
		if((*ma)>(*mb))
			return +1;
		else if((*ma)<(*mb))
			return -1;
		else
			return 0;
}

unsigned int NewMessageHandler(char * message, MessageHandlerFunc handlerfunc)
{
	MessageHandler * newmh=(MessageHandler *)wAlloc(sizeof(MessageHandler));
	newmh->message=RegisterWindowMessage(message);
	newmh->handler=handlerfunc;
	if(messagehandlers==0)
		messagehandlers=BT_create(MessageHandlerCompare);
	BT_insert(messagehandlers,(void *)newmh);
	return newmh->message;
}

LRESULT CALLBACK GetMsgHook(int code,WPARAM wParam,LPARAM lParam)
{
	unsigned int i;

	if(!InterlockedExchange(&initialized,1))
	{
		//initilialization:

		//	-	setup messagehandlers

		customcallmessage=NewMessageHandler("customcallmessage",handle_customcallmessage);

		allocmessage=NewMessageHandler("allocmessage",handle_allocmessage);

		freemessage=NewMessageHandler("freemessage",handle_freemessage);

		newstackmessage=NewMessageHandler("newstackmessage",handle_newstackmessage);

		pushmessage=NewMessageHandler("pushmessage",handle_pushmessage);

		stdcallmessage=NewMessageHandler("stdcallmessage",handle_stdcallmessage);

		ccallmessage=NewMessageHandler("ccallmessage",handle_ccallmessage);

		thiscallmessage=NewMessageHandler("thiscallmessage",handle_thiscallmessage);

		memcpymessage=NewMessageHandler("memcpymessage",handle_memcpymessage);

		//gethookinfomessage=NewMessageHandler("gethookinfomessage",handle_gethookinfomessage);
		getcallibrationinfomessage=NewMessageHandler("getcallibrationinfomessage",handle_getcallibrationinfomessage);

		stdthiscallmessage=NewMessageHandler("stdthiscallmessage",handle_stdthiscallmessage);

		clientlockmessage=NewMessageHandler("clientlockmessage",handle_clientlockmessage);

		memsetzeromessage=NewMessageHandler("memsetzeromessage",handle_memsetzeromessage);

		setpacketfiltermessage=NewMessageHandler("setpacketfiltermessage",handle_setpacketfiltermessage);

		removepacketfiltermessage=NewMessageHandler("removepacketfiltermessage",handle_removepacketfiltermessage);

		querypacketfiltermessage=NewMessageHandler("querypacketfiltermessage",handle_querypacketfiltermessage);

		addgumpinstancemessage=NewMessageHandler("addgumpinstancemessage",handle_addgumpinstancemessage);

		additeminstancemessage=NewMessageHandler("additeminstancemessage",handle_additeminstancemessage);

		lockgumpinstancemessage=NewMessageHandler("lockgumpinstancemessage",handle_lockgumpinstancemessage);

		lockiteminstancemessage=NewMessageHandler("lockiteminstancemessage",handle_lockiteminstancemessage);

		unlockgumpinstancemessage=NewMessageHandler("unlockgumpinstancemessage",handle_unlockgumpinstancemessage);

		unlockiteminstancemessage=NewMessageHandler("unlockiteminstancemessage",handle_unlockiteminstancemessage);

		safestdthiscallmessage=NewMessageHandler("safestdthiscallmessage",handle_safestdthiscallmessage);

		skipuoaithiscallmessage=NewMessageHandler("skipuoaithiscallmessage",handle_skipuoaithiscallmessage);

		clientunlockmessage=RegisterWindowMessage("clientunlockmessage");//unlock is used within an embedded message loop in the clientlockmessage-handler and requires no seperate handlers

		//	-	skipuoai boolean is used to prevent sent/received packets to passed to UOAI
		//		this is usefull when re-sending filtered packets

		skipuoai=FALSE;

		//	-	caches, will be changed soon

		ItemInstanceCacheLock=CreateSyncObject(INFINITE);
		ItemInstanceCache=BT_create(infocompare);
		GumpInstanceCacheLock=CreateSyncObject(INFINITE);
		GumpInstanceCache=BT_create(infocompare);
		VtblInfoCacheLock=CreateSyncObject(INFINITE);
		VtblInfoCache=BT_create(infocompare);

		//	- initially no packets are filtered

		for(i=0;i<256;i++)
			filtered[i]=FALSE;

		//	- old: eventqueue is used to pass packets and other events to UOAI instances
		//eventqueue=CreateMMQueue();

		//	- new: eventserver is a local socket server that passes packets and other events to UOAI
		eventserver=CreateServer(0,TRUE,0,0);
		
		//	-	useless?

		CreateClientLockEvent();

		//	-	callibrate all required offsets (this is where the magic happens :) )

		callibrations=CallibrateUOClient();

		//	-	patch hooks (set send/recv/connection_loss hooks)
		PatchHooks();

		//	-	local ipc window which receives window messages that expose the injected dlls API
		//		by using windows messages to expose the API, everything is synchronized with the client

		CreateLocalIPCWindow(GetCurrentProcessId());
		
		//	-	Required, injection is done using a windows hook, if the application setting this hook is released, the library would get unloaded, unless its loaded internally too.

		LoadLibrary("UOClientDll.dll");//ensure uoclientdll is not unloaded when UOAI shuts down
	}
	else if(((MSG *)lParam)->message==WM_KEYDOWN)
	{
		if(!iskeydown)
		{
			//pass key down event to all UOAI instances
			BroadcastEvent(TYPE_KEY_DOWN,sizeof(unsigned int),(BYTE *)&(((MSG *)lParam)->wParam));
			iskeydown=1;
		}
	}
	else if(((MSG *)lParam)->message==WM_KEYUP)
	{
		if(iskeydown)
		{
			//pass key up event to all UOAI instances
			BroadcastEvent(TYPE_KEY_UP,sizeof(unsigned int),(BYTE *)&(((MSG *)lParam)->wParam));
			iskeydown=0;
		}
	}
	//transparent hooking
	return CallNextHookEx(0,code,wParam,lParam);
}

UOCLIENTDLLAPI void injectself(DWORD dwThreadId)
{
	//old verison: SetWindowsHookEx(WH_CBT,(HOOKPROC)GetMsgHook,GetModuleHandle("UOClientDll.dll"),dwThreadId);
	
	//sets a getmessage hook and therefore effectively injects this dll into the specified trhead on the client's process
	curhook=SetWindowsHookEx(WH_GETMESSAGE,(HOOKPROC)GetMsgHook,GetModuleHandle("UOClientDll.dll"),dwThreadId);

	return;
}

//useless?
void CreateClientLockEvent()
{
	char clientlockeventname[256]={'\0'};

	sprintf(clientlockeventname,"Global\\CLIENTLOCKEVENT_%x",GetCurrentProcessId());
	clientlockevent=CreateEvent(NULL,FALSE,FALSE,clientlockeventname);
	
	return;
}

//local IPC window, created on the getmessage hook, which lives on the client's main thread and therefore message handling on this window is synchronized with the client
void CreateLocalIPCWindow(DWORD id)
{
	WNDCLASS rpcwinclass;

	char winname[30]={'\0'};

	sprintf(winname,"IPCWIN_%x",id);

	rpcwinclass.cbClsExtra=0;
	rpcwinclass.cbWndExtra=0;
	rpcwinclass.hbrBackground=0;
	rpcwinclass.hCursor=0;
	rpcwinclass.hIcon=0;
	rpcwinclass.hInstance=GetModuleHandle(0);
	rpcwinclass.lpfnWndProc=IPCWndProc;
	rpcwinclass.lpszClassName=winname;
	rpcwinclass.lpszMenuName=0;
	rpcwinclass.style=0;

	RegisterClass(&rpcwinclass);

	LocalIPCHWnd=CreateWindow(winname,winname,WS_OVERLAPPEDWINDOW,CW_USEDEFAULT,CW_USEDEFAULT,CW_USEDEFAULT,CW_USEDEFAULT,HWND_MESSAGE,0,GetModuleHandle(0),0);

	return;
}

//performs an stdthiscall on a seperate thread
//barely used, but usefull for debugging purposes
unsigned int handle_safestdthiscallmessage(unsigned int wParam, unsigned int lParam)
{
	HANDLE hThread;
	hThread=CreateThread(0,0,safe_stdthiscall,(void *)wParam,0,0);
	WaitForSingleObject(hThread,INFINITE);
	return 0;
}

//exposes memset(address, 0, size)
unsigned int handle_memsetzeromessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)memset((void *)wParam,0,(size_t)lParam);
}

unsigned int handle_additeminstancemessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)AddItemInstance(wParam,lParam);
}

unsigned int handle_addgumpinstancemessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)AddGumpInstance(wParam,lParam);
}

unsigned int handle_lockiteminstancemessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)LockItemInstance(wParam);
}

unsigned int handle_lockgumpinstancemessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)LockGumpInstance(wParam);
}

unsigned int handle_unlockiteminstancemessage(unsigned int wParam, unsigned int lParam)
{
	UnlockItemInstance(wParam);
	return 1;
}

unsigned int handle_unlockgumpinstancemessage(unsigned int wParam, unsigned int lParam)
{
	UnlockGumpInstance(wParam);
	return 1;
}

unsigned int handle_allocmessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)wAlloc((size_t)wParam);
}

unsigned int handle_freemessage(unsigned int wParam, unsigned int lParam)
{
	if(wParam!=0)
		wFree((void *)wParam);

	return 1;
}

/*unsigned int handle_gethookinfomessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)&hooks;
}*/
unsigned int handle_getcallibrationinfomessage(unsigned int wParam,unsigned int lParam)
{
	return (unsigned int)callibrations;
}

unsigned int handle_memcpymessage(unsigned int wParam, unsigned int lParam)
{
	return domemcpy((Stack *)wParam);
}

unsigned int handle_newstackmessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)newstack();
}

unsigned int handle_pushmessage(unsigned int wParam, unsigned int lParam)
{
	return stackpush((Stack *)wParam,(unsigned int)lParam);
}

unsigned int handle_stdcallmessage(unsigned int wParam, unsigned int lParam)
{
	return dostdcall((unsigned int)wParam,(Stack *)lParam);
}

unsigned int handle_ccallmessage(unsigned int wParam, unsigned int lParam)
{
	return doccall((unsigned int)wParam,(Stack *)lParam);
}

unsigned int handle_thiscallmessage(unsigned int wParam, unsigned int lParam)
{
	return dothiscall((unsigned int)wParam,(Stack *)lParam);
}

unsigned int handle_customcallmessage(unsigned int wParam, unsigned int lParam)
{
	return docustomcall((Stack *)wParam);
}

unsigned int handle_skipuoaithiscallmessage(unsigned int wParam, unsigned int lParam)
{
	skipuoai=TRUE;
	dothiscall((unsigned int)wParam,(Stack *)lParam);
	skipuoai=FALSE;
	return 1;
}

unsigned int handle_stdthiscallmessage(unsigned int wParam, unsigned int lParam)
{
	return dostdthiscall((unsigned int)wParam,(Stack *)lParam);
}

//client locking means entering an embedded message loop
//which effectively locks everything the client does outside its message loop
//this includes locking all packet sending/receiving (packet sending/recieving is done each time no windows messages require handling -- so each time peekmessage returns false... sent packets are queued until that point)
unsigned int handle_clientlockmessage(unsigned int wParam, unsigned int lParam)
{
	MSG msg;
	unsigned int lockcount;

	SetEvent(clientlockevent);
	lockcount=1;
	while(GetMessage(&msg,0,0,0))
	{
		if(msg.message==clientlockmessage)
		{
			lockcount++;
			SetEvent(clientlockevent);
		}
		else if(msg.message==clientunlockmessage)
		{
			lockcount--;
			if(lockcount==0)
				break;
		}
		else
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}
	return 1;
}

unsigned int handle_querypacketfiltermessage(unsigned int wParam, unsigned int lParam)
{
	if((wParam>=0)&&(wParam<256))
		return (unsigned int)filtered[(unsigned int)wParam];
	else
		return 0;
}

unsigned int handle_setpacketfiltermessage(unsigned int wParam, unsigned int lParam)
{
	if((wParam>=0)&&(wParam<256))
		filtered[(unsigned int)wParam]=TRUE;
	return 1;

}
unsigned int handle_removepacketfiltermessage(unsigned int wParam, unsigned int lParam)
{
	if((wParam>=0)&&(wParam<256))
		filtered[(unsigned int)wParam]=FALSE;
	return 1;
}

LRESULT CALLBACK IPCWndProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam)
{
	MessageHandler * curhandler;
	if(messagehandlers)
	{
		if(curhandler=(MessageHandler *)BT_find(messagehandlers,(void *)&uMsg))
			return (LRESULT)curhandler->handler(wParam,lParam);
		else
			return DefWindowProc(hwnd,uMsg,wParam,lParam);
	}
	else
		return DefWindowProc(hwnd,uMsg,wParam,lParam);
}

/* old version:
LRESULT CALLBACK IPCWndProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam)
{
	MSG msg;
	unsigned int lockcount=0;
	HANDLE hThread;

	if(uMsg==additeminstancemessage)
	{
		return AddItemInstance(wParam,lParam);
	}
	else if(uMsg==safestdthiscallmessage)
	{
		hThread=CreateThread(0,0,safe_stdthiscall,(void *)wParam,0,0);
		WaitForSingleObject(hThread,INFINITE);
		return 0;
	}
	else if(uMsg==memsetzeromessage)
	{
		return (LRESULT)memset((void *)wParam,0,(size_t)lParam);
	}
	else if(uMsg==addgumpinstancemessage)
	{
		return AddGumpInstance(wParam,lParam);
	}
	else if(uMsg==lockiteminstancemessage)
	{
		return LockItemInstance(wParam);
	}
	else if(uMsg==lockgumpinstancemessage)
	{
		return LockGumpInstance(wParam);
	}
	else if(uMsg==unlockiteminstancemessage)
	{
		UnlockItemInstance(wParam);
		return 1;
	}
	else if(uMsg==unlockgumpinstancemessage)
	{
		UnlockGumpInstance(wParam);
		return 1;
	}
	else if(uMsg==allocmessage)
	{
		return (LRESULT)wAlloc((size_t)wParam);
	}
	else if(uMsg==freemessage)
	{
		if(wParam!=0)
			wFree((void *)(wParam));
		return 1;
	}
	else if(uMsg==gethookinfomessage)
	{
		return (unsigned int)&hooks;
	}
	else if(uMsg==memcpymessage)
	{
		return domemcpy((Stack *)wParam);
	}
	else if(uMsg==newstackmessage)
	{
		return (LRESULT)newstack();
	}
	else if(uMsg==pushmessage)
	{
		return stackpush((Stack *)wParam,(unsigned int)lParam);
	}
	else if(uMsg==stdcallmessage)
	{
		return dostdcall((unsigned int)wParam,(Stack *)lParam);
	}
	else if(uMsg==ccallmessage)
	{
		return doccall((unsigned int)wParam,(Stack *)lParam);
	}
	else if(uMsg==thiscallmessage)
	{
		return dothiscall((unsigned int)wParam,(Stack *)lParam);
	}
	else if(uMsg==customcallmessage)
	{
		return docustomcall((Stack *)wParam);
	}
	else if(uMsg==skipuoaithiscallmessage)
	{
		skipuoai=TRUE;
		dothiscall((unsigned int)wParam,(Stack *)lParam);
		skipuoai=FALSE;
		return 1;
	}
	else if(uMsg==stdthiscallmessage)
	{
		return dostdthiscall((unsigned int)wParam,(Stack *)lParam);
	}
	else if(uMsg==clientlockmessage)
	{
		SetEvent(clientlockevent);
		lockcount=1;
		while(GetMessage(&msg,0,0,0))
		{
			if(msg.message==clientlockmessage)
			{
				lockcount++;
				SetEvent(clientlockevent);
			}
			else if(msg.message==clientunlockmessage)
			{
				lockcount--;
				if(lockcount==0)
					break;
			}
			else
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
		return 1;
	}
	else if(uMsg==querypacketfiltermessage)
	{
		if((wParam>=0)&&(wParam<256))
			return filtered[(unsigned int)wParam];
		else
			return 0;
	}
	else if(uMsg==setpacketfiltermessage)
	{
		if((wParam>=0)&&(wParam<256))
			filtered[(unsigned int)wParam]=TRUE;
		return 1;
	}
	else if(uMsg==removepacketfiltermessage)
	{
		if((wParam>=0)&&(wParam<256))
			filtered[(unsigned int)wParam]=FALSE;
		return 1;
	}
	else if(uMsg==addreadermessage)
	{
		MMQueueAddReader(eventqueue,(unsigned int)wParam,(unsigned int)lParam);
		return 1;
	}
	else if(uMsg==removereadermessage)
	{
		MMQueueRemoveReader(eventqueue,(unsigned int)wParam);
		return 1;
	}
	else
		return DefWindowProc(hwnd,uMsg,wParam,lParam);
}*/

unsigned int domemcpy(Stack * memcpystack)
{
	unsigned int size=stackpop(memcpystack);
	unsigned int to=stackpop(memcpystack);
	unsigned int from=stackpop(memcpystack);
	unsigned int toreturn=(unsigned int)memcpy((void *)to,(const void *)from,(size_t)size);
	deletestack(memcpystack);
	return toreturn;
}

Stack * newstack()
{
	Stack * toreturn=(Stack *)wAlloc(sizeof(Stack));
	toreturn->itemcount=0;
	toreturn->top=0;
	return toreturn;
}

unsigned int stackpush(Stack * onstack,unsigned int topush)
{
	StackElement * newse;
	
	if(onstack==0)
		return 0;

	newse=(StackElement *)wAlloc(sizeof(StackElement));
	newse->element=topush;
	newse->next=onstack->top;
	onstack->top=newse;
	onstack->itemcount++;
	return topush;
}

unsigned int stackpop(Stack * fromstack)
{
	StackElement * prevtop;
	unsigned int toreturn=0;

	if(fromstack==0)
		return 0;

	prevtop=fromstack->top;
	if(prevtop)
	{
		toreturn=prevtop->element;
		fromstack->top=prevtop->next;
		fromstack->itemcount--;
		wFree(prevtop);
	}
	return toreturn;
}

void deletestack(Stack * todelete)
{
	if(todelete)
	{
		while(todelete->itemcount)
			stackpop(todelete);
		wFree(todelete);
	}
	return;
}

unsigned int dostdcall(unsigned int offset,Stack * parameterstack)
{
	unsigned int toreturn=0;
	unsigned int par=0;
	unsigned int i=0;
	void (*testfunc)(void)=(void (__cdecl *)(void ))offset;

	if(!offset)
		toreturn=0;
	else if((!parameterstack)||(parameterstack->itemcount==0))
	{
		_asm
		{
			call testfunc
			mov toreturn, eax
		}
	}
	else
	{
		while(parameterstack->itemcount)
		{
			par=stackpop(parameterstack);
			_asm
			{
				push par
			}
		}
		_asm
		{
			call testfunc
			mov toreturn, eax
		}
	}
	if(parameterstack)
		deletestack(parameterstack);
	return toreturn;
}

unsigned int doccall(unsigned int offset,Stack * parameterstack)
{
	unsigned int toreturn=0;
	unsigned int par=0;
	unsigned int stacksize=0;
	unsigned int i=0;
	void (*testfunc)(void)=(void (__cdecl *)(void ))offset;

	if(!offset)
		toreturn=0;
	else if((!parameterstack)||(parameterstack->itemcount==0))
	{
		_asm
		{
			call testfunc
			mov toreturn, eax
		}
	}
	else
	{
		while(parameterstack->itemcount)
		{
			par=stackpop(parameterstack);
			_asm
			{
				push par
			}
			stacksize+=4;
		}
		_asm
		{
			call testfunc
			add esp, stacksize
			mov toreturn, eax
		}
	}
	if(parameterstack)
		deletestack(parameterstack);
	return toreturn;
}

unsigned int dothiscall(unsigned int offset,Stack * parameterstack)
{
	unsigned int toreturn=0;
	unsigned int par=0;
	unsigned int stacksize=0;
	unsigned int i=0;
	unsigned int thispar=0;
	void (*testfunc)(void)=(void (__cdecl *)(void ))offset;

	if(!offset)
		toreturn=0;
	else if((!parameterstack)||(parameterstack->itemcount==0))
		toreturn=0;//need at least the this parameter
	else
	{
		thispar=stackpop(parameterstack);
		if(thispar==0)
			toreturn=0;
		else
		{
			while(parameterstack->itemcount)
			{
				par=stackpop(parameterstack);
				_asm
				{
					push par
				}
				stacksize+=4;
			}
			_asm
			{
				mov ecx, thispar
				call testfunc
				add esp, stacksize
				mov toreturn, eax
			}
		}
	}
	if(parameterstack)
		deletestack(parameterstack);
	return toreturn;
}

unsigned int docustomcall(Stack * parameterstack)
{
	unsigned int offset=0;
	unsigned int toreturn=0;
	unsigned int par=0;
	unsigned int stacksize=0;
	unsigned int i=0;
	unsigned int thispar=0;
	void (*testfunc)(void);

	if(parameterstack)
	{
		offset=stackpop(parameterstack);

		if(offset)
		{
			testfunc=(void (__cdecl *)(void ))offset;
			thispar=stackpop(parameterstack);
			stacksize=stackpop(parameterstack);
			while(parameterstack->itemcount)
			{
				par=stackpop(parameterstack);
				_asm
				{
					push par
				}
			}
			if(thispar&&stacksize)
			{
				_asm
				{
					mov ecx, thispar
					call testfunc
					add esp, stacksize
					mov toreturn, eax
				}
			}
			else if(thispar)
			{
				_asm
				{
					mov ecx, thispar
					call testfunc
					mov toreturn, eax
				}
			}
			else if(stacksize)
			{
				_asm
				{
					call testfunc
					add esp, stacksize
					mov toreturn, eax
				}
			}
			else
			{
				_asm
				{
					call testfunc
					mov toreturn, eax
				}
			}
		}
	
		deletestack(parameterstack);
	}

	return toreturn;
}

//threadfunc for safe stdthiscall :: by making the call on a seperate thread, stack corruption gets less messy, which might be interesting during debugging/playing around.. typically this is not used though and as i didnt need it for any other calling sequences yet, it wasnt implemented for any other than the std-this-call (stack cleaned up by called function, parameters passed reversed and this-parameter passed through ecx register)
DWORD WINAPI safe_stdthiscall(void * stack)
{
	unsigned int calladdress;
	Stack * curstack=(Stack *)stack;
	calladdress=stackpop(curstack);
	return dostdthiscall(calladdress,curstack);
}


unsigned int dostdthiscall(unsigned int offset,Stack * parameterstack)
{
	unsigned int toreturn=0;
	unsigned int par=0;
	unsigned int i=0;
	unsigned int thispar=0;
	void (*testfunc)(void)=(void (__cdecl *)(void ))offset;

	if(!offset)
		toreturn=0;
	else if((!parameterstack)||(parameterstack->itemcount==0))
		toreturn=0;//need at least the this parameter
	else
	{
		thispar=stackpop(parameterstack);
		if(thispar==0)
			toreturn=0;
		else
		{
			while(parameterstack->itemcount)
			{
				par=stackpop(parameterstack);
				_asm
				{
					push par
				}
			}
			_asm
			{
				mov ecx, thispar
				call testfunc
				mov toreturn, eax
			}
		}
	}
	if(parameterstack)
		deletestack(parameterstack);
	return toreturn;
}

unsigned int SendHook(BYTE * tosend)
{
	unsigned int psockobj;
	unsigned int psize;
	BYTE curpacket;
	unsigned int toreturn;

	_asm
	{
		mov psockobj, ecx
	}

	curpacket=(*tosend);
	psize=getpacketsize(tosend);
	toreturn=0;
	droppacket=FALSE;
	if(psize)
	{
		if((skipuoai!=FALSE)||(filtered[curpacket]==FALSE))
		{
			//BroadcastEvent(TYPE_SENT_PACKET,psize,tosend);
			_asm
			{
				mov ecx, psockobj
			}
			toreturn=callibrations->pOriginalSend(tosend);
			if(skipuoai==FALSE)
			{
				BroadcastEvent(TYPE_SENT_PACKET,psize,tosend);
			}
		}
		else
		{
			//(*((unsigned int *)(psockobj+0xA00F0)))=0x10000+1;
			if(skipuoai==FALSE)
			{
				BroadcastEvent(TYPE_FILTERED_SENT_PACKET,psize,tosend);
			}
			toreturn=0;
			droppacket=TRUE;
		}
	}
	return toreturn;	
}

void _stdcall SendHookB(unsigned int para,unsigned int parb)
{
	unsigned int pObj;
	unsigned int pDest;

	_asm
	{
		mov pObj, ecx
	}

	if(droppacket)
		return;
	else
	{
		pDest=(unsigned int)callibrations->pOriginalSendb;
		_asm
		{
			mov ecx, pObj
			push para
			push parb
			call pDest
		}
	}
}
void _stdcall SendHookC(unsigned int para)
{
	unsigned int pObj;
	unsigned int pDest;

	_asm
	{
		mov pObj, ecx
	}

	if(droppacket)
		return;
	else
	{
		pDest=(unsigned int)callibrations->pOriginalSendc;
		_asm
		{
			mov ecx, pObj
			push para
			call pDest
		}
	}
}

void __stdcall HandlePacketHook(BYTE * received)
{
	unsigned int psockobj;
	unsigned int psize;
	BYTE curpacket;

	_asm
	{
		mov psockobj, ecx
	}
	
	curpacket=(*received);
	psize=getpacketsize(received);

	/*if(psize)
	{
		if(filtered[curpacket]==FALSE)
			BroadcastEvent(TYPE_RECEIVED_PACKET,psize,received);
		else
			BroadcastEvent(TYPE_FILTERED_RECEIVED_PACKET,psize,received);
	}*/

	if(filtered[curpacket]==FALSE)
	{
		_asm
		{
			mov ecx, psockobj
		}
		callibrations->pOriginalHandlePacket(received);
	}

	if(psize)
	{
		if(filtered[curpacket]==FALSE)
			BroadcastEvent(TYPE_RECEIVED_PACKET,psize,received);
		else
			BroadcastEvent(TYPE_FILTERED_RECEIVED_PACKET,psize,received);
	}

	return;
}

unsigned int getpacketsize(BYTE * packet)
{
	unsigned int packetsize;

	packetsize=(*((unsigned int *)(callibrations->pPacketInfo+3*(*packet)*4)));
	if(packetsize==0x8000)
	{
		packetsize=(unsigned int)((*((unsigned short *)(packet+1))));
	}

	return packetsize;
}

void Cleanup()
{
	MessageHandler * curmh;

	DestroyWindow(LocalIPCHWnd);

	/////TO BE ADDED HERE : CLEANUP OF CACHES

	CloseServer(eventserver);

	WSACleanup();

	CloseHandle(clientlockevent);

	//cleanup messages
	while(curmh=(MessageHandler *)BT_leftmost(messagehandlers))
	{
		BT_remove(messagehandlers,(void *)curmh);
		wFree((void *)curmh);
	}
	BT_delete(messagehandlers);

	ASMParser_cleanup();//cleanup of libdisasm if initialized

	initialized=0;

	return;
}

char * int2euo(unsigned int toconvert)
{
	char * toreturn=(char *)wAlloc(20);
	int i= (toconvert ^ 0x45) + 7, j=0;
	while(i) {toreturn[j] = (i % 26) + 'A'; i /= 26; ++j; }
	toreturn[j] = 0;
	return toreturn;
}

int infocompare(void * a,void * b)
{
	unsigned int * ia=(unsigned int *)a;
	unsigned int * ib=(unsigned int *)b;
	
	if((*ib)>(*ia))
		return 1;
	else if((*ib)<(*ia))
		return -1;
	else
		return 0;
}

unsigned int IsMulti(unsigned int itemoffset)
{
	if(itemoffset)
	{
		if((*((unsigned int *)(itemoffset+0xA0)))!=0)
			return 1;
		else
			return 0;
	}
	
	return 0;
}

unsigned int FindItem(unsigned int tofind,unsigned int parent)
{
	unsigned int founditem=0;
	unsigned int pCurrent=0;
	unsigned int oNext=0;

	if(parent==0)
	{
		pCurrent=(*((unsigned int *)(callibrations->pItemList)));
		oNext=oNextItem;
	}
	else
	{
		if(IsMulti(parent))
		{
			pCurrent=(*((unsigned int *)(parent+0xA0)));
			pCurrent=(*((unsigned int *)(parent)));
			oNext=4*4;
		}
		else
		{
			pCurrent=(*((unsigned int *)(parent+oContents)));
			oNext=oContentsNext;
		}
	}
	while(pCurrent!=0)
	{
		if(pCurrent==tofind)
		{
			founditem=pCurrent;
			break;
		}
		else
		{
			pCurrent=(*((unsigned int *)(pCurrent+oNext)));
		}
	}
	return founditem;
}


unsigned int FindGump(unsigned int tofind, unsigned int parent)
{
	unsigned int foundgump=0;
	unsigned int currentgump=0;

	if(parent==0)
		currentgump=(*((unsigned int *)(callibrations->pGumpList)));
	else
		currentgump=(*((unsigned int *)(parent+oSubGumps)));

	while(currentgump)
	{
		if(currentgump==tofind)
		{
			foundgump=currentgump;
			break;
		}
		else
			currentgump=(*((unsigned int *)(currentgump+oGumpNext)));
	}
	
	return foundgump;
}

void SetVtblHook(unsigned int vtbloffset,unsigned int functioncount,unsigned int index, unsigned int newfunction)
{
	VtblInfo * curvtbl;

	unsigned int * temp;

	HANDLE hProcess;
	DWORD prevprotect;

	Lock(VtblInfoCacheLock);

	if(curvtbl=BT_find(VtblInfoCache,&vtbloffset))//we already have this vtbl?
	{
		if(functioncount>curvtbl->functioncount)//more functions? -> get the ones we don't have yet
		{
			temp=curvtbl->originalvtbl;
			curvtbl->originalvtbl=(unsigned int *)wAlloc(sizeof(unsigned int)*functioncount);
			memcpy((void *)curvtbl->originalvtbl,(void *)vtbloffset,sizeof(unsigned int)*functioncount);//get functions we don't have + previous hooked vtbl
			memcpy((void *)curvtbl->originalvtbl,(void *)temp,sizeof(unsigned int)*curvtbl->functioncount);//as some might be hooked, we need the previous version
			curvtbl->functioncount=functioncount;
			wFree(temp);

			//install hook
			hProcess=GetCurrentProcess();
			VirtualProtectEx(hProcess,(void *)vtbloffset,sizeof(unsigned int)*functioncount,PAGE_EXECUTE_READWRITE,&prevprotect);
			(*((unsigned int *)(vtbloffset+index*sizeof(unsigned int))))=newfunction;
			VirtualProtectEx(hProcess,(void *)vtbloffset,sizeof(unsigned int)*functioncount,prevprotect,&prevprotect);
			CloseHandle(hProcess);
		}
	}
	else//insert new vtbl
	{
		//store vtbl information
		curvtbl=(VtblInfo *)wAlloc(sizeof(VtblInfo));
		curvtbl->functioncount=functioncount;
		curvtbl->originalvtbl=(unsigned int *)wAlloc(sizeof(unsigned int)*functioncount);
		memcpy((void *)curvtbl->originalvtbl,(void *)vtbloffset,sizeof(unsigned int)*functioncount);//get original vtbl
		curvtbl->vtbloffset=vtbloffset;
		BT_insert(VtblInfoCache,curvtbl);

		//install the hook
		hProcess=GetCurrentProcess();
		VirtualProtectEx(hProcess,(void *)vtbloffset,sizeof(unsigned int)*functioncount,PAGE_EXECUTE_READWRITE,&prevprotect);
		(*((unsigned int *)(vtbloffset+index*sizeof(unsigned int))))=newfunction;
		VirtualProtectEx(hProcess,(void *)vtbloffset,sizeof(unsigned int)*functioncount,prevprotect,&prevprotect);
		CloseHandle(hProcess);
	}

	Unlock(VtblInfoCacheLock);

	return;
}

unsigned int GetOriginalVtblEntry(unsigned int vtbloffset,unsigned int index)
{
	VtblInfo * curvtbl;
	unsigned int toreturn=0;
	unsigned int * actualvtbl;

	Lock(VtblInfoCacheLock);

	if(curvtbl=BT_find(VtblInfoCache,&vtbloffset))
	{
		if(index<curvtbl->functioncount)
		{
			toreturn=curvtbl->originalvtbl[index];
		}
	}

	if(toreturn==0)
	{
		actualvtbl=(unsigned int *)vtbloffset;
		toreturn=actualvtbl[index];
	}

	Unlock(VtblInfoCacheLock);

	return toreturn;
}

BOOL AddGumpInstance(unsigned int toadd,unsigned int parent)
{
	InstanceInfo * curinstance;
	BOOL toreturn=FALSE;

	Lock(GumpInstanceCacheLock);

	if((toadd!=0)&&(FindGump(toadd,parent)==toadd))
	{
		if((curinstance=BT_find(GumpInstanceCache,&toadd))==0)
		{
			curinstance=(InstanceInfo *)wAlloc(sizeof(InstanceInfo));
			curinstance->deleted=FALSE;
			curinstance->lockcount=0;
			curinstance->offset=toadd;
			curinstance->parent=parent;
			SetVtblHook((*((unsigned int *)toadd)),1,0,(unsigned int)GumpDestructorHook);
			BT_insert(GumpInstanceCache,curinstance);
		}
		toreturn=TRUE;
	}

	Unlock(GumpInstanceCacheLock);

	return toreturn;
}

BOOL AddItemInstance(unsigned int toadd,unsigned int parent)
{
	InstanceInfo * curinstance;
	BOOL toreturn=FALSE;

	Lock(ItemInstanceCacheLock);

	if((toadd!=0)&&(toadd!=(unsigned int)-1))
	{
		if(FindItem(toadd,parent)==toadd)
		{
			toreturn=TRUE;
			if((curinstance=BT_find(ItemInstanceCache,&toadd))==0)//if not already present, insert new one
			{
				curinstance=(InstanceInfo *)wAlloc(sizeof(InstanceInfo));
				curinstance->deleted=FALSE;
				curinstance->lockcount=0;
				curinstance->offset=toadd;
				curinstance->parent=parent;
				SetVtblHook((*((unsigned int *)toadd)),2,1,(unsigned int)ItemDestructorHook);
				BT_insert(ItemInstanceCache,curinstance);
			}
		}
	}

	Unlock(ItemInstanceCacheLock);

	return toreturn;
}

void __stdcall ItemDestructorHook()
{
	unsigned int pItem;
	InstanceInfo * curinstance;

	_asm
	{
		mov pItem, ecx
	}

	Lock(ItemInstanceCacheLock);

	if(pItem!=0)
	{
		if(curinstance=BT_find(ItemInstanceCache,&pItem))
		{
			if(curinstance->lockcount==0)
			{		
				BroadcastEvent(TYPE_DESTRUCTOR,sizeof(unsigned int),(BYTE *)&pItem);
				BT_remove(ItemInstanceCache,curinstance);
				wFree(curinstance);
				DestructItem(pItem);
			}
			else
				curinstance->deleted=TRUE;
		}
		else
			DestructItem(pItem);
	}
	else
		DestructItem(pItem);

	Unlock(ItemInstanceCacheLock);

	return;
}

void __stdcall GumpDestructorHook(unsigned int argument)
{
	unsigned int pGump;
	InstanceInfo * curinstance;

	_asm
	{
		mov pGump, ecx
	}

	Lock(GumpInstanceCacheLock);
	if((pGump!=0)&&(argument==1)&&(curinstance=BT_find(GumpInstanceCache,&pGump)))
	{
		if(curinstance->lockcount==0)
		{
			BroadcastEvent(TYPE_GUMP_DESTRUCTOR,sizeof(unsigned int),(BYTE *)&pGump);
			BT_remove(GumpInstanceCache,curinstance);
			wFree(curinstance);
			DestructGump(pGump);
		}
		//else
		//	curinstance->deleted=TRUE;
	}
	else
		DestructGump(pGump);
	Unlock(GumpInstanceCacheLock);

	return;
}

BOOL LockItemInstance(unsigned int tolock)
{
	BOOL toreturn=FALSE;

	unsigned int containeritem, itemgump;

	InstanceInfo * curinstance;

	Lock(ItemInstanceCacheLock);

	containeritem=(*((unsigned int *)(tolock+0x84)));
	itemgump=(*((unsigned int *)(tolock+0xC4)));

	//debugprintf("locking item %x\n",tolock);

	if((containeritem!=0)&&(containeritem!=(unsigned int)-1))
	{
		if(AddItemInstance(containeritem,0))
		{
			//debugpush();
			LockItemInstance(containeritem);
			//debugpop();
		}
	}

	if(AddGumpInstance(itemgump,0))
	{
		LockGumpInstance(itemgump);
	}

	if(curinstance=BT_find(ItemInstanceCache,&tolock))
	{
		if(curinstance->deleted==FALSE)
		{
			curinstance->lockcount++;
			toreturn=TRUE;
		}
	}

	Unlock(ItemInstanceCacheLock);

	return toreturn;
}

BOOL LockGumpInstance(unsigned int tolock)
{
	BOOL toreturn=FALSE;

	InstanceInfo * curinstance;

	Lock(GumpInstanceCacheLock);

	if(curinstance=BT_find(GumpInstanceCache,&tolock))
	{
		//if(curinstance->deleted==FALSE)
		//{
			curinstance->lockcount++;
			toreturn=TRUE;
		//}
	}

	Unlock(GumpInstanceCacheLock);

	return toreturn;
}

void UnlockItemInstance(unsigned int tounlock)
{
	InstanceInfo * curinstance;

	unsigned int containeritem,itemgump;

	Lock(ItemInstanceCacheLock);

	containeritem=(*((unsigned int *)(tounlock+0x84)));
	itemgump=(*((unsigned int *)(tounlock+0xC4)));

	//debugprintf("unlocking item %x\n",tounlock);

	if(curinstance=BT_find(ItemInstanceCache,&tounlock))
	{
		if(curinstance->lockcount>0)
			curinstance->lockcount--;
		if((curinstance->deleted)&&(curinstance->lockcount==0))
		{
			BroadcastEvent(TYPE_DESTRUCTOR,sizeof(unsigned int),(BYTE *)&tounlock);
			BT_remove(ItemInstanceCache,curinstance);
			wFree(curinstance);
			//debugprintf("destructing %x\n",tounlock);
			DestructItem(tounlock);
			//debugprintf("destroyed %x\n",tounlock);
		}
	}

	if((containeritem!=0)&&(containeritem!=(unsigned int)-1))
	{
		//debugpush();
		UnlockItemInstance(containeritem);
		//debugpop();
	}

	if(itemgump)
		UnlockGumpInstance(itemgump);

	Unlock(ItemInstanceCacheLock);

	return;
}

void UnlockGumpInstance(unsigned int tounlock)
{
	InstanceInfo * curinstance;

	Lock(GumpInstanceCacheLock);
	if(curinstance=BT_find(GumpInstanceCache,&tounlock))
	{
		if(curinstance->lockcount>0)
			curinstance->lockcount--;
		/*if((curinstance->deleted)&&(curinstance->lockcount==0))
		{
			//delayed destruction
			BroadcastEvent(TYPE_GUMP_DESTRUCTOR,sizeof(unsigned int),(BYTE *)&tounlock);
			BT_remove(GumpInstanceCache,curinstance);
			wFree(curinstance);
			DestructGump(tounlock);
		}*/
	}
	Unlock(GumpInstanceCacheLock);

	return;
}

void DestructGump(unsigned int todestruct)
{
	pGumpDestructor odestr;
	
	odestr=(pGumpDestructor)GetOriginalVtblEntry((*((unsigned int *)todestruct)),0);
	_asm
	{
		mov ecx, todestruct
	}
	odestr(1);
	return;
}

void DestructItem(unsigned int todestruct)
{
	pItemDestructor odestr;

	//debugprintf("destructing item %x\n",todestruct);

	//if this is a container destroy all contents first?
		
	odestr=(pItemDestructor)GetOriginalVtblEntry((*((unsigned int *)todestruct)),1);
	
	//debugprintf("destructorfunc: %x\n",(unsigned int)odestr);
	//debugpush();
	_asm
	{
		mov ecx, todestruct
	}
	odestr();

	//debugpop();
	//debugprintf("item %x destructed\n",todestruct);

	return;
}

short * duplicateunicodestring(short * ustr)
{
	short * toreturn;
	unsigned int curpos=0;
	//get length
	while((*(ustr+curpos))!=0)
		curpos++;
	if(curpos==0)
		toreturn=0;
	else
	{
		toreturn=(short *)wAlloc(sizeof(short)*curpos);
		memcpy((void *)toreturn,(const void *)ustr,sizeof(short)*curpos);
	}
	return toreturn;	
}

void __stdcall OnConnectionLossHook(unsigned int par)
{
	callibrations->pOriginalConnLossHandler(par);

	BroadcastEvent(TYPE_CONN_LOSS,4,(BYTE *)&par);

	return;
}

void PatchHooks()
{
	int reloffset;

	//store hook info
	callibrations->pHandlePacketHook=(unsigned int)HandlePacketHook;
	callibrations->pSendHook=(unsigned int)SendHook;
	callibrations->pSendbHook=(unsigned int)SendHookB;
	callibrations->pSendcHook=(unsigned int)SendHookC;
	callibrations->pConnLossHook=(unsigned int)OnConnectionLossHook;

	//set hooks:

	//handle packet hook (aka recv hook, this hooks just after receiving and decoding/decrypting the packet... the hook is a hook of the function that handles different packets - in the network object's vtbl)
	WriteUInt(callibrations->pSocketObjectVtbl+0x20,callibrations->pHandlePacketHook);

	//send hook (aka "queuepacket" hook -> the client uses one single function to write and queue a packet, sending is done later on from the program's main loop, the hook is a hook of the first function called from this "queuepacket" function, as the queuepacket function itself is called from too many places to hook it.)
	reloffset=callibrations->pSendHook-(callibrations->pPatchPos2+4);
	WriteInt(callibrations->pPatchPos2,reloffset);
	
	//next two function calls in the "queuepacket" function must be hooked to for the purpose of packet filtering... this is required as i couldnt hook the queuepacket function directly, so i can't just return and ignore the packet from the first hook
	reloffset=callibrations->pSendbHook-(callibrations->pPatchPos3+4);
	WriteInt(callibrations->pPatchPos3,reloffset);
	reloffset=callibrations->pSendcHook-(callibrations->pPatchPos4+4);
	WriteInt(callibrations->pPatchPos4,reloffset);

	//hook of a function called when the clients network socket looses connection (in the network objects vtbl)
	WriteUInt(callibrations->pSocketObjectVtbl+3*4,callibrations->pConnLossHook);

	return;
}