#include "COMHELP.h"
#include <INITGUID.H>
#include "W2EDatastructures.h"
#include "W2EDebug.h"

ITypeLib * TLDataStructures=0;
VariantListVtbl IVariantList={ IDISPATCHFUNCTIONS ,VLCount,VLItem,VLNewEnum,VLAddItem,VLRemoveItem,VLAddItem,VLPop,VLQueue,VLPop };
EnumVariantListVtbl IEnumVariantList={ IUNKNOWNFUNCTIONS , VLNext, VLSkip, VLReset, VLClone };
LinkedListVtbl ILinkedList={ IDISPATCHFUNCTIONS , LLCount,LLItem,LLNewEnum,LLAddItem,LLRemoveItem,LLAddItem,LLPop,LLQueue,LLPop };
EnumLinkedListVtbl IEnumLinkedList={ IUNKNOWNFUNCTIONS , LLNext, LLSkip, LLReset, LLClone };
W2EConnectionPointVtbl IW2EConnectionPoint={ IUNKNOWNFUNCTIONS,GetConnectionInterface,GetConnectionPointContainer,Advise,Unadvise,EnumConnections };
W2EConnectionPointContainerVtbl IW2EConnectionPointContainer={ IUNKNOWNFUNCTIONS, EnumConnectionPoints,FindConnectionPoint };

void W2EDatastructuresInit()
{
	if(!TLDataStructures)
	{
		LoadRegTypeLib(&GUID_DataStructures_TYPELIB,1,0,0,&TLDataStructures);
		
		VariantListDesc.activeobject=0;
		VariantListDesc.ClassIID=(GUID *)&IID_VariantList;
		VariantListDesc.constructor=VariantListConstructor;
		VariantListDesc.destructor=VariantListDestructor;
		VariantListDesc.IDispatchBased=1;
		VariantListDesc.objectsize=sizeof(VariantList);
		VariantListDesc.pVtbl=(IUnknownVtbl *)&IVariantList;
		VariantListDesc.subobjects=0;
		VariantListDesc.norefcount=0;
		VariantListDesc.outgoinginterfaces=0;
		TLDataStructures->lpVtbl->GetTypeInfoOfGuid((void *)TLDataStructures,&IID_VariantList,&VariantListDesc.ClassTypeInfo);

		EnumVariantListDesc.activeobject=0;
		EnumVariantListDesc.ClassIID=(GUID *)&IID_IEnumVARIANT;
		EnumVariantListDesc.ClassTypeInfo=0;
		EnumVariantListDesc.constructor=EnumVariantListConstructor;
		EnumVariantListDesc.destructor=EnumVariantListDestructor;
		EnumVariantListDesc.IDispatchBased=0;
		EnumVariantListDesc.norefcount=0;
		EnumVariantListDesc.objectsize=sizeof(EnumVariantList);
		EnumVariantListDesc.pVtbl=(IUnknownVtbl *)&IEnumVariantList;
		EnumVariantListDesc.subobjects=0;
		EnumVariantListDesc.outgoinginterfaces=0;

		LinkedListDesc.activeobject=0;
		LinkedListDesc.ClassIID=(GUID *)&IID_LinkedList;
		LinkedListDesc.constructor=LinkedListConstructor;
		LinkedListDesc.destructor=LinkedListDestructor;
		LinkedListDesc.IDispatchBased=1;
		LinkedListDesc.objectsize=sizeof(LinkedList);
		LinkedListDesc.pVtbl=(IUnknownVtbl *)&ILinkedList;
		LinkedListDesc.subobjects=0;
		LinkedListDesc.norefcount=0;
		LinkedListDesc.outgoinginterfaces=0;
		TLDataStructures->lpVtbl->GetTypeInfoOfGuid((void *)TLDataStructures,&IID_LinkedList,&LinkedListDesc.ClassTypeInfo);

		IEnumLinkedList.QueryInterface = LLQueryInterface;//EnumLinkedList has an overloaded QueryInterface

		EnumLinkedListDesc.activeobject=0;
		EnumLinkedListDesc.ClassIID=(GUID *)&IID_IEnumUnknown;
		EnumLinkedListDesc.ClassTypeInfo=0;
		EnumLinkedListDesc.constructor=EnumLinkedListConstructor;
		EnumLinkedListDesc.destructor=EnumLinkedListDestructor;
		EnumLinkedListDesc.IDispatchBased=0;
		EnumLinkedListDesc.norefcount=0;
		EnumLinkedListDesc.objectsize=sizeof(EnumLinkedList);
		EnumLinkedListDesc.pVtbl=(IUnknownVtbl *)&IEnumLinkedList;
		EnumLinkedListDesc.subobjects=0;
		EnumLinkedListDesc.outgoinginterfaces=0;

		W2EConnectionPointDesc.activeobject=0;
		W2EConnectionPointDesc.ClassIID=(GUID *)&IID_IConnectionPoint;
		W2EConnectionPointDesc.constructor=W2EConnectionPointConstructor;
		W2EConnectionPointDesc.destructor=W2EConnectionPointDestructor;
		W2EConnectionPointDesc.IDispatchBased=0;
		W2EConnectionPointDesc.norefcount=0;
		W2EConnectionPointDesc.objectsize=sizeof(W2EConnectionPoint);
		W2EConnectionPointDesc.pVtbl=(IUnknownVtbl *)&IW2EConnectionPoint;
		W2EConnectionPointDesc.outgoinginterfaces=0;
		W2EConnectionPointDesc.subobjects=0;
		W2EConnectionPointDesc.ClassTypeInfo=0;

		W2EConnectionPointContainerDesc.activeobject=0;
		W2EConnectionPointContainerDesc.ClassIID=(GUID *)&IID_IConnectionPointContainer;
		W2EConnectionPointContainerDesc.ClassTypeInfo=0;
		W2EConnectionPointContainerDesc.constructor=W2EConnectionPointContainerConstructor;
		W2EConnectionPointContainerDesc.destructor=W2EConnectionPointContainerDestructor;
		W2EConnectionPointContainerDesc.IDispatchBased=0;
		W2EConnectionPointContainerDesc.norefcount=0;
		W2EConnectionPointContainerDesc.objectsize=sizeof(W2EConnectionPointContainer);
		W2EConnectionPointContainerDesc.outgoinginterfaces=0;
		W2EConnectionPointContainerDesc.pVtbl=(IUnknownVtbl *)&IW2EConnectionPointContainer;
		W2EConnectionPointContainerDesc.subobjects=0;

		TLDataStructures->lpVtbl->Release(TLDataStructures);
	}
	return;
}

