#ifndef W2EDATASTRUCTURES_INCLUDED
#define W2EDATASTRUCTURES_INCLUDED

#include "COMHELP.h"

/////////////////////////////////////
// a. VariantList = Linked List of Variants, providing an IEnumVariant enumerator, and providing stack- and fifo-based access

// {87F5EBE8-660F-4305-9138-6741D322746F}
DEFINE_GUID(GUID_DataStructures_TYPELIB, 
0x87f5ebe8, 0x660f, 0x4305, 0x91, 0x38, 0x67, 0x41, 0xd3, 0x22, 0x74, 0x6f);

typedef struct VariantListElementStruct
{
	VARIANT element;
	struct VariantListElementStruct * next;
} VariantListElement;

// {A0069CE6-8AF9-4a51-AF10-58CF1335E2F4}
DEFINE_GUID(CLSID_VariantList, 
0xa0069ce6, 0x8af9, 0x4a51, 0xaf, 0x10, 0x58, 0xcf, 0x13, 0x35, 0xe2, 0xf4);

// {692685ED-C29B-433a-983A-082A5E50D32C}
DEFINE_GUID(IID_VariantList, 
0x692685ed, 0xc29b, 0x433a, 0x98, 0x3a, 0x8, 0x2a, 0x5e, 0x50, 0xd3, 0x2c);


typedef struct
{
	ObjectHeader LLHeader;
	long count;
	VariantListElement * firstelement;
	VariantListElement * lastelement;
} VariantList;

typedef struct
{
	IDISPATCHVTBL;
	HRESULT (STDMETHODCALLTYPE * Count)(VariantList * pThis,long * count);
	HRESULT (STDMETHODCALLTYPE * Item)(VariantList * pThis, long itemindex, VARIANT * item);
	HRESULT (STDMETHODCALLTYPE * NewEnum)(VariantList * pThis,IUnknown ** newenum);
	HRESULT (STDMETHODCALLTYPE * AddItem)(VariantList * pThis,VARIANT toadd);
	HRESULT (STDMETHODCALLTYPE * RemoveItem)(VariantList * pThis, long itemindex);
	//stack access
	HRESULT (STDMETHODCALLTYPE * Push)(VariantList * pThis,VARIANT topush);
	HRESULT (STDMETHODCALLTYPE * Pop)(VariantList * pThis, VARIANT * popped);
	//fifo access
	HRESULT (STDMETHODCALLTYPE * Queue)(VariantList * pThis,VARIANT toqueue);
	HRESULT (STDMETHODCALLTYPE * Dequeue)(VariantList * pThis, VARIANT * dequeued);
} VariantListVtbl;

/*// {E1DA82B0-C3B2-4b34-9BA0-41DAD1D12009}
DEFINE_GUID(IID_EnumVariantList, 
0xe1da82b0, 0xc3b2, 0x4b34, 0x9b, 0xa0, 0x41, 0xda, 0xd1, 0xd1, 0x20, 0x9);*/

typedef struct
{
	ObjectHeader LLEnumHeader;
	VariantList * ownedlist;
	VariantListElement * currentelement;
} EnumVariantList;

typedef struct
{
	IUNKNOWNVTBL;
	HRESULT (STDMETHODCALLTYPE * Next)(EnumVariantList * pThis,ULONG count,VARIANT * rgelt,ULONG * actualcount);
	HRESULT (STDMETHODCALLTYPE * Skip)(EnumVariantList * pThis,ULONG count);
	HRESULT (STDMETHODCALLTYPE * Reset)(EnumVariantList * pThis);
	HRESULT (STDMETHODCALLTYPE * Clone)(EnumVariantList * pThis,IUnknown ** newenum);
} EnumVariantListVtbl;

HRESULT STDMETHODCALLTYPE VLCount(VariantList * pThis,long * count);
HRESULT STDMETHODCALLTYPE VLItem(VariantList * pThis, long itemindex, VARIANT * item);
HRESULT STDMETHODCALLTYPE VLNewEnum(VariantList * pThis,IUnknown ** newenum);

HRESULT STDMETHODCALLTYPE VLAddItem(VariantList * pThis,VARIANT toadd);
HRESULT STDMETHODCALLTYPE VLRemoveItem(VariantList * pThis, long itemindex);
HRESULT STDMETHODCALLTYPE VLPop(VariantList * pThis, VARIANT * popped);
HRESULT STDMETHODCALLTYPE VLQueue(VariantList * pThis, VARIANT toqueue);

void STDMETHODCALLTYPE VariantListConstructor(void * pThis);
void STDMETHODCALLTYPE VariantListDestructor(void * pThis);

HRESULT STDMETHODCALLTYPE VLNext(EnumVariantList * pThis,ULONG count,VARIANT * rgelt,ULONG * actualcount);
HRESULT STDMETHODCALLTYPE VLSkip(EnumVariantList * pThis,ULONG count);
HRESULT STDMETHODCALLTYPE VLReset(EnumVariantList * pThis);
HRESULT STDMETHODCALLTYPE VLClone(EnumVariantList * pThis,IUnknown ** newenum);

