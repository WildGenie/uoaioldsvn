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
//																													-//
//						NOTE: you must include ALLOCATION.h befòre you include BinaryTree.h.						-//
//---------------------------------------------------------------------------------------------------- Wim Decelle --//

#include "BinaryTree.h"

BinaryTreeNode * findinsertionpoint(BinaryTree * intree,void * tofind,int * lastcomparison);
BinaryTreeNode * createnode(BinaryTreeNode * parent,void * data);
void balanceup(BinaryTree * tree,BinaryTreeNode * fromnode);
void recalculateheight(BinaryTreeNode * curnode);
BinaryTreeNode * rotateleft(BinaryTreeNode * torotate);
BinaryTreeNode * rotateright(BinaryTreeNode * torotate);
int getbalance(BinaryTreeNode * ofnode);
BinaryTreeNode * findreplacement(BinaryTreeNode * ofnode);
void swapnodes(BinaryTreeNode * nodea,BinaryTreeNode * nodeb);
void DeleteLeafNode(BinaryTree * intree,BinaryTreeNode * todelete);
BinaryTreeNode * inordernext(BinaryTreeNode * ofnode);
BinaryTreeNode * inorderprevious(BinaryTreeNode * ofnode);
BinaryTreeNode * leftmost(BinaryTree * oftree);
BinaryTreeNode * rightmost(BinaryTree * oftree);

BinaryTree * BT_create(BTCompare comparisonfunction)
{
	BinaryTree * toreturn=(BinaryTree *)wAlloc(sizeof(BinaryTree));
	toreturn->compare=comparisonfunction;
	toreturn->itemcount=0;
	toreturn->root=0;
	toreturn->btsync=CreateSyncObject(INFINITE);
	return toreturn;
}

BinaryTreeNode * createnode(BinaryTreeNode * parent,void * data)
{
	BinaryTreeNode * toreturn=(BinaryTreeNode *)wAlloc(sizeof(BinaryTreeNode));
	toreturn->data=data;
	toreturn->height=1;
	toreturn->left=0;
	toreturn->parent=parent;
	toreturn->right=0;
	return toreturn;
}

BinaryTreeNode * findinsertionpoint(BinaryTree * intree,void * tofind,int * lastcomparison)
{
	BinaryTreeNode * curnode;

	(*lastcomparison)=0;
	if(!intree)
		return 0;
	if(!intree->root)
		return 0;

	curnode=intree->root;
	while(curnode)
	{
		(*lastcomparison)=intree->compare(curnode->data,tofind);
		if(((*lastcomparison)<0)&&(curnode->left))
			curnode=curnode->left;
		else if(((*lastcomparison)>0)&&(curnode->right))
			curnode=curnode->right;
		else
			return curnode;
	}
	return curnode;//should never be reached
}

int BT_insert(BinaryTree * pThis,void * toinsert)
{
	BinaryTreeNode * newnode;
	BinaryTreeNode * insertionnode;
	int comparisonresult;

	//special cases
	if(!pThis)//invalid tree
		return 0;

	Lock(pThis->btsync);//sync-lock

	if(!pThis->root)//empty tree
	{
		newnode=createnode(0,toinsert);
		pThis->root=newnode;
		pThis->itemcount=1;
		Unlock(pThis->btsync);
		return 1;
	}

	insertionnode=findinsertionpoint(pThis,toinsert,&comparisonresult);
	if(comparisonresult<0)
		insertionnode->left=createnode(insertionnode,toinsert);
	else if(comparisonresult>0)
		insertionnode->right=createnode(insertionnode,toinsert);
	else//already in tree
	{
		Unlock(pThis->btsync);
		return 0;
	}

	balanceup(pThis,insertionnode);

	pThis->itemcount++;

	Unlock(pThis->btsync);//sync-unlock

	return 1;
}

void recalculateheight(BinaryTreeNode * curnode)
{
	if((curnode->left)&&(curnode->right))
		curnode->height=max(curnode->left->height,curnode->right->height)+1;
	else if(curnode->left)
		curnode->height=curnode->left->height+1;
	else if(curnode->right)
		curnode->height=curnode->right->height+1;
	else
		curnode->height=1;
	return;
}

