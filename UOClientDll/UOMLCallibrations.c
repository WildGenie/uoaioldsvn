#include "UOMLCallibrations.h"

#include "W2EDebug.h"
#include "UOMLVersionInfo.h"

CallibrationInfo * CallibrateUOClient()
{
	CallibrationInfo * tocallibrate;
	ParsedFunction * curfunc, * subfunc, * subfuncb;
	Instruction * curins;
	UO2DClientVersion ver;

	enum x86_insn_type sockobjcallib_new[]={insn_xor,insn_mov,insn_mov};
	enum x86_insn_type eventmacrocallib_new[]={insn_push,insn_call};
	//enum x86_insn_type eventmacrocallib_new2[]={insn_call,insn_call,insn_call,insn_call,insn_call,insn_call,insn_call,insn_call};
	enum x86_insn_type eventmacrocallib_new3[]={insn_call,insn_call,insn_call,insn_call};
	enum x86_insn_type ccallcallib[]={insn_call,insn_add};//call func; add esp, stacksize;
	enum x86_insn_type targetcallib[]={insn_dec,insn_sub};//decrease or sub is used in targetkind switching
	enum x86_insn_type jumps_callib[]={insn_jmp,insn_jcc};//patched or non-patched jump?
	enum x86_insn_type calltest[]={insn_call,insn_test};//patched or non-patched jump?
	enum x86_insn_type fourcalls[]={insn_call,insn_call,insn_call,insn_call};
	enum x86_insn_type movorpush[]={insn_mov,insn_push};
	enum x86_insn_type movcmps[]={insn_mov,insn_cmp};
	enum x86_insn_type propsandiptablecallib_new[]={insn_push,insn_push,insn_call};

	unsigned int skilladdresses[4]={0,0,0,0};

	unsigned int i;

	unsigned int switchtable=0;

	unsigned int emswitchtable=0;

	unsigned int curpos=0;
	unsigned int loopstart=0;
	unsigned int loopend=0;
	unsigned int jumptarget=0;

	unsigned int packethandlerfunc=0;

	unsigned int packetswitchindex=0;
	unsigned int packetswitchoffset=0;

	unsigned int subpacketswitchindex=0;
	unsigned int subpacketswitchoffset=0;

	BYTE packetindex=0;

	unsigned int pathfindhandler=0;
	unsigned int targcurshandler=0;

	unsigned int prevpos=0;

	tocallibrate=wAlloc(sizeof(CallibrationInfo));

	//ensure libdisasm was initialized
	ASMParser_inits();

	//A. Find WinMain from executables entry point

	//read entry point address from client's executable
	curpos=GetEntryPointOffset();
	if(!curpos)
	{
		//wError(CALLIBRATION_ERROR,"Failed to obtain the entry point of the client's executable!");
		debugprintf("Failed to obtain the entry point of the client's executable!\n");
		return 0;
	}

	//parse complete entry point function (start chunk)
	curfunc=ASMParseFunction(curpos,TRUE);//parse start function chunk
	
	//winmain is the function with stack size 0x10 called from the start chunk
	subfunc=FuncFind(curfunc,0x10,FALSE);

	tocallibrate->pWinMain=subfunc->startaddress;
	
	//cleanup start chunk, no longer used
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;
	
#ifdef DEBUG_CALLIBRATION
	debugprintf("WinMain: %x\n",tocallibrate->pWinMain);
#endif
	
	//B. Winmain -> sockobjectvtbl (table of network functions)

	//to find the network function table (sockobjvtbl), we find "xor eax, eax; mov ecx, eax; mov dwordxxxxxxxx, eax;"
	curins=ASMFindMultiple(curfunc,sockobjcallib_new,3,FALSE);	
	//then find the second call which does all the network initializations
	curins=ASMFind(curfunc,insn_call,FALSE);
	curins=ASMFind(curfunc,insn_call,FALSE);
	//within this second network init function, ...
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	//..., the vtbl of the network object is initialized in a 6 byte "mov [esi], offset sockobjectvtbl" statement
	while(curins=ASMFindSized(subfunc,insn_mov,6,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_expression)
			break;
	}
	//now read the network vtbl's offset
	tocallibrate->pSocketObjectVtbl=ReadUInt(curins->address+2);
	//cleanup network init function parsing
	DeleteParsedFunction(subfunc);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pSocketObjectVtbl: %x\n",tocallibrate->pSocketObjectVtbl);
#endif

	//C. WinMain -> pEventMacro, UpdateItemList(removes mobiles that walked off the screen, etc)

	curins=ASMLast(curfunc);//to end of winmain

	//find retn10 to be sure that we are at the end
	while(!((curins->instruction.type==insn_return)&&(GetOpValue(curins,0)==0x10)))
		curins=ASMFind(curfunc,insn_return,TRUE);

	//find push 0, call relative offset (backwards)
	while(curins=ASMFindMultiple(curfunc,eventmacrocallib_new,2,TRUE))
	{
		if((curins->instruction.explicit_count==1)&&(GetOpValue(curins,0)==0))
		{
			curins=ASMNext(curfunc);
			break;
		}
	}
	//follow call
	subfunc=ASMParseFunction((unsigned int)((int)curins->address+(int)5+ReadInt(curins->address+1)),TRUE);
	//find series of 8 subsequent calls
	//curins=ASMFindMultiple(subfunc,eventmacrocallib_new2,8,FALSE);
	curins=ASMFindMultiple(subfunc,eventmacrocallib_new3,4,FALSE);
	//first call from 8th call
	curins=ASMFind(subfunc,insn_call,FALSE);//ASMNext(subfunc);
	curins=ASMFind(subfunc,insn_call,FALSE);//ASMNext(subfunc);
	curins=ASMFind(subfunc,insn_call,FALSE);//ASMNext(subfunc);
	curins=ASMFind(subfunc,insn_call,FALSE);//ASMNext(subfunc);
	curins=ASMFind(subfunc,insn_call,FALSE);//ASMNext(subfunc);
	curins=ASMFind(subfunc,insn_call,FALSE);//ASMNext(subfunc);
	curins=ASMFind(subfunc,insn_call,FALSE);//ASMNext(subfunc);
	//follow call 8
	subfuncb=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(subfunc);
	//find first call
	curins=ASMFind(subfuncb,insn_call,FALSE);
	tocallibrate->pEventMacro=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pEventMacro: %x\n",tocallibrate->pEventMacro);
#endif

	DeleteParsedFunction(subfuncb);
	DeleteParsedFunction(curfunc);

	//callibrate from eventmacro here

	curfunc=ASMParseFunction(tocallibrate->pEventMacro,TRUE);

	//a. find event macro switch table

	//->7 byte jmp with op_expression as first paramameter
	while(curins=ASMFind(curfunc,insn_jmp,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_expression)
			break;
	}

	emswitchtable=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("emswitchtable: %x\n",emswitchtable);
#endif

	DeleteParsedFunction(curfunc);

	curpos=ReadUInt(emswitchtable+4*13);//LastSkill Macro
	
	curfunc=ASMParseFunction(curpos,FALSE);//only required chunk is parsed

	while(curins=ASMNext(curfunc))
	{
		if(curins->instruction.type==insn_test)//mov eax, bLoggedIn; test eax, eax;
		{
			curins=ASMFind(curfunc,insn_mov,TRUE);
			tocallibrate->pBLoggedIn=ReadUInt(curins->address+1);
			break;
		}
		else if(curins->instruction.type==insn_cmp)//cmp bLoggedIn,0
		{
			tocallibrate->pBLoggedIn=ReadUInt(curins->address+2);
			break;
		}
	}
	

#ifdef DEBUG_CALLIBRATION
	debugprintf("pBLoggedIn: %x\n",tocallibrate->pBLoggedIn);
#endif

	//find mov eax, LastSkill
	while(curins=ASMFindSized(curfunc,insn_mov,5,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}

	//2. pLastSkill
	tocallibrate->pLastSkill=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLastSkill: %x\n",tocallibrate->pLastSkill);
#endif

	//close lastskill macro
	DeleteParsedFunction(curfunc);
	
	curpos=ReadUInt(emswitchtable+4*15);//LastSpell Macro

	//open LastSpell Macro chunk
	curfunc=ASMParseFunction(curpos,FALSE);//only 1 chunk

	//skip bLoggedIn check, since it might use a "mov eax, bLoggedIn construction"
	while(curins=ASMNext(curfunc))
	{
		if((curins->instruction.type==insn_test)||(curins->instruction.type==insn_cmp))
			break;
	}
	//find mov eax, LastSpell
	while(curins=ASMFindSized(curfunc,insn_mov,5,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}

	//3. pLastSpell
	tocallibrate->pLastSpell=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLastSpell: %x\n",tocallibrate->pLastSpell);
#endif

	DeleteParsedFunction(curfunc);

	curpos=ReadUInt(emswitchtable+4*16);//LastObject Macro
	
	//open LastObject macro chunk
	curfunc=ASMParseFunction(curpos,FALSE);//only 1 chunk

	//skip bLoggedIn check, since it might use a "mov eax, bLoggedIn construction"
	while(curins=ASMNext(curfunc))
	{
		if((curins->instruction.type==insn_test)||(curins->instruction.type==insn_cmp))
			break;
	}
	//find mov eax, LastObjectID
	while(curins=ASMFindSized(curfunc,insn_mov,5,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}

	//4. pLastObjectID
	tocallibrate->pLastObject=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLastObject: %x\n",tocallibrate->pLastObject);
#endif

	//next call is finditem call
	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pFindItemByID=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pFindItemByID: %x\n",tocallibrate->pFindItemByID);
#endif

	/*curpos=Remote_Find(curpos,callrelnear,1,FALSE,0);
	curpos++;

	//FindItemByID
	tocallibrate->pFindItemByID=(unsigned int)((int)curpos+4+ReadInt(curpos));

	curpos=Remote_Find(curpos,callrelnear,1,FALSE,0);
	curpos++;*/

	//pLastObjectFunc

	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pLastObjectFunc=ParseAddress(curins);

	//tocallibrate->pLastObjectFunc=(unsigned int)((int)curpos+4+ReadInt(curpos));

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLastObjectFunc: %x\n",tocallibrate->pLastObjectFunc);
#endif

	DeleteParsedFunction(curfunc);
	
	//use pFindItemByID to get the Player object and the ItemList pointer
	curfunc=ASMParseFunction(tocallibrate->pFindItemByID,FALSE);

	//find mov eax, Player (used in finditembyid to check if the searched ID is player->ID)
	while(curins=ASMFindSized(curfunc,insn_mov,5,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}

	//7. pPlayer
	tocallibrate->pPlayer=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pPlayer: %x\n",tocallibrate->pPlayer);
#endif

	//find mov eax, ItemList
	while(curins=ASMFindSized(curfunc,insn_mov,5,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}

	//8. pItemList
	tocallibrate->pItemList=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pItemList: %x\n",tocallibrate->pItemList);
#endif

	DeleteParsedFunction(curfunc);
	
	//use pLastObjectFunc to get LastObjectType
	curfunc=ASMParseFunction(tocallibrate->pLastObjectFunc,FALSE);

	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(ReadUInt(curins->address+curins->instruction.size-4)==tocallibrate->pLastObject)
			break;
	}

	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type!=op_register)
			break;
	}

	//9. pLastObjectType
	tocallibrate->pLastObjectType=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLastObjectType: %x\n",tocallibrate->pLastObjectType);
#endif

	//curpos=Remote_Find(curpos,retn,1,FALSE,0);

	//2 posibilities: either before retn, you find add esp 10 (83 C4 10) or add esp 8
	//in the first case, find 2 subsequent push 0's, then find again one, then the prev instruction is mov regvar, gumplist
	//in the second case, see below

	//new version: find a call preceded by a "mov eax, gumplist" and 5 pushes, normally push 0, push eax, push 0, push 0
	// do this by just finding "add esp, 14h" just after the call
	// if you cannot find it, then the call was moved to a subprocedure, so check all called functions for this kind call
	//all searching is done backwards from the retn

	tocallibrate->pGumpList=0;
	curins=ASMFind(curfunc,insn_return,FALSE);
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,TRUE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x14)
		{
			curins=ASMFind(curfunc,insn_call,TRUE);
			tocallibrate->pFindStatusGump=ParseAddress(curins);
			while(curins=ASMFind(curfunc,insn_mov,TRUE))
			{
				if((curins->instruction.size>4)&&(curins->instruction.operands[0].op.type==op_register))
					break;
			}
			tocallibrate->pGumpList=ReadUInt(curins->address+curins->instruction.size-4);
			break;
		}
		ASMPrevious(curfunc);
	}
	if(tocallibrate->pGumpList==0)//not found, loop all subprocedures
	{
		curins=ASMFind(curfunc,insn_return,FALSE);
		while((tocallibrate->pGumpList==0)&&(curins=ASMFind(curfunc,insn_call,TRUE)))
		{
			subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
			while((tocallibrate->pGumpList==0)&&(curins=ASMFindMultiple(subfunc,ccallcallib,2,FALSE)))
			{
				curins=ASMNext(subfunc);
				if(ReadByte(curins->address+curins->instruction.size-1)==0x14)
				{
					curins=ASMFind(subfunc,insn_call,TRUE);
					tocallibrate->pFindStatusGump=ParseAddress(curins);
					while(curins=ASMFindSized(subfunc,insn_mov,5,TRUE))
					{
						if(curins->instruction.operands[0].op.type==op_register)
							break;
					}			
					tocallibrate->pGumpList=ReadUInt(curins->address+curins->instruction.size-4);
					DeleteParsedFunction(curfunc);
					curfunc=subfunc;
					subfunc=0;
					break;
				}
				ASMPrevious(subfunc);
			}
			if(subfunc)
				DeleteParsedFunction(subfunc);
		}
	}

	DeleteParsedFunction(curfunc);
	
#ifdef DEBUG_CALLIBRATION
	debugprintf("pFindStatusGump: %x\n",tocallibrate->pFindStatusGump);
	debugprintf("pGumpList: %x\n",tocallibrate->pGumpList);
#endif

	//LastTarget macro -> Last Target stuff
	curpos=ReadUInt(emswitchtable+4*21);
	curfunc=ASMParseFunction(curpos,FALSE);

	if(ASMFindFirst(curfunc,targetcallib,2,FALSE)==0)//handling is done in a subprocedure if no dec or sub instruction is found
	{
		ASMFirst(curfunc);
		//jump to lasttargetmacro subprocedure
		curins=ASMFind(curfunc,insn_call,FALSE);
		subfunc=ASMParseFunction(ParseAddress(curins),TRUE);//we need all chunks in this case
		DeleteParsedFunction(curfunc);
		curfunc=subfunc;
		subfunc=0;
	}
	else
	{
		ASMFirst(curfunc);
		//skip bLoggedIn check, since it might use a "mov eax, bLoggedIn construction"
		while(curins=ASMNext(curfunc))
		{
			if((curins->instruction.type==insn_test)||(curins->instruction.type==insn_cmp))
				break;
		}
	}
	
	//mov regvar, xxxx with xxxx=targcurs, kind, tile, z, y, x

	//find next "mov regvar, xxxx;"
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}

	tocallibrate->pBTargCurs=ReadUInt(curins->address+curins->instruction.size-4);

	//find next "mov regvar, xxxx;"
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}
	tocallibrate->pLTargetKind=ReadUInt(curins->address+curins->instruction.size-4);

	//find next "mov regvar, xxxx;"
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}
	tocallibrate->pLTargetTile=ReadUInt(curins->address+curins->instruction.size-4);

	//find next "mov regvar, xxxx;"
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}
	tocallibrate->pLTargetZ=ReadUInt(curins->address+curins->instruction.size-4);

	//find next "mov regvar, xxxx;"
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}
	tocallibrate->pLTargetY=ReadUInt(curins->address+curins->instruction.size-4);

	//find next "mov regvar, xxxx;"
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}
	tocallibrate->pLTargetX=ReadUInt(curins->address+curins->instruction.size-4);


