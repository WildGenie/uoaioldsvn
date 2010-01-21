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
#include "SyncQueue.h"
#include "UOMLVersionInfo.h"

unsigned int initialized=0;
unsigned int eventtimersetup=0;

HANDLE clientlockevent;

unsigned int setupeventtimermessage=0;
unsigned int hookitemdestructormessage=0;

unsigned int allocmessage=0;
unsigned int freemessage=0;
unsigned int memcpymessage=0;
unsigned int memsetzeromessage=0;

unsigned int newstackmessage=0;
unsigned int pushmessage=0;

unsigned int customcallmessage=0;
unsigned int stdcallmessage=0;
unsigned int ccallmessage=0;
unsigned int thiscallmessage=0;
unsigned int stdthiscallmessage=0;
unsigned int safestdthiscallmessage=0;

unsigned int skipuoaistdthiscallmessage=0;

unsigned int getcallibrationinfomessage=0;
//unsigned int setcallibrationinfomessage=0;

unsigned int clientlockmessage=0;
unsigned int clientunlockmessage=0;

unsigned int setpacketfiltermessage=0;
unsigned int removepacketfiltermessage=0;
unsigned int querypacketfiltermessage=0;

unsigned int geteventportmessage=0;

unsigned int patchencryptionmessage=0;

BOOL skipuoai=FALSE;

BOOL filtered[256];

HWND LocalIPCHWnd=0;

CallibrationInfo * callibrations=0;

ServerSocket * eventserver=0;
ServerSocket * ipcserver=0;

BOOL droppacket;

#define TYPE_RECEIVED_PACKET 0
#define TYPE_SENT_PACKET 1
#define TYPE_KEY_DOWN 2
#define TYPE_KEY_UP 3
#define TYPE_CONN_LOSS 8
#define TYPE_PACKET_HANDLED 16
#define TYPE_DESTRUCTOR 32

#define TYPE_PACKET_PASS 0
#define TYPE_PACKET_CHANGED 1
#define TYPE_PACKET_FILTERED 2

unsigned int iskeydown=0;

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

void EnsureEventBufferSize(unsigned int size)
{
	while(eventbuffersize<size)
	{
		eventbuffer=wReAlloc(eventbuffer,eventbuffersize*2);
		eventbuffersize*=2;
	}
	return;
}

unsigned int BroadcastEvent(unsigned type, unsigned int size, BYTE * data)
{
	BinaryTreeEnum * btenum;
	ClientSocket * curclient;

	unsigned int response=0;
	unsigned int length=0;
	unsigned int rtype;
	unsigned int rsize;
	int status;

	if(eventserver)
	{
		//resize buffer if needed
		EnsureEventBufferSize(2*(size+8));
		
		//write event
		(*((unsigned int *)eventbuffer))=type;
		(*((unsigned int *)(eventbuffer+4)))=size;
		wCopy(eventbuffer+8,data,size);

		//broadcast/replace if needed
		Lock(eventserver->synclock);
		btenum=BT_newenum(eventserver->clients);
		while(curclient=(ClientSocket *)BT_next(btenum))
		{
			if(CSend(curclient,eventbuffer,size+8))
			{
				while((status=CHasData(curclient))==0)
					HandlePacketIPC(FALSE);

				if(status!=SOCKET_ERROR)
				{
					length=size+8;
					while(CRecv(curclient,eventbuffer+size+8,&length)&&(length>=4))
					{
						rtype=(*((unsigned int *)(eventbuffer+size+8)));
	
						if(rtype==TYPE_PACKET_CHANGED)
						{
							rsize=(*((unsigned int *)(eventbuffer+size+12)));
							if(rsize<=size)
							{
							//EnsureEventBufferSize(2*(rsize+8));
							wCopy(data,eventbuffer+size+16,rsize);
							wCopy(eventbuffer+8,data,rsize);
							//size=rsize;
							}
							length=size+8;
						}
						else
						{
							response|=rtype;
							break;
						}

						while((status=CHasData(curclient))==0)
							HandlePacketIPC(FALSE);
						if(status==SOCKET_ERROR)
							break;
					}
				}
			}
		}
		BT_enumdelete(btenum);
		Unlock(eventserver->synclock);
	}

	return response;
}

