#include "Collection.h"
#include "W2EDebug.h"

int CollPointerCompare(void * a,void * b)
{
	unsigned int ua=(unsigned int)a;
	unsigned int ub=(unsigned int)b;
	if(ua>ub)
		return -1;
	else if(ua<ub)
		return +1;
	else
		return 0;
}

int CollUINTCompare(void * a,void * b)
{
	unsigned int ua=(*((unsigned int *)a));
	unsigned int ub=(*((unsigned int *)b));
	
	if(ua>ub)
		return -1;
	else if(ua<ub)
		return +1;
	else
		return 0;
}

Collection * newcollection()
{
	Collection * toreturn=wAlloc(sizeof(Collection));
	toreturn->count=0;
	toreturn->keycount=0;
	toreturn->keytree=BT_create(CollUINTCompare);
	toreturn->synclock=CreateSyncObject(INFINITE);
	return toreturn;
}

unsigned int AddKey(Collection * to,unsigned int keydimension,unsigned int * keyoffset,unsigned int * keysize)//add key and return the reference id for the key
{
	KeyEntry * newkeyentry;
	
	Lock(to->synclock);

	newkeyentry=wAlloc(sizeof(KeyEntry));
	newkeyentry->keyid=to->keycount++;
	newkeyentry->keyoffset=keyoffset;
	newkeyentry->keysize=keysize;
	newkeyentry->keydimension=keydimension;
	newkeyentry->datatree=BT_create(CollUINTCompare);
	
	BT_insert(to->keytree,(void *)newkeyentry);
	
	Unlock(to->synclock);

	return newkeyentry->keyid;
}

unsigned int readkey(void * from,KeyEntry * keyinfo,unsigned int dimension)
{
	BYTE bytekey;
	unsigned short shortkey;

	switch(keyinfo->keysize[dimension])
	{
	case 1:
		bytekey=*((BYTE *)((unsigned int)from+keyinfo->keyoffset[dimension]));
		return bytekey;
	case 2:
		shortkey=*((unsigned short *)((unsigned int)from+keyinfo->keyoffset[dimension]));
		return shortkey;
	case 3:
		//weird one
		shortkey=*((unsigned short *)((unsigned int)from+keyinfo->keyoffset[dimension]));
		bytekey=*((BYTE *)((unsigned int)from+keyinfo->keyoffset[dimension])+2);
		return shortkey+bytekey*0x10000;
	case 4:
		return (*((unsigned int *)((unsigned int)from+keyinfo->keyoffset[dimension])));
	default:
		return (unsigned int)0;//shouldn't happen
	}
}

void CAdd(Collection * to,void * toadd)
{
	BinaryTreeEnum * keyenum;
	KeyEntry * curkey;
	DataEntry * curdataentry;
	unsigned int currentdimension;
	BinaryTree * curtree;
	unsigned int curkeyval;

	Lock(to->synclock);

	keyenum=BT_newenum(to->keytree);
	while(curkey=(KeyEntry *)BT_next(keyenum))
	{
		curtree=curkey->datatree;
		currentdimension=0;
		while(currentdimension<curkey->keydimension)
		{
			curkeyval=readkey(toadd,curkey,currentdimension);
			if((curdataentry=(DataEntry *)BT_find(curtree,&curkeyval))==0)//not in the tree yet
			{
				curdataentry=wAlloc(sizeof(DataEntry));
				curdataentry->key=curkeyval;
				if(currentdimension==(curkey->keydimension-1))
					curdataentry->data=BT_create(CollPointerCompare);
				else
					curdataentry->data=(void *)BT_create(CollUINTCompare);
				BT_insert(curtree,(void *)curdataentry);
			}
			curtree=(BinaryTree *)curdataentry->data;
			if(currentdimension==(curkey->keydimension-1))
				BT_insert(curtree,toadd);
			currentdimension++;
		}
	}
	BT_enumdelete(keyenum);

	Unlock(to->synclock);

	return;
}

