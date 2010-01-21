#include "MMQueue.h"
//#include "W2EDebug.h"

#include "DebuggingTools.h" //for isrunning-function

unsigned int queuecount=0;

//IPC through memory-mapped file : writer Queue -> memory mapped file -> reader Queue, 1 writer, multiple readers allowed
//MAX 1 WRITER PER PROCESS!!!!!!!!

MMQueue * CreateMMQueue()
{
	MMQueue * toreturn;

	char globalshutdownname[256]={'\0'};
	char mmfilename[256]={'\0'};

	unsigned int curpid;

	curpid=GetCurrentProcessId();

	sprintf(globalshutdownname,"MMQUEUE_GS_%x",curpid);
	sprintf(mmfilename,"MMQUEUE_MF_%x",curpid);

/* allocate descriptor */
#ifdef USE_GLOBAL_ALLOCATION
	toreturn=(MMQueue *)GlobalAlloc(GMEM_FIXED,sizeof(MMQueue));
#else
	toreturn=(MMQueue *)malloc(sizeof(MMQueue));
#endif	

	toreturn->keeprunning=1;

/* create events */
	toreturn->globalshutdown=CreateEvent(NULL,TRUE,FALSE,globalshutdownname);
	toreturn->readevent=0;
	toreturn->writtenevent=0;

/* create mm-file (one memory page backed by the system page file) */

	toreturn->filemapping=CreateFileMapping(INVALID_HANDLE_VALUE,0,PAGE_READWRITE,0,4096,mmfilename);
	toreturn->queuebuffer=MapViewOfFile(toreturn->filemapping,FILE_MAP_READ|FILE_MAP_WRITE,0,0,4096);

/* create internal queue */
	toreturn->internalqueue=CreateQueue();

/* we are the writer */
	toreturn->isWriter=1;

/* no readers yet */
	toreturn->readers=0;

/* no onqueue callback, no extra */
	toreturn->onqueue=0;
	toreturn->extra=0;

/* create writer thread */
	toreturn->queuethreadhandle=CreateThread(NULL,0,MMQueueWriterThread,(void *)toreturn,0,&(toreturn->queuethreadid));

	toreturn->queuenumber=0;

	return toreturn;
}

MMQueue * OpenMMQueue(DWORD remotepid)
{
	MMQueue * toreturn;

	char globalshutdownname[256]={'\0'};
	char mmfilename[256]={'\0'};
	char readname[256]={'\0'};
	char writename[256]={'\0'};

	unsigned int curpid;
	unsigned int queuenumber;

	curpid=GetCurrentProcessId();
	queuenumber=queuecount++;

	sprintf(globalshutdownname,"MMQUEUE_GS_%x",remotepid);
	sprintf(mmfilename,"MMQUEUE_MF_%x",remotepid);
	sprintf(readname,"MMQUEUE_RE_%x_%x",curpid,queuenumber);
	sprintf(writename,"MMQUEUE_WR_%x_%x",curpid,queuenumber);

/* allocate descriptor */
#ifdef USE_GLOBAL_ALLOCATION
	toreturn=(MMQueue *)GlobalAlloc(GMEM_FIXED,sizeof(MMQueue));
#else
	toreturn=(MMQueue *)malloc(sizeof(MMQueue));
#endif	

	toreturn->keeprunning=1;
	toreturn->writerpid=remotepid;

/* create events */
	toreturn->globalshutdown=CreateEvent(NULL,TRUE,FALSE,globalshutdownname);
	toreturn->readevent=CreateEvent(NULL,FALSE,FALSE,readname);
	toreturn->writtenevent=CreateEvent(NULL,FALSE,FALSE,writename);

/* open mm-file (one memory page backed by the system page file) */

	toreturn->filemapping=OpenFileMapping(PAGE_READWRITE,FALSE,mmfilename);
	toreturn->queuebuffer=(BYTE *)MapViewOfFile(toreturn->filemapping,FILE_MAP_READ,0,0,4096);

/* create internal queue */
	toreturn->internalqueue=CreateQueue();

/* this is a reader */
	toreturn->isWriter=0;

/* readers don't have readers */
	toreturn->readers=0;

/* no onqueue callback, no extra */
	toreturn->onqueue=0;
	toreturn->extra=0;

/* create reader thread */
	toreturn->queuethreadhandle=CreateThread(NULL,0,MMQueueReaderThread,(void *)toreturn,0,&(toreturn->queuethreadid));

	toreturn->queuenumber=queuenumber;

	return toreturn;
}

