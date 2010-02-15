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
    public partial class frmListBuilder : Form
    {
        public Type m_Type;
        public Object m_Object;

        public static object CreateList(Type listtype)
        {
            frmListBuilder fb = new frmListBuilder(listtype, null);
            fb.ShowDialog();
            return fb.m_Object;
        }

        public static void UpdateList(object toupdate)
        {
            frmListBuilder fb = new frmListBuilder(toupdate.GetType(), toupdate);
            fb.ShowDialog();
            return;
        }

        public frmListBuilder(Type t, object toupdate)
        {
            InitializeComponent();

            m_Type = t;
            m_Object = toupdate;

            UpdateAvailable();
            ObjectInitializer();
        }

        private void UpdateAvailable()
        {
            if (m_Type == typeof(FilterList))
            {
                lstAvailable.Items.Clear();
                foreach (Filter f in Filter.filters)
                    lstAvailable.Items.Add(f);
            }
            else if (m_Type == typeof(CodeBlock))
            {
                lstAvailable.Items.Clear();
                foreach (FilterList f in FilterList.filterlists)
                    lstAvailable.Items.Add(f);
            }
            else if (m_Type == typeof(Sequence))
            {
                lstAvailable.Items.Clear();
                foreach (CodeBlock f in CodeBlock.codeblocks)
                    lstAvailable.Items.Add(f);
            }
            else if (m_Type == typeof(ActionList))
            {
                lstAvailable.Items.Clear();
                foreach (UOAIBasic.Action f in UOAIBasic.Action.actions)
                    lstAvailable.Items.Add(f);
            }
        }

        private void ObjectInitializer()
        {
            if (m_Object == null)
                return;
            if (m_Type == typeof(FilterList))
            {
                FilterList fthis = (FilterList)m_Object;
                textBox1.Text = fthis.name;
                foreach (Filter f in fthis.filters)
                    listBox1.Items.Add(f);
            }
            else if (m_Type == typeof(CodeBlock))
            {
                CodeBlock fthis = (CodeBlock)m_Object;
                textBox1.Text = fthis.name;
                foreach (FilterList f in fthis.FilterLists)
                    listBox1.Items.Add(f);
            }
            else if (m_Type == typeof(Sequence))
            {
                Sequence fthis = (Sequence)m_Object;
                textBox1.Text = fthis.name;
                foreach (CodeBlock f in fthis.CodeBlocks)
                    listBox1.Items.Add(f);
            }
            else if (m_Type == typeof(ActionList))
            {
                ActionList fthis = (ActionList)m_Object;
                textBox1.Text = fthis.name;
                foreach (UOAIBasic.Action f in fthis.Actions)
                    listBox1.Items.Add(f);
            }
        }

        private void DoAdd()
        {
            if (lstAvailable.SelectedItem != null)
            {
                int curidx = listBox1.SelectedIndex;
                if(curidx>=0)
                    listBox1.Items.Insert(curidx,lstAvailable.SelectedItem);
                else
                    listBox1.Items.Add(lstAvailable.SelectedItem);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DoAdd();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
        }

        private void SetCurrent(Object toset)
        {
            UpdateAvailable();

            if (toset != null)
                lstAvailable.SelectedItem = toset;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Hide();

            if (m_Type == typeof(FilterList))
            {
                SetCurrent(FilterBuilder.CreateFilter());
            }
            else if (m_Type == typeof(CodeBlock))
            {
                SetCurrent(frmListBuilder.CreateList(typeof(FilterList)));
            }
            else if (m_Type == typeof(Sequence))
            {
                SetCurrent(frmListBuilder.CreateList(typeof(CodeBlock)));
            }
            else if (m_Type == typeof(ActionList))
            {
                SetCurrent(ActionBuilder.CreateAction());
            }

            DoAdd();

            Show();
        }

        private string GenerateName()
        {
            if (textBox1.Text == "")
            {
                if (m_Type == typeof(FilterList))
                    return "FilterList_" + FilterList.filterlists.Count.ToString();
                else if (m_Type == typeof(CodeBlock))
                    return "CodeBlock_" + CodeBlock.codeblocks.Count.ToString();
                else if (m_Type == typeof(Sequence))
                    return "Sequence_" + Sequence.sequences.Count.ToString();
                else if (m_Type == typeof(ActionList))
                    return "ActionList_" + ActionList.actionlists.Count.ToString();
                else
                    return "invalid";
            }
            else
            {
                return textBox1.Text.Replace(' ', '_');
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (m_Type == typeof(FilterList))
            {
                FilterList fl=(FilterList)m_Object;
                if (fl != null)
                {
                    fl.name = GenerateName();
                    fl.filters.Clear();
                    foreach (Filter curf in listBox1.Items)
                        fl.filters.Add(curf);
                    fl.Update();
                }
                else
                {
                    fl = new FilterList(GenerateName());
                    foreach (Filter curf in listBox1.Items)
                        fl.filters.Add(curf);
                    m_Object = fl;
                }
            }
            else if (m_Type == typeof(CodeBlock))
            {
                CodeBlock fl = (CodeBlock)m_Object;
                if (fl != null)
                {
                    fl.name = GenerateName();
                    fl.FilterLists.Clear();
                    foreach (FilterList curf in listBox1.Items)
                        fl.FilterLists.Add(curf);
                    fl.Update();
                }
                else
                {
                    fl = new CodeBlock(GenerateName());
                    foreach (FilterList curf in listBox1.Items)
                        fl.FilterLists.Add(curf);
                    m_Object = fl;
                }
            }
            else if (m_Type == typeof(Sequence))
            {
                Sequence fl = (Sequence)m_Object;
                if (fl != null)
                {
                    fl.name = GenerateName();
                    fl.CodeBlocks.Clear();
                    foreach (CodeBlock curf in listBox1.Items)
                        fl.CodeBlocks.Add(curf);
                    fl.Update();
                }
                else
                {
                    fl = new Sequence(GenerateName());
                    foreach (CodeBlock curf in listBox1.Items)
                        fl.CodeBlocks.Add(curf);
                    m_Object = fl;
                }
            }
            else if (m_Type == typeof(ActionList))
            {
                ActionList fl = (ActionList)m_Object;
                if (fl != null)
                {
                    fl.name = GenerateName();
                    fl.Actions.Clear();
                    foreach (UOAIBasic.Action curf in listBox1.Items)
                        fl.Actions.Add(curf);
                    fl.Update();
                }
                else
                {
                    fl = new ActionList(GenerateName());
                    foreach (UOAIBasic.Action curf in listBox1.Items)
                        fl.Actions.Add(curf);
                    m_Object = fl;
                }
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            m_Object = null;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                int idx=listBox1.Items.IndexOf(listBox1.SelectedItem);
                if (idx > 0)
                {
                    object curitem = listBox1.SelectedItem;
                    listBox1.Items.Remove(curitem);
                    listBox1.Items.Insert(idx - 1, curitem);
                    listBox1.SelectedItem = curitem;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                int idx = listBox1.Items.IndexOf(listBox1.SelectedItem);
                if (idx < (listBox1.Items.Count-1))
                {
                    object curitem = listBox1.SelectedItem;
                    listBox1.Items.Remove(curitem);
                    listBox1.Items.Insert(idx + 1, curitem);
                    listBox1.SelectedItem = curitem;
                }
            }
        }

        private void frmListBuilder_Load(object sender, EventArgs e)
        {

        }

        private void frmListBuilder_MouseClick(object sender, MouseEventArgs e)
        {
            listBox1.SelectedItem = null;
        }
    }
}