/*void BroadcastEvent(unsigned int type,unsigned int size,BYTE * data)
{
	if(eventserver)
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
		//wait for reply of each socket here
	}
}*/

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
	newmh->name=message;
	if(messagehandlers==0)
		messagehandlers=BT_create(MessageHandlerCompare);
	BT_insert(messagehandlers,(void *)newmh);
	return newmh->message;
}

LRESULT CALLBACK GetMsgHookb(int code,WPARAM wParam,LPARAM lParam)
{
	if(((MSG *)lParam)->message==WM_KEYDOWN)
	{
		if(InterlockedExchange(&iskeydown,1)==0)
		{
			//pass key down event to all UOAI instances
			BroadcastEvent(TYPE_KEY_DOWN,sizeof(unsigned int),(BYTE *)&(((MSG *)lParam)->wParam));
		}
	}
	else if(((MSG *)lParam)->message==WM_KEYUP)
	{
		if(InterlockedExchange(&iskeydown,0)==1)
		{
			//pass key up event to all UOAI instances
			BroadcastEvent(TYPE_KEY_UP,sizeof(unsigned int),(BYTE *)&(((MSG *)lParam)->wParam));
		}
	}

	return CallNextHookEx(0,code,wParam,lParam);
}

LRESULT CALLBACK GetMsgHook(int code,WPARAM wParam,LPARAM lParam)
{
	unsigned int i;

	if(!InterlockedExchange(&initialized,1))
	{
		//initilialization:

		debugprintf("initializing\n");

		//	-	setup messagehandlers

		setupeventtimermessage=NewMessageHandler("setupeventtimermessage",handle_setupeventtimermessage);
		hookitemdestructormessage=NewMessageHandler("hookitemdestructormessage",handle_hookitemdestructormessage);
		customcallmessage=NewMessageHandler("customcallmessage",handle_customcallmessage);
		allocmessage=NewMessageHandler("allocmessage",handle_allocmessage);
		freemessage=NewMessageHandler("freemessage",handle_freemessage);
		newstackmessage=NewMessageHandler("newstackmessage",handle_newstackmessage);
		pushmessage=NewMessageHandler("pushmessage",handle_pushmessage);
		stdcallmessage=NewMessageHandler("stdcallmessage",handle_stdcallmessage);
		ccallmessage=NewMessageHandler("ccallmessage",handle_ccallmessage);
		thiscallmessage=NewMessageHandler("thiscallmessage",handle_thiscallmessage);
		memcpymessage=NewMessageHandler("memcpymessage",handle_memcpymessage);
		getcallibrationinfomessage=NewMessageHandler("getcallibrationinfomessage",handle_getcallibrationinfomessage);
		//setcallibrationinfomessage=NewMessageHandler("setcallibrationinfomessage",handle_setcallibrationinfomessage);
		stdthiscallmessage=NewMessageHandler("stdthiscallmessage",handle_stdthiscallmessage);
		clientlockmessage=NewMessageHandler("clientlockmessage",handle_clientlockmessage);
		memsetzeromessage=NewMessageHandler("memsetzeromessage",handle_memsetzeromessage);
		setpacketfiltermessage=NewMessageHandler("setpacketfiltermessage",handle_setpacketfiltermessage);
		removepacketfiltermessage=NewMessageHandler("removepacketfiltermessage",handle_removepacketfiltermessage);
		querypacketfiltermessage=NewMessageHandler("querypacketfiltermessage",handle_querypacketfiltermessage);
		safestdthiscallmessage=NewMessageHandler("safestdthiscallmessage",handle_safestdthiscallmessage);
		skipuoaistdthiscallmessage=NewMessageHandler("skipuoaistdthiscallmessage",handle_skipuoaistdthiscallmessage);
		clientunlockmessage=RegisterWindowMessage("clientunlockmessage");//unlock is used within an embedded message loop in the clientlockmessage-handler and requires no seperate handlers		
		geteventportmessage=NewMessageHandler("geteventportmessage",handle_geteventportmessage);
		patchencryptionmessage=NewMessageHandler("patchencryptionmessage",handle_patchencryptionmessage);

		//	-	skipuoai boolean is used to prevent sent/received packets to passed to UOAI
		//		this is usefull when re-sending filtered packets

		skipuoai=FALSE;

		//	-	caches

		VtblInfoCacheLock=CreateSyncObject(INFINITE);
		VtblInfoCache=BT_create(UOCUIntCompare);

		//	- initially no packets are filtered

		for(i=0;i<256;i++)
		{
			filtered[i]=FALSE;
		}

		//	- old: eventqueue is used to pass packets and other events to UOAI instances
		//eventqueue=CreateMMQueue();

		//	- new: eventserver is a local socket server that passes packets and other events to UOAI
		eventserver=CreateServer(0,TRUE,0,0);
		ipcserver=CreateServer(0,TRUE,0,0);

		//	- setup broadcast buffer (initially 1 page)
		eventbuffersize=4096;
		eventbuffer=wAlloc(4096);

		
		//	-	clientlock message can only be posted, not sent (since there is no return)
		//		but we need to know when the posted message is received... at that point the client lock event is set
		//	- MessageHandledEvent : set when message was handled by the ipc_window_proc; this to emulate sendmessage

		CreateEvents();

		//	-	callibrate all required offsets (this is where the magic happens :) )

		callibrations=CallibrateUOClient();

		//	-	patch hooks (set send/recv/connection_loss hooks)
		PatchHooks();

		//	-	local ipc window which receives window messages that expose the injected dlls API
		//		by using windows messages to expose the API, everything is synchronized with the client

		CreateLocalIPCWindow(GetCurrentProcessId());
		
		//	-	Required, injection is done using a windows hook, if the application setting this hook is released, the library would get unloaded, unless its loaded internally too.

		LoadLibrary("UOClientDll.dll");//ensure uoclientdll is not unloaded when UOAI shuts down

		SetWindowsHookEx(WH_GETMESSAGE,(HOOKPROC)GetMsgHookb,GetModuleHandle("UOClientDll.dll"),GetCurrentThreadId());
	}
	
	//transparent hooking
	return CallNextHookEx(0,code,wParam,lParam);
}