#ifdef DEBUG_CALLIBRATION
	debugprintf("pBTargCurs: %x\n",tocallibrate->pBTargCurs);
	debugprintf("pLTargetKind: %x\n",tocallibrate->pLTargetKind);
	debugprintf("pLTargetTile: %x\n",tocallibrate->pLTargetTile);
	debugprintf("pLTargetZ: %x\n",tocallibrate->pLTargetZ);
	debugprintf("pLTargetY: %x\n",tocallibrate->pLTargetY);
	debugprintf("pLTargetX: %x\n",tocallibrate->pLTargetX);
#endif

	//find lasttargetkind=3 - handler to get lTargetID
	//there should be 3 jz/jnz's above the current position, the first of those is the jump to ltargetkind=3 handling
	curins=ASMFind(curfunc,insn_jcc,TRUE);
	curins=ASMFind(curfunc,insn_jcc,TRUE);
	curins=ASMFind(curfunc,insn_jcc,TRUE);
	curpos=ParseAddress(curins);
	subfunc=ASMParseFunction(curpos,FALSE);
	
	//find next "mov regvar, xxxx;"
	while(curins=ASMFind(subfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}
	tocallibrate->pLTargetID=ReadUInt(curins->address+curins->instruction.size-4);

	#ifdef DEBUG_CALLIBRATION
		debugprintf("pLTargetID: %x\n",tocallibrate->pLTargetID);
	#endif

	DeleteParsedFunction(subfunc);
	DeleteParsedFunction(curfunc);
	subfunc=0;

	//OpenDoor macro -> SocketObject, LoginCrypt-function
	curpos=ReadUInt(emswitchtable+4*11);
	curfunc=ASMParseFunction(curpos,FALSE);

	//follow call to OpenDoorMacro-handler function
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find last "lea" instruction before the end of the function (loads the packet buffer i think)
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFindOpCode(curfunc,0x8D,TRUE);
	
	//follow the next call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find "mov ecx, socketobject"
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_register)
			break;
	}

	tocallibrate->pSockObject=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pSockObject: %x\n",tocallibrate->pSockObject);
