using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//some simple asm generation tools
//simplifies injection, etc.
namespace Assembler
{
    public abstract class AsmInstructionBuilder
    {
        public abstract int Size { get; }
        public abstract void Write(Stream tostream, int address);
    }

    public class AsmBuilder
    {
        private List<AsmInstructionBuilder> m_Instructions;

        public AsmBuilder()
        {
            m_Instructions = new List<AsmInstructionBuilder>();
        }
        public AsmBuilder(List<AsmInstructionBuilder> instructions)
        {
            m_Instructions = instructions;
        }

        public int Size
        {
            get
            {
                int toreturn = 0;
                foreach (AsmInstructionBuilder insn in m_Instructions)
                    toreturn += insn.Size;
                return toreturn;
            }
        }

        public List<AsmInstructionBuilder> Instructions { get { return m_Instructions; } }

        public void Write(Stream tostream, int address)
        {
            int curaddress = address;
            foreach (AsmInstructionBuilder insn in m_Instructions)
            {
                insn.Write(tostream, curaddress);
                curaddress += insn.Size;
            }
        }
    }

    public class MovEaxImmediate : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0xB8, 0, 0, 0, 0 };

        public MovEaxImmediate(uint value)
        {
            BitConverter.GetBytes(value).CopyTo(m_instruction, 1);
        }

        public override int Size
        {
            get { return 5; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin); 
            tostream.Write(m_instruction, 0, 5);
        }
    }

    public class MovEaxMemory : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0xA1, 0, 0, 0, 0 };

        public MovEaxMemory(uint address)
        {
            BitConverter.GetBytes(address).CopyTo(m_instruction, 1);
        }

        public override int Size
        {
            get { return 5; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 5);
        }
    }

    public class MovMemoryEax : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0xA3, 0, 0, 0, 0 };

        public MovMemoryEax(uint address)
        {
            BitConverter.GetBytes(address).CopyTo(m_instruction, 1);
        }

        public override int Size
        {
            get { return 5; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 5);
        }
    }

    public class CallRelative:AsmInstructionBuilder
    {
        private int m_Address;
        private byte[] m_Instruction = new byte[] { 0xE8, 0, 0, 0, 0 };
        
        public CallRelative(int absolute_address)
        {
            m_Address = absolute_address;
        }

        public override int Size
        {
            get { return 5; }
        }

        public override void Write(Stream tostream, int address)
        {
            int relativeaddress = m_Address - (address + 5);
            BitConverter.GetBytes(relativeaddress).CopyTo(m_Instruction, 1);
            tostream.Seek(address, SeekOrigin.Begin); 
            tostream.Write(m_Instruction, 0, 5);
        }
    }

    public class CallFunctionPointer : AsmInstructionBuilder
    {
        private byte[] m_Instruction = new byte[] { 0xFF, 0x15, 0, 0, 0, 0 };

        public CallFunctionPointer(uint pointer_address)
        {
            BitConverter.GetBytes(pointer_address).CopyTo(m_Instruction, 2);
        }

        public override int Size
        {
            get { return 6; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 6);
        }
    }

    public class CallEax : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0xFF, 0xD0 };
        public CallEax()
        {
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin); 
            tostream.Write(m_Instruction, 0, 2);
        }
    }

    public class PushEax : AsmInstructionBuilder
    {
        private byte[] m_Instuction = { 0x50 };

        public PushEax()
        {
        }

        public override int Size
        {
            get { return 1; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instuction, 0, 1);
        }
    }

    public class PushImmediate : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x68, 0, 0, 0, 0 };
        public PushImmediate(uint ImmediateValue)
        {
            BitConverter.GetBytes(ImmediateValue).CopyTo(m_Instruction, 1);
        }

        public override int Size
        {
            get { return 5; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 5);
        }
    }

    public class BackupEsp : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x89, 0x25, 0, 0, 0, 0 };
        public BackupEsp(uint to_address)
        {
            BitConverter.GetBytes(to_address).CopyTo(m_Instruction, 2);
        }

        public override int Size
        {
            get { return 6; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 6);
        }
    }

    public class RestoreEsp : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x8B, 0x25, 0, 0, 0, 0 };
        public RestoreEsp(uint from_address)
        {
            BitConverter.GetBytes(from_address).CopyTo(m_Instruction, 2);
        }

        public override int Size
        {
            get { return 6; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 6);
        }
    }

    public class DereferEax : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x8B, 0x00};
        public DereferEax()
        {
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 2);
        }
    }

    /*public class DereferEaxTable : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x8B, 0x40, 0 };
        public DereferEaxTable(byte index)
        { 
            m_Instruction[2] = index;
        }

        public override int Size
        {
            get { return 3; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 3);
        }
    }*/

    public class DereferEaxTable : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x8B, 0x80, 0, 0, 0, 0 };
        public DereferEaxTable(uint index)
        {
            BitConverter.GetBytes(index).CopyTo(m_Instruction, 2);
        }

        public override int Size
        {
            get { return 6; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 6);
        }
    }

    public class JmpRelative : AsmInstructionBuilder
    {
        private int m_Address;
        private byte[] m_Instruction = new byte[] { 0xE9, 0, 0, 0, 0 };

        public JmpRelative(int absolute_address)
        {
            m_Address = absolute_address;
        }

        public override int Size
        {
            get { return 5; }
        }

        public override void Write(Stream tostream, int address)
        {
            int relativeaddress = m_Address - (address + 5);
            BitConverter.GetBytes(relativeaddress).CopyTo(m_Instruction, 1);
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 5);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UnmanagedContext
    {
        public uint EFLAGS;
        public uint EDI;
        public uint ESI;
        public uint EBP;
        public uint ESP;
        public uint EBX;
        public uint EDX;
        public uint ECX;
        public uint EAX;
    }

    public class PushAll : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x60, 0x9C };
        public PushAll()
        {
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 2);
        }
    }

    public class PopAll : AsmInstructionBuilder
    {
        private byte[] m_Instruction = { 0x9D, 0x61 };
        public PopAll()
        {
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_Instruction, 0, 2);
        }
    }

    //8B 0D
    public class MovEcxMemory : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0x8B, 0x0D, 0, 0, 0, 0 };

        public MovEcxMemory(uint address)
        {
            BitConverter.GetBytes(address).CopyTo(m_instruction, 2);
        }

        public override int Size
        {
            get { return 6; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 6);
        }
    }

    //8B 15
    public class MovEdxMemory : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0x8B, 0x15, 0, 0, 0, 0 };

        public MovEdxMemory(uint address)
        {
            BitConverter.GetBytes(address).CopyTo(m_instruction, 2);
        }

        public override int Size
        {
            get { return 6; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 6);
        }
    }

    //8B C2
    public class MovEaxEdx : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0x8B, 0xC2 };

        public MovEaxEdx()
        {
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 2);
        }
    }


    public class JzRelativeShort : AsmInstructionBuilder
    {
        private int m_targetaddress;
        private byte[] m_instruction = { 0x74, 0 };

        public JzRelativeShort(int targetaddress)
        {
            m_targetaddress = targetaddress;
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            m_instruction[1] = (byte)(m_targetaddress - (address + 2));
            tostream.Write(m_instruction, 0, 2);
        }
    }

    //85 C0 test eax, eax
    public class TestEaxEax : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0x85, 0xC0 };

        public TestEaxEax()
        {
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 2);
        }
    }

    public class Rtn : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0xC3 };

        public Rtn()
        {
        }

        public override int Size
        {
            get { return 1; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 1);
        }
    }

    public class RtnStackSize : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0xC2 , 0 , 0 };

        public RtnStackSize(ushort stacksize)
        {
            BitConverter.GetBytes(stacksize).CopyTo(m_instruction, 1);
        }

        public override int Size
        {
            get { return 3; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 3);
        }
    }

    public class AddEdx : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0x83, 0xC2, 0 };

        public AddEdx(byte toadd)
        {
            m_instruction[2] = toadd;
        }

        public override int Size
        {
            get { return 3; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 3);
        }
    }

    public class DecEcx : AsmInstructionBuilder
    {
        private byte[] m_instruction = { 0x49 };

        public DecEcx()
        {
        }

        public override int Size
        {
            get { return 1; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            tostream.Write(m_instruction, 0, 1);
        }
    }

    public class JnzRelativeShort : AsmInstructionBuilder
    {
        private int m_targetaddress;
        private byte[] m_instruction = { 0x75, 0 };

        public JnzRelativeShort(int targetaddress)
        {
            m_targetaddress = targetaddress;
        }

        public override int Size
        {
            get { return 2; }
        }

        public override void Write(Stream tostream, int address)
        {
            tostream.Seek(address, SeekOrigin.Begin);
            m_instruction[1] = (byte)(sbyte)(m_targetaddress - (address + 2));
            tostream.Write(m_instruction, 0, 2);
        }
    }
}