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
        public dataFormatFr(ref DataGridView Dg)
        {
            tranferColumns = new List<object>(Dg.Columns.Count);
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
            if (checkedListBox1.Items.Count == 0)
            {
                MessageBox.Show("Can not generate SQL please create table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            cmdSQL += "insert into ";
            MessageBox.Show(cmdSQL);
        }
    }
}