#endif

	//next call is call to logincrypt
	curins=ASMFind(curfunc,insn_call,FALSE);

	tocallibrate->pLoginCrypt=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLoginCrypt: %x\n",tocallibrate->pLoginCrypt);
#endif

	DeleteParsedFunction(curfunc);

	//AlwaysRun Toggle Macro -> bAlwaysRun
	curpos=ReadUInt(emswitchtable+4*31);
	curfunc=ASMParseFunction(curpos,FALSE);
	//find mov, read pAlwaysRun from it
	curins=ASMFind(curfunc,insn_mov,FALSE);	
	//21. pAlwaysRun
	tocallibrate->pAlwaysRun=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pAlwaysRun: %x\n",tocallibrate->pAlwaysRun);
#endif

	DeleteParsedFunction(curfunc);

	//AttackLast Macro (0x1A) -> pTextOut
	curpos=ReadUInt(emswitchtable+4*0x1A);

	curfunc=ASMParseFunction(curpos,FALSE);

	//old version: if attacklast macro chunk starts with a call, then follow the call, otherwise macro-handling is done here
	//new version: find 0xFFFFFFFF operand (-1) which is used to mark no lasttarget is set, if it is there handling is done here, otherwise the first call in the chunk must do the handling


	//if((curins=ASMFind(curfunc,insn_cmp,FALSE)==0)//Is handling done in a subprocedure?
	
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(ReadUInt(curins->address+curins->instruction.size-4)==tocallibrate->pLTargetID)
			break;
	}

	if(curins==0)
	{
		ASMFirst(curfunc);
		curins=ASMFind(curfunc,insn_call,FALSE);
		subfunc=ASMParseFunction(ParseAddress(curins),FALSE);//switch to subprocedure
		DeleteParsedFunction(curfunc);
		curfunc=subfunc;
		subfunc=0;
		while(curins=ASMFind(curfunc,insn_mov,FALSE))
		{
			if(ReadUInt(curins->address+curins->instruction.size-4)==tocallibrate->pLTargetID)
				break;
		}
	}

	curins=ASMFind(curfunc,insn_call,FALSE);
	
	//29. pTextOut
	tocallibrate->pTextOut=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pTextOut: %x\n",tocallibrate->pTextOut);
#endif

	DeleteParsedFunction(curfunc);

	//pTextOut -> last call = AddToJournal -> first jmp (0xEB) (jmpsize > 5) -> first call -> first call -> find mov pJournalStart, eax
	//new version : pTextOut->last call->call followed by "add esp, 0x0C" (stack cleanup of ccall)->first call->find mov pJournalStart, eax (0xA3 opcode instruction)
	//update : pTextOut->lastcall->find cmp, jcc twice + follow jcc's->second call->firstcall->find mov pJournalStart, eax
	curfunc=ASMParseFunction(tocallibrate->pTextOut,FALSE);

	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find twice = cmp al, x; jnz xxxxx + follow jcc
	for(i=0;i<2;i++)
	{
		curins=ASMFind(curfunc,insn_cmp,FALSE);
		curins=ASMFind(curfunc,insn_jcc,FALSE);
		ASMJumpToAddress(curfunc,ParseAddress(curins));
	}
	//follow second call
	curins=ASMFind(curfunc,insn_call,FALSE);
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//follow first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find mov JournalStart, eax (0xA3 opcode instruction)
	curins=ASMFindOpCode(curfunc,0xA3,FALSE);

	tocallibrate->pJournalStart=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pJournalStart: %x\n",tocallibrate->pJournalStart);
#endif

	DeleteParsedFunction(curfunc);

	//30. GumpOpener
	curpos=ReadUInt(emswitchtable+4*7);//Open Macro
	curfunc=ASMParseFunction(curpos,FALSE);
	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseLinear(ParseAddress(curins));//contains switch jump so, which is not parsed correctly if we don't parse lineary
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;
	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseLinear(ParseAddress(curins));//contains switch jump so, which is not parsed correctly if we don't parse lineary
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	tocallibrate->pGumpOpener=curfunc->startaddress;

#ifdef DEBUG_CALLIBRATION
	debugprintf("pGumpOpener: %x\n",tocallibrate->pGumpOpener);
#endif

	//find "push statusgumpsize" with statusgumpsize=0x1F4, parse non-linear from here
	while(curins=ASMFind(curfunc,insn_push,FALSE))
	{
		if(ReadUInt(curins->address+1)==0x1F4)
			break;
	}
	//switch to non-linear parsing
	subfunc=ASMParseFunction(curins->address,FALSE);//contains switch jump so, which is not parsed correctly if we don't parse lineary
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;
	//first call is allocate (allocates status gump)
	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pAllocate=ParseAddress(curins);
	//second call is the statusgump_constructor
	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pStatusGumpConstructor=ParseAddress(curins);
	//last a call to showgump is made (after a jump, but in non-linear mode that jmp should be made automatically)
	/*curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pShowGump=ParseAddress(curins);*/
		
#ifdef DEBUG_CALLIBRATION
	debugprintf("pAllocate: %x\n",tocallibrate->pAllocate);
	debugprintf("pStatusGumpConstructor: %x\n",tocallibrate->pStatusGumpConstructor);
	//debugprintf("pShowGump: %x\n",tocallibrate->pShowGump);
#endif

	DeleteParsedFunction(curfunc);

	//statusgumpconstructor -> partymemberlist, showgump

	curfunc=ASMParseFunction(tocallibrate->pStatusGumpConstructor,TRUE);

	while(curins=ASMFind(curfunc,insn_call,FALSE))
	{
		if(ParseAddress(curins)==tocallibrate->pTextOut)
			break;
	}

	while(curins=ASMFind(curfunc,insn_mov,TRUE))
	{
		if((curins->instruction.operands[0].op.type=op_expression)||(curins->instruction.operands[1].op.type=op_expression))
			break;
	}

	tocallibrate->pPartyMembers=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pPartyMembers: %x\n",tocallibrate->pPartyMembers);
#endif

	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	tocallibrate->pShowGump=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pShowGump: %x\n",tocallibrate->pShowGump);