void STDMETHODCALLTYPE EnumVariantListConstructor(void * pThis);
void STDMETHODCALLTYPE EnumVariantListDestructor(void * pThis);

ClassDesc VariantListDesc;
ClassDesc EnumVariantListDesc;
/////////////////////////////////////


/////////////////////////////////////
// b. LinkedList = LinkedList of "void *" elements, provinding fifo- and stack- based access
//		+ providing an enumerator that can act as an IEnumUnknown, IEnumConnections, IEnumConnectionPoints

typedef struct LinkedListElementStruct
{
	void * element;
	struct LinkedListElementStruct * next;
} LinkedListElement;

// {47DCD868-5570-4f46-9464-74541911E7AD}
DEFINE_GUID(CLSID_LinkedList, 
0x47dcd868, 0x5570, 0x4f46, 0x94, 0x64, 0x74, 0x54, 0x19, 0x11, 0xe7, 0xad);

// {5F0D031F-4913-4300-9774-CC6A7E221EE4}
DEFINE_GUID(IID_LinkedList, 
0x5f0d031f, 0x4913, 0x4300, 0x97, 0x74, 0xcc, 0x6a, 0x7e, 0x22, 0x1e, 0xe4);

typedef struct
{
	ObjectHeader LLHeader;
	long count;
	LinkedListElement * firstelement;
	LinkedListElement * lastelement;
	GUID * EnumeratorIID;
} LinkedList;

typedef struct
{
	IDISPATCHVTBL;
	HRESULT (STDMETHODCALLTYPE * Count)(LinkedList * pThis,long * count);
	HRESULT (STDMETHODCALLTYPE * Item)(LinkedList * pThis, long itemindex, void ** item);
	HRESULT (STDMETHODCALLTYPE * NewEnum)(LinkedList * pThis,IUnknown ** newenum);
	HRESULT (STDMETHODCALLTYPE * AddItem)(LinkedList * pThis,void * toadd);
	HRESULT (STDMETHODCALLTYPE * RemoveItem)(LinkedList * pThis, long itemindex);
	//stack access
	HRESULT (STDMETHODCALLTYPE * Push)(LinkedList * pThis,void * topush);
	HRESULT (STDMETHODCALLTYPE * Pop)(LinkedList * pThis,void ** popped);
	//fifo access
	HRESULT (STDMETHODCALLTYPE * Queue)(LinkedList * pThis,void * toqueue);
	HRESULT (STDMETHODCALLTYPE * Dequeue)(LinkedList * pThis, void ** dequeued);
} LinkedListVtbl;

typedef struct
{
	ObjectHeader LLEnumHeader;
	LinkedList * ownedlist;
	LinkedListElement * currentelement;
} EnumLinkedList;

typedef struct
{
	IUNKNOWNVTBL;
	HRESULT (STDMETHODCALLTYPE * Next)(EnumLinkedList * pThis,ULONG count,void ** rgelt,ULONG * actualcount);
	HRESULT (STDMETHODCALLTYPE * Skip)(EnumLinkedList * pThis,ULONG count);
	HRESULT (STDMETHODCALLTYPE * Reset)(EnumLinkedList * pThis);
	HRESULT (STDMETHODCALLTYPE * Clone)(EnumLinkedList * pThis,IUnknown ** newenum);
} EnumLinkedListVtbl;

HRESULT STDMETHODCALLTYPE LLCount(LinkedList * pThis,long * count);
HRESULT STDMETHODCALLTYPE LLItem(LinkedList * pThis, long itemindex, void ** item);
HRESULT STDMETHODCALLTYPE LLNewEnum(LinkedList * pThis,IUnknown ** newenum);

HRESULT STDMETHODCALLTYPE LLAddItem(LinkedList * pThis,void * toadd);
HRESULT STDMETHODCALLTYPE LLRemoveItem(LinkedList * pThis, long itemindex);
HRESULT STDMETHODCALLTYPE LLPop(LinkedList * pThis, void ** popped);
HRESULT STDMETHODCALLTYPE LLQueue(LinkedList * pThis, void * toqueue);

void STDMETHODCALLTYPE LinkedListConstructor(void * pThis);
void STDMETHODCALLTYPE LinkedListDestructor(void * pThis);

HRESULT STDMETHODCALLTYPE LLNext(EnumLinkedList * pThis,ULONG count,void ** rgelt,ULONG * actualcount);
HRESULT STDMETHODCALLTYPE LLSkip(EnumLinkedList * pThis,ULONG count);
HRESULT STDMETHODCALLTYPE LLReset(EnumLinkedList * pThis);
HRESULT STDMETHODCALLTYPE LLClone(EnumLinkedList * pThis,IUnknown ** newenum);

