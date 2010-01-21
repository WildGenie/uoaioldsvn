#include "W2EDebug.h"

#include <windows.h>
#include <stdio.h>
#include <stdarg.h>

int currentindent=0;
int consoleallocated=0;

void openconsole()
{
	if(AllocConsole())
	{
		freopen("CONIN$","rb",stdin);
		freopen("CONOUT$","wb",stdout);
		freopen("CONOUT$","wb",stderr);
		consoleallocated=1;
	}
	return;
}

void debugpush()
{
	currentindent++;
	return;
}

void debugpop()
{
	if(currentindent)
		currentindent--;
	return;
}

void debugprintf(const char * format , ...)
{
	va_list argp;
	int i=0;
	if(!consoleallocated)
		openconsole();
	for(i=0;i<currentindent;i++)
		printf("\t\t");
	va_start(argp, format);
	vprintf(format, argp);
	va_end(argp);
	return;
}