void CloseMMQueue(MMQueue * toclose)
{
	MMQueueReader * queuereader, * nextreader;
	MMQueueElement * curel;
	//unsigned int waitresult;

	toclose->keeprunning=0;

	//writer : set global shutdown event
	if(toclose->isWriter)
		SetEvent(toclose->globalshutdown);

	//terminate thread
	if(toclose->queuethreadhandle)
		if(WaitForSingleObject(toclose->queuethreadhandle,333)!=WAIT_OBJECT_0)
			TerminateThread(toclose->queuethreadhandle,0);

	//close handles and memory mapped file
	if(toclose->queuebuffer)
		UnmapViewOfFile(toclose->queuebuffer);

	if(toclose->filemapping)
		CloseHandle(toclose->filemapping);

	if(toclose->writtenevent)
		CloseHandle(toclose->writtenevent);
	if(toclose->readevent)
		CloseHandle(toclose->readevent);

	//cleanup and remove internal queue
	while(curel=(MMQueueElement *)DeQueue(toclose->internalqueue))
	{
#ifdef USE_GLOBAL_ALLOCATION
		GlobalFree(curel->data);
		GlobalFree(curel);
#else
		free(curel->data);
		free(curel);
#endif
	}
	DeleteQueue(toclose->internalqueue);

	//cleanup readers
	queuereader=toclose->readers;
	while(queuereader)
	{
		nextreader=queuereader->next;
		CloseHandle(queuereader->readevent);
		CloseHandle(queuereader->writtenevent);
#ifdef USE_GLOBAL_ALLOCATION
		GlobalFree(queuereader);
#else
		free(queuereader);
#endif
		queuereader=nextreader;
		break;
	}

	//cleanup descriptor
#ifdef USE_GLOBAL_ALLOCATION
	GlobalFree(toclose);
#else
	free(toclose);
#endif

	return;
}

void _copybytes(BYTE * buffer,unsigned int destpos,BYTE * bytes,unsigned int srcpos, unsigned int size)
{
#ifdef USE_GLOBAL_ALLOCATION
	CopyMemory(buffer+destpos,bytes+srcpos,size);
#else
	memcpy(buffer+destpos,bytes+srcpos,size);
#endif
	return;
}

