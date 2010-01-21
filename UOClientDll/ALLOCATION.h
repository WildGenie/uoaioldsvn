//-------------------------------------------------------------------------------------------------------------------//
//- ALLOCATION.h :: Synchronized allocation, mainly in COM libraries, requires the use of GlobalAlloc/GlobalFree.   -//
//					To ensure (parts of) the code used in in those libraries is reusable in situations where        -//
//					global allocation is not required, the allocation methods are wrapped into wAlloc and wFree.	-//
//					This way the option to add debugging-versions of the allocation functions remains open too.		-//
//---------------------------------------------------------------------------------------------------- Wim Decelle --//

#ifndef ALLOCATION_INCLUDED
#define ALLOCATION_INCLUDED

#ifdef GLOBAL_ALLOCATION

#include <windows.h>

#define wAlloc(alloc_size) GlobalAlloc(GMEM_FIXED,alloc_size)

#define wFree(tofree) GlobalFree((HGLOBAL)tofree)

#define wCopy(destination,source,size) CopyMemory(destination,source,size)

#define wReAlloc(address,newsize) GlobalReAlloc((HGLOBAL)address,newsize,GMEM_FIXED)

#else

#include <stdlib.h>

#define wAlloc(alloc_size) malloc(alloc_size)

#define wFree(tofree) free((void *)tofree)

#define wCopy(destination,source,size) memcpy(destination,source,size)

#define wReAlloc(address,newsize) realloc((void *)address,newsize)

#endif

#endif