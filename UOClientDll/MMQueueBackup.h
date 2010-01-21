#ifndef MMQUEUE_INCLUDED
#define MMQUEUE_INCLUDED

#define _CRT_SECURE_NO_DEPRECATE

//IPC through memory-mapped file : writer Queue -> memory mapped file -> reader Queue, 1 writer, multiple readers allowed

#include <windows.h>
#include "SyncQueue.h"
#include <stdio.h>
#include "DebuggingTools.h"

#define MAX_TYPE 6

#define MAX_WAIT_ATTEMPTS 5

//#define USE_GLOBAL_ALLOCATION

#define MMQUEUETIMEOUT 10000//maximum time in milliseconds the writer will wait for a reader to read the data

typedef void (*MMQueueCallback)(void * extra,BYTE * bytes,unsigned int type,unsigned int size);

typedef struct MMQueueReaderStruct
{
	unsigned int readerpid;
	HANDLE writtenevent;
	HANDLE readevent;
	struct MMQueueReaderStruct * next;
	struct MMQueueReaderStruct * previous;
} MMQueueReader;

typedef struct MMQueueElementStruct
{
	unsigned int type;
	unsigned int size;
	BYTE * data;
} MMQueueElement;

typedef struct MMQueueStruct
{
	//reader or writer?
	unsigned int isWriter;

	//queue/dequeue thread id/handle
	DWORD queuethreadid;
	HANDLE queuethreadhandle;

	//internal queue, information goes either from the mm-file->syncqueue or from syncqueue->mm-file
	Queue * internalqueue;

	//  - filemapping handle
	HANDLE filemapping;

	// - actual mm-file
	BYTE * queuebuffer;

	//readers (pid+write/readevents)
	MMQueueReader * readers;

	// - global shutdownevents
	HANDLE globalshutdown;//used by a writer, to release all readers at once (writer shutdown)

	// - this readers read/written events (if it is a reader)
	HANDLE writtenevent;
	HANDLE readevent;

	unsigned int queuenumber;

	unsigned int writerpid;

	MMQueueCallback onqueue;

	void * extra;

	unsigned int keeprunning;
} MMQueue;

MMQueue * CreateMMQueue();
MMQueue * OpenMMQueue(DWORD remotepid);
void CloseMMQueue(MMQueue * toclose);

DWORD WINAPI MMQueueReaderThread(void * thequeue);
DWORD WINAPI MMQueueWriterThread(void * thequeue);

void MMDoQueue(MMQueue * onqueue,unsigned int type,unsigned int size,BYTE * data);
BYTE * MMDeQueue(MMQueue * offqueue,unsigned int * type,unsigned int * size);

void MMQueueRemoveReader(MMQueue * thisqueue,unsigned int readerpid);
void MMQueueAddReader(MMQueue * thisqueue,unsigned int readerpid,unsigned int queuenumber);

#endif