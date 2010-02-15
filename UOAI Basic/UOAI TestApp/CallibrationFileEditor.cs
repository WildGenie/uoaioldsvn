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
    public partial class CallibrationFileEditor : Form
    {
        private CallibrationFile cfile;

        public CallibrationFileEditor()
        {
            InitializeComponent();

            UpdateForm();
        }

        void UpdateForm()
        {
            UpdateActionLists();
            UpdateActions();
            UpdateSequences();
            UpdateCodeBlocks();
            UpdateFilterLists();
            UpdateFilters();
        }

        void UpdateActionLists()
        {
            actionlists.Items.Clear();
            actionlists.Items.Add((string)"New...");
            foreach (ActionList f in ActionList.actionlists)
                actionlists.Items.Add(f);
        }

        void UpdateActions()
        {
            actions.Items.Clear();
            actions.Items.Add((string)"New...");
            foreach (UOAIBasic.Action f in UOAIBasic.Action.actions)
                actions.Items.Add(f);
        }

        void UpdateSequences()
        {
            sequences.Items.Clear();
            sequences.Items.Add((string)"New...");
            foreach (Sequence f in Sequence.sequences)
                sequences.Items.Add(f);
        }

        void UpdateCodeBlocks()
        {
            codeblocks.Items.Clear();
            codeblocks.Items.Add((string)"New...");
            foreach (CodeBlock f in CodeBlock.codeblocks)
                codeblocks.Items.Add(f);
        }

        void UpdateFilterLists()
        {
            filterlists.Items.Clear();
            filterlists.Items.Add((string)"New...");
            foreach (FilterList f in FilterList.filterlists)
                filterlists.Items.Add(f);
        }

        void UpdateFilters()
        {
            filters.Items.Clear();
            filters.Items.Add((string)"New...");
            foreach (Filter f in Filter.filters)
                filters.Items.Add(f);
        }

        private void actionlists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (actionlists.SelectedItem != null)
            {
                Hide();
                if (actionlists.SelectedItem.GetType() == typeof(string))
                    frmListBuilder.CreateList(typeof(ActionList));
                else
                    frmListBuilder.UpdateList(actionlists.SelectedItem);
                Show();
                UpdateForm();
            }
        }

        private void actions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (actions.SelectedItem != null)
            {
                Hide();
                if (actions.SelectedItem.GetType() == typeof(string))
                    ActionBuilder.CreateAction();
                else
                    ActionBuilder.UpdateAction((UOAIBasic.Action)actions.SelectedItem);
                Show();
                UpdateForm();
            }
        }

        private void sequences_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sequences.SelectedItem != null)
            {
                Hide();
                if (sequences.SelectedItem.GetType() == typeof(string))
                    frmListBuilder.CreateList(typeof(Sequence));
                else
                    frmListBuilder.UpdateList(sequences.SelectedItem);
                Show();
                UpdateForm();
            }
        }

        private void codeblocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (codeblocks.SelectedItem != null)
            {
                Hide();
                if (codeblocks.SelectedItem.GetType() == typeof(string))
                    frmListBuilder.CreateList(typeof(CodeBlock));
                else
                    frmListBuilder.UpdateList(codeblocks.SelectedItem);
                Show();
                UpdateForm();
            }
        }

        private void filterlists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (filterlists.SelectedItem != null)
            {
                Hide();
                if (filterlists.SelectedItem.GetType() == typeof(string))
                    frmListBuilder.CreateList(typeof(FilterList));
                else
                    frmListBuilder.UpdateList(filterlists.SelectedItem);
                Show();
                UpdateForm();
            }
        }

        private void filters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (filters.SelectedItem != null)
            {
                Hide();
                if (filters.SelectedItem.GetType() == typeof(string))
                    FilterBuilder.CreateFilter();
                else
                    FilterBuilder.UpdateFilter((Filter)filters.SelectedItem);
                Show();
                UpdateForm();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cfile = new CallibrationFile();
            foreach (ActionList al in ActionList.actionlists)
                cfile.actionlists.Add(al);
            SaveFileDialog newsf = new SaveFileDialog();
            if (newsf.ShowDialog() == DialogResult.OK)
                cfile.Save(newsf.FileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog newof = new OpenFileDialog();
            if (newof.ShowDialog() == DialogResult.OK)
                cfile = CallibrationFile.Load(newof.FileName);
            UpdateForm();
        }
    }
}