BinaryTreeNode * rotateleft(BinaryTreeNode * torotate)
{
	BinaryTreeNode * oldparent=torotate->parent;
	BinaryTreeNode * newtop=torotate->right;
	
	torotate->right=newtop->left;
	if(newtop->left)
		newtop->left->parent=torotate;
	
	newtop->left=torotate;
	torotate->parent=newtop;

	newtop->parent=oldparent;
	if(oldparent&&(oldparent->left==torotate))
		oldparent->left=newtop;
	else if(oldparent)
		oldparent->right=newtop;

	recalculateheight(torotate);
	recalculateheight(newtop);

	return newtop;
}

BinaryTreeNode * rotateright(BinaryTreeNode * torotate)
{
	BinaryTreeNode * oldparent=torotate->parent;
	BinaryTreeNode * newtop=torotate->left;
	
	torotate->left=newtop->right;
	if(newtop->right)
		newtop->right->parent=torotate;
	
	newtop->right=torotate;
	torotate->parent=newtop;

	newtop->parent=oldparent;
	if(oldparent&&(oldparent->left==torotate))
		oldparent->left=newtop;
	else if(oldparent)
		oldparent->right=newtop;

	recalculateheight(torotate);
	recalculateheight(newtop);

	return newtop;
}

int getbalance(BinaryTreeNode * ofnode)
{
	if((ofnode->left)&&(ofnode->right))
		return ((ofnode->right->height)-(ofnode->left->height));
	else if(ofnode->left)
		return 0-(ofnode->left->height);
	else if(ofnode->right)
		return (ofnode->right->height)-0;
	else
		return 0;
}

void balanceup(BinaryTree * tree,BinaryTreeNode * fromnode)
{
	int curbalance;
	while(1)
	{
		curbalance=getbalance(fromnode);
		if(curbalance<-1)
		{
			if(getbalance(fromnode->left)>0)
				fromnode->left=rotateleft(fromnode->left);
			fromnode=rotateright(fromnode);
		}
		else if(curbalance>+1)
		{
			if(getbalance(fromnode->right)<0)
				fromnode->right=rotateright(fromnode->right);
			fromnode=rotateleft(fromnode);
		}
		recalculateheight(fromnode);
		if(!fromnode->parent)
		{
			tree->root=fromnode;
			break;
		}
		fromnode=fromnode->parent;
	}
	return;
}

void * BT_find(BinaryTree * pThis,void * tofind)
{
	int comparisonresult;
	void * toreturn;
	BinaryTreeNode * curnode;
	
	Lock(pThis->btsync);//sync-lock
	curnode=findinsertionpoint(pThis,tofind,&comparisonresult);
	if((!curnode)||(comparisonresult!=0))
	{
		Unlock(pThis->btsync);//sync-unlock
		return 0;
	}
	else
	{
		toreturn=curnode->data;
		Unlock(pThis->btsync);//sync-unlock
		return toreturn;
	}
}

void swapnodes(BinaryTreeNode * nodea,BinaryTreeNode * nodeb)
{
	void * temp=nodea->data;
	nodea->data=nodeb->data;
	nodeb->data=temp;
	return;
}

BinaryTreeNode * findreplacement(BinaryTreeNode * ofnode)
{
	BinaryTreeNode * toreturn;
	if(!ofnode)
		return 0;
	if(ofnode->right)//left most in the right subtree
	{
		toreturn=ofnode->right;
		while(toreturn->left)
			toreturn=toreturn->left;
		return toreturn;
	}
	else if(ofnode->left)//or rightmost in the left subtree
	{
		toreturn=ofnode->left;
		while(toreturn->right)
			toreturn=toreturn->right;
		return toreturn;
	}
	else
		return ofnode;
}

void DeleteLeafNode(BinaryTree * intree,BinaryTreeNode * todelete)
{
	BinaryTreeNode * parent;

	if((!intree)||(!todelete))
		return;

	parent=todelete->parent;
	if(!parent)
		intree->root=0;
	else
	{
		if(parent->left==todelete)
			parent->left=0;
		else if(parent->right==todelete)
			parent->right=0;
		balanceup(intree,parent);
	}
	wFree(todelete);
	intree->itemcount--;
	return;
}

void * BT_remove(BinaryTree * pThis,void * toremove)
{
	void * toreturn;
	int comparisonresult;
	BinaryTreeNode * swapnode;
	BinaryTreeNode * curnode;
	
	Lock(pThis->btsync);
	curnode=findinsertionpoint(pThis,toremove,&comparisonresult);
	if((!curnode)||(comparisonresult!=0))
	{
		Unlock(pThis->btsync);
		return 0;
	}
	toreturn=curnode->data;
	swapnode=findreplacement(curnode);
	swapnodes(curnode,swapnode);
	curnode=swapnode;
	while((curnode->left)||(curnode->right))
	{
		swapnode=findreplacement(curnode);
		swapnodes(curnode,swapnode);
		curnode=swapnode;
	}
	DeleteLeafNode(pThis,curnode);
	Unlock(pThis->btsync);

	return toreturn;
}

