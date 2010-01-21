#include "COMHELP.h"
#include <time.h>
#include "W2EDebug.h"

#define FIRSTREFTIMEOUT 3333

GuidInfo * guiddebug=0;

DWORD TotalRefCount=0;
DWORD InternalRefCount=0;
DWORD FirstRef=1;
DWORD StartTime=0;
DWORD LockCount=0;
IClassFactoryVtbl DefaultClassFactoryVtbl={DefaultQueryInterface,DefaultAddRef,DefaultRelease,DefaultClassCreateInstance,DefaultClassLockServer};
ClassDesc DefaultClassFactoryDesc = { (IUnknownVtbl  *) & DefaultClassFactoryVtbl , 0 , 0 , 0 , (GUID *)&IID_IClassFactory , 0 , 0, sizeof(ClassFactory) , 0 , 1 };

void debugprintrefcounts()
{
	debugprintf("TotalRefCount: %u, InternalRefCount: %u\n",TotalRefCount,InternalRefCount);
	return;
}

void RemoveOutgoingInterfaces(ClassDesc * toremove)
{
	if(toremove->outgoinginterfaces)
		RecursiveDeleteOutgoingInterfaces(toremove->outgoinginterfaces);
	toremove->outgoinginterfaces=0;
	return;
}

void RecursiveDeleteOutgoingInterfaces(OutgoingInterface * todelete)
{
	if(todelete->next)
		RecursiveDeleteOutgoingInterfaces(todelete->next);
	GlobalFree((HGLOBAL)todelete);
	return;
}

void AddOutgoingInterface(ClassDesc * ownerdesc,GUID * OutgoingIID,unsigned int maxcallbackcount)
{
	OutgoingInterface * newoi=(OutgoingInterface *)GlobalAlloc(GMEM_FIXED,sizeof(OutgoingInterface));
	newoi->MaxCallbackCount=maxcallbackcount;
	newoi->OutgoingIID=OutgoingIID;
	newoi->next=ownerdesc->outgoinginterfaces;
	ownerdesc->outgoinginterfaces=newoi;
	return;
}

void AddSubObject(ClassDesc * ownerdesc,ClassDesc * subobjectdesc)
{
	SubObject * newso=(SubObject *)GlobalAlloc(GMEM_FIXED,sizeof(SubObject));
	newso->classdesc=subobjectdesc;
	newso->next=ownerdesc->subobjects;
	ownerdesc->subobjects=newso;
	return;
}

void RecursiveDeleteSubobject(SubObject * todelete)
{
	if(todelete->next)
		RecursiveDeleteSubobject(todelete->next);
	GlobalFree((HGLOBAL)todelete);
	return;
}

void RemoveSubObjects(ClassDesc * toremove)
{
	if(toremove->subobjects)
		RecursiveDeleteSubobject(toremove->subobjects);
	toremove->subobjects=0;
	return;
}

void InternalAddRef()
{
	InterlockedIncrement(&InternalRefCount);
	return;
}
void InternalRelease()
{
	InterlockedDecrement(&InternalRefCount);
	return;
}

int CanUnloadNow()
{	
	if(!StartTime)
	{
		InterlockedExchange(&StartTime,clock());
		return 0;
	}
	else if(FirstRef)
	{
		if((clock()-StartTime)>=FIRSTREFTIMEOUT)
			InterlockedExchange(&FirstRef,0);
		return 0;
	}
	else if((TotalRefCount-InternalRefCount)||LockCount)
		return 0;
	else
		return 1;
}

ClassFactory * CreateFactory(ClassDesc  * classinfo)
{
	ClassFactory * newcf=GlobalAlloc(GMEM_FIXED,sizeof(ClassFactory));
	newcf->cfheader.classinfo=&DefaultClassFactoryDesc;
	newcf->cfheader.owner=0;
	newcf->cfheader.pVtbl=(IUnknownVtbl *)&DefaultClassFactoryVtbl;
	newcf->cfheader.refcount=1;
	newcf->ownedclassinfo=classinfo;
	return newcf;
}