DWORD WINAPI MMQueueReaderThread(void * thequeue)
{
	MMQueueElement * newel;
	MMQueue * curqueue=(MMQueue *)thequeue;
	HANDLE handles[2];
	DWORD waitresult;

	unsigned int read,toread;

	handles[0]=curqueue->globalshutdown;
	handles[1]=curqueue->writtenevent;

	//either we get a global shutdown, local shutdown or a written event
	while(curqueue->keeprunning)
	{
		while((waitresult=WaitForMultipleObjects(2,handles,FALSE,MMQUEUETIMEOUT))==WAIT_TIMEOUT)
		{
			if((!isrunning(curqueue->writerpid))||(curqueue->keeprunning==0))
			{
				curqueue->queuethreadhandle=0;
				curqueue->queuethreadid=0;
				return 0;
			}
		}

		if(waitresult==(WAIT_OBJECT_0))//global shutdown
		{
			curqueue->queuethreadhandle=0;
			curqueue->queuethreadid=0;
			return 0;
		}
		else if(waitresult==(WAIT_OBJECT_0+1))//written event
		{
			//read info
			read=0;
#ifdef USE_GLOBAL_ALLOCATION
			newel=(MMQueueElement *)GlobalAlloc(GMEM_FIXED,sizeof(MMQueueElement));
#else
			newel=(MMQueueElement *)malloc(sizeof(MMQueueElement));
#endif
			_copybytes((BYTE *)&(newel->type),0,curqueue->queuebuffer,0,sizeof(unsigned int));
			_copybytes((BYTE *)&(newel->size),0,curqueue->queuebuffer,sizeof(unsigned int),sizeof(unsigned int));

			//if(newel->type<=MAX_TYPE)
			//{
#ifdef USE_GLOBAL_ALLOCATION
			newel->data=(BYTE *)GlobalAlloc(GMEM_FIXED,newel->size);
#else
			newel->data=(BYTE *)malloc(newel->size);
#endif

			while(read<(newel->size))
			{
				toread=newel->size-read;
				if(toread>(4096-2*sizeof(unsigned int)))
					toread=(4096-2*sizeof(unsigned int));

				_copybytes(newel->data,read,curqueue->queuebuffer,(sizeof(unsigned int)*2),toread);
				read+=toread;

				SetEvent(curqueue->readevent);

				if(read<(newel->size))
				{
					//wait for next written
					waitresult=WaitForMultipleObjects(2,handles,FALSE,MMQUEUETIMEOUT);
					if(waitresult!=(WAIT_OBJECT_0+1))//shut down
					{
#ifdef USE_GLOBAL_ALLOCATION
						GlobalFree(newel->data);
						GlobalFree(newel);
#else
						free(newel->data);
						free(newel);
#endif
						curqueue->queuethreadhandle=0;
						curqueue->queuethreadid=0;
						return 0;
					}
				}
				else
				{
					//trigger callback if set
					if(curqueue->onqueue)
					{
						curqueue->onqueue(curqueue->extra,newel->data,newel->type,newel->size);
					}
					//queue it
					DoQueue(curqueue->internalqueue,(void *)newel);
				}
			}
			//}
		}
		else//error
		{
			Sleep(0);
		}
	}

	return 1;
}

DWORD WINAPI MMQueueWriterThread(void * thequeue)
{
	unsigned int i;

	unsigned int written,towrite;

	DWORD waitresult;

	MMQueueElement * curelement;

	MMQueueReader * curreader;
	MMQueueReader * nextreader;

	MMQueue * curqueue=(MMQueue *)thequeue;

	HANDLE ehandles[2]={curqueue->internalqueue->queueevent,curqueue->globalshutdown};

	while(curqueue->keeprunning)
	{
		//check control events : global shutdown, local shutdown, add reader, remove reader
		while((waitresult=WaitForMultipleObjects(2,ehandles,FALSE,MMQUEUETIMEOUT))==WAIT_TIMEOUT)
			;

		if(waitresult==WAIT_OBJECT_0)//queue event -> queue data
		{
			//queue information if available
			if(curelement=(MMQueueElement *)DeQueue(curqueue->internalqueue))
			{

				written=0;
				towrite=0;

				while(written<curelement->size)
				{
					towrite=curelement->size-written;
					if(towrite>(4096-2*sizeof(unsigned int)))
						towrite=(4096-2*sizeof(unsigned int));

					//pass to mmfile
					_copybytes(curqueue->queuebuffer,0,(BYTE *)&(curelement->type),0,sizeof(unsigned int));
					_copybytes(curqueue->queuebuffer,sizeof(unsigned int),(BYTE *)&(curelement->size),0,sizeof(unsigned int));
					_copybytes(curqueue->queuebuffer,2*sizeof(unsigned int),curelement->data,written,towrite);			

					//inform all reader threads about it

					curreader=curqueue->readers;

					while(curreader)
					{
						nextreader=curreader->next;
						ResetEvent(curreader->readevent);
						SetEvent(curreader->writtenevent);
						i=0;
						while((waitresult=WaitForSingleObject(curreader->readevent,MMQUEUETIMEOUT))==WAIT_TIMEOUT)
						{
							i++;
							if((!isrunning(curreader->readerpid))||(i==MAX_WAIT_ATTEMPTS))
							{
								//remove reader
								MMQueueRemoveReader(curqueue,curreader->readerpid);
								break;
							}
						}
						curreader=nextreader;
					}
					ResetEvent(curqueue->writtenevent);
	
					written+=towrite;
				}

				//cleanup MMQueueElement
#ifdef USE_GLOBAL_ALLOCATION
				if(curelement->data)
					GlobalFree(curelement->data);
				GlobalFree(curelement);
#else
				if(curelement->data)
					free(curelement->data);
				free(curelement);
#endif
			}
			else//shouldn't happen
				Sleep(0);
		}
		else if(waitresult==WAIT_OBJECT_0+1)
			break;
		else//error
			Sleep(0);
	}

	return 1;
}

