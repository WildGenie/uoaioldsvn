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
            ClientEvents cures;
            foreach (Client curclient in ClientList.Default)
            {
                curclient.PatchEncryption();
                cures = new ClientEvents(curclient);
                cures.SynchronizationObject = this;
                cures.OnPacketHandled += curclient_OnPacketHandled;
                cures.OnPacketReceive += curclient_OnPacketReceive;
                cures.OnPacketSend += curclient_OnPacketSend;
            }
        }

        void curclient_OnPacketHandled()
        {
            if (listBox1.Items.Count == 10)
                listBox1.Items.RemoveAt(9);
            listBox1.Items.Insert(0, "received packet handled");
            return;
        }

        bool curclient_OnPacketReceive(ProcessInjection.UnmanagedBuffer packet)
        {
            if (listBox1.Items.Count == 10)
                listBox1.Items.RemoveAt(9);
            listBox1.Items.Insert(0, "received packet 0x" + packet.ReadAt<byte>(0).ToString("X"));
            return true;
        }

        bool curclient_OnPacketSend(ProcessInjection.UnmanagedBuffer packet)
        {
            if (listBox1.Items.Count == 10)
                listBox1.Items.RemoveAt(9);
            listBox1.Items.Insert(0, "sent packet 0x" + packet.ReadAt<byte>(0).ToString("X"));
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
            string[] buffs=textBox2.Text.Split(new char[]{' '});
            byte[] bytes = new byte[buffs.Length];
            for (uint i = 0; i < buffs.Length; i++)
            {
                bytes[i] = byte.Parse(buffs[i], System.Globalization.NumberStyles.HexNumber);
            }
            asmInstruction curins=disassembler.disassemble(bytes);
            MessageBox.Show(Filter.GetOpDataAsUInt(curins.Operands[1]).ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (Client cl in ClientList.Default)
            {
                cl.UnHookTest();
            }
        }
    }
}