void BT_deletenode(BinaryTree * pThis,BinaryTreeNode * curnode)
{
	BinaryTreeNode * swapnode;
	swapnode=findreplacement(curnode);
	swapnodes(curnode,swapnode);
	curnode=swapnode;
	while((curnode->left)||(curnode->right))
	{
		swapnode=findreplacement(curnode);
		swapnodes(curnode,swapnode);
		curnode=swapnode;
	}
	DeleteLeafNode(pThis,curnode);
	return;
}

void BT_delete(BinaryTree * todelete)
{
	Lock(todelete->btsync);
	while(todelete->root)
		BT_deletenode(todelete,todelete->root);
	Unlock(todelete->btsync);
	DeleteSyncObject(todelete->btsync);
	wFree(todelete);
	return;
}

BinaryTreeNode * leftmost(BinaryTree * oftree)
{
	BinaryTreeNode * toreturn;
	
	Lock(oftree->btsync);
	toreturn=oftree->root;
	while(toreturn&&(toreturn->left))
		toreturn=toreturn->left;
	Unlock(oftree->btsync);

	return toreturn;
}

BinaryTreeNode * rightmost(BinaryTree * oftree)
{
	BinaryTreeNode * toreturn;
	
	Lock(oftree->btsync);
	toreturn=oftree->root;
	while(toreturn&&(toreturn->right))
		toreturn=toreturn->right;
	Unlock(oftree->btsync);

	return toreturn;
}

BinaryTreeNode * inordernext(BinaryTreeNode * ofnode)
{
	BinaryTreeNode * curnode;
	if(!ofnode)
		return 0;
	if(ofnode->right)
	{
		curnode=ofnode->right;
		while(curnode->left)
			curnode=curnode->left;
		return curnode;
	}
	else
	{
		curnode=ofnode;
		while(curnode->parent)
		{
			if(curnode->parent->left==curnode)
				return curnode->parent;
			else
				curnode=curnode->parent;
		}
		return 0;
	}
}

BinaryTreeNode * inorderprevious(BinaryTreeNode * ofnode)
{
	BinaryTreeNode * curnode;
	if(!ofnode)
		return 0;
	if(ofnode->left)
	{
		curnode=ofnode->left;
		while(curnode->right)
			curnode=curnode->right;
		return curnode;
	}
	else
	{
		curnode=ofnode;
		while(curnode->parent)
		{
			if(curnode->parent->right==curnode)
				return curnode->parent;
			else
				curnode=curnode->parent;
		}
		return 0;
	}
}

BinaryTreeEnum * BT_newenum(BinaryTree * oftree)
{
	BinaryTreeEnum * toreturn=(BinaryTreeEnum *)wAlloc(sizeof(BinaryTreeEnum));
	toreturn->tree=oftree;
	toreturn->curnode=0;
	return toreturn;
}

void * BT_next(BinaryTreeEnum * btenum)
{
	BinaryTreeNode * newnode;
	if(!btenum->curnode)
		newnode=leftmost(btenum->tree);
	else
		newnode=inordernext(btenum->curnode);
	if(!newnode)
		return 0;
	else
	{
		btenum->curnode=newnode;
		return btenum->curnode->data;
	}
}

void * BT_previous(BinaryTreeEnum * btenum)
{
	btenum->curnode=inorderprevious(btenum->curnode);
	if(!btenum->curnode)
		return 0;
	else
		return btenum->curnode->data;
}

void * BT_end(BinaryTreeEnum * btenum)
{
	btenum->curnode=rightmost(btenum->tree);
	return btenum->curnode->data;
}

void * BT_reset(BinaryTreeEnum * btenum)
{
	btenum->curnode=0;
	return 0;
}

void BT_enumdelete(BinaryTreeEnum * todelete)
{
	wFree(todelete);
	return;
}

void * BT_leftmost(BinaryTree * tree)
{
	BinaryTreeNode * lm=leftmost(tree);
	if(!lm)
		return 0;
	else
		return lm->data;
}

void * BT_rightmost(BinaryTree * tree)
{
	BinaryTreeNode * rm=rightmost(tree);
	if(!rm)
		return 0;
	else
		return rm->data;
}
