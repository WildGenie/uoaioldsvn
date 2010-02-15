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
    public partial class FilterBuilder : Form
    {
        public Filter m_Filter;
        private bool setting = false;

        public static void UpdateFilter(Filter toupdate)
        {
            FilterBuilder fb = new FilterBuilder(toupdate);
            fb.ShowDialog();
            return;
        }

        public static Filter CreateFilter()
        {
            FilterBuilder fb = new FilterBuilder(null);
            fb.ShowDialog();
            return fb.m_Filter;
        }

        public FilterBuilder(Filter tobuild)
        {
            InitializeComponent();

            foreach (FilterType ft in Enum.GetValues(typeof(FilterType)))
                chkFilterType.Items.Add(ft);

            InitializeFromFilter(tobuild);
        }

        public void UpdateFilterLists()
        {
            lstFilterLists.Items.Clear();
            foreach (FilterList fl in FilterList.filterlists)
                lstFilterLists.Items.Add(fl);
        }

        private void FillWithEnum(Type oftype)
        {
            chEnumPar.Items.Clear();
            foreach (object curobj in Enum.GetValues(oftype))
                chEnumPar.Items.Add(curobj);
            return;
        }

        private void SetFilterType(FilterType toset)
        {
            if (!setting)
            {
                setting = true;
                chkFilterType.SelectedItem = toset;
                switch (toset)
                {
                    case FilterType.ExplicitOpCount:
                        chEnumPar.Enabled = false;
                        chkDword1.Enabled = true;
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.FilterOr:
                        chEnumPar.Enabled = false;
                        chkDword1.Enabled = false;
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.OpCount:
                        chEnumPar.Enabled = false;
                        chkDword1.Enabled = true;
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.OpData:
                        chEnumPar.Enabled = false;
                        chkDword1.Enabled = true;
                        chkDword2.Enabled = true;
                        break;
                    case FilterType.OpDataType:
                        chEnumPar.Enabled = true;
                        chkDword1.Enabled = true;
                        FillWithEnum(typeof(x86_op_datatype));
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.OpKnownData:
                        chEnumPar.Enabled = true;
                        FillWithEnum(typeof(UOCallibration.CallibratedFeatures));
                        chkDword1.Enabled = true;
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.OpType:
                        chEnumPar.Enabled = true;
                        FillWithEnum(typeof(x86_op_type));
                        chkDword1.Enabled = true;
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.Size:
                        chEnumPar.Enabled = false;
                        chkDword1.Enabled = true;
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.Type:
                        chEnumPar.Enabled = true;
                        FillWithEnum(typeof(x86_insn_type));
                        chkDword1.Enabled = false;
                        chkDword2.Enabled = false;
                        break;
                    case FilterType.OpDataNotEqual:
                        chEnumPar.Enabled = false;
                        chkDword1.Enabled = true;
                        chkDword2.Enabled = true;
                        break;
                }
                setting = false;
            }
        }

        void InitializeFromFilter(Filter curFilter)
        {
            UpdateFilterLists();

            if ((m_Filter = curFilter) == null)
                return;

            FilterType curfiltertype = (FilterType)curFilter.type;

            SetFilterType(curfiltertype);

            chkName.Text = curFilter.name;
            
            switch (curfiltertype)
            {
                case FilterType.ExplicitOpCount:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    break;
                case FilterType.FilterOr:
                    lstFilterLists.SelectedItem = curFilter.filterlistpar;
                    break;
                case FilterType.OpCount:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    break;
                case FilterType.OpData:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    chkDword2.Text = curFilter.uintpar2.ToString();
                    break;
                case FilterType.OpDataNotEqual:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    chkDword2.Text = curFilter.uintpar2.ToString();
                    break;
                case FilterType.OpDataType:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    chEnumPar.SelectedItem = (x86_op_datatype)curFilter.enumpar;
                    break;
                case FilterType.OpKnownData:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    chEnumPar.SelectedItem = (UOCallibration.CallibratedFeatures)curFilter.enumpar;
                    break;
                case FilterType.OpType:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    chEnumPar.SelectedItem = (x86_op_type)curFilter.enumpar;
                    break;
                case FilterType.Size:
                    chkDword1.Text = curFilter.uintpar.ToString();
                    break;
                case FilterType.Type:
                    chEnumPar.SelectedItem = (x86_insn_type)curFilter.enumpar;
                    break;
            }

        }

        private void chkFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chkFilterType.SelectedItem != null)
                SetFilterType((FilterType)chkFilterType.SelectedItem);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_Filter != null)
            {
                m_Filter.name = chkName.Text == "" ? "Filter_" + Filter.filters.Count.ToString() : chkName.Text;
                m_Filter.filterlistpar = (FilterList)lstFilterLists.SelectedItem;
                m_Filter.type = (FilterType)chkFilterType.SelectedItem;
                switch (m_Filter.type)
                {
                    case FilterType.OpDataType:
                        m_Filter.enumpar = (uint)(x86_op_datatype)chEnumPar.SelectedItem;
                        break;
                    case FilterType.OpType:
                        m_Filter.enumpar = (uint)(x86_op_type)chEnumPar.SelectedItem;
                        break;
                    case FilterType.Type:
                        m_Filter.enumpar = (uint)(x86_insn_type)chEnumPar.SelectedItem;
                        break;
                    case FilterType.OpKnownData:
                        m_Filter.enumpar = (uint)(UOCallibration.CallibratedFeatures)chEnumPar.SelectedItem;
                        break;
                    default:
                        break;
                }
                uint.TryParse(chkDword1.Text, out m_Filter.uintpar);
                uint.TryParse(chkDword2.Text, out m_Filter.uintpar2);
                m_Filter.Update();
            }
            else
            {
                m_Filter = new Filter(chkName.Text == "" ? "Filter_" + Filter.filters.Count.ToString() : chkName.Text);
                m_Filter.filterlistpar = (FilterList)lstFilterLists.SelectedItem;
                m_Filter.type = (FilterType)chkFilterType.SelectedItem;
                switch (m_Filter.type)
                {
                    case FilterType.OpDataType:
                        m_Filter.enumpar = (uint)(x86_op_datatype)chEnumPar.SelectedItem;
                        break;
                    case FilterType.OpType:
                        m_Filter.enumpar = (uint)(x86_op_type)chEnumPar.SelectedItem;
                        break;
                    case FilterType.Type:
                        m_Filter.enumpar = (uint)(x86_insn_type)chEnumPar.SelectedItem;
                        break;
                    case FilterType.OpKnownData:
                        m_Filter.enumpar = (uint)(UOCallibration.CallibratedFeatures)chEnumPar.SelectedItem;
                        break;
                    default:
                        break;
                }
                uint.TryParse(chkDword1.Text, out m_Filter.uintpar);
                uint.TryParse(chkDword2.Text, out m_Filter.uintpar2);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            
            FilterList fl=(FilterList)frmListBuilder.CreateList(typeof(FilterList));

            UpdateFilterLists();

            if (fl != null)
                lstFilterLists.SelectedItem = fl;
            
            Show();
        }
    }
}
