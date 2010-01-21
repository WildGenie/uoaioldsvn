#ifndef SYNCQUEUE_INCLUDED
#define SYNCQUEUE_INCLUDED

#define _CRT_SECURE_NO_DEPRECATE

#include "ALLOCATION.h"

#include <windows.h>
#include "Synchronization.h"

typedef struct QueueElementStruct
{
	void * element;
	struct QueueElementStruct * prev;
	struct QueueElementStruct * next;
} QueueElement;

typedef struct QueueStruct
{
	SyncObject * synclock;
	QueueElement * first;
	QueueElement * last;
	unsigned int count;
	HANDLE queueevent;
} Queue;

void WaitOnQueue(Queue * thequeue);
Queue * CreateQueue();
void DeleteQueue(Queue * todelete);
void DoQueue(Queue * onqueue,void * toqueue);
void * DeQueue(Queue * onqueue);
unsigned int QueueCount(Queue * onqueue);

#endif