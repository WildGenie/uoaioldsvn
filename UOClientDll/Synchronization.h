//Synchronization Object (cfr. Metered Sections : http://msdn.microsoft.com/en-us/library/ms810428.aspx )

#ifndef SYNCHRONIZATION_INCLUDED
#define SYNCHRONIZATION_INCLUDED

#define _CRT_SECURE_NO_DEPRECATE

#include "ALLOCATION.h"

#include <windows.h>

typedef struct
{
	unsigned int timeout;
	LONG threadswaiting;
	LONG spinlock;
	BOOL available;
	LONG lockcount;
	HANDLE hEvent;
	DWORD threadid;//thread that has the lock
} SyncObject;

SyncObject * CreateSyncObject(unsigned int timeout);
void DeleteSyncObject(SyncObject * todelete);
BOOL Lock(SyncObject * tolock);
void Unlock(SyncObject * tounlock);

#endif