unsigned int IsMobile(unsigned int oitem)
{
	unsigned int * vtbloffset;
	unsigned int result;

	void (*ismobilefunc)(void);
	
	if(oitem)
	{
		vtbloffset=(*((unsigned int **)oitem));
		ismobilefunc=(void (__cdecl *)(void ))vtbloffset[0x24];
		_asm
		{
			mov ecx, oitem
			call ismobilefunc;
			mov result, eax;
		}
	}

	return result;
}

UOCLIENTDLLAPI void injectself(DWORD dwThreadId)
{
	//old verison: SetWindowsHookEx(WH_CBT,(HOOKPROC)GetMsgHook,GetModuleHandle("UOClientDll.dll"),dwThreadId);
	
	debugprintf("injecting\n");

	//sets a getmessage hook and therefore effectively injects this dll into the specified trhead on the client's process
	curhook=SetWindowsHookEx(WH_GETMESSAGE,(HOOKPROC)GetMsgHook,GetModuleHandle("UOClientDll.dll"),dwThreadId);

	return;
}

UOCLIENTDLLAPI void MultiClientPatch(DWORD pid)
{
	ParsedFunction * winmain, * curfunc;
	Instruction * curins;

	unsigned int curtarget;

	enum x86_insn_type calltest[]={insn_call,insn_test};

	if(!isrunning(pid))
		return;

	//ensure libdisasm.dll was loaded:
	ASMParser_inits();

	//read entry point address from client's executable
	curtarget=RGetEntryPointOffset(pid);
	if(!curtarget)
		return;

	//parse complete entry point function (start chunk)
	curfunc=RASMParseFunction(pid,curtarget,TRUE);//parse start function chunk
	
	//winmain is the function with stack size 0x10 called from the start chunk
	winmain=RFuncFind(curfunc,0x10,FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=0;

	//find jcc in winmain, follow it
	ASMFirst(winmain);

	curins=ASMFind(winmain,insn_jcc,FALSE);

	ASMJumpToAddress(winmain,ParseAddress(curins));

	//follow first E8 call
	curins=ASMFind(winmain,insn_call,FALSE);
	while(RReadByte(pid,curins->address)!=0xE8)
		curins=ASMFind(winmain,insn_call,FALSE);

	curfunc=RASMParseFunction(pid,ParseAddress(curins),TRUE);

		//find call, test ... then patch the next jcc to a jmp
		curins=ASMFindMultiple(curfunc,calltest,2,FALSE);
		curins=ASMFind(curfunc,insn_jcc,FALSE);
//MultiClientPatch1
		curtarget=ParseAddress(curins);

		RWriteByte(pid,curins->address,0xE9);
		RWriteInt(pid,curins->address+1,curtarget-(curins->address+5));

		//find call, test ... then patch the next jcc to a jmp
		curins=ASMFindMultiple(curfunc,calltest,2,FALSE);
		curins=ASMFind(curfunc,insn_jcc,FALSE);
//MultiClientPatch2
		curtarget=ParseAddress(curins);

		RWriteByte(pid,curins->address,0xE9);
		RWriteInt(pid,curins->address+1,curtarget-(curins->address+5));
		//exit this subfunction, back to winmain
		DeleteParsedFunction(curfunc);
		curfunc=0;
	//find push 0x40 (messagebox type) (0x6A 0x40)
	while(curins=ASMFind(winmain,insn_push,FALSE))
	{
		if(RReadByte(pid,curins->address+curins->instruction.size-1)==0x40)
			break;
	}
	//previous jcc should become a jmp
	curins=ASMFind(winmain,insn_jcc,TRUE);
//MultiClientPatch3
	curtarget=ParseAddress(curins);

	RWriteByte(pid,curins->address,0xE9);
	RWriteInt(pid,curins->address+1,curtarget-(curins->address+5));
	//find "call xxxxxxxx; cmp eax, 0xB7;"
	while(curins=ASMFind(winmain,insn_cmp,FALSE))
	{
		if((curins->instruction.size>4)&&(RReadUInt(pid,curins->address+curins->instruction.size-4)==0xB7))
		{
			curins=ASMPrevious(winmain);
			if(curins->instruction.type==insn_call)
				break;
			ASMNext(winmain);
		}
	}
	//find previous and next jcc + patch them
	curins=ASMFind(winmain,insn_jcc,TRUE);
//MultiClientPatch4
	curtarget=ParseAddress(curins);

	RWriteByte(pid,curins->address,0xE9);
	RWriteInt(pid,curins->address+1,curtarget-(curins->address+5));
	curins=ASMFind(winmain,insn_jcc,FALSE);
//MultiClientPatch5
	curtarget=ParseAddress(curins);

	RWriteByte(pid,curins->address,0xE9);
	RWriteInt(pid,curins->address+1,curtarget-(curins->address+5));
	//exit winmain
	DeleteParsedFunction(winmain);

	return;
}

//useless?
void CreateEvents()
{
	char eventname[256]={'\0'};

	sprintf(eventname,"Global\\CLIENTLOCKEVENT_%x",GetCurrentProcessId());
	clientlockevent=CreateEvent(NULL,FALSE,FALSE,eventname);

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

/*unsigned int handle_setcallibrationinfomessage(unsigned int wParam,unsigned int lParam)
{
	callibrations=(CallibrationInfo *)wParam;
	PatchHooks();
	return 1;
}*/

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

unsigned int handle_skipuoaistdthiscallmessage(unsigned int wParam, unsigned int lParam)
{
	skipuoai=TRUE;
	dostdthiscall((unsigned int)wParam,(Stack *)lParam);
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
	Queue * msgqueue;
	MSG * curmsg;
	int toadd;

	SetEvent(clientlockevent);
	lockcount=1;

	msgqueue=CreateQueue();

	while(1)
	{
		if(PeekMessage(&msg,0,0,0,PM_REMOVE))
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
				{
					break;
				}
			}
			else
			{
				curmsg=(MSG *)wAlloc(sizeof(MSG));
				wCopy(curmsg,&msg,sizeof(MSG));
				DoQueue(msgqueue,(void *)curmsg);
			}
		}
		if((toadd=HandlePacketIPC(TRUE))!=0)
		{
			if(toadd>0)
			{
				lockcount++;
				SetEvent(clientlockevent);
			}
			else
			{
				lockcount--;
				if(lockcount==0)
				{
					break;
				}
			}
		}
	}

	while(curmsg=(MSG *)DeQueue(msgqueue))
	{
		TranslateMessage(curmsg);
		DispatchMessage(curmsg);
		wFree(curmsg);
	}

	DeleteQueue(msgqueue);

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
		{
			return (LRESULT)curhandler->handler(wParam,lParam);
		}
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
	unsigned int flags;

	_asm
	{
		mov psockobj, ecx
	}

	curpacket=(*tosend);
	psize=getpacketsize(tosend);
	toreturn=0;
	flags=0;
	droppacket=FALSE;
	/*if(psize)
	{
		if(skipuoai==FALSE)
		{
			//flags=BroadcastEvent(TYPE_SENT_PACKET,psize,tosend);
			if(flags&TYPE_PACKET_FILTERED)
			{
				toreturn=0;
				droppacket=TRUE;
			}
			else
			{
				_asm
				{
					mov ecx, psockobj
				}
				toreturn=callibrations->pOriginalSend(tosend);
			}
		}
		else
		{
			_asm
			{
				mov ecx, psockobj
			}
			toreturn=callibrations->pOriginalSend(tosend);
		}
	}*/

	if((skipuoai==FALSE)&&(psize>0))
	{
		flags=BroadcastEvent(TYPE_SENT_PACKET,psize,tosend);
		if(flags&TYPE_PACKET_FILTERED)
		{
			droppacket=TRUE;
			return 0;
		}
	}

	_asm
	{
		mov ecx, psockobj
	}
	toreturn=callibrations->pOriginalSend(tosend);

	if((skipuoai==FALSE)&&(psize>0))
		BroadcastEvent(TYPE_PACKET_HANDLED,1,tosend);

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
	//BYTE curpacket;

	_asm
	{
		mov psockobj, ecx
	}
	
	//curpacket=(*received);
	psize=getpacketsize(received);

	if((skipuoai==FALSE)&&(psize>0))
	{
		if(BroadcastEvent(TYPE_RECEIVED_PACKET,psize,received)&TYPE_PACKET_FILTERED)
			return;
	}
	
	_asm
	{
		mov ecx, psockobj
	}
	callibrations->pOriginalHandlePacket(received);

	BroadcastEvent(TYPE_PACKET_HANDLED,1,received);

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

	wFree(eventbuffer);

	CloseServer(eventserver);

	WSACleanup();

	CloseHandle(clientlockevent);

	while(curmh=(MessageHandler *)BT_leftmost(messagehandlers))
	{
		BT_remove(messagehandlers,(void *)curmh);
		wFree((void *)curmh);
	}
	BT_delete(messagehandlers);

	ASMParser_cleanup();

	initialized=0;

	return;
}