void STDMETHODCALLTYPE EnumLinkedListConstructor(void * pThis);
void STDMETHODCALLTYPE EnumLinkedListDestructor(void * pThis);

HRESULT STDMETHODCALLTYPE LLQueryInterface(void * pThis,REFIID riid,void ** ppv);//EnumLinkedList QueryInterface

ClassDesc LinkedListDesc;
ClassDesc EnumLinkedListDesc;
/////////////////////////////////////

/////////////////////////////////////
// c. Implementation of IConnectionPoint and IConnectionPointContainer
//		-> both are not really datastructures, but...
//		-> both need an enumaration (Connections and ConnectionPoints), so they require the above implemented linkedlist
//		-> and datastructures that will follow, such as the BinaryTree, require a callback (comparison-callback)
//		=> given this fact, that there is an inter-dependency in between W2EDatastructures and the IConnectionPoint/IConnectionPointContainer implementation, ...
//			... it seemed best to insert the latter one right here

// {E2A6CFC7-3EE3-4a02-86FD-F9C47A39D348}
//DEFINE_GUID(IID_W2EConnectionPointContainer, 
//0xe2a6cfc7, 0x3ee3, 0x4a02, 0x86, 0xfd, 0xf9, 0xc4, 0x7a, 0x39, 0xd3, 0x48);

typedef struct
{
	ObjectHeader CPCHeader;
	LinkedList * LLConnectionPoints;//linked list of W2EConnectionPoints
} W2EConnectionPointContainer;

typedef struct
{
	ObjectHeader CPHeader;
	LinkedList * LLConnections;//LinkedList of CONNECTDATA structures
	long MaxConnectionCount;//0 if 'infinite'
	GUID * CallbackIID;//IID of supported outgoing interface
	W2EConnectionPointContainer * CPContainer;//container backref
} W2EConnectionPoint;

typedef struct
{
	IUNKNOWNVTBL;
	HRESULT (STDMETHODCALLTYPE * GetConnectionInterface)(W2EConnectionPoint * pThis,IID * pIID);
	HRESULT (STDMETHODCALLTYPE * GetConnectionPointContainer)(W2EConnectionPoint * pThis,W2EConnectionPointContainer ** ppCPC);
	HRESULT (STDMETHODCALLTYPE * Advise)(W2EConnectionPoint * pThis,IUnknown * pUnk,DWORD * pdwCookie);
	HRESULT (STDMETHODCALLTYPE * Unadvise)(W2EConnectionPoint * pThis,DWORD dwCookie);
	HRESULT (STDMETHODCALLTYPE * EnumConnections)(W2EConnectionPoint * pThis,EnumLinkedList ** ppEnum);
} W2EConnectionPointVtbl;

typedef struct
{
	IUNKNOWNVTBL;
	HRESULT (STDMETHODCALLTYPE * EnumConnectionPoints)(W2EConnectionPointContainer * pThis,EnumLinkedList ** ppEnum);
	HRESULT (STDMETHODCALLTYPE * FindConnectionPoint)(W2EConnectionPointContainer * pThis,REFIID riid,W2EConnectionPoint ** ppCP);
} W2EConnectionPointContainerVtbl;

HRESULT STDMETHODCALLTYPE GetConnectionInterface(W2EConnectionPoint * pThis,IID * pIID);
HRESULT STDMETHODCALLTYPE GetConnectionPointContainer(W2EConnectionPoint * pThis,W2EConnectionPointContainer ** ppCPC);
HRESULT STDMETHODCALLTYPE Advise(W2EConnectionPoint * pThis,IUnknown * pUnk,DWORD * pdwCookie);
HRESULT STDMETHODCALLTYPE Unadvise(W2EConnectionPoint * pThis,DWORD dwCookie);
HRESULT STDMETHODCALLTYPE EnumConnections(W2EConnectionPoint * pThis,EnumLinkedList ** ppEnum);

HRESULT STDMETHODCALLTYPE EnumConnectionPoints(W2EConnectionPointContainer * pThis,EnumLinkedList ** ppEnum);
HRESULT STDMETHODCALLTYPE FindConnectionPoint(W2EConnectionPointContainer * pThis,REFIID riid,W2EConnectionPoint ** ppCP);

void STDMETHODCALLTYPE W2EConnectionPointContainerConstructor(void * pThis);
void STDMETHODCALLTYPE W2EConnectionPointContainerDestructor(void * pThis);
void STDMETHODCALLTYPE W2EConnectionPointConstructor(void * pThis);
void STDMETHODCALLTYPE W2EConnectionPointDestructor(void * pThis);

ClassDesc W2EConnectionPointContainerDesc;
ClassDesc W2EConnectionPointDesc;

void InvokeEvent(void * onobject,REFIID eventguid,DISPID toinvoke,DISPPARAMS * params,VARIANT * result);

/////////////////////////////////////

void W2EDatastructuresInit();

#endif