#endif

	DeleteParsedFunction(curfunc);

	//pLoginCrypt->sendhook position, pOriginalSend times 3 + ADD LOGIN ENCRYPTION PATCH POSITION + TARGET 1
	curfunc=ASMParseFunction(tocallibrate->pLoginCrypt,TRUE);

	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pPatchPos2=curins->address+curins->instruction.size-4;
	tocallibrate->pOriginalSend=(unsigned int (__cdecl *)(unsigned char *))ParseAddress(curins);

	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pPatchPos3=curins->address+curins->instruction.size-4;
	tocallibrate->pOriginalSendb=ParseAddress(curins);

	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pPatchPos4=curins->address+curins->instruction.size-4;
	tocallibrate->pOriginalSendc=ParseAddress(curins);
		
	//find 2 times jnz to the same location, second one is the one to be patched to jmp when patching login-encryption
	//if not found, then it exists in a sub-procedure

	//skip to call
	ASMFind(curfunc,insn_call,FALSE);

	//from here find 2 times jcc with same target
	prevpos=0;
	while(curins=ASMFindFirst(curfunc,jumps_callib,2,FALSE))
	{
		curpos=ParseAddress(curins);
		if(prevpos&&(curpos==prevpos))
			break;
		prevpos=curpos;
	}
	
	prevpos=0;

	if(curins==0)
	{
		ASMLast(curfunc);
		while((curins==0)&&(curins=ASMFind(curfunc,insn_call,TRUE)))
		{
			//enter function
			subfunc=ASMParseFunction(ParseAddress(curins),TRUE);

			prevpos=0;
			while(curins=ASMFindFirst(subfunc,jumps_callib,2,FALSE))
			{
				curpos=ParseAddress(curins);
				if(prevpos&&(curpos==prevpos))
				{
					DeleteParsedFunction(curfunc);
					curfunc=subfunc;
					subfunc=0;
					break;
				}
				prevpos=curpos;
			}

			//exit function
			if(subfunc)
				DeleteParsedFunction(subfunc);
		}
	}

	if(curins)
	{
		tocallibrate->logincryptpatchpos=curins->address;
		tocallibrate->logincryptpatchtarget=ParseAddress(curins);
	}

#ifdef DEBUG_CALLIBRATION
	debugprintf("logincryptpatchpos: %x\n",tocallibrate->logincryptpatchpos);
	debugprintf("logincryptpatchtarget: %x\n",tocallibrate->logincryptpatchtarget);
#endif

	DeleteParsedFunction(curfunc);

	//SocketObject -> Send, Recv, PacketHandler
	//old: (this does work most of the time, but it happens from time to time that the socket object is not setup correctly yet, causing all vtbl-access to fail
	//curpos=ReadUInt(tocallibrate->pSockObject);
	//socketobjectvtbl=ReadUInt(curpos);
	//new1: the logout_OK_packet (0xD1) can be traced down to the socket constructor, where the socketobjectvtbl is accessed directly
	//new1 doesn't work -- i can not get hold of the packethandler without the vtbl, so i can not lookup the logout_OK_packet
	//new2: socketobject is also constructed in winmain, we should get hold of it through there

	//22. pRecv
	tocallibrate->pRecv=ReadUInt(tocallibrate->pSocketObjectVtbl+5*4);//recv is the 6th function in the vtbl

#ifdef DEBUG_CALLIBRATION
	debugprintf("pRecv: %x\n",tocallibrate->pRecv);
#endif

	//pRecv-> recvcryptpatchpos, recvcryptpatchtarget, HandlePacket
	//find "cmp eax, -1" which checks if recv failed
	//next jcc is where the crypt-patch is to be done
	//folllow that jump, then follow the next jcc
	//then the first call is a call to "HandlePacket"

	//update: when already patched, the first patchpos might not be found
	//so it is better not to depend on its presence
	//therefore we will double check that the cmp eax, -1 is close enough to the jmp
	//if not, no patching is allowed
	//+ we will find the handlepacket in another way:
	//find a "cmp eax, 0x2733", which is a check for a specific WSA socket error (probably disconnection)
	//then follow the next jcc and find the first call

	//update: pHandlePacket was removed, it is actually not used anymore and is not really the packet-handler anyway
	//			the actual packethandler is the 9th function in the network object (socketobject) 's vtbl.

	curfunc=ASMParseFunction(tocallibrate->pRecv,TRUE);
	while(curins=ASMFind(curfunc,insn_cmp,FALSE))
	{
		if(ReadByte(curins->address+curins->instruction.size-1)==0xFF)
			break;
	}
	prevpos=curins->address;

	curins=ASMFind(curfunc,insn_jcc,FALSE);

	curins=ASMFindFirst(curfunc,jumps_callib,2,FALSE);
	
	if(curins->instruction.type==insn_jcc)//prevent callibrating an already patched client (encryption patch)
	{
		tocallibrate->recvcryptpatchpos=curins->address;
		tocallibrate->recvcryptpatchtarget=ParseAddress(curins);
	}

	while(curins=ASMFind(curfunc,insn_cmp,FALSE))
	{
		if((curins->instruction.size>4)&&(ReadUInt(curins->address+curins->instruction.size-4)==0x2733))
			break;
	}
	curins=ASMFind(curfunc,insn_jcc,FALSE);
	ASMJumpToAddress(curfunc,ParseAddress(curins));
	curins=ASMFind(curfunc,insn_call,FALSE);
	
	curpos=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("recvcryptpatchpos: %x\n",tocallibrate->recvcryptpatchpos);
	debugprintf("recvcryptpatchtarget: %x\n",tocallibrate->recvcryptpatchtarget);
	//debugprintf("pHandlePacket: %x\n",tocallibrate->pHandlePacket);
#endif

	DeleteParsedFunction(curfunc);

	//if(!hi)
	//{
		//packet-handler -> packetinfo table (for CMD-byte <-> packet-size matching)

	curfunc=ASMParseFunction(curpos,TRUE);
	
		//find "shr edx, 0xF", then find backwards movzx edx, packetinfo[edx*4]
		while(curins=ASMFind(curfunc,insn_shr,FALSE))
		{
			if(ReadByte(curins->address+curins->instruction.size-1)==0xF)
				break;
		}

		curins=ASMFind(curfunc,insn_mov,TRUE);
		tocallibrate->pPacketInfo=ReadUInt(curins->address+curins->instruction.size-4);

		DeleteParsedFunction(curfunc);
	//}

#ifdef DEBUG_CALLIBRATION
	debugprintf("pPacketInfo: %x\n",tocallibrate->pPacketInfo);
#endif

	curpos=ReadUInt(tocallibrate->pSocketObjectVtbl+6*4);//send is 7th function in the vtbl

	curfunc=ASMParseFunction(curpos,FALSE);

	curins=ASMFind(curfunc,insn_call,FALSE);

	tocallibrate->pSend=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pSend: %x\n",tocallibrate->pSend);
#endif

	DeleteParsedFunction(curfunc);

	curfunc=ASMParseFunction(tocallibrate->pSend,TRUE);

	//from here find 2 times jcc with same target
	prevpos=0;
	while(curins=ASMFindFirst(curfunc,jumps_callib,2,FALSE))
	{
		curpos=ParseAddress(curins);
		if(prevpos&&(curpos==prevpos))
			break;
		prevpos=curpos;
	}

	if(curins)
	{
		curins=ASMFindFirst(curfunc,jumps_callib,2,TRUE);
		if(curins->instruction.type==insn_jcc)//prevents recallibrating an already patched client (encryption patch)
		{
			tocallibrate->sendcryptpatchpos=curins->address;
			tocallibrate->sendcryptpatchtarget=ParseAddress(curins);
		}
	}

	DeleteParsedFunction(curfunc);

#ifdef DEBUG_CALLIBRATION
	debugprintf("sendcryptpatchpos: %x\n",tocallibrate->sendcryptpatchpos);
	debugprintf("sendcryptpatchtarget: %x\n",tocallibrate->sendcryptpatchtarget);
#endif

	packethandlerfunc=ReadUInt(tocallibrate->pSocketObjectVtbl+8*4);//packethandler is the 9th function in the vtbl

	tocallibrate->pPacketHandlerFunc=packethandlerfunc;
	tocallibrate->pOriginalHandlePacket=(void (__stdcall *)(unsigned char *))packethandlerfunc;

#ifdef DEBUG_CALLIBRATION
	debugprintf("pPacketHandlerFunc: %x\n",tocallibrate->pPacketHandlerFunc);
