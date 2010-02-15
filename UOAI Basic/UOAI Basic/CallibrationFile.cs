using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

using UOAIBasic;
using libdisasm;

namespace UOAIBasic
{
    public enum FilterType
    {
        Type,
        OpCount,
        OpType,
        OpDataType,
        OpData,
        Size,
        ExplicitOpCount,
        OpKnownData,
        FilterOr,
        OpDataNotEqual
    }

    public enum ActionType
    {
        DISASM_CHUNK,
        DISASM_FUNCTION,
        FIND_SEQUENCE,
        FOLLOW_CALL,
        JUMP_KNOWN,
        RETURN_ADDRESS,
        RETURN_DATA,
        RETURN_DISP,
        SET_BACKWARDS,
        TO_END,
        TO_START,
        RETURN_TARGETADDRESS,
        FUNC_FIND,
        SWITCH,
        CONDITIONAL,
        ASSIGN_VALUE,
        ASSIGN_KNOWN,
        CHECK_INSTRUCTION,
        EXECUTE_ACTIONSLIST,
        TRY_ACTIONLIST,
        FOLLOW_JMP,
        RETURN_SCALE,
        FIND_LOOP,
        RETURN_CHUNK_ADDRESS,
        MOVE_NEXT,
        MOVE_PREVIOUS
    }

    [Serializable()]
    public class Filter : ISerializable
    {
        public static BinaryTree<string, Filter> filters = new BinaryTree<string, Filter>();

        public FilterType type;
        public string m_name;
        public uint uintpar;
        public uint uintpar2;
        public FilterList filterlistpar;
        public uint enumpar;

        public string name { get { return m_name; } set { m_name = value; } }

        public Filter(string _name)
        {
            m_name = _name;
            filters.Add(m_name, this);
            FilterList fl=new FilterList(m_name);
            fl.filters.Add(this);
        }

        public static uint GetOpDataAsUInt(asmOperand fromop)
        {
            switch (fromop.DataType)
            {
                case x86_op_datatype.op_dword:
                    return fromop.Data.dword;
                case x86_op_datatype.op_word:
                    return (uint)fromop.Data.word;
                case x86_op_datatype.op_byte:
                    return (uint)fromop.Data._byte;
                default:
                    return (uint)fromop.Data.dword;
            }
        }

        private bool CheckOpData(asmInstruction tocheck, int opnumber, uint opdata, bool negate)
        {
            if ((opnumber < 0) || (opnumber >= tocheck.Operands.Count))
                return false;
            switch (tocheck.Operands[opnumber].Type)
            {
                case x86_op_type.op_immediate:
                case x86_op_type.op_absolute:
                    if(!negate)
                        return (GetOpDataAsUInt(tocheck.Operands[opnumber]) == opdata);
                    else
                        return !(GetOpDataAsUInt(tocheck.Operands[opnumber]) == opdata);
                case x86_op_type.op_expression:
                    if(!negate)
                        return (tocheck.Operands[opnumber].Data.expression.disp == opdata);
                    else
                        return !(tocheck.Operands[opnumber].Data.expression.disp == opdata);
                case x86_op_type.op_offset:
                    if (!negate)
                        return (tocheck.Operands[opnumber].Data.offset == opdata);
                    else
                        return !(tocheck.Operands[opnumber].Data.offset == opdata);
                case x86_op_type.op_relative_far:
                    if (!negate)
                        return (((uint)(((int)tocheck.Address+tocheck.Instruction.size+tocheck.Operands[opnumber].Data.relative_far))) == opdata);
                    else
                        return !(((uint)(((int)tocheck.Address + tocheck.Instruction.size + tocheck.Operands[opnumber].Data.relative_far))) == opdata);
                case x86_op_type.op_relative_near:
                    if (!negate)
                        return (((uint)(((int)tocheck.Address + tocheck.Instruction.size + tocheck.Operands[opnumber].Data.relative_near)) == opdata));
                    else
                        return !(((uint)(((int)tocheck.Address + tocheck.Instruction.size + tocheck.Operands[opnumber].Data.relative_near)) == opdata));
                default:
                    return false;
            }
        }

