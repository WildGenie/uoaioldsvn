#include "ASMParser.h"

#include "W2EDebug.h"

/*HMODULE libdisasm=0;
px86_disasm x86_disasm=0;
px86_cleanup x86_cleanup=0;
px86_init x86_init=0;*/

int asmparser_initialized=0;

/*BOOL ASMParser_inits(char * libdisasmpath)
{
	if(!asmparser_initialized)
	{
		if(libdisasm=LoadLibrary(libdisasmpath))
		{
			x86_init=(px86_init)GetProcAddress(libdisasm,"x86_init");
			x86_cleanup=(px86_cleanup)GetProcAddress(libdisasm,"x86_cleanup");
			x86_disasm=(px86_disasm)GetProcAddress(libdisasm,"x86_disasm");
			if(x86_init&&x86_cleanup&&x86_disasm)
			{
				x86_init(opt_none, NULL, NULL);
				asmparser_initialized=1;
				return TRUE;
			}
		}
		return FALSE;
	}
	else
		return TRUE;
	
}*/

void ASMParser_inits()
{
	if(asmparser_initialized==0)
	{
		x86_init(opt_none, NULL, NULL);
		asmparser_initialized=1;
	}
	return;
}


void ASMParser_cleanup()
{
	if(asmparser_initialized)
		x86_cleanup();
	return;
}

int instructioncompare(void * a,void * b)
{
	unsigned int * ua=(unsigned int *)a;
	unsigned int * ub=(unsigned int *)b;

	if((*ua)<(*ub))
		return +1;
	else if((*ua)>(*ub))
		return -1;
	else
		return 0;
}

/*Instruction * ASMParseInstruction(unsigned int address)
{
	Instruction * toreturn;
	char buf[1024];

	if(Read(address,buf,1024))
	{
		toreturn=(Instruction *)wAlloc(sizeof(Instruction));
		toreturn->address=address;
		if(x86_disasm(buf,1024,0,0,&(toreturn->instruction))>0)
			return toreturn;
		wFree(toreturn);
	}

	return 0;
}*/

ParsedFunction * NewParsedFunction()
{
	ParsedFunction * toreturn=(ParsedFunction *)wAlloc(sizeof(ParsedFunction));

	toreturn->pid=0;
	toreturn->btenum=0;
	toreturn->instructions=BT_create(instructioncompare);

	return toreturn;
}

void ResetParsedFunction(ParsedFunction * tocleanup)
{
	Instruction * curins;

	if(tocleanup->btenum)
	{
		BT_enumdelete(tocleanup->btenum);
	}

	tocleanup->btenum=BT_newenum(tocleanup->instructions);

	while(curins=(Instruction *)BT_next(tocleanup->btenum))
	{
		BT_previous(tocleanup->btenum);
		BT_remove(tocleanup->instructions,(void *)curins);
		wFree(curins);
	}

	BT_enumdelete(tocleanup->btenum);
	
	return;
}

void DeleteParsedFunction(ParsedFunction * todelete)
{
	ResetParsedFunction(todelete);
	BT_delete(todelete->instructions);
	wFree(todelete);
	return;
}

void ASMParseChunk(ParsedFunction * parser,unsigned int address,BOOL allowbranching)
{
	char buf[1024];
	Instruction * newinstruction;
	unsigned int curaddress;
	unsigned int targetaddress;

	curaddress=address;

	while(1)
	{
		if(BT_find(parser->instructions,&curaddress))
			return;

		//read
		if(!RRead(parser->pid,curaddress,buf,1024))
			break;
		
		newinstruction=wAlloc(sizeof(Instruction));
	
		//disassemble
		if(!x86_disasm(buf,1024,0,0,&(newinstruction->instruction)))
		{
			wFree(newinstruction);
			break;
		}

		//insert
		newinstruction->address=curaddress;
		BT_insert(parser->instructions,(void *)newinstruction);

		//retn?
		if(newinstruction->instruction.type==insn_return)
			return;//chunk parsed

		//jnz or jz?
		if((newinstruction->instruction.type==insn_jcc)&&allowbranching)
		{
			if(targetaddress=ParseAddress(newinstruction))
			{
				if(BT_find(parser->instructions,&targetaddress)==0)//chunk does not exist yet
				{
					ASMParseChunk(parser,targetaddress,TRUE);//parse chunk
				}
			}
			//else
			//	error: could not parse chunck address
		}

		//jmp -> parse address or return if not parseable
		if(newinstruction->instruction.type==insn_jmp)//try to follow the jmp to the next instruction
		{
			if((curaddress=ParseAddress(newinstruction))==0)
				return;//jmp failed, so chunk is ended
			else if(BT_find(parser->instructions,&curaddress)!=0)//already parsed, f.e. loop
				return;
		}
		else//just move to the next instruction
		{
			curaddress+=newinstruction->instruction.size;
		}
	}

	return;
}

