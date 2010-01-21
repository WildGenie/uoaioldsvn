#ifndef BUFFERHANDLER_INCLUDED
#define BUFFERHANDLER_INCLUDED

#include <windows.h>
#include <string.h>

//BufferHandler : sequential reading/writing to a buffer (with optional host/network-order swap)

typedef struct
{
	char * buffer;
	unsigned int size;
	unsigned int pos;
	int swap;//if host<->network order swaps are required, this is 1, default is 0
} BufferHandler;

BufferHandler * CreateHandler(char * buffer,unsigned int size);
BufferHandler * AllocBuffer(unsigned int size);
void FreeBuffer(BufferHandler * tofree);

int bh_write(BufferHandler * onbuffer,char * towrite,unsigned int size,int ignoreswap);

int write_char(BufferHandler * bh,char towrite);
int write_int(BufferHandler * bh,int towrite);
int write_uint(BufferHandler * bh,unsigned int towrite);
int write_short(BufferHandler * bh,short towrite);
int write_ushort(BufferHandler * bh,unsigned short towrite);
int write_str(BufferHandler * bh,char * towrite);
int write_strn(BufferHandler * bh,char * towrite,unsigned int length);
int write_byte(BufferHandler * bh,BYTE towrite);

char * bh_read(BufferHandler * onbuffer,unsigned int size,int ignoreswap);

char read_char(BufferHandler * bh);
int read_int(BufferHandler * bh);
unsigned int read_uint(BufferHandler * bh);
short read_short(BufferHandler * bh);
unsigned short read_ushort(BufferHandler * bh);
char * read_str(BufferHandler * bh);
char * read_strn(BufferHandler * bh,unsigned int length);
BYTE read_byte(BufferHandler * bh);
short * read_ustrn(BufferHandler * bh,unsigned int length);
short * read_ustr(BufferHandler * bh);

#endif