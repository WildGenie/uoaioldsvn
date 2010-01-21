#ifndef UOMLVERSIONINFO_INCLUDED
#define UOMLVERSIONINFO_INCLUDED

#include <windows.h>
#include "ALLOCATION.h"

#ifdef BUILDING_DLL
# define UOCLIENTDLLAPI __declspec (dllexport)
#else /* Not BUILDING_DLL */
# define UOCLIENTDLLAPI __declspec (dllimport)
#endif /* Not BUILDING_DLL */

typedef struct verstruct
{
	unsigned short v1;
	unsigned short v2;
	unsigned short v3;
	unsigned short v4;
} UO2DClientVersion;

UOCLIENTDLLAPI char * GetClientVersion(unsigned int pid);
UO2DClientVersion Get2DClientVersion(unsigned int pid);
VS_FIXEDFILEINFO * GetClientVersionInfo(char * cpath,char * cexe);

#endif