ParsedFunction * ASMParseFunction(unsigned int address,BOOL allchunks)
{	
	ParsedFunction * toreturn=NewParsedFunction();

	toreturn->pid=0;
	toreturn->linearsearch=FALSE;
	toreturn->startaddress=address;

	ASMParseChunk(toreturn,address,allchunks);

	toreturn->btenum=BT_newenum(toreturn->instructions);
	
	return toreturn;
}

Instruction * ASMFindOpCode(ParsedFunction * parser,BYTE tofind,BOOL backwards)
{
	Instruction * curins;

	if(backwards==FALSE)
	{
		while(curins=ASMNext(parser))
		{
			if(RReadByte(parser->pid,curins->address)==tofind)
				return curins;
		}
	}
	else
	{
		while(curins=ASMPrevious(parser))
		{
			if(RReadByte(parser->pid,curins->address)==tofind)
				return curins;
		}
	}

	return 0;
}

Instruction * ASMFindFirst(ParsedFunction * parser,enum x86_insn_type * tofind,unsigned int count,BOOL backwards)
{
	Instruction * curins;
	unsigned int i;

	if(backwards==FALSE)
	{
		while(curins=ASMNext(parser))
		{
			for(i=0;i<count;i++)
			{
				if(curins->instruction.type==tofind[i])
					return curins;
			}
		}
	}
	else
	{
		while(curins=ASMPrevious(parser))
		{
			for(i=0;i<count;i++)
			{
				if(curins->instruction.type==tofind[i])
					return curins;
			}
		}
	}

	return 0;
}

Instruction * ASMFind(ParsedFunction * parser,enum x86_insn_type tofind,BOOL backwards)
{
	Instruction * curins;

	if(backwards==FALSE)
	{
		while(curins=ASMNext(parser))
		{
			if(curins->instruction.type==tofind)
				return curins;
		}
	}
	else
	{
		while(curins=ASMPrevious(parser))
		{
			if(curins->instruction.type==tofind)
				return curins;
		}
	}

	return 0;
}

Instruction * ASMNext(ParsedFunction * parser)
{
	Instruction * toreturn=0;
	if(parser->linearsearch==FALSE)
		return (Instruction *)BT_next(parser->btenum);
	else
	{
		if(toreturn=ASMParseInstruction(parser->pid,parser->startaddress))
		{
			BT_insert(parser->instructions,toreturn);
			parser->startaddress+=toreturn->instruction.size;
		}
		return toreturn;
	}
}

Instruction * ASMPrevious(ParsedFunction * parser)
{
	if(parser->linearsearch==FALSE)
		return (Instruction *)BT_previous(parser->btenum);
	else
		return 0;//can't find backwards in linear search mode... instruction-sizes are unknown
}

int GetOpSignedValue(Instruction * insn,unsigned int idx)
{
	if((insn)&&(idx<(insn->instruction.operand_count)))
	{
		switch(insn->instruction.operands[idx].op.datatype)
		{
		case op_byte:
			return (int)(signed char)insn->instruction.operands[idx].op.data.byte;
		case op_word:
			return (int)(signed short)insn->instruction.operands[idx].op.data.word;
		case op_dword:
			return (int)insn->instruction.operands[idx].op.data.dword;
		default:
			return 0;
		}
	}

	return 0;
}