        public bool Check(asmInstruction tocompare, BinaryTree<uint, long> knowns)
        {
            switch (type)
            {
                case FilterType.OpKnownData:
                    if (knowns != null)
                    {
                        if (((int)uintpar) < tocompare.Operands.Count)
                        {
                            if ((knowns.ContainsKey(enumpar)) && CheckOpData(tocompare, (int)uintpar, (uint)knowns[enumpar], false))
                                return true;
                        }
                    }
                    break;
                case FilterType.FilterOr:
                    foreach (Filter f in filterlistpar.filters)
                    {
                        if (f.Check(tocompare, knowns))
                            return true;
                    }
                    break;
                case FilterType.ExplicitOpCount:
                        if (tocompare.Instruction.explicit_count == uintpar)
                            return true;
                    break;
                case FilterType.OpCount:
                    if (((int)uintpar) < tocompare.Operands.Count)
                    {
                        if (tocompare.Instruction.operand_count == uintpar)
                            return true;
                    }
                    break;
                case FilterType.OpData:
                    if (((int)uintpar) < tocompare.Operands.Count)
                    {
                        if (CheckOpData(tocompare, (int)uintpar, uintpar2, false))
                            return true;
                    }
                    break;
                case FilterType.OpDataType:
                    if (((int)uintpar) < tocompare.Operands.Count)
                    {
                        if (tocompare.Operands[(int)uintpar].DataType == (x86_op_datatype)enumpar)
                            return true;
                    }
                    break;
                case FilterType.OpType:
                    if (((int)uintpar) < tocompare.Operands.Count)
                    {
                        if (tocompare.Operands[(int)uintpar].Type == (x86_op_type)enumpar)
                            return true;
                    }
                    break;
                case FilterType.Size:
                    if (uintpar == tocompare.Instruction.size)
                        return true;
                    break;
                case FilterType.Type:
                    if (tocompare.Instruction.type == (x86_insn_type)enumpar)
                        return true;
                    break;
                case FilterType.OpDataNotEqual:
                    if (((int)uintpar) < tocompare.Operands.Count)
                    {
                        if (CheckOpData(tocompare, (int)uintpar, uintpar2, true))
                            return true;
                    }
                    break;
            }
            return false;
        }

        public Filter(SerializationInfo info, StreamingContext context)
        {
            m_name = (string)info.GetValue("Name", typeof(string));
            uintpar = (uint)info.GetValue("uintpar", typeof(uint));
            uintpar2 = (uint)info.GetValue("uintpar2", typeof(uint));
            type = (FilterType)(uint)info.GetValue("type", typeof(uint));
            filterlistpar = (FilterList)info.GetValue("filterlistpar", typeof(FilterList));
            enumpar = (uint)info.GetValue("enumpar", typeof(uint));
            filters.Add(name, this);
            FilterList fl = new FilterList(m_name);
            fl.filters.Add(this);
        }

        public void Update()
        {
            filters.Remove(name);
            filters.Add(name, this);
        }

        public override string ToString()
        {
            return name;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", name);
            info.AddValue("uintpar", uintpar);
            info.AddValue("uintpar2", uintpar2);
            info.AddValue("type", (uint)type);
            info.AddValue("filterlistpar", filterlistpar, typeof(FilterList));
            info.AddValue("enumpar", (uint)enumpar);
        }

        #endregion
    }

    [Serializable()]
    public class FilterList : ISerializable
    {
        public static BinaryTree<string, FilterList> filterlists = new BinaryTree<string, FilterList>();

        private string m_name;
        private List<Filter> m_filters = new List<Filter>();

        public string name { get { return m_name; } set { m_name = value; } }
        public List<Filter> filters { get { return m_filters; } }

        public bool Check(asmInstruction tocompare, BinaryTree<uint, long> knowns)
        {
            foreach (Filter f in filters)
            {
                if (!f.Check(tocompare, knowns))
                    return false;
            }
            return true;
        }

        public FilterList(string _name)
        {
            m_name = _name;
            filterlists.Add(name, this);
            CodeBlock cb = new CodeBlock(m_name);
            cb.FilterLists.Add(this);
        }

        public FilterList(SerializationInfo info, StreamingContext context)
        {
            m_name = (string)info.GetValue("Name", typeof(string));
            int count=(int)info.GetValue("Count", typeof(int));
            for (uint i = 0; i < count; i++)
                filters.Add((Filter)info.GetValue("Filter" + i.ToString(), typeof(Filter)));
            filterlists.Add(name, this);
            CodeBlock cb = new CodeBlock(m_name);
            cb.FilterLists.Add(this);
        }