#endif

	//PacketHandler -> packetswitch -> handler code for specific packets

	/* new version:
	find loop
	find loop exit jmp or jcc (first jump to an out of the loop-range address)
	outside loop -> find jmp calculated (switch-jmp)
	-> backwards find jcc pswitch2
	-> follow jcc
	-> find jmp calculated : this is packet-switch jump
	-> backwards find movzx packetswitchindices
	*/

	curfunc=ASMParseFunction(tocallibrate->pPacketHandlerFunc,TRUE);

	curins=ASMFindSized(curfunc,insn_mov,6,FALSE);
	tocallibrate->pMapInfo=ReadUInt(curins->address+2);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pMapInfo: %x\n",tocallibrate->pMapInfo);
#endif

	//find loop
	while(curins=ASMFindFirst(curfunc,jumps_callib,2,FALSE))
	{
		jumptarget=ParseAddress(curins);
		if((jumptarget<curins->address)&&(jumptarget>=curfunc->startaddress))
			break;
	}

	loopstart=jumptarget;
	loopend=curins->address+curins->instruction.size;

	//jump to loop start
	ASMJumpToAddress(curfunc,loopstart);

	//find first jump out of the loop and follow it (if no such jump, the loop is exitted at its end)
	while((curins=ASMFindFirst(curfunc,jumps_callib,2,FALSE))&&(curins->address<loopend))
	{
		jumptarget=ParseAddress(curins);
		if((jumptarget<loopstart)||(jumptarget>=loopend))
		{
			ASMJumpToAddress(curfunc,jumptarget);
			break;
		}
	}

	//we are out of the loop now, find switch jump, follow previous jcc and find switch jump again

	while(curins=ASMFind(curfunc,insn_jmp,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_expression)
			break;
	}

	//find 2 jcc's to the same address (backwards)
	jumptarget=0;
	prevpos=0;
	while(curins=ASMFind(curfunc,insn_jcc,TRUE))
	{
		jumptarget=ParseAddress(curins);
		if(prevpos&&(prevpos==jumptarget))
			break;
		else
			prevpos=jumptarget;
	}

	ASMJumpToAddress(curfunc,jumptarget);

	while(curins=ASMFind(curfunc,insn_jmp,FALSE))
	{
		if(curins->instruction.operands[0].op.type==op_expression)
			break;
	}

	packetswitchoffset=ReadUInt(curins->address+curins->instruction.size-4);

	curins=ASMFind(curfunc,insn_mov,TRUE);

	packetswitchindex=ReadUInt(curins->address+curins->instruction.size-4);

	#ifdef DEBUG_CALLIBRATION
		debugprintf("packetswitchindex: %x\n",packetswitchindex);//switch index table
		debugprintf("packetswitchoffset: %x\n",packetswitchoffset);//switch offset table
	#endif
		
	DeleteParsedFunction(curfunc);


	//callibrations from specific packet handlers:
	//offset(index(packetnumber - 0xB)*4)=packet_handler_offset

	//packet 0x41 (update tile data packet) -> plandtiledata, pstatictiledata
	packetindex=ReadByte(packetswitchindex+(0x41-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	//enter handler chunk -> handler function
	curfunc=ASMParseFunction(curpos,FALSE);
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find loop
	while(curins=ASMFindFirst(curfunc,jumps_callib,2,FALSE))
	{
		jumptarget=ParseAddress(curins);
		if((jumptarget<curins->address)&&(jumptarget>=curfunc->startaddress))
			break;
	}

	loopstart=jumptarget;
	loopend=curins->address+curins->instruction.size;

	//find last call befroe the return
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	//follow call
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	//find loop
	while(curins=ASMFindFirst(subfunc,jumps_callib,2,FALSE))
	{
		jumptarget=ParseAddress(curins);
		if((jumptarget<curins->address)&&(jumptarget>=subfunc->startaddress))
			break;
	}
	//find "mov eax, LandTileData" at the start of the loop
	ASMJumpToAddress(subfunc,jumptarget);
	while(curins=ASMFind(subfunc,insn_mov,FALSE))
	{
		if((curins->instruction.size>4)&&(curins->instruction.operands[0].op.type==op_register))
			break;
	}

	tocallibrate->pLandTileData=ReadUInt(curins->address+curins->instruction.size-4);//read landtiledata

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLandTileData: %x\n",tocallibrate->pLandTileData);
#endif

	DeleteParsedFunction(subfunc);

	//find first jge (jcc) before the loop
	ASMJumpToAddress(curfunc,loopstart);
	curins=ASMFind(curfunc,insn_jcc,TRUE);
	//follow jge
	ASMJumpToAddress(curfunc,ParseAddress(curins));
	//find last call before the return
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	//enter function,
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	//find loop
	while(curins=ASMFindFirst(subfunc,jumps_callib,2,FALSE))
	{
		jumptarget=ParseAddress(curins);
		if((jumptarget<curins->address)&&(jumptarget>=subfunc->startaddress))
			break;
	}
	//find "mov eax, StaticTileData" at the start of the loop
	ASMJumpToAddress(subfunc,jumptarget);
	while(curins=ASMFind(subfunc,insn_mov,FALSE))
	{
		if((curins->instruction.size>4)&&(curins->instruction.operands[0].op.type==op_register))
			break;
	}
	tocallibrate->pStaticTileData=ReadUInt(curins->address+curins->instruction.size-4);//read statictiledata

#ifdef DEBUG_CALLIBRATION
	debugprintf("pStaticTileData: %x\n",tocallibrate->pStaticTileData);
#endif
	

	//cleanup
	DeleteParsedFunction(subfunc);
	DeleteParsedFunction(curfunc);
	subfunc=0;

	//packet 0x4E (set personal light level packet) -> pReadFromMulFile
	//-> packethandler -> lastcall = update_personal_light_level -> first call is something that updates the map -> find sar eax, 3 (part of coordinates -> mapblock calculation) -> follow the next call (readfrommapfile) -> last call is read from mul file
	packetindex=ReadByte(packetswitchindex+(0x4E-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	//first call, last call, first call, call just after "sar eax, 3", then last call = call to LoadFromMulFile
	curfunc=ASMParseFunction(curpos,FALSE);

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//last call
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find "sar eax, 3" + follow the next call
	while(curins=ASMFind(curfunc,insn_shr,FALSE))
	{
		if(ReadByte(curins->address+curins->instruction.size-1)==0x03)
			break;
	}
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//last call is a call to LoadFromMulFile
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	tocallibrate->pLoadFromMulFile=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLoadFromMulFile: %x\n",tocallibrate->pLoadFromMulFile);
#endif



	//packet 0xA1 = new item packet -> LoadFromIndexedFile (callibration to complicated?)
	packetindex=ReadByte(packetswitchindex+(0x1A-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(curpos,FALSE);

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//search backwards from return
	curins=ASMFind(curfunc,insn_return,FALSE);

	/* OLD : bugged, stacksize changed
	//call xxxxxx; add esp, 0x28; (ccall with stacksize 0x28)
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,TRUE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x28)
		{
			curins=ASMPrevious(curfunc);
			subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
			DeleteParsedFunction(curfunc);
			curfunc=subfunc;
			subfunc=0;
			break;
		}
		ASMPrevious(curfunc);
	}*/

	//new: last call
	curins=ASMFind(curfunc,insn_call,TRUE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//2nd ccall with stacksize 0xC
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,FALSE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x0C)
		{
			break;
		}
	}
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,FALSE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x0C)
		{
			curins=ASMPrevious(curfunc);
			subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
			DeleteParsedFunction(curfunc);
			curfunc=subfunc;
			subfunc=0;
			break;
		}
	}

	//call xxxx, test eax, eax; before first return
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFindMultiple(curfunc,calltest,2,TRUE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;	
	
	//ccall with stacksize 0x08
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,FALSE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x08)
		{
			curins=ASMPrevious(curfunc);
			subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
			DeleteParsedFunction(curfunc);
			curfunc=subfunc;
			subfunc=0;
			break;
		}
	}

	//ccall with stacksize 0x14 is the one we look for (LoadFromIndexedFile)
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,FALSE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x14)
		{
			curins=ASMPrevious(curfunc);
			break;
		}
	}

	tocallibrate->pLoadFromIndexedFile=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pLoadFromIndexedFile: %x\n",tocallibrate->pLoadFromIndexedFile);