void RemoveFromTree(BinaryTree * tree,void * toremove,unsigned int dimension,KeyEntry * curkey)
{
	DataEntry * curdataentry;
	unsigned int curkeyvalue;
	BinaryTree * subtree;

	curkeyvalue=readkey(toremove,curkey,dimension);
	if(curdataentry=(DataEntry *)BT_find(tree,&curkeyvalue))
	{
		if(dimension==(curkey->keydimension-1))
		{
			subtree=(BinaryTree *)curdataentry->data;
			BT_remove(subtree,toremove);
			if(subtree->itemcount==0)
			{
				BT_delete(subtree);
				BT_remove(tree,(void *)curdataentry);
				wFree((void *)curdataentry);
			}
		}
		else
		{
			subtree=(BinaryTree *)curdataentry->data;
			RemoveFromTree(subtree,toremove,dimension+1,curkey);
			if(subtree->itemcount==0)
			{
				//remove subtree
				BT_remove(tree,(void *)curdataentry);
				BT_delete(subtree);
				wFree((void *)curdataentry);
			}
		}
	}

	return;
}

void CRemove(Collection * from,void * toremove)
{
	BinaryTreeEnum * keyenum;
	KeyEntry * curkeyentry;

	Lock(from->synclock);

	keyenum=BT_newenum(from->keytree);
	
	while(curkeyentry=(KeyEntry *)BT_next(keyenum))
	{
		RemoveFromTree(curkeyentry->datatree,toremove,0,curkeyentry);
	}

	BT_enumdelete(keyenum);

	Unlock(from->synclock);

	return;
}

CEnum * CFind(Collection * in,unsigned int keyid,unsigned int * key,unsigned int searchdimensions)
{
	unsigned int dimension;
	KeyEntry * curkeyentry;
	BinaryTree * curtree;
	DataEntry * curdataentry;
	CEnum * newcenum;
	unsigned int i;

	Lock(in->synclock);

	if(curkeyentry=(KeyEntry *)BT_find(in->keytree,&keyid))
	{
		curtree=curkeyentry->datatree;
		dimension=0;
		while(curdataentry=(DataEntry *)BT_find(curtree,&(key[dimension])))
		{
			if(dimension==searchdimensions-1)//found
			{
				newcenum=(CEnum *)wAlloc(sizeof(CEnum));
				newcenum->dimensioncount=1+(curkeyentry->keydimension-searchdimensions);
				newcenum->btenums=wAlloc(sizeof(BinaryTreeEnum *)*newcenum->dimensioncount);
				newcenum->btenums[0]=BT_newenum((BinaryTree *)curdataentry->data);
				for(i=1;i<newcenum->dimensioncount;i++)
					newcenum->btenums[i]=0;
				Unlock(in->synclock);
				return newcenum;
			}

			//next dimension
			curtree=(BinaryTree *)curdataentry->data;
			dimension++;
		}
	}

	Unlock(in->synclock);

	return 0;
}

CEnum * CFindSimple(Collection * in,unsigned int keyid,unsigned int key)
{
	return CFind(in,keyid,&key,1);
}

void EmptyTree(BinaryTree * tree,KeyEntry * keyinfo,unsigned int dimension,CollEntryDeleteHandler onEntryDeletion)
{
	BinaryTreeEnum * btenum, * btenumb;
	DataEntry * curdataentry;
	void * curdata;

	btenum=BT_newenum(tree);

	while(curdataentry=(DataEntry *)BT_next(btenum))
	{
		if(dimension==keyinfo->keydimension-1)
		{
			if(onEntryDeletion)
			{
				btenumb=BT_newenum((BinaryTree *)curdataentry->data);
				while(curdata=BT_next(btenumb))
					onEntryDeletion(curdata);
				BT_enumdelete(btenumb);
			}
			BT_delete((BinaryTree *)curdataentry->data);
		}
		else
		{
			EmptyTree((BinaryTree *)curdataentry->data,keyinfo,dimension+1,onEntryDeletion);
			BT_delete((BinaryTree *)curdataentry->data);
		}
		BT_previous(btenum);
		BT_remove(tree,(void *)curdataentry);
		wFree(curdataentry);
	}

	BT_enumdelete(btenum);

	return;
}