HRESULT STDMETHODCALLTYPE DefaultClassCreateInstance(void * pThis,IUnknown * punkOuter, REFIID vTableGuid, void **ppv)
{
	HRESULT hr;
	ObjectHeader * newobj, * newsubobj;
	unsigned int i=0;
	unsigned int tempvar=0;
	SubObject * curso=0;
	ClassFactory * ohThis=(ClassFactory *)pThis;

	(*ppv)=0;

	if(punkOuter)
		hr=CLASS_E_NOAGGREGATION;
	else
	{
		if(ohThis->ownedclassinfo->activeobject)
		{
			(*ppv)=ohThis->ownedclassinfo->activeobject;
			
			((ObjectHeader *)(*ppv)) -> pVtbl -> AddRef((*ppv));
			hr = ((ObjectHeader *)(*ppv)) -> pVtbl -> QueryInterface( (*ppv) , vTableGuid , ppv ) ;
			((ObjectHeader *)(*ppv)) -> pVtbl -> Release ( (*ppv) ) ;
		}
		else
		{
			//calculate size of object + subobjects in tempvar
			curso=ohThis->ownedclassinfo->subobjects;
			tempvar=ohThis->ownedclassinfo->objectsize;
			while(curso)
			{
				tempvar+=curso->classdesc->objectsize;
				curso=curso->next;
			}

			//allocate space for object + subobjects
			newobj=(ObjectHeader *)GlobalAlloc(GMEM_FIXED,tempvar);//allocate object + SubObjects
			if(!newobj)
				hr=E_OUTOFMEMORY;//allocation failed
			else
			{
				//init object
				newobj->classinfo=ohThis->ownedclassinfo;
				newobj->owner=0;
				newobj->pVtbl=ohThis->ownedclassinfo->pVtbl;
				
				//init all subobjects
				tempvar=ohThis->ownedclassinfo->objectsize;
				curso=ohThis->ownedclassinfo->subobjects;
				while(curso)
				{
					//init curreent subobject
					newsubobj=(ObjectHeader *)((char *)newobj+tempvar);
					newsubobj->classinfo=curso->classdesc;
					newsubobj->owner=newobj;
					newsubobj->pVtbl=curso->classdesc->pVtbl;
					newsubobj->refcount=1;//guard : a subobject is cleaned up together with its owner

					//call constructor
					if(newsubobj->classinfo->constructor)
						newsubobj->classinfo->constructor((void *)newsubobj);

					//next subobject
					tempvar+=curso->classdesc->objectsize;
					curso=curso->next;
				}

				//call constructor
				if(ohThis->ownedclassinfo->constructor)
					ohThis->ownedclassinfo->constructor((void  *)newobj);

				//get requested interface
				newobj->refcount=0;
				newobj->pVtbl->AddRef((void *)newobj);
				hr=newobj->pVtbl->QueryInterface((void *)newobj,vTableGuid,ppv);
				newobj->pVtbl->Release((void *)newobj);
			}			
		}
	}
	debugprintf("Created instance %p of desc %p\n",(*ppv),ohThis->ownedclassinfo);
	return hr;
}

