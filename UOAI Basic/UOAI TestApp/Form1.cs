using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UOAIBasic;
using libdisasm;
using System.Windows.Input;

namespace UOAI_TestApp
{
    public partial class frmTestApp : Form
    {
        public frmTestApp()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //so client events is the remoted object?
            ClientEvents cures;

            //this is setting up the events i take it?
            foreach (Client curclient in ClientList.Default)
            {
                curclient.PatchEncryption(); //patching encryption
                cures = new ClientEvents(curclient); //making a new client event for the client.
                cures.ASynchronous = false;//we don't need to filter packets so we can receive them asynchronously (begininvoke rather then invoke is used)
                cures.SynchronizationObject = this;//tell UOAI Basic to use this form's Invoke function to invoke events
                cures.OnPacketHandled += curclient_OnPacketHandled;//setting up the packet handled void to handle events
                cures.OnPacketReceive += curclient_OnPacketReceive;
                cures.OnPacketSend += curclient_OnPacketSend;
                cures.OnKeyDown += new ClientEvents.OnKeyDownDelegate(cures_OnKeyDown);
                cures.OnKeyUp += new ClientEvents.OnKeyUpDelegate(cures_OnKeyUp);
            }
        }

        bool cures_OnKeyUp(uint VirtualKeyCode)
        {
            Key thekey = KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode);
            label1.Text = "Last Key Pressed: " + thekey.ToString();
            if ((thekey >= Key.F1) && (thekey <= Key.F24))
                return false;//don't pass function keys
            return true;
        }

        bool cures_OnKeyDown(uint VirtualKeyCode, bool repeated)
        {
            Key thekey = KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode);
            if(!repeated)
                label1.Text = "Key Down: " + thekey.ToString();
            if ((thekey >= Key.F1) && (thekey <= Key.F24))
                return false;//don't pass function keys
            return true;
        }

        void curclient_OnPacketHandled()
        {
            if (listBox1.Items.Count == 10)
                listBox1.Items.RemoveAt(9);
            listBox1.Items.Insert(0, "Received packet handled");
            return;
        }

        bool curclient_OnPacketReceive(ProcessInjection.UnmanagedBuffer packet)
        {
            if (listBox1.Items.Count == 10)
                listBox1.Items.RemoveAt(9);
            byte packetcmd = packet.ReadAt<byte>(0);
            listBox1.Items.Insert(0, "Received packet 0x" + packetcmd.ToString("X"));
            return true;
        }

        bool curclient_OnPacketSend(ProcessInjection.UnmanagedBuffer packet)
        {
            if (listBox1.Items.Count == 10)
                listBox1.Items.RemoveAt(9);
            byte packetcmd = packet.ReadAt<byte>(0);
            listBox1.Items.Insert(0, "Sent packet 0x" + packetcmd.ToString("X"));
            if (packetcmd == 0x06)
                return false;//drop doubleclick packet
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Win32API.WindowHandler wh=Win32API.WindowHandler.FindWindow("Ultima Online",-1,-1)[0];
            UOCallibration.Callibrate(wh.onProcess);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new CallibrationFileEditor().Show();
        }

        private void frmTestApp_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                string[] buffs = textBox2.Text.Split(new char[] { ' ' });
                byte[] bytes = new byte[buffs.Length];
                for (uint i = 0; i < buffs.Length; i++)
                {
                    bytes[i] = byte.Parse(buffs[i], System.Globalization.NumberStyles.HexNumber);
                }
                asmInstruction curins = disassembler.disassemble(bytes);
                MessageBox.Show(Filter.GetOpDataAsUInt(curins.Operands[1]).ToString());
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }
    }
}
