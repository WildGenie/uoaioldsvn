#include "SyncStack.h"

Stack * CreateStack()
{
	char pusheventname[256]={'\0'};
	Stack * newstack=(Stack *)wAlloc(sizeof(Stack));
	newstack->count=0;
	newstack->first=0;
	newstack->last=0;
	newstack->synclock=CreateSyncObject(INFINITE);
	sprintf(pusheventname,"Local\\STACKPUSHEVENT_%x",(unsigned int)newstack);
	newstack->pushevent=CreateEvent(NULL,TRUE,FALSE,pusheventname);
	return newstack;
}

void StackPush(Stack * onstack,void * topush)
{
	StackElement * newse=(StackElement *)wAlloc(sizeof(StackElement));

	newse->element=topush;
	
	Lock(onstack->synclock);
	
	newse->next=onstack->first;
	if(onstack->first)
		onstack->first->prev=newse;
	newse->prev=0;

	onstack->first=newse;

	if(!onstack->last)
		onstack->last=newse;

	if(onstack->count==0)
	{
		onstack->count++;
		SetEvent(onstack->pushevent);
	}
	else
		onstack->count++;	

	Unlock(onstack->synclock);
	
	return;
}

void * StackPop(Stack * fromstack)
{
	StackElement * toremove, * newstart;
	void * toreturn;

	Lock(fromstack->synclock);

	if(fromstack->count>0)
	{
		toremove=fromstack->first;

		toreturn=toremove->element;

		newstart=toremove->next;
		if(newstart)
		{
			newstart->prev=0;
			fromstack->first=newstart;
		}
		else
		{
			fromstack->last=0;
			fromstack->first=0;
		}
		
		fromstack->count--;
		
		if(fromstack->count==0)
			ResetEvent(fromstack->pushevent);
	}
	else
	{
		toremove=0;
		toreturn=0;
	}

	Unlock(fromstack->synclock);

	if(toremove)
		wFree(toremove);

	return toreturn;
}

void DeleteStack(Stack * todelete)
{
	Lock(todelete->synclock);

	while(todelete->first)
		StackPop(todelete);
		//wFree(StackPop(todelete));

	Unlock(todelete->synclock);

	DeleteSyncObject(todelete->synclock);
	CloseHandle(todelete->pushevent);
	wFree(todelete);
	return;
}

unsigned int StackCount(Stack * onstack)
{
	unsigned int toreturn=0;
	Lock(onstack->synclock);
	toreturn=onstack->count;
	Unlock(onstack->synclock);
	return toreturn;
}

void WaitOnStack(Stack * thestack)
{
	WaitForSingleObject(thestack->pushevent,INFINITE);
	return;
}