/*unsigned int IsMulti(unsigned int itemoffset)
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

unsigned int FindItemByID(unsigned int id,unsigned int parent)
{
	unsigned int founditem=0;
	unsigned int pCurrent=0;
	unsigned int oNext=0;

	if(parent==0)
	{
		pCurrent=(*((unsigned int *)(callibrations->pItemList)));
		oNext=callibrations->oItemNext;
	}
	else
	{
		if(IsMulti(parent))
		{
			pCurrent=(*((unsigned int *)(parent+0xA0)));
			pCurrent=(*((unsigned int *)(pCurrent)));
			oNext=4*4;
		}
		else
		{
			pCurrent=(*((unsigned int *)(parent+(callibrations->oItemContents))));
			oNext=callibrations->oItemContentsNext;
		}
	}
	while(pCurrent!=0)
	{
		if((*((unsigned int *)(pCurrent+0x80)))==id)
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


unsigned int FindItem(unsigned int tofind,unsigned int parent)
{
	unsigned int founditem=0;
	unsigned int pCurrent=0;
	unsigned int oNext=0;

	if(parent==0)
	{
		pCurrent=(*((unsigned int *)(callibrations->pItemList)));
		oNext=(callibrations->oItemNext);
	}
	else
	{
		if(IsMulti(parent))
		{
			pCurrent=(*((unsigned int *)(parent+0xA0)));
			pCurrent=(*((unsigned int *)(pCurrent)));
			oNext=4*4;
		}
		else
		{
			pCurrent=(*((unsigned int *)(parent+callibrations->oItemContents)));
			oNext=callibrations->oItemContentsNext;
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
		currentgump=(*((unsigned int *)(parent+(callibrations->oGumpSubGumpFirst))));

	while(currentgump)
	{
		if(currentgump==tofind)
		{
			foundgump=currentgump;
			break;
		}
		else
			currentgump=(*((unsigned int *)(currentgump+(callibrations->oGumpNext))));
	}
	
	return foundgump;
}*/

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
	}

	//install hook
	hProcess=GetCurrentProcess();
	VirtualProtectEx(hProcess,(void *)vtbloffset,sizeof(unsigned int)*functioncount,PAGE_EXECUTE_READWRITE,&prevprotect);
	(*((unsigned int *)(vtbloffset+index*sizeof(unsigned int))))=newfunction;
	VirtualProtectEx(hProcess,(void *)vtbloffset,sizeof(unsigned int)*functioncount,prevprotect,&prevprotect);
	CloseHandle(hProcess);

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