HRESULT STDMETHODCALLTYPE DoCreateInstance(ClassDesc * ofclass, int internal, REFIID vTableGuid, void **ppv)
{
	HRESULT hr;
	ObjectHeader * newobj, * newsubobj;
	unsigned int i=0;
	unsigned int tempvar=0;
	SubObject * curso=0;

	(*ppv)=0;

		if(ofclass->activeobject)
		{
			(*ppv)=ofclass->activeobject;
			
			((ObjectHeader *)(*ppv)) -> pVtbl -> AddRef((*ppv));
			hr = ((ObjectHeader *)(*ppv)) -> pVtbl -> QueryInterface( (*ppv) , vTableGuid , ppv ) ;
			((ObjectHeader *)(*ppv)) -> pVtbl -> Release ( (*ppv) ) ;
		}
		else
		{
			//calculate size of object + subobjects in tempvar
			curso=ofclass->subobjects;
			tempvar=ofclass->objectsize;
			while(curso)
			{
				tempvar+=curso->classdesc->objectsize;
				curso=curso->next;
			}

			//allocate space for object + subobjects
			newobj=(ObjectHeader *)GlobalAlloc(GMEM_FIXED,tempvar);//allocate object + SubObjects
			if(!newobj)
				hr=E_OUTOFMEMORY;//allocation failed
			else
			{
				//init object
				newobj->classinfo=ofclass;
				newobj->owner=0;
				newobj->pVtbl=ofclass->pVtbl;
				
				//init all subobjects
				tempvar=ofclass->objectsize;
				curso=ofclass->subobjects;
				while(curso)
				{
					//init curreent subobject
					newsubobj=(ObjectHeader *)((char *)newobj+tempvar);
					newsubobj->classinfo=curso->classdesc;
					newsubobj->owner=newobj;
					newsubobj->pVtbl=curso->classdesc->pVtbl;
					newsubobj->refcount=1;//guard : a subobject is cleaned up together with its owner

					//call constructor
					if(newsubobj->classinfo->constructor)
						newsubobj->classinfo->constructor((void *)newsubobj);

					//next subobject
					tempvar+=curso->classdesc->objectsize;
					curso=curso->next;
				}

				//call constructor
				if(ofclass->constructor)
					ofclass->constructor((void  *)newobj);

				//get requested interface
				newobj->refcount=0;
				newobj->pVtbl->AddRef((void *)newobj);
				hr=newobj->pVtbl->QueryInterface((void *)newobj,vTableGuid,ppv);
				newobj->pVtbl->Release((void *)newobj);
			}			
		}

		if((!hr)&&internal)
			InternalAddRef();

	debugprintf("Created instance %p\n",(*ppv));
	return hr;
}

DISPID DispIDFromTypeLib(ITypeLib * tlib,REFIID guid,LPOLESTR methodname)
{
	DISPID toreturn;
	ITypeInfo * curti;
	tlib->lpVtbl->GetTypeInfoOfGuid(tlib,(REFGUID)guid,&curti);
	curti->lpVtbl->GetIDsOfNames(curti,&methodname,1,&toreturn);
	curti->lpVtbl->Release(curti);
	return toreturn;
}

DISPPARAMS * BuildParams(unsigned int parcount)
{
	unsigned int i=0;
	DISPPARAMS * toreturn=(DISPPARAMS *)GlobalAlloc(GMEM_FIXED,sizeof(DISPPARAMS));
	toreturn->cArgs=0;
	toreturn->cNamedArgs=0;
	toreturn->rgdispidNamedArgs=0;
	if(parcount>0)
	{
		toreturn->rgvarg=(VARIANT *)GlobalAlloc(GMEM_FIXED,sizeof(VARIANT)*parcount);
		for(i=0;i<parcount;i++)
			VariantInit(toreturn->rgvarg+i);
	}
	else
		toreturn->rgvarg=0;
	return toreturn;
}

void PassInt(DISPPARAMS * arguments,int topass)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_INT;
	(arguments->rgvarg+arguments->cArgs)->intVal=topass;
	arguments->cArgs++;
	return;
}

void PassUInt(DISPPARAMS * arguments,unsigned int topass)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_UINT;
	(arguments->rgvarg+arguments->cArgs)->uintVal=topass;
	arguments->cArgs++;
	return;
}

void PassShort(DISPPARAMS * arguments,short topass)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_I2;
	(arguments->rgvarg+arguments->cArgs)->iVal=topass;
	arguments->cArgs++;
	return;
}

void PassUShort(DISPPARAMS * arguments,unsigned short topass)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_UI2;
	(arguments->rgvarg+arguments->cArgs)->uiVal=topass;
	arguments->cArgs++;
	return;
}

void PassSChar(DISPPARAMS * arguments,signed char topass)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_I1;
	(arguments->rgvarg+arguments->cArgs)->cVal=topass;
	arguments->cArgs++;
	return;
}

