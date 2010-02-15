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
    public partial class ActionBuilder : Form
    {
        public static UOAIBasic.Action CreateAction()
        {
            ActionBuilder ab = new ActionBuilder(null);
            ab.ShowDialog();
            return ab.m_Action;
        }

        public static void UpdateAction(UOAIBasic.Action toupdate)
        {
            ActionBuilder ab = new ActionBuilder(toupdate);
            ab.ShowDialog();
            return;
        }

        public UOAIBasic.Action m_Action;
        private bool setting = false;

        public ActionBuilder(UOAIBasic.Action ofaction)
        {
            InitializeComponent();

            //fill in enum values
            foreach (ActionType at in Enum.GetValues(typeof(ActionType)))
                actTypes.Items.Add(at);
            foreach (UOCallibration.CallibratedFeatures cf in Enum.GetValues(typeof(UOCallibration.CallibratedFeatures)))
            {
                enumpar.Items.Add(cf);
                knownpar2.Items.Add(cf);
            }

            //update sequence list
            UpdateSequenceList();

            //update action lists
            UpdateActionLists();

            //update filter list
            UpdateFilterList();

            //fill in initial values
            InitializeFromAction(ofaction);
        }

        private void UpdateSequenceList()
        {
            object backup = seqPar.SelectedItem;
            seqPar.Items.Clear();
            foreach (Sequence curseq in Sequence.sequences)
                seqPar.Items.Add(curseq);
            seqPar.SelectedItem = backup;
        }

        private void UpdateActionLists()
        {
            object backup1 = actlist.SelectedItem;
            object backup2 = ontrue.SelectedItem;
            object backup3 = onfalse.SelectedItem;
            actlist.Items.Clear();
            ontrue.Items.Clear();
            onfalse.Items.Clear();
            foreach (ActionList cural in ActionList.actionlists)
            {
                actlist.Items.Add(cural);
                ontrue.Items.Add(cural);
                onfalse.Items.Add(cural);
            }
            actlist.SelectedItem = backup1;
            ontrue.SelectedItem = backup2;
            onfalse.SelectedItem = backup3;
        }

        private void UpdateFilterList()
        {
            object backup = filterlist.SelectedItem;
            filterlist.Items.Clear();
            foreach (FilterList fl in FilterList.filterlists)
                filterlist.Items.Add(fl);
            filterlist.SelectedItem = backup;
        }

        private void InitializeFromAction(UOAIBasic.Action toset)
        {
            if ((m_Action=toset) == null)
                return;

            ActionName.Text = toset.name;
            ActionType curatype = toset.m_type;

            SetActionType(curatype);

            UpdateSequenceList();

            switch (curatype)
            {
                case ActionType.DISASM_CHUNK:
                    enumpar.SelectedItem = toset.knownpar;
                    break;
                case ActionType.DISASM_FUNCTION:
                    enumpar.SelectedItem = toset.knownpar;
                    break;
                case ActionType.FIND_SEQUENCE:
                    seqPar.SelectedItem = toset.seqpar;
                    break;
                case ActionType.FOLLOW_CALL:
                    boolpar.Checked = toset.boolpar;
                    break;
                case ActionType.JUMP_KNOWN:
                    uintpar.Text = toset.uintpar.ToString();
                    break;
                case ActionType.RETURN_ADDRESS:
                    enumpar.SelectedItem = toset.knownpar;
                    break;
                case ActionType.RETURN_DATA:
                    enumpar.SelectedItem = toset.knownpar;
                    uintpar.Text = toset.uintpar.ToString();
                    break;
                case ActionType.RETURN_DISP:
                    enumpar.SelectedItem = toset.knownpar;
                    uintpar.Text = toset.uintpar.ToString();
                    break;
                case ActionType.SET_BACKWARDS:
                    boolpar.Checked = toset.boolpar;
                    break;
                case ActionType.FUNC_FIND:
                    uintpar.Text = toset.uintpar.ToString();
                    break;
                case ActionType.SWITCH:
                    enumpar.SelectedItem = toset.knownpar;
                    uintpar.Text = toset.uintpar.ToString();
                    boolpar.Checked = toset.boolpar;
                    break;
                case ActionType.CONDITIONAL:
                    actlist.SelectedItem = toset.actlist;
                    ontrue.SelectedItem = toset.ontrue;
                    onfalse.SelectedItem = toset.onfalse;
                    break;
                case ActionType.ASSIGN_VALUE:
                    enumpar.SelectedItem = toset.knownpar;
                    uintpar.Text = toset.uintpar.ToString();
                    break;
                case ActionType.ASSIGN_KNOWN:
                    enumpar.SelectedItem = toset.knownpar;
                    knownpar2.SelectedItem = toset.knownpar2;
                    break;
                case ActionType.CHECK_INSTRUCTION:
                    filterlist.SelectedItem = toset.tocheck;
                    break;
                case ActionType.EXECUTE_ACTIONSLIST:
                    actlist.SelectedItem = toset.actlist;
                    break;
                case ActionType.TRY_ACTIONLIST:
                    actlist.SelectedItem = toset.actlist;
                    break;
                case ActionType.FOLLOW_JMP:
                    boolpar.Checked = toset.boolpar;
                    break;
                case ActionType.RETURN_SCALE:
                    enumpar.SelectedItem = toset.knownpar;
                    uintpar.Text = toset.uintpar.ToString();
                    break;
                case ActionType.RETURN_CHUNK_ADDRESS:
                    enumpar.SelectedItem = toset.knownpar;
                    break;
                case ActionType.RETURN_TARGETADDRESS:
                    enumpar.SelectedItem = toset.knownpar;
                    uintpar.Text = toset.uintpar.ToString();
                    break;
                case ActionType.FIND_LOOP:
                case ActionType.MOVE_NEXT:
                case ActionType.MOVE_PREVIOUS:
                    break;
            }
        }

        private void SetActionType(ActionType toset)
        {
            if (!setting)
            {
                setting = true;
                actTypes.SelectedItem = toset;
                switch (toset)
                {
                    case ActionType.DISASM_CHUNK:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.DISASM_FUNCTION:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.FIND_SEQUENCE:
                        enumpar.Enabled = false;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.FOLLOW_CALL:
                        enumpar.Enabled = false;
                        boolpar.Enabled = true;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.JUMP_KNOWN:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.RETURN_ADDRESS:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.RETURN_DATA:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = true;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.RETURN_DISP:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = true;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.SET_BACKWARDS:
                        enumpar.Enabled = false;
                        boolpar.Enabled = true;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.FUNC_FIND:
                        enumpar.Enabled = false;
                        boolpar.Enabled = false;
                        uintpar.Enabled = true;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.RETURN_TARGETADDRESS:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = true;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.TO_END:
                        enumpar.Enabled = false;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.TO_START:
                        enumpar.Enabled = false;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.SWITCH:
                        enumpar.Enabled = true;
                        boolpar.Enabled = true;
                        uintpar.Enabled = true;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.CONDITIONAL:
                        enumpar.Enabled = false;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = true;
                        ontrue.Enabled = true;
                        onfalse.Enabled = true;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.ASSIGN_VALUE:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = true;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.ASSIGN_KNOWN:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = true;
                        break;
                    case ActionType.CHECK_INSTRUCTION:
                        enumpar.Enabled = false;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = true;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.EXECUTE_ACTIONSLIST:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = true;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.TRY_ACTIONLIST:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = true;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.FOLLOW_JMP:
                        enumpar.Enabled = false;
                        boolpar.Enabled = true;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.RETURN_SCALE:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = true;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.RETURN_CHUNK_ADDRESS:
                        enumpar.Enabled = true;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                    case ActionType.FIND_LOOP:
                    case ActionType.MOVE_NEXT:
                    case ActionType.MOVE_PREVIOUS:
                        enumpar.Enabled = false;
                        boolpar.Enabled = false;
                        uintpar.Enabled = false;
                        seqPar.Enabled = false;
                        actlist.Enabled = false;
                        ontrue.Enabled = false;
                        onfalse.Enabled = false;
                        filterlist.Enabled = false;
                        knownpar2.Enabled = false;
                        break;
                }
                setting = false;
            }
        }

        private void actTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(actTypes.SelectedItem!=null)
                SetActionType((ActionType)actTypes.SelectedItem);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Sequence newseq=(Sequence)frmListBuilder.CreateList(typeof(Sequence));
            
            UpdateSequenceList();
            
            if (newseq != null)
            {
                seqPar.SelectedItem = newseq;
            }

            this.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_Action != null)
            {
                m_Action.name = ActionName.Text;
                if (m_Action.name == "")
                    m_Action.name = "Action_" + UOAIBasic.Action.actions.Count.ToString();
                m_Action.boolpar = boolpar.Checked;
                m_Action.m_type = (ActionType)actTypes.SelectedItem;
                m_Action.seqpar = (Sequence)seqPar.SelectedItem;
                uint.TryParse(uintpar.Text, out m_Action.uintpar);
                if(enumpar.SelectedItem!=null)
                    m_Action.knownpar = (UOCallibration.CallibratedFeatures)enumpar.SelectedItem;
                if(knownpar2.SelectedItem!=null)
                    m_Action.knownpar2 = (UOCallibration.CallibratedFeatures)knownpar2.SelectedItem;
                if (actlist.SelectedItem != null)
                    m_Action.actlist = (ActionList)actlist.SelectedItem;
                if (ontrue.SelectedItem != null)
                    m_Action.ontrue = (ActionList)ontrue.SelectedItem;
                if (onfalse.SelectedItem != null)
                    m_Action.onfalse = (ActionList)onfalse.SelectedItem;
                if (filterlist.SelectedItem != null)
                    m_Action.tocheck = (FilterList)filterlist.SelectedItem;
                m_Action.Update();
            }
            else
            {
                m_Action = new UOAIBasic.Action(ActionName.Text==""?"Action_" + UOAIBasic.Action.actions.Count.ToString():ActionName.Text);
                m_Action.boolpar = boolpar.Checked;
                m_Action.m_type = (ActionType)actTypes.SelectedItem;
                m_Action.seqpar = (Sequence)seqPar.SelectedItem;
                uint.TryParse(uintpar.Text, out m_Action.uintpar);
                if (enumpar.SelectedItem != null)
                    m_Action.knownpar = (UOCallibration.CallibratedFeatures)enumpar.SelectedItem;
                if (knownpar2.SelectedItem != null)
                    m_Action.knownpar2 = (UOCallibration.CallibratedFeatures)knownpar2.SelectedItem;
                if (actlist.SelectedItem != null)
                    m_Action.actlist = (ActionList)actlist.SelectedItem;
                if (ontrue.SelectedItem != null)
                    m_Action.ontrue = (ActionList)ontrue.SelectedItem;
                if (onfalse.SelectedItem != null)
                    m_Action.onfalse = (ActionList)onfalse.SelectedItem;
                if (filterlist.SelectedItem != null)
                    m_Action.tocheck = (FilterList)filterlist.SelectedItem;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m_Action = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            ActionList newal = (ActionList)frmListBuilder.CreateList(typeof(ActionList));

            UpdateActionLists();

            if (newal != null)
            {
                actlist.SelectedItem = newal;
            }

            this.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
            ActionList newal = (ActionList)frmListBuilder.CreateList(typeof(ActionList));

            UpdateActionLists();

            if (newal != null)
            {
                ontrue.SelectedItem = newal;
            }

            this.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Hide();
            ActionList newal = (ActionList)frmListBuilder.CreateList(typeof(ActionList));

            UpdateActionLists();

            if (newal != null)
            {
                onfalse.SelectedItem = newal;
            }

            this.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Hide();
            FilterList newfl = (FilterList)frmListBuilder.CreateList(typeof(FilterList));

            UpdateFilterList();

            if (newfl != null)
            {
                filterlist.SelectedItem = newfl;
            }

            this.Show();
        }
    }
}