        public void Update()
        {
            filterlists.Remove(name);
            filterlists.Add(name, this);
        }

        public override string ToString()
        {
            return name;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int i = 0;
            info.AddValue("Name", name);
            info.AddValue("Count", filters.Count);
            for (i = 0; i < filters.Count; i++)
                info.AddValue("Filter" + i.ToString(), filters[i], typeof(Filter));
        }

        #endregion
    }

    [Serializable()]
    public class CodeBlock : ISerializable
    {
        public static BinaryTree<string, CodeBlock> codeblocks = new BinaryTree<string, CodeBlock>();

        private string m_name;
        private List<FilterList> m_FilterLists = new List<FilterList>();

        public string name { get { return m_name; } set { m_name = value; } }
        public List<FilterList> FilterLists { get { return m_FilterLists; } }

        public CodeBlock(string _name)
        {
            m_name = _name;
            codeblocks.Add(name, this);
            Sequence seq = new Sequence(m_name);
            seq.CodeBlocks.Add(this);
        }

        public CodeBlock(SerializationInfo info, StreamingContext context)
        {
            m_name = (string)info.GetValue("Name", typeof(string));
            int count=(int)info.GetValue("Count", typeof(int));
            for (uint i = 0; i < count; i++)
                FilterLists.Add((FilterList)info.GetValue("FilterList" + i.ToString(), typeof(FilterList)));
            codeblocks.Add(name, this);
            Sequence seq = new Sequence(m_name);
            seq.CodeBlocks.Add(this);
        }

        public bool Find(asmChunk onchunk, bool backwards, BinaryTree<uint, long> knowns, out asmInstruction found)
        {
            bool _found = true;
            asmInstruction backup, fullbackup;
            
            fullbackup = onchunk.Current;
            found = null;
            while (onchunk.Current != null)
            {
                backup = onchunk.Current;
                _found = true;
                for(int i=0;i<FilterLists.Count;i++)
                {
                    if (!FilterLists[i].Check(onchunk.Current, knowns))
                    {
                        _found = false;
                        break;
                    }

                    if (i != (FilterLists.Count - 1))
                    {
                        onchunk.MoveNext();

                        if (onchunk.Current == null)
                        {
                            _found = false;
                            break;
                        }
                    }
                }
                
                onchunk.Current = backup;
                if (_found)
                {
                    found = onchunk.Current;
                    return true;
                }
                if (backwards)
                    onchunk.MovePrevious();
                else
                    onchunk.MoveNext();
            }
            onchunk.Current = fullbackup;
            return false;
        }

        public void Update()
        {
            codeblocks.Remove(name);
            codeblocks.Add(name, this);
        }

        public override string ToString()
        {
            return name;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int i = 0;
            info.AddValue("Name", name);
            info.AddValue("Count", FilterLists.Count);
            for (i = 0; i < FilterLists.Count; i++)
                info.AddValue("FilterList" + i.ToString(), FilterLists[i], typeof(FilterList));
        }

        #endregion
    }

    [Serializable()]
    public class Sequence : ISerializable
    {        
        public static BinaryTree<string, Sequence> sequences = new BinaryTree<string, Sequence>();

        private string m_name;
        private List<CodeBlock> m_CodeBlocks = new List<CodeBlock>();

        public string name { get { return m_name; } set { m_name = value; } }
        public List<CodeBlock> CodeBlocks { get { return m_CodeBlocks; } }

        public bool Find(asmChunk onchunk, bool backwards, BinaryTree<uint, long> knowns, out asmInstruction found)
        {
            found = null;
            for (int i = 0; i < CodeBlocks.Count; i++)
            {
                if (!CodeBlocks[i].Find(onchunk, backwards, knowns, out found))
                    return false;
                if (i != (CodeBlocks.Count - 1))
                {
                    if (backwards)
                        onchunk.MovePrevious();
                    else
                        onchunk.MoveNext();
                }
            }
            return true;
        }

        public Sequence(string _name)
        {
            m_name = _name;
            sequences.Add(name, this);
            Action a = new Action(m_name);
            a.m_type = ActionType.FIND_SEQUENCE;
            a.seqpar = this;
        }

        public Sequence(SerializationInfo info, StreamingContext context)
        {
            m_name = (string)info.GetValue("Name", typeof(string));
            uint count=(uint)info.GetValue("Count", typeof(uint));
            for (uint i = 0; i < count; i++)
                CodeBlocks.Add((CodeBlock)info.GetValue("CodeBlock" + i.ToString(), typeof(CodeBlock)));
            sequences.Add(name, this);
        }

