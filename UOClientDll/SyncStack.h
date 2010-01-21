#ifndef SYNCSTACK_INCLUDED
#define SYNCSTACK_INCLUDED

#include "ALLOCATION.h"

#include <windows.h>
#include "Synchronization.h"

typedef struct StackElementStruct
{
	void * element;
	struct StackElementStruct * prev;
	struct StackElementStruct * next;
} StackElement;

typedef struct StackStruct
{
	SyncObject * synclock;
	StackElement * first;
	StackElement * last;
	unsigned int count;
	HANDLE pushevent;
} Stack;

void WaitOnStack(Stack * thequeue);
Stack * CreateStack();
void DeleteStack(Stack * todelete);
void StackPush(Stack * onstack,void * topush);
void * StackPop(Stack * fromstack);
unsigned int StackCount(Stack * thestack);

#endif
