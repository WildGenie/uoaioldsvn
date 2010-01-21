//-------------------------------------------------------------------------------------------------------------------//
//- BinaryTree.c/.h :: Reusable implementation of an AVL balanced binarytree, which serves well as a general        -//
//						collection datastructure, as both insertion and searching scale logarithmic, while size		-//
//						of the tree scales linear with the amount of data stored.									-//
//						I will use this tree structure most of the time if i need a collection. At some points a	-//
//						well-designed hash-table would perform better. But a hashtable only works if you can        -//
//						predict the required size: too small and it will be slow, too large and it will waste		-//
//						memory. A linked implementation of a balanced binary tree, on the other hand, dynamically   -//
//						sizes, so never really wastes memory, and, though slower than a suitably large hashtable,   -//
//						the AVL balancing algorithm reduces the worst case performance to logarithmic in the trees  -//
//						height, so it is practically always fast enough.										    -//
//						Synchronization was added to this second version.											-//
//---------------------------------------------------------------------------------------------------- Wim Decelle --//

#ifndef BINARYTREE_INCLUDED
#define BINARYTREE_INCLUDED

//get allocation functions
#include "ALLOCATION.h"

//binarytree will be synchronized
#include "Synchronization.h"

//we need max macro
#ifndef max
#define max(a,b) (((a)>(b))?(a):(b))
#endif

typedef int (*BTCompare)(void *,void *);

typedef struct BTNodeStruct
{
	void * data;
	unsigned int height;
	struct BTNodeStruct * left;
	struct BTNodeStruct * right;
	struct BTNodeStruct * parent;
} BinaryTreeNode;

typedef struct
{
	unsigned int itemcount;
	BinaryTreeNode * root;
	BTCompare compare;
	SyncObject * btsync;
} BinaryTree;

typedef struct
{
	BinaryTree * tree;
	BinaryTreeNode * curnode;
} BinaryTreeEnum;

BinaryTree * BT_create(BTCompare comparisonfunction);
int BT_insert(BinaryTree * pThis,void * toinsert);
void * BT_find(BinaryTree * pThis,void * tofind);
void * BT_remove(BinaryTree * pThis,void * toremove);
void BT_delete(BinaryTree * todelete);

BinaryTreeEnum * BT_newenum(BinaryTree * oftree);
void * BT_next(BinaryTreeEnum * btenum);
void * BT_previous(BinaryTreeEnum * btenum);
void * BT_reset(BinaryTreeEnum * btenum);
void * BT_end(BinaryTreeEnum * btenum);
void BT_enumdelete(BinaryTreeEnum * todelete);

void * BT_leftmost(BinaryTree * tree);
void * BT_rightmost(BinaryTree * tree);

#endif