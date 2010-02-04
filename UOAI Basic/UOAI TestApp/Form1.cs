using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UOAIBasic;

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
            foreach (Client curclient in ClientList.Default)
                curclient.WriteLine(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Win32API.WindowHandler wh=Win32API.WindowHandler.FindWindow("Ultima Online",-1,-1)[0];
            UOCallibration.Callibrate(wh.onProcess);
        }
    }
}
