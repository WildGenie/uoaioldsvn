#include "Bufferhandler.h"
#include "ALLOCATION.h"

BufferHandler * CreateHandler(char * buffer,unsigned int size)
{
	BufferHandler * toreturn=(BufferHandler *)wAlloc(sizeof(BufferHandler));
	toreturn->buffer=buffer;
	toreturn->size=size;
	toreturn->pos=0;
	toreturn->swap=0;
	return toreturn;
}

BufferHandler * AllocBuffer(unsigned int size)
{
	BufferHandler * toreturn=(BufferHandler *)wAlloc(sizeof(BufferHandler));
	toreturn->buffer=(char *)wAlloc(sizeof(char)*size);
	toreturn->size=size;
	toreturn->pos=0;
	toreturn->swap=0;
	return toreturn;
}

int bh_write(BufferHandler * onbuffer,char * towrite,unsigned int size,int ignoreswap)
{
	unsigned int i;
	
	if((onbuffer->size-onbuffer->pos)<size)
		return 0;
	if((!(onbuffer->swap))||ignoreswap)
		memcpy((void *)(onbuffer->buffer+onbuffer->pos),(const void *)towrite,size);
	else
	{
		for(i=0;i<size;i++)
		{
			(*(onbuffer->buffer+onbuffer->pos+i))=(*(towrite+size-i-1));
		}
	}

	onbuffer->pos+=size;

	return 1;
}

char * bh_read(BufferHandler * onbuffer,unsigned int size,int ignoreswap)
{
	char * toreturn;
	unsigned int i;
	
	if((onbuffer->size-onbuffer->pos)<size)
		return NULL;

	toreturn=(char *)wAlloc(sizeof(char)*size);

	if((!(onbuffer->swap))||ignoreswap)
		memcpy((void *)toreturn,(const void *)(onbuffer->buffer+onbuffer->pos),size);
	else
	{
		for(i=0;i<size;i++)
		{
			(*(toreturn+i))=(*(onbuffer->buffer+onbuffer->pos+size-i-1));
		}
	}

	onbuffer->pos+=size;

	return toreturn;
}

void FreeBuffer(BufferHandler * tofree)
{
	wFree(tofree->buffer);
	wFree(tofree);
	return;
}

int write_char(BufferHandler * bh,char towrite)
{
	return bh_write(bh,(char *)&towrite,sizeof(char),0);
}

int write_int(BufferHandler * bh,int towrite)
{
	return bh_write(bh,(char *)&towrite,sizeof(int),0);
}

int write_uint(BufferHandler * bh,unsigned int towrite)
{
	return bh_write(bh,(char *)&towrite,sizeof(unsigned int),0);
}

int write_short(BufferHandler * bh,short towrite)
{
	return bh_write(bh,(char *)&towrite,sizeof(short),0);
}

int write_ushort(BufferHandler * bh,unsigned short towrite)
{
	return bh_write(bh,(char *)&towrite,sizeof(unsigned short),0);
}

int write_str(BufferHandler * bh,char * towrite)
{
	return bh_write(bh,towrite,strlen(towrite)+1,1);
}

int write_strn(BufferHandler * bh,char * towrite,unsigned int length)
{
	return bh_write(bh,towrite,length,1);
}

int write_byte(BufferHandler * bh,BYTE towrite)
{
	return bh_write(bh,(char *)&towrite,sizeof(BYTE),0);
}

char read_char(BufferHandler * bh)
{
	char toreturn;
	if((bh->size-bh->pos)<1)
		return 0;//throw error here
	else
	{
		toreturn=(*(bh->buffer+bh->pos));
		bh->pos++;
		return toreturn;
	}
}

int read_int(BufferHandler * bh)
{
	int toreturn;
	char * cpy;

	if(!(cpy=bh_read(bh,sizeof(int),0)))
		return 0;//throw error here
	else
	{
		toreturn=(*((int *)cpy));
		wFree(cpy);
		return toreturn;
	}
}

unsigned int read_uint(BufferHandler * bh)
{
	unsigned int toreturn;
	char * cpy;

	if(!(cpy=bh_read(bh,sizeof(unsigned int),0)))
		return 0;//throw error here
	else
	{
		toreturn=(*((unsigned int *)cpy));
		wFree(cpy);
		return toreturn;
	}
}

short read_short(BufferHandler * bh)
{
	short toreturn;
	char * cpy;

	if(!(cpy=bh_read(bh,sizeof(short),0)))
		return 0;//throw error here
	else
	{
		toreturn=(*((short *)cpy));
		wFree(cpy);
		return toreturn;
	}
}

unsigned short read_ushort(BufferHandler * bh)
{
	unsigned short toreturn;
	char * cpy;

	if(!(cpy=bh_read(bh,sizeof(unsigned short),0)))
		return 0;//throw error here
	else
	{
		toreturn=(*((unsigned short *)cpy));
		wFree(cpy);
		return toreturn;
	}
}

char * read_str(BufferHandler * bh)
{
	unsigned int count;

	char * cpy;

	count=0;
	while(((bh->pos+count)<bh->size)&&((*(bh->buffer+bh->pos+count))!='\0'))
		count++;
	if((bh->pos+count)<bh->size)
		count++;//include 0-termination, if any

	if(!count)
		return NULL;

	cpy=(char *)wAlloc(sizeof(char)*(count+1));
	memset(cpy,0,sizeof(char)*(count+1));
	memcpy(cpy,bh->buffer+bh->pos,count);

	bh->pos+=count;

	return cpy;
}

char * read_strn(BufferHandler * bh,unsigned int length)
{
	return bh_read(bh,length,1);
}

short * read_ustr(BufferHandler * bh)//SWAP EACH CHARACTER!
{
	short * toreturn;
	unsigned int ulen, i;

	ulen=0;
	while(((bh->pos+ulen*2)<(bh->size-1))&&((*((short *)(bh->buffer+bh->pos+ulen*2)))!=0))
		ulen++;
	if((bh->pos+ulen*2)<(bh->size-1))
		ulen++;

	if(!ulen)
		return 0;

	toreturn=(short *)wAlloc(sizeof(short)*ulen);
	for(i=0;i<ulen;i++)
		toreturn[i]=read_short(bh);

	return toreturn;
}

short * read_ustrn(BufferHandler * bh,unsigned int length)
{
	unsigned int i;
	short * toreturn=(short *)wAlloc(sizeof(short)*(length+1));

	for(i=0;i<length;i++)
		toreturn[i]=read_short(bh);

	toreturn[i]=0;

	return toreturn;
}

BYTE read_byte(BufferHandler * bh)
{
	BYTE toreturn;
	if((bh->size-bh->pos)<1)
		return 0;//throw error here
	else
	{
		toreturn=(*((BYTE *)(bh->buffer+bh->pos)));
		bh->pos++;
		return toreturn;
	}
}