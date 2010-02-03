using System;
using System.IO;
using System.Text;
using Win32API;

namespace ProcessHandling
{
    //default implementation of the process interface
    public class ProcessStream : Stream
    {
        //private members
        private uint m_PID;
        private uint m_Handle;
        private int m_CurrentPosition;

        //constructor(s)
        public ProcessStream(UInt32 PID)
        {
            m_PID = PID;
            m_Handle = Imports.OpenProcess(Imports.ProcessAccess.VMRead | Imports.ProcessAccess.VMWrite | Imports.ProcessAccess.QueryInformation | Imports.ProcessAccess.CreateThread | Imports.ProcessAccess.VMOperation, false, PID);
            if (m_Handle == 0)
                throw new Exception("Failed to open a Client Process (pid = " + PID.ToString() + ")\nTry running this application with Administrator privileges!");
        }

        //Process Interface implementation
        public uint PID { get { return m_PID; } }
        public uint Handle { get { return m_Handle; } }
        public bool IsRunning
        {
            get
            {
                UInt32 exitcode;

                if (m_Handle != 0)
                {
                    if (Imports.GetExitCodeProcess(m_Handle, out exitcode))
                    {
                        if (exitcode == 259)//STILL_ACTIVE
                            return true;
                    }
                }

                return false;
            }
        }

        private byte[] _Read(int address, int bytecount)
        {
            byte[] toreturn = null;
            UInt32 prevprotect;
            UInt32 bytesread;

            if (Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, Imports.PAGE_READWRITE, out prevprotect))
            {
                toreturn = new byte[bytecount];

                Imports.ReadProcessMemory(m_Handle, (uint)address, toreturn, (uint)bytecount, out bytesread);

                Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, prevprotect, out prevprotect);
            }

            return toreturn;
        }
        private bool _Write(int address, byte[] towrite)
        {
            UInt32 prevprotect;
            UInt32 byteswritten;
            UInt32 bytecount = (UInt32)towrite.Length;

            if (Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, Imports.PAGE_READWRITE, out prevprotect))
            {
                if (Imports.WriteProcessMemory(m_Handle, (uint)address, towrite, (uint)bytecount, out byteswritten))
                {
                    if (byteswritten == bytecount)
                    {
                        Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, prevprotect, out prevprotect);
                        return true;
                    }
                }

                Imports.VirtualProtectEx(m_Handle, (uint)address, (uint)bytecount, prevprotect, out prevprotect);
            }

            return false;
        }

        public override bool CanRead
        {
            get { return true; }
        }

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
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                return m_CurrentPosition;
            }
            set
            {
                m_CurrentPosition = (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] readbytes=_Read(m_CurrentPosition, count);
            readbytes.CopyTo(buffer, offset);
            return readbytes.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                m_CurrentPosition = (int)offset;
            else if (origin == SeekOrigin.Current)
                m_CurrentPosition += (int)offset;
            else
                throw new Exception("SeekOrigin.End not allowed for process streams!");
            return (long)m_CurrentPosition;

        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] m_Temp=new byte[count];
            Buffer.BlockCopy(buffer,offset,m_Temp,0,count);
            _Write(m_CurrentPosition, m_Temp);
        }
    }
}