void DeleteCollection(Collection * todelete,CollEntryDeleteHandler onEntryDeletion)
{
	BinaryTreeEnum * btenum;
	KeyEntry * curkeyentry;
	unsigned int first=1;

	Lock(todelete->synclock);

	btenum=BT_newenum(todelete->keytree);
	while(curkeyentry=(KeyEntry *)BT_next(btenum))
	{
		if(first)
		{
			EmptyTree(curkeyentry->datatree,curkeyentry,0,onEntryDeletion);
			first=0;
		}
		else
			EmptyTree(curkeyentry->datatree,curkeyentry,0,0);
		BT_delete(curkeyentry->datatree);
		wFree(curkeyentry);
	}
	BT_enumdelete(btenum);

	Unlock(todelete->synclock);

	DeleteSyncObject(todelete->synclock);
	BT_delete(todelete->keytree);
	wFree(todelete);

	return;
}

CEnum * CNewEnum(Collection * toenumerate,unsigned int keyid)
{
	CEnum * toreturn;
	KeyEntry * enumkey;
	//unsigned int curdimension;
	//BinaryTree * curtree;
	//DataEntry * curdataentry;
	//BinaryTreeEnum * btenum;

	Lock(toenumerate->synclock);

	toreturn=wAlloc(sizeof(CEnum));

	enumkey=(KeyEntry *)BT_find(toenumerate->keytree,(void *)&keyid);

	toreturn->dimensioncount=enumkey->keydimension+1;
	toreturn->btenums=(BinaryTreeEnum **)wAlloc(sizeof(BinaryTreeEnum *)*(enumkey->keydimension+1));
	memset(toreturn->btenums,0,(sizeof(BinaryTreeEnum *)*(enumkey->keydimension+1)));
	toreturn->btenums[0]=BT_newenum(enumkey->datatree);
	//initialize enums
	/*curdimension=0;
	curtree=enumkey->datatree;
	while(curdimension<enumkey->keydimension)
	{
		btenum=BT_newenum(curtree);
		toreturn->btenums[curdimension]=btenum;
		curdataentry=(DataEntry *)BT_next(btenum);
		curtree=(BinaryTree *)curdataentry->data;
		curdimension++;
	}
	//initialize final enum	
	btenum=BT_newenum(curtree);
	toreturn->btenums[curdimension]=btenum;*/

	Unlock(toenumerate->synclock);

	return toreturn;
}

void * CMoveFirst(CEnum * cenum)
{

	BinaryTree * curtree;
	unsigned int curdimension;
	DataEntry * curentry;
	BinaryTreeEnum * curenum;

	curdimension=0;

	curenum=cenum->btenums[curdimension];
	BT_reset(curenum);
	curentry=(DataEntry *)BT_next(curenum);

	if(!curentry)
		return 0;

	curtree=(BinaryTree *)curentry->data;
	curdimension++;

	//loop back to the last dimension, replacing the enumerators
	while(curdimension<(cenum->dimensioncount-1))
	{
		BT_enumdelete(cenum->btenums[curdimension]);//remove old enum
		curenum=BT_newenum(curtree);//create new enum
		cenum->btenums[curdimension]=curenum;//replace old with new enum
			
		curentry=BT_next(curenum);
		curtree=(BinaryTree *)curentry->data;
			
		curdimension++;
	}

	BT_enumdelete(cenum->btenums[curdimension]);
	curenum=BT_newenum(curtree);
	cenum->btenums[curdimension]=curenum;

	return BT_next(curenum);
}

void * CNext(CEnum * cenum)
{
	unsigned int curdimension;
	void * toreturn;
	BinaryTreeEnum * curenum;
	BinaryTree * curtree;
	DataEntry * curentry;

	if((cenum->dimensioncount>0)&&(cenum->btenums[1]==0))
		return CMoveFirst(cenum);

	curdimension=cenum->dimensioncount-1;
	curenum=cenum->btenums[curdimension];
	if(toreturn=BT_next(curenum))
		return toreturn;
	else if(curdimension>0)
	{
		curdimension--;
		curenum=cenum->btenums[curdimension];
		//lower dimension until you find an enumerator that does have a next entry
		while((curentry=(DataEntry *)BT_next(curenum))==0)
		{
			if(curdimension==0)
				return 0;
			curdimension--;
			curenum=cenum->btenums[curdimension];
		}

		curtree=(BinaryTree *)curentry->data;
		curdimension++;

		//loop back to the last dimension, replacing the enumerators
		while(curdimension<(cenum->dimensioncount-1))
		{
			BT_enumdelete(cenum->btenums[curdimension]);//remove old enum
			curenum=BT_newenum(curtree);//create new enum
			cenum->btenums[curdimension]=curenum;//replace old with new enum
			
			curentry=BT_next(curenum);
			curtree=(BinaryTree *)curentry->data;
			
			curdimension++;
		}

		BT_enumdelete(cenum->btenums[curdimension]);
		curenum=BT_newenum(curtree);
		cenum->btenums[curdimension]=curenum;

		return BT_next(curenum);
	}
	else
		return 0;

}