        public void Update()
        {
            sequences.Remove(name);
            sequences.Insert(name, this);
        }

        public override string ToString()
        {
            return name;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int i = 0;
            info.AddValue("Name", name);
            info.AddValue("Count", CodeBlocks.Count);
            for (i = 0; i < CodeBlocks.Count; i++)
                info.AddValue("CodeBlock" + i.ToString(), CodeBlocks[i], typeof(CodeBlock));
        }

        #endregion

    }

    [Serializable()]
    public class Action : ISerializable
    {
        public static BinaryTree<string, Action> actions = new BinaryTree<string, Action>();
        public ActionType m_type;
        public uint uintpar;
        public bool boolpar;
        public UOCallibration.CallibratedFeatures knownpar;
        public UOCallibration.CallibratedFeatures knownpar2;
        public Sequence seqpar;

        //conditional stuff
        public ActionList actlist;//used as condition, but also for nested actionlist execution
        public ActionList ontrue;
        public ActionList onfalse;

        //current instruction verification
        public FilterList tocheck;

        public string name;

        public Action(string _name)
        {
            name = _name;
            actions.Add(name, this);
        }

        public Action(SerializationInfo info, StreamingContext context)
        {
            name=(string)info.GetValue("Name", typeof(string));
            boolpar=(bool)info.GetValue("boolpar", typeof(bool));
            uintpar=(uint)info.GetValue("uintpar", typeof(uint));
            knownpar = (UOCallibration.CallibratedFeatures)(uint)info.GetValue("knownpar", typeof(uint));
            knownpar2 = (UOCallibration.CallibratedFeatures)(uint)info.GetValue("knownpar2", typeof(uint));
            m_type=(ActionType)(uint)info.GetValue("type", typeof(uint));
            seqpar=(Sequence)info.GetValue("sequence", typeof(Sequence));
            actlist=(ActionList)info.GetValue("actlist", typeof(ActionList));
            ontrue=(ActionList)info.GetValue("ontrue", typeof(ActionList));
            onfalse=(ActionList)info.GetValue("onfalse", typeof(ActionList));
            tocheck=(FilterList)info.GetValue("tocheck", typeof(FilterList));
            actions.Add(name, this);
        }

        public void Update()
        {
            actions.Remove(name);
            actions.Add(name, this);
        }

        public override string ToString()
        {
            return name;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", name);
            info.AddValue("boolpar", this.boolpar);
            info.AddValue("uintpar", this.uintpar);
            info.AddValue("knownpar", (uint)this.knownpar);
            info.AddValue("knownpar2", (uint)this.knownpar2);
            info.AddValue("type", (uint)this.m_type);
            info.AddValue("sequence", this.seqpar,typeof(Sequence));
            info.AddValue("actlist", this.actlist, typeof(ActionList));
            info.AddValue("ontrue", this.ontrue, typeof(ActionList));
            info.AddValue("onfalse", this.onfalse, typeof(ActionList));
            info.AddValue("tocheck", this.tocheck, typeof(FilterList));
        }

        #endregion
    }

    [Serializable()]
    public class ActionList : ISerializable
    {
        public static BinaryTree<string, ActionList> actionlists = new BinaryTree<string, ActionList>();
        public string name;
        public List<Action> Actions = new List<Action>();

        private static uint StackSize(asmChunk tocheck)
        {
            asmOperand curop;
            asmInstruction found;
            tocheck.ToStart();
            if (tocheck.FindByType(x86_insn_type.insn_return, false, out found))
            {
                for (int i = 0; i < found.Operands.Count; i++)
                {
                    curop = found.Operands[i];
                    if (curop.Type == x86_op_type.op_immediate)
                    {
                        return (uint)curop.Data;
                    }
                }
            }
            return 0;
        }

        private static bool FuncFind(Stream onstream, asmChunk fromfunction, uint required_stacksize, bool backwards, out asmInstruction found)
        {
            asmChunk curfunc;
            found = null;
            while (fromfunction.FindByType(x86_insn_type.insn_call, backwards, out found))
            {
                onstream.Position = found.ReadAddressOperand();
                curfunc = disassembler.disassemble_chunk(onstream);
                if (StackSize(curfunc) == required_stacksize)
                    return true;
                fromfunction.MoveNext();
            }
            return false;
        }