void PassUChar(DISPPARAMS * arguments,unsigned char topass)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_UI1;
	(arguments->rgvarg+arguments->cArgs)->bVal=topass;
	arguments->cArgs++;
	return;
}

void PassString(DISPPARAMS * arguments,LPOLESTR topass)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_BSTR;
	(arguments->rgvarg+arguments->cArgs)->bstrVal=SysAllocString(topass);
	arguments->cArgs++;
	return;
}

BSTR StringToBSTR(char * string)
{
	DWORD len;
	BSTR  toreturn;
	len = MultiByteToWideChar(CP_ACP, 0, string, -1, 0, 0);
	toreturn = SysAllocStringLen(0, len);
	MultiByteToWideChar(CP_ACP, 0, string, -1, toreturn, len);
	return toreturn;
}

void PassObject(DISPPARAMS * arguments,IDispatch * dispobj)
{
	(arguments->rgvarg+arguments->cArgs)->vt=VT_DISPATCH;
	(arguments->rgvarg+arguments->cArgs)->pdispVal=dispobj;
	arguments->cArgs++;
	return;
}

void DeleteParams(DISPPARAMS * toremove)
{
	unsigned int i=0;
	for(i=0;i<toremove->cArgs;i++)
	{
		if((*(toremove->rgvarg+i)).vt==VT_BSTR)
		{
			SysFreeString((*(toremove->rgvarg+i)).bstrVal);
			(*(toremove->rgvarg+i)).vt=VT_NULL;
			(*(toremove->rgvarg+i)).bstrVal=NULL;
		}
		else if((*(toremove->rgvarg+i)).vt==VT_DISPATCH)
		{
			(*(toremove->rgvarg+i)).pdispVal->lpVtbl->Release((*(toremove->rgvarg+i)).pdispVal);
			(*(toremove->rgvarg+i)).vt=VT_NULL;
			(*(toremove->rgvarg+i)).pdispVal=NULL;
		}
		else
			VariantClear(toremove->rgvarg+i);
	}
	GlobalFree((HGLOBAL)toremove->rgvarg);
	GlobalFree((HGLOBAL)toremove);
	return;
}

HRESULT STDMETHODCALLTYPE DefaultClassLockServer(void * pThis,BOOL flock)
{
	if(flock)
		InterlockedIncrement(&LockCount);
	else
		InterlockedDecrement(&LockCount);
	return(NOERROR);
}

ULONG STDMETHODCALLTYPE DefaultGetTypeInfoCount(void * pThis, UINT * pCount)
{
	debugprintf("Default get type info count\n");
	*pCount = 1;
   return(S_OK);
}

ULONG STDMETHODCALLTYPE DefaultGetTypeInfo(void * pThis, UINT itinfo, LCID lcid, ITypeInfo ** pTypeInfo)
{	
	ObjectHeader * oh=(ObjectHeader *)pThis;
	
	*pTypeInfo = 0;
	
	if(itinfo)
		return ResultFromScode(DISP_E_BADINDEX);

	if(!((oh->classinfo->ClassTypeInfo)))
		return TYPE_E_ELEMENTNOTFOUND;
	
	*pTypeInfo = (oh->classinfo->ClassTypeInfo);

	return 0;
}

ULONG STDMETHODCALLTYPE DefaultGetIDsOfNames(void * pThis, REFIID riid, LPOLESTR * rgszNames, UINT cNames, LCID lcid, DISPID * rgdispid)
{	
	HRESULT hr;
	ObjectHeader * oh=(ObjectHeader *)pThis;

	debugprintf("DefaultGetIDsOfNames\n");

	if(!((oh->classinfo->ClassTypeInfo)))
		return TYPE_E_ELEMENTNOTFOUND;

	hr=DispGetIDsOfNames((oh->classinfo->ClassTypeInfo),rgszNames,cNames,rgdispid);
	return hr;
}