#endif

	DeleteParsedFunction(curfunc);

	//packet 0xD1 = logout status packet -> pCharName and pServer are reset here
	packetindex=ReadByte(packetswitchindex+(0xD1-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(curpos,FALSE);
	
	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//follow last jcc before the first return, then find the first call and follow it
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_jcc,TRUE);
	ASMJumpToAddress(curfunc,ParseAddress(curins));
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find 4 subsequent calls
	curins=ASMFindMultiple(curfunc,fourcalls,4,FALSE);
	
	//then find mov xxxxx, 0 twice, first one is xxxxx=ServerName, second one is xxxxx=PlayerName
	while(curins=ASMFindSized(curfunc,insn_mov,7,FALSE))
	{
		if(ReadByte(curins->address+6)==0)
			break;
	}

	tocallibrate->pServerName=ReadUInt(curins->address+2);

	while(curins=ASMFindSized(curfunc,insn_mov,7,FALSE))
	{
		if(ReadByte(curins->address+6)==0)
			break;
	}

	tocallibrate->pCharName=ReadUInt(curins->address+2);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pServerName: %x\n",tocallibrate->pServerName);
	debugprintf("pCharName: %x\n",tocallibrate->pCharName);
#endif

	DeleteParsedFunction(curfunc);

	//packet 0x3A = skill update packet -> skill-stuff
	packetindex=ReadByte(packetswitchindex+(0x3A-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	//first call
	curfunc=ASMParseFunction(curpos,FALSE);
	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find push -1, then mov expression, .... 4 times	from the end
	ASMLast(curfunc);

	while(curins=ASMFind(curfunc,insn_push,TRUE))
	{
		if(ReadByte(curins->address+curins->instruction.size-1)==0xFF)
			break;
	}

	for(i=0;i<4;i++)
	{
		while(curins=ASMFind(curfunc,insn_mov,TRUE))
		{
			if(curins->instruction.operands[0].op.type==op_expression)
				break;
		}

		skilladdresses[i]=ReadUInt(curins->address+curins->instruction.size-4);
	}

	for(i=0;i<4;i++)
	{
		//find "push skilladdress[x]" or "mov regvar, skilladdress", mods, reals, locks, caps (if searching backwards)
		while(curins=ASMFindFirst(curfunc,movorpush,2,TRUE))
		{
			if(curins->instruction.size>4)
			{
				curpos=ReadUInt(curins->address+curins->instruction.size-4);
				if((curpos==skilladdresses[0])||(curpos==skilladdresses[1])||(curpos==skilladdresses[2])||(curpos==skilladdresses[3]))
					break;
			}
		}
		if(curins)
		{
			switch(i)
			{
			case 0:
				tocallibrate->pSkillModifiedValues=curpos;
				break;
			case 1:
				tocallibrate->pSkillRealValues=curpos;
				break;
			case 2:
				tocallibrate->pSkillLocks=curpos;
				break;
			case 3:
				tocallibrate->pSkillCaps=curpos;
				break;
			}
		}
	}

#ifdef DEBUG_CALLIBRATION
	debugprintf("pSkillCaps: %x\n",tocallibrate->pSkillCaps);
	debugprintf("pSkillLocks: %x\n",tocallibrate->pSkillLocks);
	debugprintf("pSkillRealValues: %x\n",tocallibrate->pSkillRealValues);
	debugprintf("pSkillModifiedValues: %x\n",tocallibrate->pSkillModifiedValues);
#endif

	DeleteParsedFunction(curfunc);

	//packet 0xD6 = properties packet -> pHoldingID (6)
	packetindex=ReadByte(packetswitchindex+(0xD6-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(curpos,FALSE);

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find loop, goto the end of the loop
	while(curins=ASMFindFirst(curfunc,jumps_callib,2,FALSE))
	{
		jumptarget=ParseAddress(curins);
		if((jumptarget<curins->address)&&(jumptarget>=curfunc->startaddress))
			break;
	}

	//find ccall, stacksize 0x10, near the end of the loop and follow it
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,TRUE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x10)
		{
			curins=ASMPrevious(curfunc);
			subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
			DeleteParsedFunction(curfunc);
			curfunc=subfunc;
			subfunc=0;
			break;
		}
		ASMPrevious(curfunc);
	}

	//find push 0x7D0
	while(curins=ASMFind(curfunc,insn_push,FALSE))
	{
		if((curins->instruction.size>4)&&(ReadUInt(curins->address+curins->instruction.size-4)==0x7D0))
			break;
	}

	//follow next call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find finditemcall
	while(curins=ASMFind(curfunc,insn_call,FALSE))
	{
		if(ParseAddress(curins)==tocallibrate->pFindItemByID)
			break;
	}

	//find mov edx, pHoldingID
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if((curins->instruction.operands[0].op.type==op_register)&&(curins->instruction.size>4))
			break;
	}

	//read pHoldingID
	tocallibrate->pHoldingID=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pHoldingID: %x\n",tocallibrate->pHoldingID);
#endif

	DeleteParsedFunction(curfunc);

	//packet 0x55 = login confirm packet // -> pShowItemPropertyGump, IPTable, IPIndex  (5,4)
	packetindex=ReadByte(packetswitchindex+(0x55-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(curpos,FALSE);

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	ASMFirst(curfunc);

	/*bugged: stacksize changed
	//find ccall, stacksize 0x08
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,FALSE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x08)
		{
			curins=ASMPrevious(curfunc);
			subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
			DeleteParsedFunction(curfunc);
			curfunc=subfunc;
			subfunc=0;
			break;
		}
	}
	*/
	//new: find two textout calls, (push 0x3, push 0x21, call textout), skipp the next call, and the next call we need to follow
	for(i=0;i<2;i++)
	{
		while(curins=ASMFindMultiple(curfunc,propsandiptablecallib_new,3,FALSE))
		{
			if(ReadByte(curins->address+curins->instruction.size-1)==0x03)
			{
				curins=ASMNext(curfunc);
				if(ReadByte(curins->address+curins->instruction.size-1)==0x21)
				{
					curins=ASMNext(curfunc);
					if(ParseAddress(curins)==tocallibrate->pTextOut)
						break;
				}
			}
		}
	}

	curins=ASMFind(curfunc,insn_call,FALSE);
	curins=ASMFind(curfunc,insn_call,FALSE);

	subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;

	
	//find 2nd ccall, stacksize 0x0C, from start
	ASMFirst(curfunc);
	for(i=0;i<2;i++)
	{
		while(curins=ASMFind(curfunc,insn_add,FALSE))
		{
			if((ReadUShort(curins->address)==0xC483)&&(ReadByte(curins->address+curins->instruction.size-1)==0x0C))
			{
				break;
			}
			//curins=ASMPrevious(curfunc);
		}
	}

	curins=ASMFind(curfunc,insn_call,TRUE);

	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find ja (out of switchtable range)
	while(curins=ASMFind(curfunc,insn_jcc,FALSE))
	{
		if(ReadUShort(curins->address)==0x870F)
			break;
	}

	//next jmp is switch-jmp
	curins=ASMFind(curfunc,insn_jmp,FALSE);
	switchtable=ReadUInt(curins->address+curins->instruction.size-4);

	DeleteParsedFunction(curfunc);

	curpos=ReadUInt(switchtable+(0x94*4));//case 0x94

	curfunc=ASMParseFunction(curpos,TRUE);

	//find ccall, stacksize 0x08
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,FALSE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x08)
		{
			curins=ASMPrevious(curfunc);
			subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
			DeleteParsedFunction(curfunc);
			curfunc=subfunc;
			subfunc=0;
			break;
		}
	}

	//find ccall, stacksize 0x0C
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,FALSE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x0C)
		{
			curins=ASMPrevious(curfunc);
			break;
		}
	}

	tocallibrate->pShowItemPropertyGump=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pShowItemPropertyGump: %x\n",tocallibrate->pShowItemPropertyGump);
