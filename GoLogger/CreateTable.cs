using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoLogger
{
    public partial class CreateTable : Form
    {
        public CreateTable()
        {
            InitializeComponent();
        }
        public List<int> GetDivisionIndex()
        {
            List<int> indexChecked = new List<int>();
            for (int i = 2; i < clbTable.Items.Count; i++)
            {
                if (clbTable.GetItemChecked(i))
                {
                    indexChecked.Add(i);
                }
            }
            return indexChecked;
        }
        public int GetTotalColumn() => clbTable.Items.Count;
        public List<string> GetColumn()
        {
            List<string> columnList = new List<string>();
            foreach (string item in clbTable.Items)
            {
                columnList.Add(item);
            }
            return columnList;
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbAdd.Text))
            {
                clbTable.Items.Add(tbAdd.Text);
                tbAdd.Clear();
            }        
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (clbTable.Items.Count > 0)
            {
                clbTable.Items.RemoveAt(clbTable.SelectedIndex);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void tbAdd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnAdd_Click(sender, e);
            }
        }
    }
}