ULONG STDMETHODCALLTYPE DefaultInvoke(void * pThis, DISPID dispid, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS *params, VARIANT *result, EXCEPINFO *pexcepinfo, UINT *puArgErr)
{
	HRESULT hr;
	ObjectHeader * oh=(ObjectHeader *)pThis;

	debugprintf("DefaultInvoke\n");

	if(!((oh->classinfo->ClassTypeInfo)))
		return TYPE_E_ELEMENTNOTFOUND;

	if (!IsEqualIID(riid, &IID_NULL))
      return(DISP_E_UNKNOWNINTERFACE);

	hr=DispInvoke(pThis, (oh->classinfo->ClassTypeInfo), dispid, wFlags, params, result, pexcepinfo, puArgErr);
	return hr;
}

HRESULT STDMETHODCALLTYPE DefaultQueryInterface(void * pThis,REFIID riid,void ** ppv)
{
	HRESULT hr;
	unsigned int curoffset=0;
	SubObject * curso=0;
	ObjectHeader * pHeader=(ObjectHeader *)pThis;

	debugprintf("qi on guid: ");
	debugprintIID((GUID *)riid);
	debugprintf("\n");

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
			debugprintf("successfull QI\n");
			return 0;
		}
		else if(IsEqualIID(riid,&IID_IUnknown))
		{
			(*ppv)=pThis;
			((IUnknownVtbl *)pHeader->pVtbl)->AddRef(pThis);
			return 0;
		}
		else if((pHeader->classinfo->IDispatchBased)&&IsEqualIID(riid,&IID_IDispatch))
		{
			(*ppv)=pThis;
			((IUnknownVtbl *)pHeader->pVtbl)->AddRef(pThis);
			return 0;
		}
		else
		{
			debugprintf("looking for subobject interface\n");
			curoffset=pHeader->classinfo->objectsize;
			curso=pHeader->classinfo->subobjects;
			while(curso)
			{
				if(IsEqualIID(riid,curso->classdesc->ClassIID))
				{
					debugprintf("returning subobject!\n");
					(*ppv)=(void *)((char *)pThis+curoffset);
					((IUnknownVtbl *)pHeader->pVtbl)->AddRef(pThis);
					return 0;
				}
				curoffset+=curso->classdesc->objectsize;
				curso=curso->next;
			}
		}
		(*ppv)=0;
		return E_NOINTERFACE;
	}
}

ULONG STDMETHODCALLTYPE DefaultAddRef(void * pThis)
{
	ObjectHeader * ohThis=(ObjectHeader *)pThis;
	if(ohThis->owner)
		return ((IUnknownVtbl *)ohThis->owner->pVtbl)->AddRef((void *)ohThis->owner);
	else if(ohThis->classinfo->norefcount)
		return ohThis->refcount;
	else
	{
		++ohThis->refcount;
		debugprintf("AddRef on %p, new refcount: %u\n",pThis,ohThis->refcount);
		InterlockedIncrement(&TotalRefCount);
		return ohThis->refcount;
	}
}

ULONG STDMETHODCALLTYPE DefaultRelease(void * pThis)
{
	SubObject * curso=0;
	unsigned int curoffset=0;
	ObjectHeader * ohThis=(ObjectHeader *)pThis;
	
	if(ohThis->owner)
		return ((IUnknownVtbl *)ohThis->owner->pVtbl)->Release((void *)ohThis->owner);
	else if(ohThis->classinfo->norefcount)
		return ohThis->refcount;
	else
	{
		ohThis->refcount--;
		InterlockedDecrement(&TotalRefCount);
		debugprintf("Release on %p, new refcount: %u\n",pThis,ohThis->refcount);
		if(ohThis->refcount==0)
		{
			debugprintf("destroying object %p\n",pThis);
			debugprintrefcounts();
			if(ohThis->classinfo->destructor)
				ohThis->classinfo->destructor(pThis);
			curso=ohThis->classinfo->subobjects;
			curoffset=ohThis->classinfo->objectsize;
			while(curso)
			{
				ohThis=(ObjectHeader *)((char *)pThis+curoffset);
				ohThis->classinfo->destructor((void *)ohThis);
				curoffset+=ohThis->classinfo->objectsize;
				curso=curso->next;
			}
			GlobalFree(pThis);//free this+subobjects
		    return 0;
		}
		return ohThis->refcount;
	}
}