#endif

	DeleteParsedFunction(curfunc);

	//pShowItemPropertyGump -> ...
	curfunc=ASMParseFunction(tocallibrate->pShowItemPropertyGump,TRUE);

	while(curins=ASMFind(curfunc,insn_add,FALSE))
	{
		if(ReadByte(curins->address+curins->instruction.size-1)==0x4)
			break;
	}

	curins=ASMFind(curfunc,insn_call,TRUE);
	tocallibrate->pPropInitA=ParseAddress(curins);

	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pPropInitA=ParseAddress(curins);
	
	while(curins=ASMFind(curfunc,insn_push,FALSE))
	{
		if((curins->instruction.size>4)&&(ReadUInt(curins->address+curins->instruction.size-4)==0xFC))
			break;
	}

	//skip 2 calls
	curins=ASMFind(curfunc,insn_call,FALSE);
	curins=ASMFind(curfunc,insn_call,FALSE);

	//follow a jmp
	curins=ASMFind(curfunc,insn_jmp,FALSE);
	ASMJumpToAddress(curfunc,ParseAddress(curins));

	//second call is the one we need
	curins=ASMFind(curfunc,insn_call,FALSE);
	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pPropsAndNameGet=ParseAddress(curins);

	//previous mov gives us the id we need
	curins=ASMFind(curfunc,insn_mov,TRUE);
	tocallibrate->pPropGumpID=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("tocallibrate->pPropsAndNameGet: %x\n",tocallibrate->pPropsAndNameGet);
	debugprintf("tocallibrate->pPropGumpID: %x\n",tocallibrate->pPropGumpID);
#endif

	DeleteParsedFunction(curfunc);

	//pGetName is call before lea ecx, esi->0xDC (int mov)
	//pGetProperties is call before lea ecx, esi->0xEC (int mov)

	curfunc=ASMParseFunction(tocallibrate->pPropsAndNameGet,TRUE);
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(ReadUInt(curins->address+curins->instruction.size-4)==0xDC)
			break;
	}
	curins=ASMFind(curfunc,insn_call,TRUE);
	tocallibrate->pGetName=ParseAddress(curins);
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(ReadUInt(curins->address+curins->instruction.size-4)==0xEC)
			break;
	}
	curins=ASMFind(curfunc,insn_call,TRUE);
	tocallibrate->pGetProperties=ParseAddress(curins);
	DeleteParsedFunction(curfunc);

	//pGetName->pDoGetName
	curfunc=ASMParseFunction(tocallibrate->pGetName,FALSE);

	//find add esp, 0x10, call before that is pDoGetName
	while(curins=ASMFind(curfunc,insn_add,FALSE))
	{
		if(ReadByte(curins->address+curins->instruction.size-1)==0x10)
			break;
	}

	curins=ASMFind(curfunc,insn_call,TRUE);

	tocallibrate->pDoGetName=ParseAddress(curins);

	curins=ASMFind(curfunc,insn_call,TRUE);
	tocallibrate->pStringObjectInit=ParseAddress(curins);
	curins=ASMFind(curfunc,insn_push,TRUE);
	tocallibrate->pDefaultNameString=ReadUInt(curins->address+curins->instruction.size-4);

	DeleteParsedFunction(curfunc);

	//pGetProperties->pDoGetProperties
	curfunc=ASMParseFunction(tocallibrate->pGetProperties,TRUE);

	//find add esp, 0x10, call before that is pDoGetProperties
	while(curins=ASMFind(curfunc,insn_add,FALSE))
	{
		if(ReadByte(curins->address+curins->instruction.size-1)==0x10)
			break;
	}

	curins=ASMFind(curfunc,insn_call,TRUE);

	tocallibrate->pDoGetProperties=ParseAddress(curins);

	while(curins=ASMFind(curfunc,insn_call,TRUE))
	{
		if(ParseAddress(curins)==tocallibrate->pStringObjectInit)
			break;
	}

	curins=ASMFind(curfunc,insn_push,TRUE);
	tocallibrate->pDefaultPropertiesString=ReadUInt(curins->address+curins->instruction.size-4);

	DeleteParsedFunction(curfunc);

	//case 0x21 // -> IPTable/Index  (4)
	curpos=ReadUInt(switchtable+(0x21*4));

	curfunc=ASMParseFunction(curpos,FALSE);
	
	//skip 1 mov expression, ... next one is IPTable mov
	for(i=0;i<2;i++)
	{
		while(curins=ASMFind(curfunc,insn_mov,FALSE))
		{
			if((curins->instruction.size>4)&&(curins->instruction.operands[0].op.type==op_expression))
				break;
		}
	}

	tocallibrate->IPTable=ReadUInt(curins->address+curins->instruction.size-4);	

	//next mov expression, .... is PortTable mov
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if((curins->instruction.size>4)&&(curins->instruction.operands[0].op.type==op_expression))
			break;
	}

	tocallibrate->PortTable=ReadUInt(curins->address+curins->instruction.size-4);

	//find 3 movs/cmps, 2 of them are moving to the same dword, that is the ipindex
	curins=ASMFindFirst(curfunc,movcmps,2,FALSE);
	curpos=ReadUInt(curins->address+curins->instruction.size-4);
	curins=ASMFindFirst(curfunc,movcmps,2,FALSE);
	prevpos=ReadUInt(curins->address+curins->instruction.size-4);
	if(curpos==prevpos)
		tocallibrate->IPIndex=curpos;
	else
	{
		curins=ASMFindFirst(curfunc,movcmps,2,FALSE);
		if(ReadUInt(curins->address+curins->instruction.size-4)==curpos)
			tocallibrate->IPIndex=curpos;
		else
			tocallibrate->IPIndex=prevpos;
	}

#ifdef DEBUG_CALLIBRATION
	debugprintf("IPTable: %x\n",tocallibrate->IPTable);
	debugprintf("PortTable: %x\n",tocallibrate->PortTable);
	debugprintf("IPIndex: %x\n",tocallibrate->IPIndex);
