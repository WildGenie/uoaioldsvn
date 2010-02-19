using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tools
{
    #region comments
    //The idea of the AutoType is to have a type that
    //can be casted from all other built-in types explicitly
    //and that can be cast back to any of those built-in types implicitly.
    //So by casting into this you can convert between types easily.
    //Only the typical built-in types or byte[] arrays are supported!
    //f.e. string mystring -> byte[] mybytes goes as follows:
    //      byte[] mybytes=(AutoType)mystring;
    //or... the opposite way around:
    //      string mystring=(AutoType)mybytes;
    //... and similar for other conversions.
    //It's main use is if you need to do some quick serialization.
    //Most of the time the generic .Write<T>(T towrite) and .Read<T>()
    //on the StreamHandler class are more suitable as serialization is
    //typically to a string, and those generic read/write functions can
    //handle more than just the built-in value types (structures f.e. too).
    #endregion
    public class AutoType
    {
        private byte[] m_Buffer;

        public AutoType(byte[] frombuffer)
        {
            m_Buffer = frombuffer;
        }
        public AutoType(byte[] frombuffer, bool reverse_bytes)
        {
            m_Buffer = frombuffer;
            if(reverse_bytes)
                Array.Reverse(m_Buffer);
        }

        public byte[] Buffer { get { return m_Buffer; } }

        public static explicit operator AutoType(byte[] buffer)
        {
            return new AutoType(buffer);
        }

        public static implicit operator Int32(AutoType a)
        {
            return BitConverter.ToInt32(a.Buffer, 0);
        }

        public static implicit operator UInt32(AutoType a)
        {
            return BitConverter.ToUInt32(a.Buffer, 0);
        }

        public static implicit operator Int16(AutoType a)
        {
            return BitConverter.ToInt16(a.Buffer, 0);
        }

        public static implicit operator UInt16(AutoType a)
        {
            return BitConverter.ToUInt16(a.Buffer, 0);
        }

        public static implicit operator UInt64(AutoType a)
        {
            return BitConverter.ToUInt64(a.Buffer, 0);
        }

        public static implicit operator Int64(AutoType a)
        {
            return BitConverter.ToInt64(a.Buffer, 0);
        }

        public static implicit operator String(AutoType a)
        {
            return ASCIIEncoding.ASCII.GetString(a.Buffer);
        }

        public static implicit operator Single(AutoType a)
        {
            return BitConverter.ToSingle(a.Buffer, 0);
        }

        public static implicit operator Double(AutoType a)
        {
            return BitConverter.ToDouble(a.Buffer, 0);
        }

        public static implicit operator Char(AutoType a)
        {
            return BitConverter.ToChar(a.Buffer, 0);
        }

        public static implicit operator Boolean(AutoType a)
        {
            return BitConverter.ToBoolean(a.Buffer, 0);
        }

        public static implicit operator byte[](AutoType a)
        {
            return a.Buffer;
        }

        public static byte[] ToByteArray<T>(T toconvert)
        {
            int size=Marshal.SizeOf(default(T));

            byte[] writebuffer=new byte[size];
            
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);
            
            Marshal.StructureToPtr(toconvert,unmanagedPointer,true);

            Marshal.Copy(unmanagedPointer,writebuffer,0,size);

            Marshal.FreeHGlobal(unmanagedPointer);

            return writebuffer;
        }

        public static explicit operator AutoType(UInt16 a)
        {
            return new AutoType(ToByteArray<UInt16>(a));
        }

        public static explicit operator AutoType(Int16 a)
        {
            return new AutoType(ToByteArray<Int16>(a));
        }

        public static explicit operator AutoType(UInt32 a)
        {
            return new AutoType(ToByteArray<UInt32>(a));
        }

        public static explicit operator AutoType(Int32 a)
        {
            return new AutoType(ToByteArray<Int32>(a));
        }

        public static explicit operator AutoType(UInt64 a)
        {
            return new AutoType(ToByteArray<UInt64>(a));
        }

        public static explicit operator AutoType(Int64 a)
        {
            return new AutoType(ToByteArray<Int64>(a));
        }

        public static explicit operator AutoType(string a)
        {
            return new AutoType(ASCIIEncoding.ASCII.GetBytes(a));
        }

        public static explicit operator AutoType(Single a)
        {
            return new AutoType(ToByteArray<Single>(a));
        }

        public static explicit operator AutoType(double a)
        {
            return new AutoType(ToByteArray<double>(a));
        }

        public static explicit operator AutoType(char a)
        {
            return new AutoType(ToByteArray<char>(a));
        }

        public static explicit operator AutoType(bool a)
        {
            return new AutoType(ToByteArray<bool>(a));
        }
    }

    //indexing a sub area of a byte[] buffer
    public class SubBuffer
    {
        private byte[] m_OriginalArray;
        private int m_low;
        private int m_high;

        public SubBuffer(byte[] of_buffer, int start)
        {
            m_OriginalArray = of_buffer;
            m_low = start;
            m_high = of_buffer.Length;
        }

        public SubBuffer(byte[] of_buffer, int start, int count)
        {
            m_OriginalArray = of_buffer;
            m_low = start;
            if (count <= (of_buffer.Length - start))
                m_high = start + count;
            else
                m_high = of_buffer.Length;
        }

        public int Length { get { return m_high - m_low; } }

        public byte this[uint i]
        {
            get
            {
                if(i<m_high)
                    return m_OriginalArray[m_low+i];
                else
                    throw new IndexOutOfRangeException();
            }
            set
            {
                if (i < m_high)
                    m_OriginalArray[m_low + i] = value;
            }
        }

        public byte[] ToByteArray()
        {
            byte[] toreturn = new byte[Length];
            Buffer.BlockCopy(m_OriginalArray, m_low, toreturn, 0, Length);
            return toreturn;
        }

        public static implicit operator byte[](SubBuffer sb)
        {
            return sb.ToByteArray();
        }

        public static explicit operator SubBuffer(byte[] fromarray)
        {
            return new SubBuffer(fromarray, 0);
        }
    }

    //translate a byte[] buffer into a stream
    public class BufferStream : Stream
    {
        private byte[] m_Buffer;
        public int m_Position = 0;

        public BufferStream(byte[] frombuffer)
        {
            m_Buffer = frombuffer;
        }
        
        public BufferStream(int size)
        {
            m_Buffer = new byte[size];
        }
        
        public override bool CanRead
        {
            get { return true; }
        }

        public byte[] AsByteArray { get { return m_Buffer; } }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return m_Buffer.Length; }
        }

        public override long Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                m_Position = (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int tocopy = Math.Min(m_Buffer.Length - m_Position, count);
            if (tocopy > 0)
            {
                Buffer.BlockCopy(m_Buffer, m_Position, buffer, offset, tocopy);
                m_Position += tocopy;
            }
            return tocopy;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            int m_Backup=m_Position;
            if (origin == SeekOrigin.Begin)
                m_Position = (int)offset;
            else if (origin == SeekOrigin.Current)
                m_Position += (int)offset;
            else
                m_Position = (int)m_Buffer.Length - (int)offset;
            if ((m_Position < 0) || (m_Position > m_Buffer.Length))
            {
                m_Position = m_Backup;
                throw new IndexOutOfRangeException();
            }
            return m_Position;
        }

        public override void SetLength(long value)
        {
            if (value > 0)
            {
                byte[] newbuffer = new byte[value];
                Buffer.BlockCopy(m_Buffer, 0, newbuffer, 0, (int)Math.Min(m_Buffer.Length, value));
                m_Buffer = newbuffer;
            }
            else
                throw new Exception("Buffer length can not be set to zero!");

            if (m_Position > m_Buffer.Length)
                m_Position = m_Buffer.Length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count <= (m_Buffer.Length - m_Position))
            {
                Buffer.BlockCopy(buffer, offset, m_Buffer, m_Position, count);
                m_Position += count;
            }
            else
                throw new IndexOutOfRangeException();
        }
    }

    //Main purpose is to add generic read/write functions for streams
    //with this you can read as: type var=Stream.Read<type>()
    //and write as: Stream.Write<type>(towrite)
    public class StreamHandler : Stream
    {
        private Stream m_Stream;
        private bool SwapByteOrder=false;

        public StreamHandler(Stream fromstream)
        {
            m_Stream = fromstream;
            SwapByteOrder = false;
        }

        public StreamHandler(Stream fromstream, bool swapbyteorder)
        {
            m_Stream = fromstream;
            SwapByteOrder = swapbyteorder;
        }

        public bool NetworkOrder
        {
            get { return SwapByteOrder; }
            set { SwapByteOrder = value; }
        }

        public T Read<T>()
        {
            int toread=Marshal.SizeOf(default(T));
            byte[] bytes = new byte[toread];
            if (Read(bytes, 0, toread) == toread)
            {
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(toread);
                Marshal.Copy(bytes, 0, unmanagedPointer, toread);

                T toreturn = (T)Marshal.PtrToStructure(unmanagedPointer, typeof(T));

                Marshal.FreeHGlobal(unmanagedPointer);
                return toreturn;
            }
            return default(T);
        }

        public void Write<T>(T towrite)
        {
            int size=Marshal.SizeOf(default(T));

            byte[] writebuffer=new byte[size];
            
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);
            
            Marshal.StructureToPtr(towrite,unmanagedPointer,true);

            Marshal.Copy(unmanagedPointer,writebuffer,0,size);

            Marshal.FreeHGlobal(unmanagedPointer);

            Write(writebuffer,0,size);

        }

        public string ReadString()
        {
            List<byte> thebytes = new List<byte>();
            byte curbyte;
            while ((curbyte = Read<byte>()) != 0)
                thebytes.Add(curbyte);
            return ASCIIEncoding.ASCII.GetString(thebytes.ToArray());
        }

        public string ReadString(int length)
        {
            byte[] charbytes = new byte[length];
            for (int i = 0; i < length; i++)
                charbytes[i] = Read<byte>();
            return ASCIIEncoding.ASCII.GetString(charbytes);
        }

        public string ReadUnicodeString()
        {
            List<char> chars = new List<char>();
            char curchar;

            while ((curchar = (char)Read<ushort>()) != 0)
                chars.Add(curchar);

            return new string(chars.ToArray());
        }

        public string ReadUnicodeString(int length)
        {
            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
                chars[i] = (char)Read<ushort>();

            return new string(chars);
        }

        public void WriteString(string towrite)
        {
            byte[] asciibytes=ASCIIEncoding.ASCII.GetBytes(towrite);
            Write(asciibytes, 0, asciibytes.Length);
            if (asciibytes[asciibytes.Length - 1] != 0)
                Write<byte>(0);
        }

        public void WriteUnicodeString(string towrite)
        {
            /*byte[] unicodebytes = UnicodeEncoding.Unicode.GetBytes(towrite);
            Write(unicodebytes, 0, unicodebytes.Length);
            if (!((unicodebytes[unicodebytes.Length - 2] == 0)&&(unicodebytes[unicodebytes.Length - 1]==0)))
                Write<ushort>(0);*/
            char[] chars = towrite.ToCharArray();
            foreach (char curchar in chars)
                Write<ushort>((ushort)curchar);
            if (chars[chars.Length - 1] != 0)
                Write<ushort>((ushort)0);
        }

        public void WriteBSTR(string towrite)
        {
            Write<uint>((uint)(towrite.Length*2));
            WriteUnicodeString(towrite);
        }

        public void EnsureAlignment(uint bit_alignment)
        {
            if ((Position % bit_alignment) != 0)
            {
                long idx = Position / bit_alignment;
                Position = (idx + 1) * bit_alignment;
            }
        }

        public override bool CanRead
        {
            get { return m_Stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return m_Stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return m_Stream.CanWrite; }
        }

        public override void Flush()
        {
            m_Stream.Flush();
        }

        public override long Length
        {
            get { return m_Stream.Length; }
        }

        public override long Position
        {
            get
            {
                return m_Stream.Position;
            }
            set
            {
                m_Stream.Position=value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readbytes = m_Stream.Read(buffer, offset, count);
            if(SwapByteOrder) Array.Reverse(buffer, offset, readbytes);
            return readbytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            m_Stream.SetLength(value);
            return;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (SwapByteOrder) Array.Reverse(buffer, offset, count);
            m_Stream.Write(buffer, offset, count);
        }
    }
}