unsigned int GetOpValue(Instruction * insn,unsigned int idx)
{
	if((insn)&&(idx<(insn->instruction.operand_count)))
	{
		switch(insn->instruction.operands[idx].op.datatype)
		{
		case op_byte:
			return (unsigned int)insn->instruction.operands[idx].op.data.byte;
		case op_word:
			return (unsigned int)insn->instruction.operands[idx].op.data.word;
		case op_dword:
			return (unsigned int)insn->instruction.operands[idx].op.data.dword;
		default:
			return 0;
		}
	}

	return 0;
}

unsigned int ParseAddress(Instruction * insn)
{
	unsigned int i;
	int rel;

	if(insn==0)
		return 0;

	for(i=0;i<insn->instruction.operand_count;i++)
	{
		switch(insn->instruction.operands[i].op.type)
		{
		case op_absolute:
			return GetOpValue(insn,i);
		case op_relative_far:
		case op_relative_near:
			rel=GetOpSignedValue(insn,i);
			return (unsigned int)((int)insn->address+(int)(insn->instruction.size)+(int)rel);
		default:
			break;
		}
	}

	return 0;
}

unsigned int GetImmediate(Instruction * insn,unsigned int idx)
{
	unsigned int i,curi;

	if(insn==0)
		return 0;

	curi=0;
	for(i=0;i<insn->instruction.operand_count;i++)
	{
		if(insn->instruction.operands[i].op.type==op_immediate)
		{
			if(idx==curi)
				return GetOpValue(insn,i);
			else
				curi++;
		}
	}
	
	return 0;
}

Instruction * ASMFindSized(ParsedFunction * parser,enum x86_insn_type tofind,unsigned int size,BOOL backwards)
{
	Instruction * curins;

	while(curins=ASMFind(parser,tofind,backwards))
	{
		if(curins->instruction.size==size)
			return curins;
	}

	return 0;
}

Instruction * ASMFindOperandByValue(ParsedFunction * parser,enum x86_op_datatype datatype,unsigned int data,BOOL backwards)
{
	Instruction * curins;
	unsigned int i;

	if(backwards==FALSE)
	{
		while(curins=ASMNext(parser))
		{
			for(i=0;i<curins->instruction.operand_count;i++)
			{
				if((curins->instruction.operands[i].op.datatype==datatype)&&(GetOpValue(curins,i)==data))
					return curins;
			}
		}
	}
	else
	{
		while(curins=ASMPrevious(parser))
		{
			for(i=0;i<curins->instruction.operand_count;i++)
			{
				if((curins->instruction.operands[i].op.datatype==datatype)&&(GetOpValue(curins,i)==data))
					return curins;
			}
		}
	}

	return 0;
}

Instruction * ASMFindOperandByType(ParsedFunction * parser,enum x86_op_type type,BOOL backwards)
{
	Instruction * curins;
	unsigned int i;

	if(backwards==FALSE)
	{
		while(curins=ASMNext(parser))
		{
			for(i=0;i<curins->instruction.operand_count;i++)
			{
				if(curins->instruction.operands[i].op.type==type)
					return curins;
			}
		}
	}
	else
	{
		while(curins=ASMPrevious(parser))
		{
			for(i=0;i<curins->instruction.operand_count;i++)
			{
				if(curins->instruction.operands[i].op.type==type)
					return curins;
			}
		}
	}

	return 0;
}

Instruction * ASMFindMultiple(ParsedFunction * parser,enum x86_insn_type * code,unsigned int instructioncount,BOOL backwards)
{
	unsigned int i;
	Instruction * startins, * curins;
	BOOL ok;

	while(startins=ASMFind(parser,code[0],backwards))
	{
		ok=TRUE;
		for(i=1;i<instructioncount;i++)
		{
			if((curins=ASMNext(parser))==0)
			{
				ok=FALSE;
				break;
			}
			if(curins->instruction.type!=code[i])
			{
				ok=FALSE;
				break;
			}
		}
		if(instructioncount>1)
		{
			while(ASMPrevious(parser)!=startins)
				;
		}
		if(ok)
			return startins;
	}

	return 0;
}