int UOCUIntCompare(void * a,void * b)
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

unsigned int handle_geteventportmessage(unsigned int wParam, unsigned int lParam)
{
	return (unsigned int)eventserver->port;
}

unsigned int handle_patchencryptionmessage(unsigned int wParam, unsigned int lParam)
{
	if((callibrations->sendcryptpatchpos)&&(callibrations->sendcryptpatchtarget))
	{
		WriteByte(callibrations->sendcryptpatchpos,0xE9);
		WriteInt(callibrations->sendcryptpatchpos+1,(callibrations->sendcryptpatchtarget)-(callibrations->sendcryptpatchpos+5));
	}

	if((callibrations->logincryptpatchpos)&&(callibrations->logincryptpatchtarget))
	{
		WriteByte(callibrations->logincryptpatchpos,0xE9);
		WriteInt(callibrations->logincryptpatchpos+1,(callibrations->logincryptpatchtarget)-(callibrations->logincryptpatchpos+5));
	}

	if((callibrations->recvcryptpatchpos)&&(callibrations->recvcryptpatchtarget))
	{
		WriteByte(callibrations->recvcryptpatchpos,0xE9);
		WriteInt(callibrations->recvcryptpatchpos+1,(callibrations->recvcryptpatchtarget)-(callibrations->recvcryptpatchpos+5));
	}
		
	return 1;
}

