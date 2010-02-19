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
        private IClient curclient;
        private INetworkObject NetworkObject;

        public frmTestApp()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (UOAI.Clients.Count > 0)
            {
                //TAKE CARE THAT ALL EVENT HANDLERS ARE """PUBLIC""" FUNCTIONS !!!
                //IF THEY ARE NOT PUBLIC, EVENTS FAIL SILENTLY !!!

                /*
                 * event detachment is still bugged
                if (curclient != null)
                {
                    curclient.OnKeyUp -= new OnKeyUpDelegate(cures_OnKeyUp);
                    curclient.OnKeyDown -= new OnKeyDownDelegate(cures_OnKeyDown);
                    curclient.OnQuit -= new SimpleDelegate(curclient_OnQuit);
                    NetworkObject.OnPacketHandled -= new SimpleDelegate(NetworkObject_OnPacketHandled);
                    NetworkObject.onPacketRecieve -= new OnPacketDelegate(NetworkObject_onPacketRecieve);
                    NetworkObject.onPacketSend -= new OnPacketDelegate(NetworkObject_onPacketSend);
                    //curclient.OnWindowsMessage -= new OnWindowsMessageDelegate(curclient_OnWindowsMessage);
                }
                 * 
                 */
                
                curclient = UOAI.Clients[0];

                //synchronize all events with this form's main thread
                curclient.SetInvokationTarget(new InvokationTarget(this), false);
                
                //prevent deadlocks by telling UOAIBasic to execute all methods that might trigger an event asynchronously
                curclient.SetAsync("IClient", "Macro", true);
                curclient.SetAsync("IClient", "SysMessage", true);
                curclient.SetAsync("INetworkObject", "SendPacket", true);
                curclient.SetAsync("INetworkObject", "HandlePacket", true);

                //install eventhandlers
                curclient.OnKeyUp += new OnKeyUpDelegate(cures_OnKeyUp);
                curclient.OnKeyDown += new OnKeyDownDelegate(cures_OnKeyDown);
                curclient.OnQuit += new SimpleDelegate(curclient_OnQuit);
                //curclient.OnWindowsMessage += new OnWindowsMessageDelegate(curclient_OnWindowsMessage);
                NetworkObject = curclient.NetworkObject;
                NetworkObject.OnPacketHandled+=new SimpleDelegate(NetworkObject_OnPacketHandled);
                NetworkObject.onPacketRecieve+=new OnPacketDelegate(NetworkObject_onPacketRecieve);
                NetworkObject.onPacketSend+=new OnPacketDelegate(NetworkObject_onPacketSend);
            }
            else
            {
                curclient = null;
                NetworkObject = null;
                MessageBox.Show("No Running Client Found!");
            }
        }

        public bool curclient_OnWindowsMessage(ref Win32API.Imports.MSG msg)
        {
            Win32API.WindowHandler.Messages message = (Win32API.WindowHandler.Messages)msg.message;
            AddToListBox(message.ToString());
            return true;                
        }

        public bool curclient_OnQuit()
        {
            AddToListBox("Client Quit!");
            return true;
        }

        public bool cures_OnKeyUp(uint VirtualKeyCode)
        {
            Key thekey = KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode);
            label1.Text = "Last Key Pressed: " + thekey.ToString();
            if ((thekey >= Key.F1) && (thekey <= Key.F24))
                return false;//don't pass function keys
            return true;
        }

        public bool cures_OnKeyDown(uint VirtualKeyCode, bool repeated)
        {
            Key thekey = KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode);
            if (!repeated)
                label1.Text = "Key Down: " + thekey.ToString();
            if ((thekey >= Key.F1) && (thekey <= Key.F24))
                return false;//don't pass function keys
            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new CallibrationFileEditor().Show();
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

        private void SendSpeechPacket(string speech)
        {
            if (curclient != null)
            {
                UnmanagedBuffer packet = curclient.AllocateBuffer((uint)(12 + (speech.Length + 1) * 2));

                //need to do something about the definition of the write function(s)
                //... generic calls across remoting boundaries are failing, so...
                //... maybe just one write taking any object to write would work best
                packet.Write((object)(byte)0xAD);
                packet.Write((object)(ushort)(12 + (speech.Length + 1) * 2));

                packet.SwapByteOrder = true;

                packet.Write((object)(byte)0x01);
                packet.Write((object)(ushort)0x21);
                packet.Write((object)(ushort)3);
                packet.WriteString("ENU");
                packet.WriteUnicodeString(speech);

                curclient.NetworkObject.SendPacket(packet);
            }
        }

        private delegate void AddToListBoxDelegate(string toadd);

        private void AddToListBox(string toadd)
        {
            if (listBox1.Items.Count == 11)
                listBox1.Items.RemoveAt(10);
            listBox1.Items.Insert(0, toadd);
        }

        public bool NetworkObject_OnPacketHandled()
        {
            AddToListBox("handled");
            return true;
        }

        public bool NetworkObject_onPacketSend(UnmanagedBuffer packet)
        {
            AddToListBox("sent: " + ((byte)packet.Read(typeof(byte))).ToString("X"));
            return true;
        }

        public bool NetworkObject_onPacketRecieve(UnmanagedBuffer packet)
        {
            AddToListBox("received: " + ((byte)packet.Read(typeof(byte))).ToString("X"));
            return true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendSpeechPacket(textBox1.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (curclient != null)
            {
                //Macro'ing might seem more slightly more complicated then it was
                //but this is a direct forward to the client's implementation...

                //build macroentry (Say macro = macro #1)
                MacroEntry me = new MacroEntry(1, 0, textBox1.Text);
                //build macrotable (max 10 macroentries... maybe i'll change this to more entries (or non-fixed size) later on, not sure if the client sets a limit... but i need a fixed size to get evertyhing marshalled automatically, so i just always copy 10 entries, 0-entries are added to get to 10 if needed)
                MacroTable mt = new MacroTable(new MacroEntry[] { me });
                //execute first entry in the table
                curclient.Macro(mt, 0);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (curclient != null)
            {
                //0x21  =   some red hue
                //3     =   typical system font
                curclient.SysMessage(0x21, 3, textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (curclient != null)
                curclient.PatchEncryption();
        }
    }
}
