//libdisasm based asm callibration tools
#ifndef ASMPARSER_INCLUDED
#define ASMPARSER_INCLUDED

#include "ALLOCATION.h"

#include "libdisasm\libdis.h"

#include "BinaryTree.h"

#include "DebuggingTools.h"

typedef struct InstructionStruct
{
	unsigned int address;
	x86_insn_t instruction;
} Instruction;

typedef struct ASMParserStruct
{
	BOOL linearsearch;
	unsigned int pid;
	unsigned int startaddress;//enumeration starts at the lowest address, the start address is not necessarily the same one if the function contains chunks at lower addresses!!!
	BinaryTree * instructions;
	BinaryTreeEnum * btenum;
} ParsedFunction;

ParsedFunction * RNewParsedFunction(unsigned int pid);
ParsedFunction * RASMParseLinear(unsigned int pid,unsigned int address);
Instruction * ASMParseInstruction(unsigned int pid, unsigned int address);
ParsedFunction * RASMParseFunction(unsigned int pid,unsigned int address,BOOL allchunks);

ParsedFunction * NewParsedFunction();
void ResetParsedFunction(ParsedFunction * tocleanup);
void DeleteParsedFunction(ParsedFunction * todelete);

ParsedFunction * ASMParseLinear(unsigned int address);
//Instruction * ASMParseInstruction(unsigned int address);
ParsedFunction * ASMParseFunction(unsigned int address,BOOL allchunks);
Instruction * ASMFind(ParsedFunction * parser,enum x86_insn_type tofind,BOOL backwards);
Instruction * ASMNext(ParsedFunction * parser);
Instruction * ASMPrevious(ParsedFunction * parser);

void ASMJumpToAddress(ParsedFunction * parser,unsigned int address);

Instruction * ASMLast(ParsedFunction * parser);
void ASMFirst(ParsedFunction * parser);

Instruction * ASMFindFirst(ParsedFunction * parser,enum x86_insn_type * tofind,unsigned int count,BOOL backwards);

Instruction * ASMFindSized(ParsedFunction * parser,enum x86_insn_type tofind,unsigned int size,BOOL backwards);
Instruction * ASMFindOperandByValue(ParsedFunction * parser,enum x86_op_datatype datatype,unsigned int data,BOOL backwards);
Instruction * ASMFindOperandByType(ParsedFunction * parser,enum x86_op_type type,BOOL backwards);
Instruction * ASMFindMultiple(ParsedFunction * parser,enum x86_insn_type * code,unsigned int instructioncount,BOOL backwards);

Instruction * ASMFindOpCode(ParsedFunction * parser,BYTE tofind,BOOL backwards);

unsigned int GetOpValue(Instruction * insn,unsigned int idx);
unsigned int ParseAddress(Instruction * insn);
unsigned int GetImmediate(Instruction * insn,unsigned int idx);

int GetOpSignedValue(Instruction * insn,unsigned int idx);

//BOOL ASMParser_inits(char * libdisasmpath);
void ASMParser_inits();
void ASMParser_cleanup();

ParsedFunction * FuncFind(ParsedFunction * func,unsigned int stacksize,BOOL backwards);
ParsedFunction * RFuncFind(ParsedFunction * func,unsigned int stacksize,BOOL backwards);

#endif