void MMDoQueue(MMQueue * onqueue,unsigned int type,unsigned int size,BYTE * data)
{
	MMQueueElement * newel;

	if(onqueue->isWriter)
	{
#ifdef USE_GLOBAL_ALLOCATION
		newel=GlobalAlloc(GMEM_FIXED,sizeof(MMQueueElement));
		newel->data=GlobalAlloc(GMEM_FIXED,size);
#else
		newel=malloc(sizeof(MMQueueElement));
		newel->data=malloc(size);
#endif
	
		_copybytes(newel->data,0,data,0,size);
		newel->type=type;
		newel->size=size;

		DoQueue(onqueue->internalqueue,(void *)newel);
	}

	return;
}

BYTE * MMDeQueue(MMQueue * offqueue,unsigned int * type,unsigned int * size)
{
	MMQueueElement * curel;

	BYTE * toreturn;

	toreturn=0;
	(*size)=0;
	(*type)=0;

	if(offqueue->isWriter==0)
	{
		if(curel=(MMQueueElement *)DeQueue(offqueue->internalqueue))
		{
			(*size)=curel->size;
			(*type)=curel->type;
			toreturn=curel->data;
#ifdef USE_GLOBAL_ALLOCATION
			GlobalFree(curel);
#else
			free(curel);
#endif
		}
	}

	return toreturn;
}

void MMQueueAddReader(MMQueue * thisqueue,unsigned int readerpid,unsigned int queuenumber)
{
	MMQueueReader * newreader, * curreader;

	char readname[256]={'\0'};
	char writename[256]={'\0'};

#ifdef USE_GLOBAL_ALLOCATION
	newreader=GlobalAlloc(GMEM_FIXED,sizeof(MMQueueReader));
#else
	newreader=malloc(sizeof(MMQueueReader));
#endif

	sprintf(readname,"MMQUEUE_RE_%x_%x",readerpid,queuenumber);
	sprintf(writename,"MMQUEUE_WR_%x_%x",readerpid,queuenumber);
	
	newreader->next=0;
	newreader->readerpid=readerpid;
	newreader->readevent=CreateEvent(NULL,FALSE,FALSE,readname);
	newreader->writtenevent=CreateEvent(NULL,FALSE,FALSE,writename);
	
	if(curreader=thisqueue->readers)
	{
		while(curreader&&(curreader->next))
			curreader=curreader->next;

		newreader->previous=curreader;
		curreader->next=newreader;
	}
	else
	{
		newreader->previous=0;
		thisqueue->readers=newreader;
	}

	return;
}

void MMQueueRemoveReader(MMQueue * thisqueue,unsigned int readerpid)
{
	MMQueueReader * curreader, * nextreader;

	curreader=thisqueue->readers;

	while(curreader)
	{
		nextreader=curreader->next;
		if(curreader->readerpid==readerpid)
		{
			if(curreader->previous)
			{
				curreader->previous->next=curreader->next;
				if(curreader->next)
					curreader->next->previous=curreader->previous;
			}
			else
			{
				if(curreader->next)
					curreader->next->previous=0;
				thisqueue->readers=curreader->next;
			}
			CloseHandle(curreader->readevent);
			CloseHandle(curreader->writtenevent);
#ifdef USE_GLOBAL_ALLOCATION
			GlobalFree(curreader);
#else
			free(curreader);
#endif
		}
		curreader=nextreader;
	}

	return;
}