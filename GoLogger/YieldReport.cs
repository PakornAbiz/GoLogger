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
    public partial class YieldReport : Form
    {
        List<object> tranferColumns;
        public YieldReport(ref DataGridView Dg)
        {
            tranferColumns = new List<object>(Dg.Columns.Count);
            for (int i = 0; i < Dg.Columns.Count; i++)
            {
                tranferColumns.Add(Dg.Columns[i].Name);
            }
            InitializeComponent();
        }

        private void YieldReport_Load(object sender, EventArgs e)
        {
            foreach (var item in tranferColumns)
            {
                clbColumns.Items.Add(item);
            }
        }

        private void clbColumns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked && clbColumns.CheckedItems.Count >= 1)
            {
                e.NewValue = CheckState.Unchecked;
            }
        }
    }
}
