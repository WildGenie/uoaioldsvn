#include "SyncQueue.h"
#include <stdio.h>

Queue * CreateQueue()
{
	char queueeventname[256]={'\0'};
	Queue * newqueue=(Queue *)wAlloc(sizeof(Queue));
	newqueue->count=0;
	newqueue->first=0;
	newqueue->last=0;
	newqueue->synclock=CreateSyncObject(INFINITE);
	sprintf(queueeventname,"Local\\QUEUEEVENT_%x",(unsigned int)newqueue);
	newqueue->queueevent=CreateEvent(NULL,TRUE,FALSE,queueeventname);
	return newqueue;
}

void DoQueue(Queue * onqueue,void * toqueue)
{
	QueueElement * newqe=(QueueElement *)wAlloc(sizeof(QueueElement));

	newqe->element=toqueue;
	
	Lock(onqueue->synclock);
	
	newqe->next=onqueue->first;
	if(onqueue->first)
		onqueue->first->prev=newqe;
	newqe->prev=0;

	onqueue->first=newqe;

	if(!onqueue->last)
		onqueue->last=newqe;

	if(onqueue->count==0)
	{
		onqueue->count++;
		SetEvent(onqueue->queueevent);
	}
	else
		onqueue->count++;	

	Unlock(onqueue->synclock);
	
	return;
}

void * DeQueue(Queue * onqueue)
{
	QueueElement * toremove, * newend;
	void * toreturn;

	Lock(onqueue->synclock);

	if(onqueue->count>0)
	{
		toremove=onqueue->last;

		toreturn=toremove->element;

		newend=toremove->prev;
		if(newend)
		{
			newend->next=0;
			onqueue->last=newend;
		}
		else
		{
			onqueue->last=0;
			onqueue->first=0;
		}
		
		onqueue->count--;
		
		if(onqueue->count==0)
			ResetEvent(onqueue->queueevent);
	}
	else
	{
		toremove=0;
		toreturn=0;
	}

	Unlock(onqueue->synclock);

	if(toremove)
		wFree(toremove);

	return toreturn;
}

void DeleteQueue(Queue * todelete)
{
	Lock(todelete->synclock);

	while(todelete->first)
		wFree(DeQueue(todelete));

	Unlock(todelete->synclock);

	DeleteSyncObject(todelete->synclock);
	CloseHandle(todelete->queueevent);
	wFree(todelete);
	return;
}

unsigned int QueueCount(Queue * onqueue)
{
	unsigned int toreturn=0;
	Lock(onqueue->synclock);
	toreturn=onqueue->count;
	Unlock(onqueue->synclock);
	return toreturn;
}

void WaitOnQueue(Queue * thequeue)
{
	WaitForSingleObject(thequeue->queueevent,INFINITE);
	return;
}