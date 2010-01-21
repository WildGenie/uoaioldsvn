#include <windows.h>
#include "Synchronization.h"

SyncObject * CreateSyncObject(unsigned int timeout)
{
	SyncObject * toreturn=(SyncObject *)wAlloc(sizeof(SyncObject));
	toreturn->threadswaiting=0;
	toreturn->timeout=timeout;
	toreturn->available=TRUE;
	toreturn->lockcount=0;
	toreturn->spinlock=0;
	toreturn->hEvent=CreateEvent(NULL, FALSE, FALSE, NULL);
	toreturn->threadid=0;
	return toreturn;
}

void DeleteSyncObject(SyncObject * todelete)
{
	if(todelete->hEvent)
		CloseHandle(todelete->hEvent);
	wFree((void *)todelete);
	return;
}

BOOL Lock(SyncObject * tolock)
{
	DWORD tid=GetCurrentThreadId();

	while(1)
	{
		while(InterlockedExchange(&(tolock->spinlock),1)!=0)//a. Spinlock access to the sync object itself
			Sleep(0);

		if((!tolock->available)&&(tolock->threadid==tid))//b. Do we already have the lock?
		{
			tolock->lockcount++;//just increase lockcount (to prevent deadlocks on nested locking)

			InterlockedExchange(&(tolock->spinlock),0);//release the spinlock on the syncobject
			
			return TRUE;
		}
		else if(tolock->available)//c. Is The lock available?
		{
			tolock->available=FALSE;//claim the object
			tolock->lockcount=1;
			tolock->threadid=tid;	
			
			InterlockedExchange(&(tolock->spinlock),0);//release the spinlock on the syncobject			
			
			return TRUE;
		}
		else//d. If not available, wait for it!
		{
			tolock->threadswaiting++;//we're gonna have to wait...

			ResetEvent(tolock->hEvent);//reset the sync objects event (event is triggered when the object becomes available)
	
			InterlockedExchange(&(tolock->spinlock),0);//release the spinlock on the syncobject
	
			if(WaitForSingleObject(tolock->hEvent,tolock->timeout)==WAIT_TIMEOUT)//wait for timeout or for the object to become available
				return FALSE;
		}
	}
}

void Unlock(SyncObject * tounlock)
{
	while(InterlockedExchange(&(tounlock->spinlock),1)!=0)//Spinlock access to the sync object itself
		Sleep(0);

	if(!(tounlock->available))//verify the object is in fact locked
	{
		if(tounlock->threadid==GetCurrentThreadId())//verify the calling thread has the lock
		{
			if(tounlock->lockcount)//decrease lockcount (nested locking)
				tounlock->lockcount--;
			
			if(tounlock->lockcount==0)//should the object be released? (nested locking)
			{
				tounlock->available=TRUE;//release the object
				tounlock->threadid=0;
				tounlock->lockcount=0;
				
				if(tounlock->threadswaiting)//any thread waiting for it to become available?
				{
					tounlock->threadswaiting--;//pass control to one waiting thread
					SetEvent(tounlock->hEvent);//trigger the event for that waiting thread
				}
			}
		}
	}

	InterlockedExchange(&(tounlock->spinlock),0);//release the spinlock on the syncobject

	return;
}