UOCLIENTDLLAPI BOOL GetDebugPrivilege()
{
	HANDLE hToken;

	if(!OpenThreadToken(GetCurrentThread(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, FALSE, &hToken))
    {
        if (GetLastError() == ERROR_NO_TOKEN)
        {
            if (!ImpersonateSelf(SecurityImpersonation))
				return FALSE;

            if(!OpenThreadToken(GetCurrentThread(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, FALSE, &hToken))
				return FALSE;
        }
        else
            return FALSE;
     }

    // enable SeDebugPrivilege
    if(!SetPrivilege(hToken, SE_DEBUG_NAME, TRUE))
    {
        CloseHandle(hToken);
        return FALSE;
    }

	CloseHandle(hToken);
	return TRUE;
}

BOOL GetBackupPrivileges()
{
	HANDLE hToken;

	if(!OpenThreadToken(GetCurrentThread(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, FALSE, &hToken))
    {
        if (GetLastError() == ERROR_NO_TOKEN)
        {
            if (!ImpersonateSelf(SecurityImpersonation))
				return FALSE;

            if(!OpenThreadToken(GetCurrentThread(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, FALSE, &hToken))
				return FALSE;
        }
        else
            return FALSE;
     }

    // enable SE_BACKUP_NAME 
    if(!SetPrivilege(hToken, SE_BACKUP_NAME, TRUE))
    {
        CloseHandle(hToken);
        return FALSE;
    }
	// enable SE_RESTORE_NAME
	if(!SetPrivilege(hToken, SE_RESTORE_NAME, TRUE))
    {
        CloseHandle(hToken);
        return FALSE;
    }

	CloseHandle(hToken);
	return TRUE;
}


BOOL SetPrivilege(HANDLE hToken,LPCTSTR Privilege,BOOL bEnablePrivilege)
{
    TOKEN_PRIVILEGES tp;
    LUID luid;
    TOKEN_PRIVILEGES tpPrevious;
    DWORD cbPrevious=sizeof(TOKEN_PRIVILEGES);

	if(!LookupPrivilegeValue( NULL, Privilege, &luid )) return FALSE;
 
    tp.PrivilegeCount           = 1;
    tp.Privileges[0].Luid       = luid;
    tp.Privileges[0].Attributes = 0;

    AdjustTokenPrivileges(hToken,FALSE,&tp,sizeof(TOKEN_PRIVILEGES),&tpPrevious,&cbPrevious);

    if (GetLastError() != ERROR_SUCCESS) return FALSE;

    tpPrevious.PrivilegeCount       = 1;
    tpPrevious.Privileges[0].Luid   = luid;

    if(bEnablePrivilege)
	{ 
		tpPrevious.Privileges[0].Attributes |= (SE_PRIVILEGE_ENABLED);
    }
    else
	{
        tpPrevious.Privileges[0].Attributes ^= (SE_PRIVILEGE_ENABLED & tpPrevious.Privileges[0].Attributes);
    }

    AdjustTokenPrivileges(hToken,FALSE,&tpPrevious,cbPrevious,NULL,NULL);

    if (GetLastError() != ERROR_SUCCESS) return FALSE;

    return TRUE;
}

void __stdcall ItemDestructorHook()
{
	pItemDestructor actualdestructor;
	unsigned int pThis;

	_asm
	{
		mov pThis, ecx
	}
	
	BroadcastEvent(TYPE_DESTRUCTOR,sizeof(unsigned int),(BYTE *)&pThis);

	actualdestructor=(pItemDestructor)GetOriginalVtblEntry((*((unsigned int *)pThis)),0);
	_asm
	{
		mov ecx, pThis
	}
	actualdestructor();

	return;
}

unsigned int handle_hookitemdestructormessage(unsigned int para, unsigned int parb)
{
	SetVtblHook((*((unsigned int *)para)),2,1,(unsigned int)ItemDestructorHook);
	return 1;
}

int HandlePacketIPC(BOOL nested)
{
	BinaryTreeEnum * btenum;
	ClientSocket * curclient;
	IPCMessage msg;
	unsigned int len;
	MessageHandler * mh;
	unsigned int response;
	int status;

	if(ipcserver==0)
	{
		return 0;
	}

	if(Lock(ipcserver->synclock))
	{
		btenum=BT_newenum(ipcserver->clients);
		while(curclient=(ClientSocket *)BT_next(btenum))
		{
			if((status=CHasData(curclient))&&(status!=SOCKET_ERROR))
			{
				len=sizeof(IPCMessage);
				if(CRecv(curclient,(BYTE *)&msg,&len))
				{
					if(nested)
					{
						if(msg.message==clientunlockmessage)
						{
							BT_enumdelete(btenum);
							Unlock(ipcserver->synclock);
							return -1;
						}
						else if(msg.message==clientlockmessage)
						{
							BT_enumdelete(btenum);
							Unlock(ipcserver->synclock);
							return +1;
						}
					}

					if(mh=(MessageHandler *)BT_find(messagehandlers,(void *)&(msg.message)))
					{
						response=mh->handler(msg.wParam,msg.lParam);
						if(msg.responsecount>0)
						{
							CSend(curclient,(BYTE *)&response,sizeof(unsigned int));
						}
					}
					else
					{
						if(msg.responsecount>0)
						{
							response=0;
							CSend(curclient,(BYTE *)&response,sizeof(unsigned int));
						}
					}
				}
			}
		}
		BT_enumdelete(btenum);
		Unlock(ipcserver->synclock);
	}
	return 0;
}

void EventTimerProc(HWND hWnd,unsigned int uMsg,unsigned int * idEvent, unsigned int dwTime)
{
	HandlePacketIPC(FALSE);
}

unsigned int handle_setupeventtimermessage(unsigned int para, unsigned int parb)
{
	debugprintf("setting up event timer, port is %x\n",ipcserver->port);
	if(InterlockedExchange(&eventtimersetup,1)==0)
		SetTimer(LocalIPCHWnd,0,250,(TIMERPROC)EventTimerProc);
	return ipcserver->port;
}
