// UOAI is a set of COM/ActiveX components that (try to) expose A.I. scripting functionality to COM-enabled
//	programming and scripting languages such as VB, VBScript, VB.NET, JavaScript, Perl, C, C++, C#, ...

//Main UOAI object allows creation of a new client (which is injected with a dll)

#include "COMHELP.h"

// {EB82EC6E-AB50-4dd4-95A3-598B06F93564}
DEFINE_GUID(GUID_UOAI_TYPELIB, 
0xeb82ec6e, 0xab50, 0x4dd4, 0x95, 0xa3, 0x59, 0x8b, 0x6, 0xf9, 0x35, 0x64);

// {F1354164-F6E2-46c4-A52E-437678FFDF52}
DEFINE_GUID(IID_UOClient, 
0xf1354164, 0xf6e2, 0x46c4, 0xa5, 0x2e, 0x43, 0x76, 0x78, 0xff, 0xdf, 0x52);

// {94D3F728-83FF-4665-B616-365AE5D647C4}
DEFINE_GUID(CLSID_UOClient, 
0x94d3f728, 0x83ff, 0x4665, 0xb6, 0x16, 0x36, 0x5a, 0xe5, 0xd6, 0x47, 0xc4);

typedef struct
{
	ObjectHeader UOCHeader;
	void * Info;
} UOClient;

typedef struct
{
	IDISPATCHVTBL;
	HRESULT (STDMETHODCALLTYPE * getx)(UOClient * pThis,int * a);
	HRESULT (STDMETHODCALLTYPE * gety)(UOClient * pThis,int * a);
	HRESULT (STDMETHODCALLTYPE * getz)(UOClient * pThis,int * a);
} UOClientVtbl;

// {C27D1F35-5001-4b97-A467-1868A5BE19A9}
DEFINE_GUID(CLSID_UOAI, 
0xc27d1f35, 0x5001, 0x4b97, 0xa4, 0x67, 0x18, 0x68, 0xa5, 0xbe, 0x19, 0xa9);

// {51BE0440-5CE7-493a-B3FD-A5AFA4672836}
DEFINE_GUID(IID_UOAI, 
0x51be0440, 0x5ce7, 0x493a, 0xb3, 0xfd, 0xa5, 0xaf, 0xa4, 0x67, 0x28, 0x36);


typedef struct
{
	ObjectHeader uoai_header;
} UOAI;

typedef struct
{
	IDISPATCHVTBL;
	HRESULT (STDMETHODCALLTYPE * LaunchClient)(UOAI * pThis,BOOL * retval);
	HRESULT (STDMETHODCALLTYPE * givenewclient)(UOAI * pThis,UOClient ** newclient);
}  UOAIVtbl;

HRESULT STDMETHODCALLTYPE getX(UOClient * pThis,int * a);
HRESULT STDMETHODCALLTYPE getY(UOClient * pThis,int * a);
HRESULT STDMETHODCALLTYPE getZ(UOClient * pThis,int * a);