/////////////////////////////////////
// a. VariantList

HRESULT STDMETHODCALLTYPE VLCount(VariantList * pThis,long * count)
{
	if(!count)
		return E_POINTER;
	else
		(*count)=pThis->count;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE VLItem(VariantList * pThis, long itemindex, VARIANT * item)
{
	VariantListElement * curel=pThis->firstelement;
	while(curel&&itemindex)
	{
		itemindex--;
		curel=curel->next;
	}
	if(curel)
		return VariantCopy(item,&(curel->element));
	else
		return S_FALSE;
}

HRESULT STDMETHODCALLTYPE VLNewEnum(VariantList * pThis,IUnknown ** newenum)
{
	HRESULT hr;
	hr=DoCreateInstance(&EnumVariantListDesc,0,&IID_IEnumVARIANT,(void **)newenum);
	if(!hr)
	{
		pThis->LLHeader.pVtbl->AddRef((void *)pThis);//keep Variant list alive as long as there are enumerators for it
		((EnumVariantList *)(*newenum))->currentelement=pThis->firstelement;
		((EnumVariantList *)(*newenum))->ownedlist=pThis;
	}
	return hr;
}

HRESULT STDMETHODCALLTYPE VLAddItem(VariantList * pThis,VARIANT toadd)
{
	VariantListElement * newel=GlobalAlloc(GMEM_FIXED,sizeof(VariantListElement));
	VariantInit(&(newel->element));
	newel->next=pThis->firstelement;
	VariantCopy(&(newel->element),&toadd);
	if(!pThis->firstelement)
		pThis->lastelement=newel;
	pThis->firstelement=newel;
	pThis->count++;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE VLRemoveItem(VariantList * pThis, long itemindex)
{
	VariantListElement * curel=pThis->firstelement;
	VariantListElement * prevel=0;
	while(curel&&itemindex--)
	{
		prevel=curel;
		curel=curel->next;
	}
	if(curel)
	{
		if(prevel)
			prevel->next=curel->next;
		else
			pThis->firstelement=curel->next;
		if(!curel->next)
			pThis->lastelement=prevel;
		VariantClear(&(curel->element));
		GlobalFree((HGLOBAL)curel);
		pThis->count--;
		return S_OK;
	}
	else
		return S_FALSE;
}

HRESULT STDMETHODCALLTYPE VLPop(VariantList * pThis, VARIANT * popped)
{
	HRESULT hr;
	hr=VLItem(pThis,0,popped);
	if(!hr)
		hr=VLRemoveItem(pThis,0);
	return hr;
}

HRESULT STDMETHODCALLTYPE VLQueue(VariantList * pThis, VARIANT toqueue)
{
	VariantListElement * newel=GlobalAlloc(GMEM_FIXED,sizeof(VariantListElement));
	VariantInit(&(newel->element));
	newel->next=0;
	VariantCopy(&(newel->element),&toqueue);
	if(!pThis->firstelement)
		pThis->firstelement=newel;
	else
		pThis->lastelement->next=newel;
	pThis->lastelement=newel;
	pThis->count++;
	return S_OK;
}

void STDMETHODCALLTYPE VariantListConstructor(void * pThis)
{
	((VariantList *)pThis)->firstelement=0;
	((VariantList *)pThis)->lastelement=0;
	((VariantList *)pThis)->count=0;
	return;
}

void STDMETHODCALLTYPE VariantListDestructor(void * pThis)
{
	VariantListElement * current=((VariantList *)pThis)->firstelement;
	VariantListElement * next=0;
	while(current)
	{
		next=current->next;
		VariantClear(&(current->element));
		GlobalFree((HGLOBAL)current);
		current=next;
	}
	((VariantList *)pThis)->count=0;
	((VariantList *)pThis)->firstelement=0;
	return;
}

HRESULT STDMETHODCALLTYPE VLNext(EnumVariantList * pThis,ULONG count,VARIANT * rgelt,ULONG * actualcount)
{
	ULONG i=0;
	ULONG originalcount=count;
	while(count&&(pThis->currentelement))
	{
		VariantCopy(rgelt+i,&(pThis->currentelement->element));
		pThis->currentelement=pThis->currentelement->next;
		i++;
		count--;
	}
	(*actualcount)=i;
	if(i==originalcount)
		return S_OK;
	else
		return S_FALSE;
}

HRESULT STDMETHODCALLTYPE VLSkip(EnumVariantList * pThis,ULONG count)
{
	ULONG i=0;
	for(i=0;i<count;i++)
	{
		if(!pThis->currentelement)
			return S_FALSE;
		else
			pThis->currentelement=pThis->currentelement->next;
	}
	return S_OK;
}

HRESULT STDMETHODCALLTYPE VLReset(EnumVariantList * pThis)
{
	pThis->currentelement=pThis->ownedlist->firstelement;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE VLClone(EnumVariantList * pThis,IUnknown ** newenum)
{
	HRESULT hr;
	hr=DoCreateInstance(&EnumVariantListDesc,0,&IID_IEnumVARIANT,(void **)newenum);
	if(!hr)
	{
		pThis->ownedlist->LLHeader.pVtbl->AddRef((void *)pThis->ownedlist);
		((EnumVariantList *)(*newenum))->currentelement=pThis->currentelement;
		((EnumVariantList *)(*newenum))->ownedlist=pThis->ownedlist;
	}
	return hr;
}

void STDMETHODCALLTYPE EnumVariantListConstructor(void * pThis)
{
	return;
}

void STDMETHODCALLTYPE EnumVariantListDestructor(void * pThis)
{
	((EnumVariantList *)pThis)->ownedlist->LLHeader.pVtbl->Release((void *)((EnumVariantList *)pThis)->ownedlist);//let go of owned Variant list
	return;
}
/////////////////////////////////////

/////////////////////////////////////
// b. LinkedList

HRESULT STDMETHODCALLTYPE LLCount(LinkedList * pThis,long * count)
{
	if(!count)
		return E_POINTER;
	else
		(*count)=pThis->count;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE LLItem(LinkedList * pThis, long itemindex, void ** item)
{
	LinkedListElement * curel=pThis->firstelement;
	
	if(!item)
		return E_POINTER;

	while(curel&&itemindex)
	{
		itemindex--;
		curel=curel->next;
	}

	if(curel)
	{
		(*item)=curel->element;
		return S_OK;
	}
	else
		return S_FALSE;
}

HRESULT STDMETHODCALLTYPE LLNewEnum(LinkedList * pThis,IUnknown ** newenum)
{
	HRESULT hr;
	hr=DoCreateInstance(&EnumLinkedListDesc,0,&IID_IUnknown,(void **)newenum);
	if(!hr)
	{
		pThis->LLHeader.pVtbl->AddRef((void *)pThis);//keep LinkedList alive as long as there are enumerators for it
		((EnumLinkedList *)(*newenum))->currentelement=pThis->firstelement;
		((EnumLinkedList *)(*newenum))->ownedlist=pThis;
	}
	return hr;
}

HRESULT STDMETHODCALLTYPE LLAddItem(LinkedList * pThis,void * toadd)
{
	LinkedListElement * newel=GlobalAlloc(GMEM_FIXED,sizeof(LinkedListElement));
	
	newel->element=toadd;
	newel->next=pThis->firstelement;
	
	if(!pThis->firstelement)
		pThis->lastelement=newel;
	
	pThis->firstelement=newel;
	pThis->count++;
	
	return S_OK;
}

HRESULT STDMETHODCALLTYPE LLRemoveItem(LinkedList * pThis, long itemindex)
{
	LinkedListElement * curel=pThis->firstelement;
	LinkedListElement * prevel=0;
	while(curel&&itemindex--)
	{
		prevel=curel;
		curel=curel->next;
	}
	if(curel)
	{
		if(prevel)
			prevel->next=curel->next;
		else
			pThis->firstelement=curel->next;
		if(!curel->next)
			pThis->lastelement=prevel;
		GlobalFree((HGLOBAL)curel);
		pThis->count--;
		return S_OK;
	}
	else
		return S_FALSE;
}

HRESULT STDMETHODCALLTYPE LLPop(LinkedList * pThis, void ** popped)
{
	HRESULT hr;
	hr=LLItem(pThis,0,popped);
	if(!hr)
		hr=LLRemoveItem(pThis,0);
	return hr;
}

HRESULT STDMETHODCALLTYPE LLQueue(LinkedList * pThis, void * toqueue)
{
	LinkedListElement * newel=GlobalAlloc(GMEM_FIXED,sizeof(LinkedListElement));
	newel->element=toqueue;
	newel->next=0;

	if(!pThis->firstelement)
		pThis->firstelement=newel;
	else
		pThis->lastelement->next=newel;

	pThis->lastelement=newel;
	pThis->count++;
	
	return S_OK;
}

void STDMETHODCALLTYPE LinkedListConstructor(void * pThis)
{
	((LinkedList *)pThis)->firstelement=0;
	((LinkedList *)pThis)->lastelement=0;
	((LinkedList *)pThis)->count=0;
	((LinkedList *)pThis)->EnumeratorIID=(GUID *)&IID_IEnumUnknown;
	return;
}

void STDMETHODCALLTYPE LinkedListDestructor(void * pThis)
{
	LinkedListElement * current=((LinkedList *)pThis)->firstelement;
	LinkedListElement * next=0;
	while(current)
	{
		next=current->next;
		GlobalFree((HGLOBAL)current);
		current=next;
	}
	((LinkedList *)pThis)->count=0;
	((LinkedList *)pThis)->firstelement=0;
	return;
}

HRESULT STDMETHODCALLTYPE LLNext(EnumLinkedList * pThis,ULONG count,void ** rgelt,ULONG * actualcount)
{
	ULONG i=0;
	ULONG originalcount=count;
	while(count&&(pThis->currentelement))
	{
		(*(rgelt+i))=pThis->currentelement->element;
		pThis->currentelement=pThis->currentelement->next;
		i++;
		count--;
	}
	(*actualcount)=i;
	if(i==originalcount)
		return S_OK;
	else
		return S_FALSE;
}

HRESULT STDMETHODCALLTYPE LLSkip(EnumLinkedList * pThis,ULONG count)
{
	ULONG i=0;
	for(i=0;i<count;i++)
	{
		if(!pThis->currentelement)
			return S_FALSE;
		else
			pThis->currentelement=pThis->currentelement->next;
	}
	return S_OK;
}

HRESULT STDMETHODCALLTYPE LLReset(EnumLinkedList * pThis)
{
	pThis->currentelement=pThis->ownedlist->firstelement;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE LLClone(EnumLinkedList * pThis,IUnknown ** newenum)
{
	HRESULT hr;
	hr=DoCreateInstance(&EnumLinkedListDesc,0,&IID_IUnknown,(void **)newenum);
	if(!hr)
	{
		pThis->ownedlist->LLHeader.pVtbl->AddRef((void *)pThis->ownedlist);
		((EnumLinkedList *)(*newenum))->currentelement=pThis->currentelement;
		((EnumLinkedList *)(*newenum))->ownedlist=pThis->ownedlist;
	}
	return hr;
}

void STDMETHODCALLTYPE EnumLinkedListConstructor(void * pThis)
{
	return;
}

void STDMETHODCALLTYPE EnumLinkedListDestructor(void * pThis)
{
	((EnumLinkedList *)pThis)->ownedlist->LLHeader.pVtbl->Release((void *)((EnumLinkedList *)pThis)->ownedlist);//let go of owned LinkedList
	return;
}

HRESULT STDMETHODCALLTYPE LLQueryInterface(void * pThis,REFIID riid,void ** ppv)//EnumLinkedList QueryInterface
{
	HRESULT hr;
	ObjectHeader * pHeader=(ObjectHeader *)pThis;

	if(pHeader->owner)
	{
		IUnknown * ownerunknown=(IUnknown *)pHeader->owner;
		hr=ownerunknown -> lpVtbl->QueryInterface((void *)pHeader->owner,riid,ppv);
		return hr;
	}
	else
	{
		if(IsEqualIID(riid,pHeader->classinfo->ClassIID))
		{
			(*ppv)=pThis;
			((IUnknownVtbl *)pHeader->pVtbl)->AddRef(pThis);
			return 0;
		}
		else if(IsEqualIID(riid,&IID_IUnknown))
		{
			(*ppv)=pThis;
			((IUnknownVtbl *)pHeader->pVtbl)->AddRef(pThis);
			return 0;
		}
		else if(IsEqualIID(riid,((EnumLinkedList *)pThis)->ownedlist->EnumeratorIID))
		{
			(*ppv)=pThis;
			((IUnknownVtbl *)pHeader->pVtbl)->AddRef(pThis);
			return 0;
		}
		(*ppv)=0;
		return E_NOINTERFACE;
	}
}
/////////////////////////////////////

/////////////////////////////////////
// c. IConnectionPoint / IConnectionPointContainer

HRESULT STDMETHODCALLTYPE GetConnectionInterface(W2EConnectionPoint * pThis,IID * pIID)
{
	if(!pIID)
		return E_POINTER;
	else if(!pThis->CallbackIID)
		return E_UNEXPECTED;
	else
	{
		CopyMemory((void *)pIID,(const void *)(pThis->CallbackIID),sizeof(GUID));
		return S_OK;
	}
}

HRESULT STDMETHODCALLTYPE GetConnectionPointContainer(W2EConnectionPoint * pThis,W2EConnectionPointContainer ** ppCPC)
{
	if(!ppCPC)
		return E_POINTER;
	else
		return DefaultQueryInterface((void *)pThis->CPContainer,&IID_IConnectionPointContainer,(void **)ppCPC);
}

HRESULT STDMETHODCALLTYPE Advise(W2EConnectionPoint * pThis,IUnknown * pUnk,DWORD * pdwCookie)
{
	void * pActual;
	CONNECTDATA * newcd;
	if((pThis->MaxConnectionCount)&&(pThis->LLConnections->count==pThis->MaxConnectionCount))
		return CONNECT_E_ADVISELIMIT;
	if((!pUnk)||(!pdwCookie))
		return E_POINTER;
	if(pUnk->lpVtbl->QueryInterface(pUnk,&IID_IDispatch,&pActual))//CallbackInterfaces need to support IDispatch and all calls will be done through Invoke!!!
		return CONNECT_E_CANNOTCONNECT;
	newcd=(CONNECTDATA *)GlobalAlloc(GMEM_FIXED,sizeof(CONNECTDATA));
	newcd->dwCookie=(DWORD)newcd;
	newcd->pUnk=pActual;
	(*pdwCookie)=(DWORD)newcd;
	debugprintf("\nsuccessfull advise\n\n");
	return LLQueue(pThis->LLConnections,(void *)newcd);
}

HRESULT STDMETHODCALLTYPE Unadvise(W2EConnectionPoint * pThis,DWORD dwCookie)
{
	CONNECTDATA * toremove;
	long i=0;
	EnumLinkedList * ConnEnum;
	ULONG actualc;
	LLNewEnum(pThis->LLConnections,(IUnknown **)&ConnEnum);
	while(LLNext(ConnEnum,1,&toremove,&actualc)==S_OK)
	{
		if(toremove->dwCookie==dwCookie)
		{
			LLRemoveItem(pThis->LLConnections,i);//remove from connection list
			toremove->pUnk->lpVtbl->Release(toremove->pUnk);//let go of outgoing object implementing the outgoing interface
			GlobalFree((HGLOBAL)toremove);//clean up the CONNECTDATA structure
			DefaultRelease((void *)ConnEnum);
			return S_OK;
		}
		i++;
	}
	DefaultRelease((void *)ConnEnum);
	return CONNECT_E_NOCONNECTION;//we don't have this connection
}

HRESULT STDMETHODCALLTYPE EnumConnections(W2EConnectionPoint * pThis,EnumLinkedList ** ppEnum)
{
	if(!ppEnum)
		return E_POINTER;
	return LLNewEnum(pThis->LLConnections,(IUnknown **)ppEnum);
}

HRESULT STDMETHODCALLTYPE EnumConnectionPoints(W2EConnectionPointContainer * pThis,EnumLinkedList ** ppEnum)
{
	if(!ppEnum)
		return E_POINTER;
	return LLNewEnum(pThis->LLConnectionPoints,(IUnknown **)ppEnum);
}

HRESULT STDMETHODCALLTYPE FindConnectionPoint(W2EConnectionPointContainer * pThis,REFIID riid,W2EConnectionPoint ** ppCP)
{
	W2EConnectionPoint * current;
	ULONG acount;
	EnumLinkedList * ConnPEnum;
	LLNewEnum(pThis->LLConnectionPoints,(IUnknown **)&ConnPEnum);
	while(LLNext(ConnPEnum,1,(void **)&current,&acount)==S_OK)
	{
		if(IsEqualIID(current->CallbackIID,riid))
		{
			current->CPHeader.pVtbl->AddRef((void *)current);
			(*ppCP)=current;
			DefaultRelease((void *)ConnPEnum);
			return S_OK;
		}
	}
	(*ppCP)=0;
	DefaultRelease((void *)ConnPEnum);
	return CONNECT_E_NOCONNECTION;
}

void STDMETHODCALLTYPE W2EConnectionPointConstructor(void * pThis)
{
	W2EConnectionPoint * curcp=(W2EConnectionPoint *)pThis;
	DoCreateInstance(&LinkedListDesc,0,&IID_LinkedList,(void **)&curcp->LLConnections);
	curcp->LLConnections->EnumeratorIID=(GUID *)&IID_IEnumConnections;
	return;
}

void STDMETHODCALLTYPE W2EConnectionPointDestructor(void * pThis)
{
	CONNECTDATA * curcd;
	W2EConnectionPoint * curcp=(W2EConnectionPoint *)pThis;
	while(curcp->LLConnections->count)
	{
		LLItem(curcp->LLConnections,0,(void **)&curcd);
		curcd->pUnk->lpVtbl->Release(curcd->pUnk);//let go of outgoing interface
		LLRemoveItem(curcp->LLConnections,0);
		GlobalFree((HGLOBAL)curcd);
	}
	curcp->LLConnections->LLHeader.pVtbl->Release((void *)curcp->LLConnections);
	return;
}

void STDMETHODCALLTYPE W2EConnectionPointContainerConstructor(void * pThis)
{
	W2EConnectionPoint * newcp;
	OutgoingInterface * curoi;
	W2EConnectionPointContainer * pCont=(W2EConnectionPointContainer *)pThis;
	DoCreateInstance(&LinkedListDesc,0,&IID_LinkedList,(void **)&(pCont->LLConnectionPoints));
	pCont->LLConnectionPoints->EnumeratorIID=(GUID *)&IID_IEnumConnectionPoints;
	curoi=pCont->CPCHeader.owner->classinfo->outgoinginterfaces;
	while(curoi)
	{
		DoCreateInstance(&W2EConnectionPointDesc,0,&IID_IConnectionPoint,(void **)&newcp);
		newcp->CallbackIID=curoi->OutgoingIID;
		newcp->MaxConnectionCount=curoi->MaxCallbackCount;
		newcp->CPContainer=pCont;
		LLAddItem(pCont->LLConnectionPoints,newcp);
		curoi=curoi->next;
	}
	return;
}

void STDMETHODCALLTYPE W2EConnectionPointContainerDestructor(void * pThis)
{
	W2EConnectionPoint * curcp;
	W2EConnectionPointContainer * pCont=(W2EConnectionPointContainer *)pThis;

	while(pCont->LLConnectionPoints->count)
	{
		LLItem(pCont->LLConnectionPoints,0,(void **)&curcp);
		curcp->CPHeader.pVtbl->Release((void *)curcp);
		LLRemoveItem(pCont->LLConnectionPoints,0);
	}

	pCont->LLConnectionPoints->LLHeader.pVtbl->Release((void *)pCont->LLConnectionPoints);

	return;
}

void InvokeEvent(void * onobject,REFIID eventguid,DISPID toinvoke,DISPPARAMS * params,VARIANT * result)
{
	EXCEPINFO testei;
	long firstpar;
	W2EConnectionPointContainer * mycp;
	W2EConnectionPoint * conp;
	EnumLinkedList * ell;
	long acount;
	CONNECTDATA * myconndata;

	DefaultQueryInterface((void *)onobject,&IID_IConnectionPointContainer,(void **)&mycp);
	FindConnectionPoint(mycp,eventguid,&conp);
	EnumConnections(conp,&ell);
	while(LLNext(ell,1,(void **)&myconndata,&acount)==S_OK)
		if(((IDispatch *)myconndata->pUnk)->lpVtbl->Invoke((void *)myconndata->pUnk,toinvoke,&IID_NULL,0,DISPATCH_METHOD,params,result,&testei,&firstpar))
			debugprintf("Invoke Event failed!\n");
	ell->LLEnumHeader.pVtbl->Release((void *)ell);
	conp->CPHeader.pVtbl->Release((void *)conp);
	mycp->CPCHeader.pVtbl->Release((void *)mycp);
	return;
}

/////////////////////////////////////