#endif

	//packet 0xBF = general commands packet //->facet (3)
	packetindex=ReadByte(packetswitchindex+(0xBF-0xB));
	curpos=ReadUInt(packetswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(curpos,FALSE);

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find switch jmp
	while(curins=ASMFind(curfunc,insn_jmp,FALSE))
	{
		if(curins->instruction.size>5)
			break;
	}

	subpacketswitchoffset=ReadUInt(curins->address+curins->instruction.size-4);

	curins=ASMFind(curfunc,insn_mov,TRUE);

	subpacketswitchindex=ReadUInt(curins->address+curins->instruction.size-4);

	DeleteParsedFunction(curfunc);
	
	//subpackets: offset(index(subpacketnumber - 1)*4)=subpacker_handler_offset
	//-> subpacket 8 is map change packet
	packetindex=ReadByte(subpacketswitchindex+(8-1));
	curpos=ReadUInt(subpacketswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(curpos,FALSE);

	//find return
	//backwards find jmp (return is in default case, which is jumped to)
	//then find backwards a call
	//first mov before that call, is mov regvar, byte facet

	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_jmp,TRUE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	curins=ASMFind(curfunc,insn_mov,TRUE);
	tocallibrate->pFacet=ReadUInt(curins->address+curins->instruction.size-4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pFacet: %x\n",tocallibrate->pFacet);
#endif

	DeleteParsedFunction(curfunc);
	
	//packet 0x38 = pathfindpacket (2)
	packetindex=ReadByte(packetswitchindex+(0x38-0xB));
	pathfindhandler=ReadUInt(packetswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(pathfindhandler,FALSE);

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),FALSE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//last call
	curins=ASMFind(curfunc,insn_return,FALSE);
	curins=ASMFind(curfunc,insn_call,TRUE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//last ccall before first return, stacksize 0x20 = calculatepath
	curins=ASMFind(curfunc,insn_return,FALSE);
	while(curins=ASMFindMultiple(curfunc,ccallcallib,2,TRUE))
	{
		curins=ASMNext(curfunc);
		if(ReadByte(curins->address+curins->instruction.size-1)==0x20)
		{
			curins=ASMPrevious(curfunc);
			break;
		}
		curins=ASMPrevious(curfunc);
	}
	tocallibrate->pCalculatePath=ParseAddress(curins);
	
	//next mov stores the result of CalculatePath into startofpath
	curins=ASMFind(curfunc,insn_mov,FALSE);
	tocallibrate->pStartOfPath=ReadUInt(curins->address+curins->instruction.size-4);

	//follow next jcc (jumps if a path was succesfully found)
	curins=ASMFind(curfunc,insn_jcc,FALSE);
	ASMJumpToAddress(curfunc,ParseAddress(curins));

	//first mov is mov bDoPathfind, 1 (sets boolean DoPathfind to TRUE)
	while(curins=ASMFind(curfunc,insn_mov,FALSE))
	{
		if(curins->instruction.size>=5)
			break;
	}
	if(curins->instruction.size==5)
		tocallibrate->pBDoPathfind=ReadUInt(curins->address+curins->instruction.size-4);
	else
		tocallibrate->pBDoPathfind=ReadUInt(curins->address+2);

	//next call is invertpath
	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pInvertPath=ParseAddress(curins);

	//now find 4 times push 0 before the next(=last) return
	curins=ASMFind(curfunc,insn_return,FALSE);
	i=0;
	while((curins=ASMFind(curfunc,insn_push,TRUE)))
	{
		if(ReadByte(curins->address+curins->instruction.size-1)==0)
		{
			i++;
			if(i==4)
				break;
		}
		else
			i=0;
	}
	
	//next call is WalkPath
	curins=ASMFind(curfunc,insn_call,FALSE);
	tocallibrate->pWalkPath=ParseAddress(curins);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pWalkPath: %x\n",tocallibrate->pWalkPath);
	debugprintf("pInvertPath: %x\n",tocallibrate->pInvertPath);
	debugprintf("pStartOfPath: %x\n",tocallibrate->pStartOfPath);
	debugprintf("pBDoPathfind: %x\n",tocallibrate->pBDoPathfind);
	debugprintf("pCalculatePath: %x\n",tocallibrate->pCalculatePath);
#endif

	DeleteParsedFunction(curfunc);

	//packet 0x6C = show target cursur -> pOnTarget (1)

	packetindex=ReadByte(packetswitchindex+(0x6C-0xB));
	targcurshandler=ReadUInt(packetswitchoffset+packetindex*4);

	curfunc=ASMParseFunction(targcurshandler,FALSE);

	//first call
	curins=ASMFind(curfunc,insn_call,FALSE);
	subfunc=ASMParseFunction(ParseAddress(curins),TRUE);
	DeleteParsedFunction(curfunc);
	curfunc=subfunc;
	subfunc=0;

	//find jcc, follow
	curins=ASMFind(curfunc,insn_jcc,FALSE);
	ASMJumpToAddress(curfunc,ParseAddress(curins));

	//find return
	curins=ASMFind(curfunc,insn_return,FALSE);

	//backwards find mov with size > 8
	while(curins=ASMFind(curfunc,insn_mov,TRUE))
	{
		if(curins->instruction.size>8)
			break;
	}	
	//read ontarget and defaultontarget
	tocallibrate->pDefaultOnTarget=ReadUInt(curins->address+curins->instruction.size-4);
	tocallibrate->pOnTarget=ReadUInt(curins->address+curins->instruction.size-8);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pOnTarget: %x\n",tocallibrate->pOnTarget);
	debugprintf("pDefaultOnTarget: %x\n",tocallibrate->pDefaultOnTarget);
#endif

	DeleteParsedFunction(curfunc);

	//pLoginKey1, pLoginKey2 from logincrypt
	//...

	tocallibrate->pOriginalConnLossHandler=(void (__stdcall *)(unsigned int))ReadUInt(tocallibrate->pSocketObjectVtbl+3*4);

#ifdef DEBUG_CALLIBRATION
	debugprintf("pOriginalConnLossHandler: %x\n",tocallibrate->pOriginalConnLossHandler);
#endif

	ver=Get2DClientVersion(GetCurrentProcessId());

	//item object fields
	tocallibrate->oItemX=0x24;
	tocallibrate->oItemY=0x26;
	tocallibrate->oItemZ=0x28;
	tocallibrate->oItemType=0x38;
	tocallibrate->oItemTypeIncrement=0x3C;
	tocallibrate->oItemDirection=0x72;
	//tocallibrate->oItemPrevious;
	tocallibrate->oItemContentsNext=0x44;
	tocallibrate->oItemContentsPrevious=0x48;
	tocallibrate->oItemColor=0x40;
	tocallibrate->oItemHighlightColor=0x42;
	tocallibrate->oItemStackCount=0x3E;

	tocallibrate->oMultiType=0x3A;
	tocallibrate->oMultiContents=0xA0;//most likely wrong now

	if(ver.v1<7)
	{
		tocallibrate->oItemID=0x80;
		tocallibrate->oItemNext=0x88;
		tocallibrate->oItemContents=0xC0;
		tocallibrate->oItemContainer=0x84;
		tocallibrate->oItemGump=0xC4;
		tocallibrate->oItemReputation=0x204;
		tocallibrate->oItemFlags=0xA4;	
		tocallibrate->oMobileStatus=0x90;
		tocallibrate->oMobileLayers=0xDC;
	}
	else
	{
		tocallibrate->oItemID=0x88;
		tocallibrate->oItemNext=0x90;
		tocallibrate->oItemContents=0xC8;
		tocallibrate->oItemContainer=0x8C;
		tocallibrate->oItemGump=0xCC;
		tocallibrate->oItemFlags=0xAC;	
		tocallibrate->oMobileStatus=0x98;
		tocallibrate->oMobileLayers=0xFC;
		tocallibrate->oItemReputation=0x204+(6*4);//wild guess, probably wrong
	}

	tocallibrate->oItemIsMobile=0x24;

	//mobile object fields
	
	tocallibrate->oMobileWarMode=0x168;
	tocallibrate->oMobileEnemy=0x23C;
	tocallibrate->oMobileRunning=0x15C;
	
	//status gump fields
	tocallibrate->oStatusName=0xC4;
	
	//journal fields
	tocallibrate->oJournalNext=0x18;
	tocallibrate->oJournalPrevious=0x1C;
	tocallibrate->oJournalText=0x0;
	tocallibrate->oJournalColor=0x4;
	tocallibrate->oPaperdollTitle=0xD0;

	//gump vtbl entries
	tocallibrate->oGumpClosable=0x130;
	tocallibrate->oGumpClose=0x0;
	tocallibrate->oGumpClick=0x20;
	tocallibrate->oGumpFocus=0x1C;
	tocallibrate->oGumpWriteChar=0x58;

	//gump fields
	tocallibrate->oGumpID=0x50;
	tocallibrate->oGumpType=0x6C;
	tocallibrate->oGumpNext=0x58;
	tocallibrate->oGumpPrevious=0x5C;
	tocallibrate->oGumpSubGumpFirst=0x60;
	tocallibrate->oGumpSubGumpLast=0x68;
	tocallibrate->oGumpX=0x34;
	tocallibrate->oGumpY=0x38;
	tocallibrate->oGumpWidth=0x24;
	tocallibrate->oGumpHeight=0x28;
	tocallibrate->oGumpName=0x08;
	tocallibrate->oGumpItem=0x4C;
	
	//button gump
	tocallibrate->oButtonGumpOnClick=0xFC;

	//input control gump
	tocallibrate->oGumpText=0x118;

	//unicode input control gump
	tocallibrate->oGumpUnicodeText=0x114;

	//generic gump
	tocallibrate->oGenericGumpID=0xD8;
	tocallibrate->GenericGumpType=0xDC;
	tocallibrate->oGumpElements=0xE0;
	tocallibrate->oGumpSubElements=0x2C;
	//generic gump element
	tocallibrate->oGumpElementType=0x38;
	tocallibrate->oGumpElementNext=0x30;
	tocallibrate->oGumpElementID=0x44;
	tocallibrate->oGumpElementSelected=0x5C;
	tocallibrate->oGumpElementText=0x3C;
	tocallibrate->oGumpElementTooltip=0x74;
	tocallibrate->oGumpElementX=0x14;
	tocallibrate->oGumpElementY=0x18;
	tocallibrate->oGumpElementWidth=0x1C;
	tocallibrate->oGumpElementHeight=0x20;
	tocallibrate->oGumpElementReleasedType=0x24;
	tocallibrate->oGumpElementPressedType=0x28;
	//generic gump element vtbl
	tocallibrate->oGumpElementClick=0x14;

	return tocallibrate;
}