void * CPrevious(CEnum * cenum)
{
	unsigned int curdimension;
	void * toreturn;
	BinaryTreeEnum * curenum;
	BinaryTree * curtree;
	DataEntry * curentry;

	curdimension=cenum->dimensioncount-1;
	curenum=cenum->btenums[curdimension];
	if(toreturn=BT_previous(curenum))
		return toreturn;
	else if(curdimension>0)
	{
		curdimension--;
		curenum=cenum->btenums[curdimension];
		//lower dimension until you find an enumerator that does have a next entry
		while((curentry=(DataEntry *)BT_previous(curenum))==0)
		{
			if(curdimension==0)
			{
				CFirst(cenum);
				return 0;
			}
			curdimension--;
			curenum=cenum->btenums[curdimension];
		}

		curtree=(BinaryTree *)curentry->data;
		curdimension++;

		//loop back to the last dimension, replacing the enumerators
		while(curdimension<(cenum->dimensioncount-1))
		{
			BT_enumdelete(cenum->btenums[curdimension]);//remove old enum
			curenum=BT_newenum(curtree);//create new enum
			cenum->btenums[curdimension]=curenum;//replace old with new enum
			
			curentry=(DataEntry *)BT_end(curenum);
			curtree=(BinaryTree *)curentry->data;
			
			curdimension++;
		}

		BT_enumdelete(cenum->btenums[curdimension]);
		curenum=BT_newenum(curtree);
		cenum->btenums[curdimension]=curenum;

		return BT_end(curenum);
	}
	else
	{
		CFirst(cenum);
		return 0;
	}

}

void * CFirst(CEnum * cenum)
{
	unsigned int curdimension;

	BT_reset(cenum->btenums[0]);

	for(curdimension=1;curdimension<cenum->dimensioncount;curdimension++)
	{
		if(cenum->btenums[curdimension]!=0)
		{
			BT_enumdelete(cenum->btenums[curdimension]);
			cenum->btenums[curdimension]=0;
		}
	}

	return 0;
}

void * CLast(CEnum * cenum)
{
	BinaryTree * curtree;
	unsigned int curdimension;
	DataEntry * curentry;
	BinaryTreeEnum * curenum;

	curdimension=0;

	curenum=cenum->btenums[curdimension];
	curentry=(DataEntry *)BT_end(curenum);

	if(!curentry)
		return 0;

	curtree=(BinaryTree *)curentry->data;
	curdimension++;

	//loop back to the last dimension, replacing the enumerators
	while(curdimension<(cenum->dimensioncount-1))
	{
		BT_enumdelete(cenum->btenums[curdimension]);//remove old enum
		curenum=BT_newenum(curtree);//create new enum
		cenum->btenums[curdimension]=curenum;//replace old with new enum
		
		curentry=(DataEntry *)BT_end(curenum);
		curtree=(BinaryTree *)curentry->data;
		
		curdimension++;
	}

	BT_enumdelete(cenum->btenums[curdimension]);
	curenum=BT_newenum(curtree);
	cenum->btenums[curdimension]=curenum;

	return BT_end(curenum);
}

void CEnumDelete(CEnum * todelete)
{
	unsigned int curdimension;

	for(curdimension=0;curdimension<todelete->dimensioncount;curdimension++)
	{
		if(todelete->btenums[curdimension]!=0)
			BT_enumdelete(todelete->btenums[curdimension]);
	}

	wFree(todelete->btenums);
	wFree(todelete);

	return;
}
