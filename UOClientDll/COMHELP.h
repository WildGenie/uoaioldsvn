#ifndef COMHELP_INCLUDED
#define COMHELP_INCLUDED

#include <windows.h>
#include <objbase.h>
//#include <activscp.h>
#include <olectl.h>

typedef struct ClassDescStruct;

typedef struct SubObjectStruct
{
	struct ClassDescStruct * classdesc;
	struct SubObjectStruct * next;
} SubObject;

typedef struct OutgoingInterfaceStruct
{
	GUID * OutgoingIID;
	unsigned int MaxCallbackCount;
	struct OutgoingInterfaceStruct * next;
} OutgoingInterface;

typedef struct ClassDescStruct
{
	IUnknownVtbl * pVtbl;
	void (STDMETHODCALLTYPE *constructor)(void * pThis);
	void (STDMETHODCALLTYPE *destructor)(void * pThis);
	int IDispatchBased;
	GUID * ClassIID;
	SubObject * subobjects;
	ITypeInfo * ClassTypeInfo;
	unsigned int objectsize;
	void * activeobject;
	int norefcount;
	OutgoingInterface * outgoinginterfaces;
} ClassDesc;

void RemoveSubObjects(ClassDesc * toremove);
void RecursiveDeleteSubobject(SubObject * todelete);
void AddSubObject(ClassDesc * ownerdesc,ClassDesc * subobjectdesc);

void RemoveOutgoingInterfaces(ClassDesc * toremove);
void RecursiveDeleteOutgoingInterfaces(OutgoingInterface * todelete);
void AddOutgoingInterface(ClassDesc * ownerdesc,GUID * OutgoingIID,unsigned int maxcallbackcount);

HRESULT STDMETHODCALLTYPE DoCreateInstance(ClassDesc * ofclass, int internal, REFIID vTableGuid, void **ppv);

DISPID DispIDFromTypeLib(ITypeLib * tlib,REFIID guid,LPOLESTR methodname);
DISPPARAMS * BuildParams(unsigned int parcount);
void PassInt(DISPPARAMS * arguments,int topass);
void PassUInt(DISPPARAMS * arguments,unsigned int topass);
void PassShort(DISPPARAMS * arguments,short topass);
void PassUShort(DISPPARAMS * arguments,unsigned short topass);
void PassSChar(DISPPARAMS * arguments,signed char topass);
void PassUChar(DISPPARAMS * arguments,unsigned char topass);
void PassString(DISPPARAMS * arguments,LPOLESTR topass);
void PassObject(DISPPARAMS * arguments,IDispatch * dispobj);
void DeleteParams(DISPPARAMS * toremove);
BSTR StringToBSTR(char * string);
void DoInvoke(IDispatch * onobject,DISPID toinvoke,DISPPARAMS * params,VARIANT * result);
void hr_print(HRESULT hr);

typedef struct GuidInfoStruct
{
	GUID * guid;
	char * name;
	struct GuidInfoStruct * next;
} GuidInfo;

void AddGuidInfo(GUID * guid,char * name);

void debugprintIID(GUID * toprint);

typedef struct OHStruct
{
	IUnknownVtbl * pVtbl;
	DWORD refcount;
	struct OHStruct * owner;
	ClassDesc * classinfo;
} ObjectHeader;

typedef struct
{
	ObjectHeader cfheader;
	ClassDesc * ownedclassinfo;
} ClassFactory;

HRESULT STDMETHODCALLTYPE DefaultClassLockServer(void * pThis,BOOL flock);
HRESULT STDMETHODCALLTYPE DefaultClassCreateInstance(void * pThis,IUnknown * punkOuter, REFIID vTableGuid, void **ppv);

ClassFactory * CreateFactory(ClassDesc  * classinfo);

HRESULT STDMETHODCALLTYPE DefaultQueryInterface(void * pThis,REFIID riid,void ** ppv);
ULONG STDMETHODCALLTYPE DefaultAddRef(void * pThis);
ULONG STDMETHODCALLTYPE DefaultRelease(void * pThis);

ULONG STDMETHODCALLTYPE DefaultInvoke(void * pThis, DISPID dispid, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS *params, VARIANT *result, EXCEPINFO *pexcepinfo, UINT *puArgErr);
ULONG STDMETHODCALLTYPE DefaultGetIDsOfNames(void * pThis, REFIID riid, LPOLESTR * rgszNames, UINT cNames, LCID lcid, DISPID * rgdispid);
ULONG STDMETHODCALLTYPE DefaultGetTypeInfo(void * pThis, UINT itinfo, LCID lcid, ITypeInfo ** pTypeInfo);
ULONG STDMETHODCALLTYPE DefaultGetTypeInfoCount(void * pThis, UINT * pCount);

int CanUnloadNow();
void InternalAddRef();
void InternalRelease();

void debugprintrefcounts();

#define IUNKNOWNVTBL \
	HRESULT ( STDMETHODCALLTYPE * QueryInterface ) ( void * pThis , REFIID riid , void ** ppv ); \
	ULONG ( STDMETHODCALLTYPE * AddRef )( void * pThis ); \
	ULONG ( STDMETHODCALLTYPE * Release )( void * pThis )

#define IDISPATCHVTBL \
	IUNKNOWNVTBL ; \
	ULONG ( STDMETHODCALLTYPE * GetTypeInfoCount ) ( void * pThis , UINT * pCount ); \
	ULONG ( STDMETHODCALLTYPE * GetTypeInfo ) ( void * pThis , UINT itinfo , LCID lcid , ITypeInfo ** pTypeInfo ); \
	ULONG ( STDMETHODCALLTYPE * GetIDsOfNames ) ( void * pThis , REFIID riid , LPOLESTR * rgszNames , UINT cNames , LCID lcid , DISPID * rgdispid ); \
	ULONG ( STDMETHODCALLTYPE * Invoke ) ( void * pThis , DISPID dispid , REFIID riid , LCID lcid , WORD wFlags , DISPPARAMS * params , VARIANT * result , EXCEPINFO * pexcepinfo , UINT * puArgErr )

#define IUNKNOWNFUNCTIONS \
	DefaultQueryInterface , DefaultAddRef , DefaultRelease

#define IDISPATCHFUNCTIONS \
	IUNKNOWNFUNCTIONS , DefaultGetTypeInfoCount , DefaultGetTypeInfo , DefaultGetIDsOfNames , DefaultInvoke

#define IUNKNOWNLOOKUP { (GUID *) & IID_IUnknown , 0 }

#define IDISPATCHLOOKUP IUNKNOWNLOOKUP , { (GUID  *) & IID_IDispatch , 0 }

#endif