        public bool ExecuteActionList(Stream onstream, BinaryTree<uint, long> knowns, ref asmChunk curChunk, ref asmInstruction curInstruction, Stack<string> ErrorStack)
        {
            bool backwards = false;

            Tools.StreamHandler sh = new Tools.StreamHandler(onstream);
            
            knowns.Add(0, onstream.Position);

            uint i = 0;

            //first action must be chunk or func parse
            foreach (Action currentaction in Actions)
            {
                switch (currentaction.m_type)
                {
                    case ActionType.DISASM_CHUNK:
                        if (!knowns.ContainsKey((uint)currentaction.knownpar))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Missing Dependency: " + currentaction.knownpar.ToString());
                            return false;
                        }
                        onstream.Position = knowns[(uint)currentaction.knownpar];
                        curChunk = disassembler.disassemble_chunk(onstream);
                        break;
                    case ActionType.DISASM_FUNCTION:
                        if (!knowns.ContainsKey((uint)currentaction.knownpar))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Missing Dependency: " + currentaction.knownpar.ToString());
                            return false;
                        }
                        onstream.Position = knowns[(uint)currentaction.knownpar];
                        curChunk = disassembler.disassemble_function(onstream);
                        break;
                    case ActionType.FIND_SEQUENCE:
                        if (curChunk == null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No disassembled code chunk available to search!");
                            return false;
                        }
                        if (!currentaction.seqpar.Find(curChunk, backwards, knowns, out curInstruction))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Failed to find sequence: " + currentaction.seqpar.name);
                            return false;
                        }
                        break;
                    case ActionType.FOLLOW_CALL:
                        if ((curInstruction == null) || (curInstruction.Instruction.type != x86_insn_type.insn_call))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Need a call instruction to follow!");
                            return false;
                        }
                        onstream.Position = curInstruction.ReadAddressOperand();
                        if (currentaction.boolpar)
                            curChunk = disassembler.disassemble_chunk(onstream);
                        else
                            curChunk = disassembler.disassemble_function(onstream);
                        break;
                    case ActionType.JUMP_KNOWN:
                        if (!knowns.ContainsKey((uint)currentaction.knownpar))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Missing Dependency: " + currentaction.knownpar.ToString());
                            return false;
                        }
                        knowns.Remove(0);
                        knowns.Add(0, knowns[(uint)currentaction.knownpar]);
                        onstream.Position = knowns[(uint)currentaction.knownpar];
                        break;
                    case ActionType.RETURN_ADDRESS:
                        if (curInstruction == null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No Current Instruction!");
                            return false;
                        }
                        knowns.Remove((uint)currentaction.knownpar);
                        knowns.Add((uint)currentaction.knownpar, curInstruction.Address);
                        break;
                    case ActionType.RETURN_DATA:
                        if ((curInstruction == null) || ((int)currentaction.uintpar > (curInstruction.Operands.Count - 1)))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No Suitable Current Instruction!");
                            return false;
                        }
                        knowns.Remove((uint)currentaction.knownpar);
                        if(curInstruction.Operands[(int)currentaction.uintpar].Type == x86_op_type.op_expression)
                            knowns.Add((uint)currentaction.knownpar, (long)curInstruction.Operands[(int)currentaction.uintpar].Data.expression.disp);
                        else if(curInstruction.Operands[(int)currentaction.uintpar].Type == x86_op_type.op_offset)
                            knowns.Add((uint)currentaction.knownpar, (long)curInstruction.Operands[(int)currentaction.uintpar].Data.offset);
                        else
                            knowns.Add((uint)currentaction.knownpar, (long)curInstruction.Operands[(int)currentaction.uintpar].Data.dword);
                        break;
                    case ActionType.RETURN_DISP:
                        if ((curInstruction == null) || ((int)currentaction.uintpar > (curInstruction.Operands.Count - 1)))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No Suitable Current Instruction!");
                            return false;
                        }
                        knowns.Remove((uint)currentaction.knownpar);
                        knowns.Add((uint)currentaction.knownpar, (long)curInstruction.Operands[(int)currentaction.uintpar].Data.expression.disp);
                        break;
                    case ActionType.SET_BACKWARDS:
                        if(backwards != currentaction.boolpar)
                        {
                            backwards = currentaction.boolpar;
                        }
                        break;
                    case ActionType.TO_END:
                        if(curChunk==null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No disassembled code chunk available!");
                            return false;
                        }
                        curChunk.ToEnd();
                        break;
                    case ActionType.TO_START:
                        if(curChunk==null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No disassembled code chunk available!");
                            return false;
                        }
                        curChunk.ToStart();
                        break;
                    case ActionType.RETURN_TARGETADDRESS:
                        if ((curInstruction == null) || ((int)currentaction.uintpar > (curInstruction.Operands.Count - 1)))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No Suitable Current Instruction!");
                            return false;
                        }
                        knowns.Remove((uint)currentaction.knownpar);
                        knowns.Add((uint)currentaction.knownpar, curInstruction.ReadAddressOperand());
                        break;
                    case ActionType.FUNC_FIND:
                        if (curChunk == null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No disassembled code chunk available!");
                            return false;
                        }
                        if (!FuncFind(onstream, curChunk, currentaction.uintpar, backwards, out curInstruction))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Failed to find stdcall function with stacksize 0x"+currentaction.uintpar.ToString("X")+"!");
                            return false;
                        }
                        break;
                    case ActionType.SWITCH:
                        if (!knowns.ContainsKey((uint)currentaction.knownpar))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Missing Dependency: " + currentaction.knownpar.ToString());
                            return false;
                        }
                        sh.Position=(knowns[(uint)currentaction.knownpar] + 4 * currentaction.uintpar);
                        onstream.Position = (long)sh.Read<uint>();
                        if (currentaction.boolpar)
                            curChunk = disassembler.disassemble_chunk(onstream);
                        else
                            curChunk = disassembler.disassemble_function(onstream);
                        break;
                    case ActionType.CONDITIONAL:
                        if (currentaction.actlist != null)
                        {
                            if (currentaction.actlist.ExecuteActionList(onstream, knowns,ref curChunk,ref curInstruction, new Stack<string>()))
                            {
                                if (currentaction.ontrue != null)
                                {
                                    if (!currentaction.ontrue.ExecuteActionList(onstream, knowns, ref curChunk, ref curInstruction, ErrorStack))
                                    {
                                        ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] OnTrue handler ("+currentaction.ontrue.name+") of Condition " + currentaction.actlist.ToString() + " failed!");
                                        return false;
                                    }
                                }                                    
                            }
                            else
                            {
                                if (currentaction.onfalse != null)
                                {
                                    if (!currentaction.onfalse.ExecuteActionList(onstream, knowns,ref curChunk,ref curInstruction, ErrorStack))
                                    {
                                        ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] OnFalse handler (" + currentaction.onfalse.name + ") of Condition " + currentaction.actlist.ToString() + " failed!");
                                        return false;
                                    }
                                }
                            }
                        }
                        break;
                    case ActionType.ASSIGN_VALUE:
                        if (knowns.ContainsKey((uint)currentaction.knownpar))
                            knowns.Remove((uint)currentaction.knownpar);
                        knowns.Add((uint)currentaction.knownpar, (long)currentaction.uintpar);
                        break;
                    case ActionType.ASSIGN_KNOWN:
                        if (knowns.ContainsKey((uint)currentaction.knownpar))
                            knowns.Remove((uint)currentaction.knownpar);
                        if (!knowns.ContainsKey((uint)currentaction.knownpar2))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Missing Dependency: " + currentaction.knownpar2.ToString());
                            return false;
                        }
                        knowns.Add((uint)currentaction.knownpar, knowns[(uint)currentaction.knownpar2]);
                        break;
                    case ActionType.CHECK_INSTRUCTION:
                        if (curInstruction == null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No Suitable Current Instruction!");
                            return false;
                        }
                        if (!currentaction.tocheck.Check(curInstruction, knowns))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Instruction verification failed!");
                            return false;
                        }
                        break;
                    case ActionType.EXECUTE_ACTIONSLIST:
                        if (!currentaction.actlist.ExecuteActionList(onstream, knowns,ref curChunk,ref curInstruction, ErrorStack))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] ActionList " + currentaction.actlist.name + " failed!");
                            return false;
                        }
                        break;
                    case ActionType.TRY_ACTIONLIST:
                        if (!currentaction.actlist.ExecuteActionList(onstream, knowns, ref curChunk, ref curInstruction, ErrorStack))
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] (Try) ActionList " + currentaction.actlist.name + " failed!");
                        break;
                    case ActionType.FOLLOW_JMP:
                        if ((curInstruction == null) || (!((curInstruction.Instruction.type == x86_insn_type.insn_jmp)||(curInstruction.Instruction.type == x86_insn_type.insn_jcc))))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No Suitable Current Instruction!");
                            return false;
                        }
                        onstream.Position = curInstruction.ReadAddressOperand();
                        if (currentaction.boolpar)
                            curChunk = disassembler.disassemble_chunk(onstream);
                        else
                            curChunk = disassembler.disassemble_function(onstream);
                        break;
                    case ActionType.RETURN_SCALE:
                        if ((curInstruction == null) || ((int)currentaction.uintpar > (curInstruction.Operands.Count - 1)))
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No Suitable Current Instruction!");
                            return false;
                        }
                        knowns.Add((uint)currentaction.knownpar, (long)curInstruction.Operands[(int)currentaction.uintpar].Data.expression.scale);
                        break;
                    case ActionType.FIND_LOOP:
                        if (curChunk != null)
                        {
                            if (!curChunk.FindLoop(out curInstruction))
                            {
                                ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] Failed to find a loop in the current code chunk!");
                                return false;
                            }
                        }
                        break;
                    case ActionType.RETURN_CHUNK_ADDRESS:
                        if (curChunk == null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No disassembled code chunk available!");
                            return false;
                        }
                        knowns.Add((uint)currentaction.knownpar, curChunk.Address);
                        break;
                    case ActionType.MOVE_NEXT:
                        if (curChunk == null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No disassembled code chunk available!");
                            return false;
                        }
                        curChunk.MoveNext();
                        break;
                    case ActionType.MOVE_PREVIOUS:
                        if (curChunk == null)
                        {
                            ErrorStack.Push("[#" + i.ToString() + " : "+currentaction.name+"] No disassembled code chunk available!");
                            return false;
                        }
                        curChunk.MovePrevious();
                        break;
                }
                i++;
            }
            return true;
        }

        public ActionList(string _name)
        {
            name = _name;
            actionlists.Add(name, this);
        }

        public ActionList(SerializationInfo info, StreamingContext context)
        {
            name=(string)info.GetValue("Name",typeof(string));
            int Count = (int)info.GetValue("Count", typeof(int));
            for (int i = 0; i < Count; i++)
                Actions.Add((Action)info.GetValue("Action" + i.ToString(), typeof(Action)));
            actionlists.Add(name, this);
        }

        public void Update()
        {
            actionlists.Remove(name);
            actionlists.Add(name, this);
        }

        public override string ToString()
        {
            return name;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int i = 0;
            info.AddValue("Name", name);
            info.AddValue("Count", Actions.Count);
            for (i = 0; i < Actions.Count; i++)
                info.AddValue("Action" + i.ToString(), Actions[i], typeof(Action));
        }

        #endregion
    }

    [Serializable()]
    public class CallibrationFile : ISerializable
    {
        public static CallibrationFile Load(string path)
        {
            CallibrationFile toreturn = null;
            if (File.Exists(path))
            {
                FileStream fs=File.OpenRead(path);
                SoapFormatter sf = new SoapFormatter();
                toreturn=(CallibrationFile)sf.Deserialize(fs);
                fs.Close();
            }
            return toreturn;
        }
        public static CallibrationFile Load(Stream fromstream)
        {
            CallibrationFile toreturn = null;
            SoapFormatter sf = new SoapFormatter();
            toreturn = (CallibrationFile)sf.Deserialize(fromstream);
            return toreturn;
        }

        public List<ActionList> actionlists=new List<ActionList>();

        public CallibrationFile()
        {
        }

        public CallibrationFile(SerializationInfo info, StreamingContext context)
        {
            int Count = (int)info.GetValue("Count", typeof(int));
            for (int i = 0; i < Count; i++)
                actionlists.Add((ActionList)info.GetValue("ActionList" + i.ToString(), typeof(ActionList)));
        }

        public void Save(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            FileStream fs = File.Create(path);
            SoapFormatter sf = new SoapFormatter();
            sf.Serialize(fs, this);
            fs.Close();
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int i=0;
            info.AddValue("Count", actionlists.Count);
            for(i=0;i<actionlists.Count;i++)
                info.AddValue("ActionList" + i.ToString(), actionlists[i], typeof(ActionList));
        }

        #endregion
    }
}
