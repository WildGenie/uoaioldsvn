#ifndef COLLECTION_INCLUDED
#define COLLECTION_INCLUDED

#include "BinaryTree.h"
#include "ALLOCATION.h"
#include "Synchronization.h"

//implementation of a general collection datastructure on top of my avl balanced binary tree implementation

typedef struct DataEntryStruct
{
	unsigned int key;
	void * data;
} DataEntry;

typedef struct KeyEntryStruct
{
	unsigned int keyid;
	unsigned int keydimension;
	unsigned int * keyoffset;//array of offsets per dimension
	unsigned int * keysize;//array of sizes per dimension
	BinaryTree * datatree;
} KeyEntry;

typedef struct CollectionStruct
{
	BinaryTree * keytree;
	unsigned int keycount;
	unsigned int count;
	SyncObject * synclock;
} Collection;

typedef struct CollectionEnumeratorStruct
{
	unsigned int dimensioncount;
	BinaryTreeEnum ** btenums;
} CEnum;

Collection * newcollection();
unsigned int AddKey(Collection * to,unsigned int keydimension,unsigned int * keyoffset,unsigned int * keysize);//add key and return the reference id for the key
CEnum * CFind(Collection * in,unsigned int keyid,unsigned int * key,unsigned int searchdimensions);
CEnum * CFindSimple(Collection * in,unsigned int keyid,unsigned int key);
void CAdd(Collection * to,void * toadd);
void CRemove(Collection * from,void * toremove);
typedef void (*CollEntryDeleteHandler)(void * data);
void DeleteCollection(Collection * todelete,CollEntryDeleteHandler onEntryDeletion);

CEnum * CNewEnum(Collection * toenumerate,unsigned int keyid);
void * CNext(CEnum * cenum);
void * CPrevious(CEnum * cenum);
void * CFirst(CEnum * cenum);
void * CLast(CEnum * cenum);
void CEnumDelete(CEnum * todelete);

#endif