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
    public partial class dataFormatFr : Form
    {
        List<object> tranferColumns;
        public string cmdSQL = string.Empty;
        public List<int> indexList;
        public dataFormatFr(ref DataGridView Dg)
        {
            tranferColumns = new List<object>(Dg.Columns.Count);
            indexList = new List<int>();
            for (int i = 0; i < Dg.Columns.Count; i++)
            {
                tranferColumns.Add(Dg.Columns[i].Name);
            }
            InitializeComponent();
        }

        private void dataFormatFr_Load(object sender, EventArgs e)
        {
            foreach (var item in tranferColumns)
            {
                checkedListBox1.Items.Add(item);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbDBTable.Text))
            {
                MessageBox.Show("Please insert table name");
                return;
            }
            cmdSQL = string.Empty;
            if (checkedListBox1.Items.Count == 0)
            {
                MessageBox.Show("Can not generate SQL please create table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }


            cmdSQL += $"insert into {tbDBTable.Text}(";
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    indexList.Add(i);
                }
            }
            for (int i = 0; i < indexList.Count; i++)
            {
                if (i != indexList.Count - 1)
                {
                    cmdSQL += (checkedListBox1.Items[indexList[i]].ToString() + ",");
                }
                else
                {
                    cmdSQL += (checkedListBox1.Items[indexList[i]].ToString() + ") values('");
                }
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