void DoInvoke(IDispatch * onobject,DISPID toinvoke,DISPPARAMS * params,VARIANT * result)
{
	EXCEPINFO testei;
	long firstpar;
	hr_print(onobject->lpVtbl->Invoke(onobject,toinvoke,&IID_NULL,0,DISPATCH_METHOD,params,result,&testei,&firstpar));
	return;
}

void hr_print(HRESULT hr)
{
	switch(hr)
	{
	case S_OK:
		debugprintf("Success");
		break;
	case DISP_E_BADPARAMCOUNT:
		debugprintf("The number of elements provided to DISPPARAMS is different from the number of arguments accepted by the method or property.");
		break;
	case DISP_E_BADVARTYPE:
		debugprintf("One of the arguments in rgvarg is not a valid variant type.");
		break;
	case DISP_E_EXCEPTION:
		debugprintf("The application needs to raise an exception. In this case, the structure passed in pExcepInfo should be filled in.");
		break;
	case DISP_E_MEMBERNOTFOUND:
		debugprintf("The requested member does not exist, or the call to Invoke tried to set the value of a read-only property.");
		break;
	case DISP_E_NONAMEDARGS:
		debugprintf("This implementation of IDispatch does not support named arguments.");
		break;
	case DISP_E_OVERFLOW:
		debugprintf("One of the arguments in rgvarg could not be coerced to the specified type.");
		break;
	case DISP_E_PARAMNOTFOUND:
		debugprintf("One of the parameter DISPIDs does not correspond to a parameter on the method. In this case, puArgErr should be set to the first argument that contains the error. You get the error DISP_E_PARAMNOTFOUND when you try to set a property and you have not initialized the cNamedArgs and rgdispidNamedArgs elements of your DISPPARAMS structure.");
		break;
	case DISP_E_TYPEMISMATCH:
		debugprintf("One or more of the arguments could not be coerced. The index within rgvarg of the first parameter with the incorrect type is returned in the puArgErr parameter.");
		break;
	case DISP_E_UNKNOWNINTERFACE:
		debugprintf("The interface identifier passed in riid is not IID_NULL.");
		break;
	case DISP_E_UNKNOWNLCID:
		debugprintf("The member being invoked interprets string arguments according to the LCID, and the LCID is not recognized. If the LCID is not needed to interpret arguments, this error should not be returned.");
		break;
	case DISP_E_PARAMNOTOPTIONAL:
		debugprintf("A required parameter was omitted.");
		break;
	case TYPE_E_ELEMENTNOTFOUND:
		debugprintf("TYPE_E_ELEMENTNOTFOUND");
		break;
	case CLASS_E_NOAGGREGATION:
		debugprintf("The pUnkOuter parameter was non-NULL and the object does not support aggregation.");
		break;
	case E_NOINTERFACE:
		debugprintf("The object that ppvObject points to does not support the interface identified by riid.");
		break;
	default:
		debugprintf("Unknown return value : %u!",hr);
		break;
	}
	return;
}

void AddGuidInfo(GUID * guid,char * name)
{
	GuidInfo * newinfo=(GuidInfo *)malloc(sizeof(GuidInfo));
	newinfo->guid=guid;
	newinfo->name=name;
	newinfo->next=guiddebug;
	guiddebug=newinfo;
	return;
}

void debugprintIID(GUID * toprint)
{
	GuidInfo * curinfo=guiddebug;
	while(curinfo)
	{
		if(IsEqualIID(toprint,curinfo->guid))
		{
			debugprintf("%s",curinfo->name);
			return;
		}
		curinfo=curinfo->next;
	}
	debugprintf("unknown guid");
	return;
}