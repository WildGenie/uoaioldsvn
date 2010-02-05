using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace libdisasm
{
    #region libdis.h

    /* ========================================= Error Reporting */
/* REPORT CODES
 *      These are passed to a reporter function passed at initialization.
 *      Each code determines the type of the argument passed to the reporter;
 *      this allows the report to recover from errors, or just log them.
 */
        public enum x86_report_codes
        {
            report_disasm_bounds,   /* RVA OUT OF BOUNDS : The disassembler could
                                   not disassemble the supplied RVA as it is
                                   out of the range of the buffer. The
                                   application should store the address and
                                   attempt to determine what section of the
                                   binary it is in, then disassemble the
                                   address from the bytes in that section.
                                        data: uint32_t rva */
            report_insn_bounds,     /* INSTRUCTION OUT OF BOUNDS: The disassembler
                                   could not disassemble the instruction as
                                   the instruction would require bytes beyond
                                   the end of the current buffer. This usually
                                   indicated garbage bytes at the end of a
                                   buffer, or an incorrectly-sized buffer.
                                        data: uint32_t rva */
            report_invalid_insn,    /* INVALID INSTRUCTION: The disassembler could
                                   not disassemble the instruction as it has an
                                   invalid combination of opcodes and operands.
                                   This will stop automated disassembly; the
                                   application can restart the disassembly
                                   after the invalid instruction.
                                        data: uint32_t rva */
            report_unknown
        }


    /* 'arg' is optional arbitrary data provided by the code passing the 
    *       callback -- for example, it could be 'this' or 'self' in OOP code.
    * 'code' is provided by libdisasm, it is one of the above
    * 'data' is provided by libdisasm and is context-specific, per the enums */
    public delegate void DISASM_REPORTER(x86_report_codes code, IntPtr data, IntPtr arg );

    public enum x86_reg_type
    {
        reg_gen = 0x00001,      /* general purpose */
        reg_in = 0x00002,      /* incoming args, ala RISC */
        reg_out = 0x00004,      /* args to calls, ala RISC */
        reg_local = 0x00008,      /* local vars, ala RISC */
        reg_fpu = 0x00010,      /* FPU data register */
        reg_seg = 0x00020,      /* segment register */
        reg_simd = 0x00040,      /* SIMD/MMX reg */
        reg_sys = 0x00080,      /* restricted/system register */
        reg_sp = 0x00100,      /* stack pointer */
        reg_fp = 0x00200,      /* frame pointer */
        reg_pc = 0x00400,      /* program counter */
        reg_retaddr = 0x00800,      /* return addr for func */
        reg_cond = 0x01000,      /* condition code / flags */
        reg_zero = 0x02000,      /* zero register, ala RISC */
        reg_ret = 0x04000,      /* return value */
        reg_src = 0x10000,      /* array/rep source */
        reg_dest = 0x20000,      /* array/rep destination */
        reg_count = 0x40000       /* array/rep/loop counter */
    }

    /* x86_reg_t : an X86 CPU register */
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 28)]
    public struct x86_reg_t
    {
        [MarshalAs(UnmanagedType.LPTStr, SizeConst = 8)]
        public string name;
        public x86_reg_type type;         /* what register is used for */
        public uint size;              /* size of register in bytes */
        public uint id;                /* register ID #, for quick compares */
        public uint alias;		/* ID of reg this is an alias for */
        public uint shift;		/* amount to shift aliased reg by */
    }

    /* x86_ea_t : an X86 effective address (address expression) */
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 66)]
    public struct x86_ea_t
    {
        public uint scale;         /* scale factor */
        public x86_reg_t indexregister;
        public x86_reg_t baseregister;   /* index, base registers */
        public int disp;          /* displacement */
        public char disp_sign;     /* is negative? 1/0 */
        public char disp_size;     /* 0, 1, 2, 4 */
    }

    /* x86_absolute_t : an X86 segment:offset address (descriptor) */
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 6)]
    public struct x86_absolute_t
    {
        public ushort segment;	/* loaded directly into CS */
        public ushort offsethigh;//offset union is translated
        public ushort offsetlow;//so that a 16 bit variable has the low part 0
    }

    public enum x86_op_type
    {      /* mutually exclusive */
        op_unused = 0,          /* empty/unused operand: should never occur */
        op_register = 1,        /* CPU register */
        op_immediate = 2,       /* Immediate Value */
        op_relative_near = 3,   /* Relative offset from IP */
        op_relative_far = 4,    /* Relative offset from IP */
        op_absolute = 5,        /* Absolute address (ptr16:32) */
        op_expression = 6,      /* Address expression (scale/index/base/disp) */
        op_offset = 7,          /* Offset from start of segment (m32) */
        op_unknown
    }

    /*#define x86_optype_is_address( optype ) \
        ( optype == op_absolute || optype == op_offset )
    #define x86_optype_is_relative( optype ) \
        ( optype == op_relative_near || optype == op_relative_far )
    #define x86_optype_is_memory( optype ) \
        ( optype > op_immediate && optype < op_unknown )*/

    public enum x86_op_datatype
    {          /* these use Intel's lame terminology */
        op_byte = 1,            /* 1 byte integer */
        op_word = 2,            /* 2 byte integer */
        op_dword = 3,           /* 4 byte integer */
        op_qword = 4,           /* 8 byte integer */
        op_dqword = 5,          /* 16 byte integer */
        op_sreal = 6,           /* 4 byte real (single real) */
        op_dreal = 7,           /* 8 byte real (double real) */
        op_extreal = 8,         /* 10 byte real (extended real) */
        op_bcd = 9,             /* 10 byte binary-coded decimal */
        op_ssimd = 10,          /* 16 byte : 4 packed single FP (SIMD, MMX) */
        op_dsimd = 11,          /* 16 byte : 2 packed double FP (SIMD, MMX) */
        op_sssimd = 12,         /* 4 byte : scalar single FP (SIMD, MMX) */
        op_sdsimd = 13,         /* 8 byte : scalar double FP (SIMD, MMX) */
        op_descr32 = 14,	/* 6 byte Intel descriptor 2:4 */
        op_descr16 = 15,	/* 4 byte Intel descriptor 2:2 */
        op_pdescr32 = 16,	/* 6 byte Intel pseudo-descriptor 32:16 */
        op_pdescr16 = 17,	/* 6 byte Intel pseudo-descriptor 8:24:16 */
        op_bounds16 = 18,	/* signed 16:16 lower:upper bounds */
        op_bounds32 = 19,	/* signed 32:32 lower:upper bounds */
        op_fpuenv16 = 20,	/* 14 byte FPU control/environment data */
        op_fpuenv32 = 21,	/* 28 byte FPU control/environment data */
        op_fpustate16 = 22,	/* 94 byte FPU state (env & reg stack) */
        op_fpustate32 = 23,	/* 108 byte FPU state (env & reg stack) */
        op_fpregset = 24,	/* 512 bytes: register set */
        op_fpreg = 25,		/* FPU register */
        op_none = 0xFF,     /* operand without a datatype (INVLPG) */
    }

    public enum x86_op_access
    {    /* ORed together */
        op_read = 1,
        op_write = 2,
        op_execute = 4
    }

    [Flags()]
    public enum x86_op_flags
    {     /* ORed together, but segs are mutually exclusive */
        op_signed = 1,          /* signed integer */
        op_string = 2,          /* possible string or array */
        op_constant = 4,        /* symbolic constant */
        op_pointer = 8,         /* operand points to a memory address */
        op_sysref = 0x010,	/* operand is a syscall number */
        op_implied = 0x020,	/* operand is implicit in the insn */
        op_hardcode = 0x40,	/* operand is hardcoded in insn definition */
        /* NOTE: an 'implied' operand is one which can be considered a side
         * effect of the insn, e.g. %esp being modified by PUSH or POP. A
         * 'hard-coded' operand is one which is specified in the instruction
         * definition, e.g. %es:%edi in MOVSB or 1 in ROL Eb, 1. The difference
         * is that hard-coded operands are printed by disassemblers and are
         * required to re-assemble, while implicit operands are invisible. */
        op_es_seg = 0x100,      /* ES segment override */
        op_cs_seg = 0x200,      /* CS segment override */
        op_ss_seg = 0x300,      /* SS segment override */
        op_ds_seg = 0x400,      /* DS segment override */
        op_fs_seg = 0x500,      /* FS segment override */
        op_gs_seg = 0x600       /* GS segment override */
    }

    /* x86_op_t : an X86 instruction operand */

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 86)]
    public struct x86_op_t
    {
        public x86_op_type type;           /* operand type */
        public x86_op_datatype datatype;       /* operand size */
        public x86_op_access access;         /* operand access [RWX] */
        public x86_op_flags flags;          /* misc flags */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 66)]
        public byte[] data;           /* this is needed to make formatting operands more sane */
        public IntPtr insn;		/* pointer to x86_insn_t owning operand */
    }

    /* Linked list of x86_op_t; provided for manual traversal of the operand
    * list in an insn. Users wishing to add operands to this list, e.g. to add
    * implicit operands, should use x86_operand_new in x86_operand_list.h */
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 90)]
    public struct x86_oplist_t
    {
        public x86_op_t op;
        public IntPtr next;
    }

    public enum x86_insn_group
    {
        insn_none = 0,		/* invalid instruction */
        insn_controlflow = 1,
        insn_arithmetic = 2,
        insn_logic = 3,
        insn_stack = 4,
        insn_comparison = 5,
        insn_move = 6,
        insn_string = 7,
        insn_bit_manip = 8,
        insn_flag_manip = 9,
        insn_fpu = 10,
        insn_interrupt = 13,
        insn_system = 14,
        insn_other = 15
    }

    public enum x86_insn_type
    {
        insn_invalid = 0,	/* invalid instruction */
        /* insn_controlflow */
        insn_jmp = 0x1001,
        insn_jcc = 0x1002,
        insn_call = 0x1003,
        insn_callcc = 0x1004,
        insn_return = 0x1005,
        /* insn_arithmetic */
        insn_add = 0x2001,
        insn_sub = 0x2002,
        insn_mul = 0x2003,
        insn_div = 0x2004,
        insn_inc = 0x2005,
        insn_dec = 0x2006,
        insn_shl = 0x2007,
        insn_shr = 0x2008,
        insn_rol = 0x2009,
        insn_ror = 0x200A,
        /* insn_logic */
        insn_and = 0x3001,
        insn_or = 0x3002,
        insn_xor = 0x3003,
        insn_not = 0x3004,
        insn_neg = 0x3005,
        /* insn_stack */
        insn_push = 0x4001,
        insn_pop = 0x4002,
        insn_pushregs = 0x4003,
        insn_popregs = 0x4004,
        insn_pushflags = 0x4005,
        insn_popflags = 0x4006,
        insn_enter = 0x4007,
        insn_leave = 0x4008,
        /* insn_comparison */
        insn_test = 0x5001,
        insn_cmp = 0x5002,
        /* insn_move */
        insn_mov = 0x6001,      /* move */
        insn_movcc = 0x6002,    /* conditional move */
        insn_xchg = 0x6003,     /* exchange */
        insn_xchgcc = 0x6004,   /* conditional exchange */
        /* insn_string */
        insn_strcmp = 0x7001,
        insn_strload = 0x7002,
        insn_strmov = 0x7003,
        insn_strstore = 0x7004,
        insn_translate = 0x7005,        /* xlat */
        /* insn_bit_manip */
        insn_bittest = 0x8001,
        insn_bitset = 0x8002,
        insn_bitclear = 0x8003,
        /* insn_flag_manip */
        insn_clear_carry = 0x9001,
        insn_clear_zero = 0x9002,
        insn_clear_oflow = 0x9003,
        insn_clear_dir = 0x9004,
        insn_clear_sign = 0x9005,
        insn_clear_parity = 0x9006,
        insn_set_carry = 0x9007,
        insn_set_zero = 0x9008,
        insn_set_oflow = 0x9009,
        insn_set_dir = 0x900A,
        insn_set_sign = 0x900B,
        insn_set_parity = 0x900C,
        insn_tog_carry = 0x9010,
        insn_tog_zero = 0x9020,
        insn_tog_oflow = 0x9030,
        insn_tog_dir = 0x9040,
        insn_tog_sign = 0x9050,
        insn_tog_parity = 0x9060,
        /* insn_fpu */
        insn_fmov = 0xA001,
        insn_fmovcc = 0xA002,
        insn_fneg = 0xA003,
        insn_fabs = 0xA004,
        insn_fadd = 0xA005,
        insn_fsub = 0xA006,
        insn_fmul = 0xA007,
        insn_fdiv = 0xA008,
        insn_fsqrt = 0xA009,
        insn_fcmp = 0xA00A,
        insn_fcos = 0xA00C,
        insn_fldpi = 0xA00D,
        insn_fldz = 0xA00E,
        insn_ftan = 0xA00F,
        insn_fsine = 0xA010,
        insn_fsys = 0xA020,
        /* insn_interrupt */
        insn_int = 0xD001,
        insn_intcc = 0xD002,    /* not present in x86 ISA */
        insn_iret = 0xD003,
        insn_bound = 0xD004,
        insn_debug = 0xD005,
        insn_trace = 0xD006,
        insn_invalid_op = 0xD007,
        insn_oflow = 0xD008,
        /* insn_system */
        insn_halt = 0xE001,
        insn_in = 0xE002,       /* input from port/bus */
        insn_out = 0xE003,      /* output to port/bus */
        insn_cpuid = 0xE004,
        /* insn_other */
        insn_nop = 0xF001,
        insn_bcdconv = 0xF002,  /* convert to or from BCD */
        insn_szconv = 0xF003    /* change size of operand */
    }

    /* These flags specify special characteristics of the instruction, such as
    * whether the inatruction is privileged or whether it serializes the
    * pipeline.
    * NOTE : These may not be accurate for all instructions; updates to the
    * opcode tables have not been completed. */
    public enum x86_insn_note
    {
        insn_note_ring0 = 1,	/* Only available in ring 0 */
        insn_note_smm = 2,	/* "" in System Management Mode */
        insn_note_serial = 4,	/* Serializing instruction */
        insn_note_nonswap = 8,	/* Does not swap arguments in att-style formatting */
        insn_note_nosuffix = 16,	/* Does not have size suffix in att-style formatting */
    }

    /* This specifies what effects the instruction has on the %eflags register */
    public enum x86_flag_status
    {
        insn_carry_set = 0x1,			/* CF */
        insn_zero_set = 0x2,			/* ZF */
        insn_oflow_set = 0x4,			/* OF */
        insn_dir_set = 0x8,			/* DF */
        insn_sign_set = 0x10,			/* SF */
        insn_parity_set = 0x20,			/* PF */
        insn_carry_or_zero_set = 0x40,
        insn_zero_set_or_sign_ne_oflow = 0x80,
        insn_carry_clear = 0x100,
        insn_zero_clear = 0x200,
        insn_oflow_clear = 0x400,
        insn_dir_clear = 0x800,
        insn_sign_clear = 0x1000,
        insn_parity_clear = 0x2000,
        insn_sign_eq_oflow = 0x4000,
        insn_sign_ne_oflow = 0x8000
    }

    /* The CPU model in which the insturction first appeared; this can be used
    * to mask out instructions appearing in earlier or later models or to
    * check the portability of a binary.
    * NOTE : These may not be accurate for all instructions; updates to the
    * opcode tables have not been completed. */
    public enum x86_insn_cpu
    {
        cpu_8086 = 1,	/* Intel */
        cpu_80286 = 2,
        cpu_80386 = 3,
        cpu_80387 = 4,
        cpu_80486 = 5,
        cpu_pentium = 6,
        cpu_pentiumpro = 7,
        cpu_pentium2 = 8,
        cpu_pentium3 = 9,
        cpu_pentium4 = 10,
        cpu_k6 = 16,	/* AMD */
        cpu_k7 = 32,
        cpu_athlon = 48
    }

    /* CPU ISA subsets: These are derived from the Instruction Groups in
    * Intel Vol 1 Chapter 5; they represent subsets of the IA32 ISA but
    * do not reflect the 'type' of the instruction in the same way that
    * x86_insn_group does. In short, these are AMD/Intel's somewhat useless 
    * designations.
    * NOTE : These may not be accurate for all instructions; updates to the
    * opcode tables have not been completed. */
    public enum x86_insn_isa
    {
        isa_gp = 1,	/* general purpose */
        isa_fp = 2,	/* floating point */
        isa_fpumgt = 3,	/* FPU/SIMD management */
        isa_mmx = 4,	/* Intel MMX */
        isa_sse1 = 5,	/* Intel SSE SIMD */
        isa_sse2 = 6,	/* Intel SSE2 SIMD */
        isa_sse3 = 7,	/* Intel SSE3 SIMD */
        isa_3dnow = 8,	/* AMD 3DNow! SIMD */
        isa_sys = 9	/* system instructions */
    }

    public enum x86_insn_prefix
    {
        insn_no_prefix = 0,
        insn_rep_zero = 1,	/* REPZ and REPE */
        insn_rep_notzero = 2,	/* REPNZ and REPNZ */
        insn_lock = 4		/* LOCK: */
    }

    /* x86_insn_t : an X86 instruction */
    [StructLayout(LayoutKind.Sequential, Size = 140, Pack = 1, CharSet = CharSet.Ansi)]
    public struct x86_insn_t
    {
        /* information about the instruction */
        [MarshalAs(UnmanagedType.U4)]
        public uint addr;             /* load address */
        [MarshalAs(UnmanagedType.U4)]
        public uint offset;           /* offset into file/buffer */
        [MarshalAs(UnmanagedType.U4)]
        public x86_insn_group group;      /* meta-type, e.g. INS_EXEC */
        [MarshalAs(UnmanagedType.U4)]
        public x86_insn_type type;        /* type, e.g. INS_BRANCH */
        [MarshalAs(UnmanagedType.U4)]
        public x86_insn_note note;	/* note, e.g. RING0 */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] bytes;
        [MarshalAs(UnmanagedType.U1)]
        public byte size;             /* size of insn in bytes */
        /* 16/32-bit mode settings */
        [MarshalAs(UnmanagedType.U1)]
        public byte addr_size;	/* default address size : 2 or 4 */
        [MarshalAs(UnmanagedType.U1)]
        public byte op_size;		/* default operand size : 2 or 4 */
        /* CPU/instruction set */
        [MarshalAs(UnmanagedType.U4)]
        public x86_insn_cpu cpu;
        [MarshalAs(UnmanagedType.U4)]
        public x86_insn_isa isa;
        /* flags */
        [MarshalAs(UnmanagedType.U4)]
        public x86_flag_status flags_set; /* flags set or tested by insn */
        [MarshalAs(UnmanagedType.U4)]
        public x86_flag_status flags_tested;
        /* stack */
        [MarshalAs(UnmanagedType.U1)]
        public byte stack_mod;	/* 0 or 1 : is the stack modified? */
        [MarshalAs(UnmanagedType.I4)]
        public int stack_mod_val;		/* val stack is modified by if known */
        /* the instruction proper */
        [MarshalAs(UnmanagedType.U4)]
        public x86_insn_prefix prefix;	/* prefixes ORed together */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string prefix_string; /* prefixes [might be truncated] */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string mnemonic;
        public IntPtr operands;		/* list of explicit/implicit operands */
        [MarshalAs(UnmanagedType.U4)]
        public uint operand_count;		/* total number of operands */
        [MarshalAs(UnmanagedType.U4)]
        public uint explicit_count;		/* number of explicit operands */
        /* convenience fields for user */
        public IntPtr block;                    /* code block containing this insn */
        public IntPtr function;                 /* function containing this insn */
        public int tag;			/* tag the insn as seen/processed */
    }

    public enum x86_options
    {		/* these can be ORed together */
        opt_none = 0,
        opt_ignore_nulls = 1,     /* ignore sequences of > 4 NULL bytes */
        opt_16_bit = 2,           /* 16-bit/DOS disassembly */
        opt_att_mnemonics = 4,    /* use AT&T syntax names for alternate opcode mnemonics */
    }

    public enum x86_asm_format
    {
        unknown_syntax = 0,		/* never use! */
        native_syntax, 			/* header: 35 bytes */
        intel_syntax, 			/* header: 23 bytes */
        att_syntax,  			/* header: 23 bytes */
        xml_syntax,			/* header: 679 bytes */
        raw_syntax			/* header: 172 bytes */
    };
    #endregion

    public class asmOperand
    {
        private x86_op_t m_op;

        public asmOperand(x86_op_t fromop)
        {
            m_op = fromop;
        }

        public x86_op_access Access { get { return m_op.access; } }
        public x86_op_flags Flags { get { return m_op.flags; } }
        public x86_op_type Type { get { return m_op.type; } }
        public x86_op_datatype DataType { get { return m_op.datatype; } }
        public asmDataHandler Data { get { return new asmDataHandler(m_op.data); } }
    }

    public class asmInstruction
    {
        private x86_insn_t m_insn;
        private List<asmOperand> m_Operands;
        private long m_addr;

        public asmInstruction(x86_insn_t frominsn, long addr)
        {
            x86_oplist_t curop;
            m_insn = frominsn;
            m_addr = addr;

            //build operand list
            m_Operands = new List<asmOperand>((int)m_insn.operand_count);
            if (m_insn.operand_count != 0)
            {
                curop = (x86_oplist_t)Marshal.PtrToStructure(frominsn.operands, typeof(x86_oplist_t));
                for (uint i = 0; i < m_insn.operand_count; i++)
                {
                    m_Operands.Add(new asmOperand(curop.op));
                    if (i < (m_insn.operand_count - 1))
                        curop = (x86_oplist_t)Marshal.PtrToStructure(curop.next, typeof(x86_oplist_t));
                }
            }
        }
        public List<asmOperand> Operands
        {
            get { return m_Operands; }
        }

        public long ReadAddressOperand()
        {
            foreach (asmOperand curop in Operands)
            {
                if ((curop.Type & x86_op_type.op_relative_far)!=0)
                    return m_addr + m_insn.size + curop.Data.relative_far;
                else if ((curop.Type & x86_op_type.op_relative_near)!=0)
                    return m_addr + m_insn.size + curop.Data.relative_near;
                else if ((curop.Type & x86_op_type.op_offset)!=0)
                    return curop.Data.offset;
            }
            
            return 0;
        }

        public x86_insn_t Instruction { get { return m_insn; } }

        public long Address { get { return m_addr; } }
    }

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
        FilterOr
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
        SET_BACKWARDS
    }

    public class asmChunk
    {
        private BinaryTree<long, asmInstruction> m_Instructions;
        private long m_Addr;
        private asmInstruction m_Current;

        public static bool ExecuteActionList(Stream onstream, long startposition, object[] ActionList, BinaryTree<uint, long> results, BinaryTree<uint, long> knowns)
        {
            asmChunk curChunk=null;
            asmInstruction curInstruction=null;
            object[] cursequence;
            long longpar;
            uint uintpar;
            int intpar;
            object[] currentaction;
            ActionType currentactiontype;
            bool backwards = false;
            bool boolpar;
            knowns.Add(0, startposition);
            //first action must be chunk or func parse
            for (uint i = 0; i < ActionList.Length; i++)
            {
                currentaction = (object[])ActionList[i];
                currentactiontype = (ActionType)currentaction[0];
                switch (currentactiontype)
                {
                    case ActionType.DISASM_CHUNK:
                        uintpar = (uint)currentaction[1];
                        if(!knowns.ContainsKey(uintpar))
                            return false;
                        longpar = knowns[uintpar];
                        onstream.Position = longpar;
                        curChunk = disassembler.disassemble_chunk(onstream);
                        break;
                    case ActionType.DISASM_FUNCTION:
                        uintpar = (uint)currentaction[1];
                        if (!knowns.ContainsKey(uintpar))
                            return false;
                        longpar = knowns[uintpar];
                        onstream.Position = longpar;
                        curChunk = disassembler.disassemble_function(onstream);
                        break;
                    case ActionType.FIND_SEQUENCE:
                        if (curChunk == null)
                            return false;
                        cursequence = (object[])currentaction[1];
                        if (!curChunk.FindSequence(cursequence, backwards, out curInstruction, knowns))
                            return false;
                        break;
                    case ActionType.FOLLOW_CALL:
                        boolpar=(bool)currentaction[1];
                        if((curInstruction==null)||(curInstruction.Instruction.type != x86_insn_type.insn_call))
                            return false;
                        onstream.Position = curInstruction.ReadAddressOperand();
                        if (boolpar)
                            curChunk = disassembler.disassemble_chunk(onstream);
                        else
                            curChunk = disassembler.disassemble_function(onstream);
                        break;
                    case ActionType.JUMP_KNOWN:
                        uintpar = (uint)currentaction[1];
                        if (!knowns.Find(uintpar, out longpar))
                            return false;
                        knowns.Remove(0);
                        knowns.Add(0, longpar);
                        break;
                    case ActionType.RETURN_ADDRESS:
                        uintpar = (uint)currentaction[1];
                        if (curInstruction == null)
                            return false;
                        knowns.Add(uintpar, curInstruction.Address);
                        break;
                    case ActionType.RETURN_DATA:
                        uintpar = (uint)currentaction[1];
                        intpar = (int)currentaction[2];
                        if ((curInstruction == null)||(intpar>(curInstruction.Operands.Count-1)))
                            return false;
                        knowns.Add(uintpar, (long)curInstruction.Operands[intpar].Data.dword);
                        break;
                    case ActionType.RETURN_DISP:
                        uintpar = (uint)currentaction[1];
                        intpar = (int)currentaction[2];
                        if ((curInstruction == null) || (intpar > (curInstruction.Operands.Count - 1)))
                            return false;
                        knowns.Add(uintpar, (long)curInstruction.Operands[intpar].Data.expression.disp);
                        break;
                    case ActionType.SET_BACKWARDS:
                        backwards = (bool)currentaction[1];
                        break;
                }
            }
            return true;
        }

        public asmInstruction this[long address]
        {
            get { return m_Instructions[address]; }
        }

        public asmChunk(long position, BinaryTree<long, asmInstruction> instructions)
        {
            m_Instructions = instructions;
            m_Addr = position;
            m_Current = m_Instructions[position];
        }
        public long Address { get { return m_Addr; } }
        public asmInstruction Current
        {
            get { return m_Current; }
            set
            {
                if (m_Instructions.ContainsKey(value.Address))
                    m_Current = value;
            }
        }
        public asmInstruction Next
        {
            get
            {
                if (m_Current != null)
                    m_Current = m_Instructions.Next(m_Current.Address);
                return m_Current;
            }
        }
        public asmInstruction MoveNext()
        {
            return Next;
        }
        public asmInstruction Previous
        {
            get
            {
                if (m_Current != null)
                    m_Current = m_Instructions.Previous(m_Current.Address);
                return m_Current;
            }
        }
        public asmInstruction MovePrevious()
        {
            return Previous;
        }
        public asmInstruction ToStart()
        {
            return (m_Current = m_Instructions.leftmost().value);
        }
        public asmInstruction ToEnd()
        {
            return (m_Current=m_Instructions.rightmost().value);
        }

        //search functions go here
        private bool CheckFilter(object[] curfilter, asmInstruction tocompare, BinaryTree<uint, long> knowns)
        {
            int opnumber;
            uint knownnumber;
            FilterType curfiltertype;

            curfiltertype = (FilterType)curfilter[0];
            switch (curfiltertype)
            {
                case FilterType.OpKnownData:
                    if (knowns != null)
                    {
                        opnumber = (int)curfilter[1];
                        knownnumber = (uint)curfilter[2];
                        if ((knowns.ContainsKey(knownnumber)) && (knowns[knownnumber] == tocompare.Operands[opnumber].Data.dword))
                            return true;
                    }
                    break;
                case FilterType.FilterOr:
                    object[] orlist = (object[]) curfilter[1];
                    for (uint i = 0; i < orlist.Length; i++)
                    {
                        if (CheckFilter((object[])orlist[i], tocompare, knowns))
                            return true;
                    }
                    break;
                case FilterType.ExplicitOpCount:
                    if (tocompare.Instruction.explicit_count == (uint)curfilter[1])
                        return true;
                    break;
                case FilterType.OpCount:
                    if (tocompare.Instruction.operand_count == (uint)curfilter[1])
                        return true;
                    break;
                case FilterType.OpData:
                    opnumber = (int)curfilter[1];
                    if (tocompare.Operands[opnumber].Data.dword == (uint)curfilter[2])
                        return true;
                    break;
                case FilterType.OpDataType:
                    opnumber = (int)curfilter[1];
                    if (tocompare.Operands[opnumber].DataType == (x86_op_datatype)curfilter[2])
                        return true;
                    break;
                case FilterType.OpType:
                    opnumber = (int)curfilter[1];
                    if (tocompare.Operands[opnumber].Type == (x86_op_type)curfilter[2])
                        return true;
                    break;
                case FilterType.Size:
                    if (tocompare.Instruction.size == (byte)curfilter[1])
                        return true;
                    break;
                case FilterType.Type:
                    if (tocompare.Instruction.type == (x86_insn_type)curfilter[1])
                        return true;
                    break;
            }
            return false;
        }

        private bool FilterListCompare(object[] insnfilterlist, asmInstruction tocompare, BinaryTree<uint, long> knowns)
        {
            for (uint i = 0; i < insnfilterlist.Length; i++)
            {
                if (!CheckFilter((object[])insnfilterlist[i], tocompare, knowns))
                    return false;
            }
            return true;
        }

        public bool FindFilter(object[] filter, bool backwards, out asmInstruction found, BinaryTree<uint, long> knowns)
        {
            bool _found = true;
            asmInstruction backup, fullbackup;
            fullbackup = m_Current;
            found = null;
            while (m_Current != null)
            {
                backup = m_Current;
                _found = true;
                for (uint i = 0; i < filter.Length; i++)
                {
                    if (!FilterListCompare((object[])filter[i], m_Current, knowns))
                    {
                        _found = false;
                        break;
                    }

                    /*if (backwards)
                        m_Current = Previous;
                    else*/
                    //always next otherwise the instruction list has to be passed reversed
                    m_Current = Next;

                    if (m_Current == null)
                    {
                        _found = false;
                        break;
                    }
                }
                m_Current = backup;
                if (_found)
                {
                    found = m_Current;
                    return true;
                }
                if (backwards)
                    m_Current = Previous;
                else
                    m_Current = Next;
            }
            m_Current = fullbackup;
            return false;
        }

        public bool FindSequence(object[] sequence, bool backwards, out asmInstruction found, BinaryTree<uint, long> knowns)
        {
            found=null;
            for (uint i = 0; i < sequence.Length; i++)
            {
                if (!FindFilter((object[])sequence[i], backwards, out found, knowns))
                    return false;
                if(i!=(sequence.Length-1))
                    MoveNext(); 
            }
            return true;
        }

        public bool FindByType(x86_insn_type type, bool backwards, out asmInstruction found)
        {
            asmInstruction backup = m_Current;
            found = null;
            while(m_Current!=null)
            {
                if(m_Current.Instruction.type == type)
                {
                    found = m_Current;
                    return true;
                }
                if (backwards)
                    m_Current = Previous;
                else
                    m_Current=Next;
            }
            m_Current = backup;
            return false;
        }
        public bool FindSized(x86_insn_type type, byte required_size, bool backwards, out asmInstruction found)
        {
            asmInstruction backup = m_Current;
            found = null;
            while (m_Current != null)
            {
                if ((m_Current.Instruction.type == type)&&(m_Current.Instruction.size==required_size))
                {
                    found = m_Current;
                    return true;
                }
                if (backwards)
                    m_Current = Previous;
                else
                    m_Current = Next;
            }
            m_Current = backup;
            return false;
        }
        public bool FindByOperandValue(asmDataHandler value, bool backwards, out asmInstruction found)
        {
            asmInstruction backup = m_Current;
            found = null;
            while (m_Current != null)
            {
                foreach (asmOperand op in m_Current.Operands)
                {
                    if (op.Data == value)
                    {
                        found = m_Current;
                        return true;
                    }
                }
                if (backwards)
                    m_Current = Previous;
                else
                    m_Current = Next;
            }
            m_Current = backup;
            return false;
        }
        public bool FindMultiple(x86_insn_type[] types, bool backwards, out asmInstruction found)
        {
            bool _found = true;
            asmInstruction backup, fullbackup;
            fullbackup = m_Current;
            found = null;
            while (m_Current != null)
            {
                backup = m_Current;
                _found = true;
                for (uint i = 0; i < types.Length; i++)
                {
                    if (m_Current.Instruction.type != types[i])
                    {
                        _found = false;
                        break;
                    }
                    
                    /*if (backwards)
                        m_Current = Previous;
                    else*///always next otherwise the instruction list has to be passed reversed
                        m_Current = Next;
                    
                    if (m_Current== null)
                    {
                        _found = false;
                        break;
                    }
                }
                m_Current = backup;
                if (_found)
                {
                    found = m_Current;
                    return true;
                }
                if (backwards)
                    m_Current = Previous;
                else
                    m_Current = Next;
            }
            m_Current = fullbackup;
            return false;
        }
        public bool FindByOperandType(x86_op_type x86_op_type, bool backwards, out asmInstruction found)
        {
            asmInstruction backup = m_Current;
            found = null;
            while (m_Current != null)
            {
                foreach (asmOperand op in m_Current.Operands)
                {
                    if (op.Type == x86_op_type)
                    {
                        found = m_Current;
                        return true;
                    }
                }
                if (backwards)
                    m_Current = Previous;
                else
                    m_Current = Next;
            }
            m_Current = backup;
            return false;
        }
    }

    public class asmDataHandler
    {
        byte[] m_RawData;

        private object RawDeserialize(Type anyType)
        {
            int rawsize = Marshal.SizeOf(anyType);
            if (rawsize > m_RawData.Length)
                return null;
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(m_RawData, 0, buffer, rawsize);
            object retobj = Marshal.PtrToStructure(buffer, anyType);
            Marshal.FreeHGlobal(buffer);
            return retobj;
        }

        public asmDataHandler(byte[] frombytes)
        {
            m_RawData = frombytes;
        }
        public sbyte _sbyte { get { return (sbyte)m_RawData[0]; } }
        public short sword { get { return BitConverter.ToInt16(m_RawData, 0); } }
        public int sdword { get { return BitConverter.ToInt32(m_RawData, 0); } }
        public Int64 sqword { get { return BitConverter.ToInt64(m_RawData, 0); } }
        public byte _byte { get { return m_RawData[0]; } }
        public ushort word { get { return BitConverter.ToUInt16(m_RawData, 0); } }
        public uint dword { get { return BitConverter.ToUInt32(m_RawData, 0); } }
        public UInt64 qword { get { return BitConverter.ToUInt64(m_RawData, 0); } }
        public double sreal { get { return BitConverter.ToDouble(m_RawData, 0); } }
        public double dreal { get { return BitConverter.ToDouble(m_RawData, 0); } }
        //to be added: public byte[] extreal
        //to be added: public byte[] bcd;
        //to be added: public UInt64 dqword;
        //to be added: public byte[] simd;
        //to be added: public byte[] fpuenv;
        public uint offset { get { return BitConverter.ToUInt32(m_RawData, 0); } }
        public x86_reg_t reg { get { return (x86_reg_t)RawDeserialize(typeof(x86_reg_t)); } }
        public sbyte relative_near { get { return _sbyte; } }
        public int relative_far { get { return sdword; } }
        public x86_absolute_t absolute { get { return (x86_absolute_t)RawDeserialize(typeof(x86_absolute_t)); } }
        public x86_ea_t expression { get { return (x86_ea_t)RawDeserialize(typeof(x86_ea_t)); } }
        public static bool operator ==(asmDataHandler a, asmDataHandler b)
        {
            if (a.m_RawData.Length != b.m_RawData.Length)
                return false;
            else
            {
                for (int i = 0; i < a.m_RawData.Length; i++)
                {
                    if (a.m_RawData[i] != b.m_RawData[i])
                        return false;
                }
                return true;
            }
        }
        public static bool operator !=(asmDataHandler a, asmDataHandler b)
        {
            if (a.m_RawData.Length != b.m_RawData.Length)
                return true;
            else
            {
                for (int i = 0; i < a.m_RawData.Length; i++)
                {
                    if (a.m_RawData[i] != b.m_RawData[i])
                        return true;
                }
                return false;
            }
        }

        static public explicit operator uint(asmDataHandler adh)
        {
            return adh.dword;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
    
    //wraps libdisasm imports
    public class disassembler
    {
        private static libdisasmDestructor m_Destructor = new libdisasmDestructor();

        [DllImport("libdisasm.dll")]
        internal static extern int x86_init(x86_options options, DISASM_REPORTER reporter, IntPtr arg);
        [DllImport("libdisasm.dll")]
        internal static extern uint x86_disasm([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 1)] byte[] buf, [MarshalAs(UnmanagedType.I4)] int buf_len, [MarshalAs(UnmanagedType.U4)] uint buf_rva, [MarshalAs(UnmanagedType.U4)] uint offset, out x86_insn_t insn);
        [DllImport("libdisasm.dll")]
        internal static extern int x86_format_insn(ref x86_insn_t insn, ref byte[] buf, int len, x86_asm_format format);
        [DllImport("libdisasm.dll")]
        internal static extern int x86_cleanup();

        #region constructor/destructor
        static disassembler()
        {
            //initiallize
            x86_init(x86_options.opt_none, null, IntPtr.Zero);
        }

        //ensure static destruction through this dummy class
        private class libdisasmDestructor
        {
            public libdisasmDestructor()
            {
            }

            ~libdisasmDestructor()
            {
                disassembler.x86_cleanup();
            }
        }
        #endregion

        //Disassembles 1 Instruction from the specified buffer
        public static asmInstruction disassemble(byte[] buffer)
        {
            x86_insn_t insn;

            if (x86_disasm(buffer, buffer.Length, 0, 0, out insn) > 0)
                return new asmInstruction(insn, 0);//dummy
            else
                return null;
        }

        public static asmInstruction disassemble(Stream fromstream)
        {
            long m_position = fromstream.Position;
            x86_insn_t insn;
            byte[] buff = new byte[32];
            fromstream.Read(buff, 0, 32);
            if (x86_disasm(buff, 32, 0, 0, out insn) > 0)
            {
                fromstream.Position -= 32 - insn.size;
                return new asmInstruction(insn, m_position);
            }
            else
            {
                fromstream.Position -= 32;
                return null;
            }
        }

        public static asmChunk disassemble_chunk(Stream fromstream)
        {
            return disassemble_chunk(fromstream, null, null);
        }

        private static asmChunk disassemble_chunk(Stream fromstream, Stack<long> chunkstack, BinaryTree<long, asmInstruction> function_Instructions )
        {
            long position = fromstream.Position;
            BinaryTree<long, asmInstruction> instructions;
            asmInstruction curinsn;

            instructions = new BinaryTree<long, asmInstruction>();

            while ((curinsn = disassemble(fromstream)) != null)
            {
                instructions.Add(curinsn.Address, curinsn);
                
                if (function_Instructions != null)
                    function_Instructions.Add(curinsn.Address, curinsn);
                
                if (curinsn.Instruction.type == x86_insn_type.insn_jmp)
                {
                    if(chunkstack!=null)                        
                        chunkstack.Push(curinsn.ReadAddressOperand());
                    break;
                }
                else if (curinsn.Instruction.type == x86_insn_type.insn_jcc)
                {
                    if(chunkstack!=null)                        
                        chunkstack.Push(curinsn.ReadAddressOperand());
                }
                else if (curinsn.Instruction.type == x86_insn_type.insn_return)
                    break;
            }
            return new asmChunk(position, instructions);
        }

        public static asmChunk disassemble_function(Stream fromstream)
        {
            BinaryTree<long, asmInstruction> chunked_instructions = new BinaryTree<long, asmInstruction>();
            Stack<long> todisassemble=new Stack<long>();
            long curpos;
            long position = fromstream.Position;
            todisassemble.Push(position);
            while (todisassemble.Count > 0)
            {
                curpos = todisassemble.Pop();
                if (!chunked_instructions.ContainsKey(curpos))
                {
                    fromstream.Position = curpos;
                    disassemble_chunk(fromstream, todisassemble, chunked_instructions);
                }
            }
            return new asmChunk(position, chunked_instructions);
        }

        //some tool functions

        public static uint StackSize(asmChunk tocheck)
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

        /// <summary>
        /// Find the first function called from the parent function with the specified stacksize (as obtained from the retrn instruction; ccall functions won't work)
        /// </summary>
        /// <param name="onstream">process or binary executable file stream</param>
        /// <param name="fromfunction">parent</param>
        /// <param name="required_stacksize">stacksize to look for</param>
        /// <param name="backwards">search forward or backwards from the current position</param>
        /// <param name="found">parsed chunk for the found function</param>
        /// <returns>whether or not a function with the specified stack size was found</returns>
        public static bool FuncFind(Stream onstream, asmChunk fromfunction, uint required_stacksize, bool backwards, out asmChunk found)
        {
            asmInstruction curins;
            asmChunk curfunc;
            found = null;
            while (fromfunction.FindByType(x86_insn_type.insn_call, false, out curins))
            {
                onstream.Position = curins.ReadAddressOperand();
                curfunc = disassemble_chunk(onstream);
                if (StackSize(curfunc) == required_stacksize)
                {
                    found = curfunc;
                    return true;
                }
                fromfunction.MoveNext();
            }
            return false;
        }
    }
}