ParsedFunction * ASMParseLinear(unsigned int address)
{
	ParsedFunction * toreturn=NewParsedFunction();

	toreturn->pid=0;
	toreturn->linearsearch=TRUE;
	toreturn->startaddress=address;
	
	return toreturn;
}

Instruction * ASMLast(ParsedFunction * parser)
{
	BT_end(parser->btenum);
	BT_previous(parser->btenum);
	return (Instruction *)BT_next(parser->btenum);
}

void ASMFirst(ParsedFunction * parser)
{
	BT_reset(parser->btenum);
	return;
}

void ASMJumpToAddress(ParsedFunction * parser,unsigned int address)
{
	Instruction * curins;
	ASMFirst(parser);
	while(curins=ASMNext(parser))
	{
		if(curins->address==address)
		{
			ASMPrevious(parser);
			break;
		}
	}
	return;
}

ParsedFunction * FuncFind(ParsedFunction * func,unsigned int stacksize,BOOL backwards)
{
	ParsedFunction * subfunc;
	Instruction * curins, * retnins;
	unsigned int subaddress;

	while(curins=ASMFind(func,insn_call,backwards))
	{
		if(subaddress=ParseAddress(curins))
		{			
			subfunc=ASMParseFunction(subaddress,TRUE);
			if((retnins=ASMFind(subfunc,insn_return,FALSE))&&(GetImmediate(retnins,0)==stacksize))
			{
				return subfunc;
			}
			DeleteParsedFunction(subfunc);
		}
	}

	return 0;
}

ParsedFunction * RFuncFind(ParsedFunction * func,unsigned int stacksize,BOOL backwards)
{
	ParsedFunction * subfunc;
	Instruction * curins, * retnins;
	unsigned int subaddress;

	while(curins=ASMFind(func,insn_call,backwards))
	{
		if(subaddress=ParseAddress(curins))
		{			
			subfunc=RASMParseFunction(func->pid,subaddress,TRUE);
			if((retnins=ASMFind(subfunc,insn_return,FALSE))&&(GetImmediate(retnins,0)==stacksize))
			{
				return subfunc;
			}
			DeleteParsedFunction(subfunc);
		}
	}

	return 0;
}

ParsedFunction * RNewParsedFunction(unsigned int pid)
{
	ParsedFunction * toreturn=(ParsedFunction *)wAlloc(sizeof(ParsedFunction));

	toreturn->pid=pid;
	toreturn->btenum=0;
	toreturn->instructions=BT_create(instructioncompare);

	return toreturn;
}

ParsedFunction * RASMParseLinear(unsigned int pid,unsigned int address)
{
	ParsedFunction * toreturn=NewParsedFunction();

	toreturn->pid=pid;
	toreturn->linearsearch=TRUE;
	toreturn->startaddress=address;
	
	return toreturn;
}

Instruction * ASMParseInstruction(unsigned int pid, unsigned int address)
{
	Instruction * toreturn;
	char buf[1024];

	if(RRead(pid,address,buf,1024))
	{
		toreturn=(Instruction *)wAlloc(sizeof(Instruction));
		toreturn->address=address;
		if(x86_disasm(buf,1024,0,0,&(toreturn->instruction))>0)
			return toreturn;
		wFree(toreturn);
	}

	return 0;
}

ParsedFunction * RASMParseFunction(unsigned int pid,unsigned int address,BOOL allchunks)
{
	ParsedFunction * toreturn=NewParsedFunction();

	toreturn->pid=pid;
	toreturn->linearsearch=FALSE;
	toreturn->startaddress=address;

	ASMParseChunk(toreturn,address,allchunks);

	toreturn->btenum=BT_newenum(toreturn->instructions);
	
	return toreturn;
}