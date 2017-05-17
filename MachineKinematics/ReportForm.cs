using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MachineKinematics
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            
        }

        public void ShowReportForm(String _title, DataGridView _dgv)
        {
            this.Text = _title;
            dgvResults.RowCount = _dgv.RowCount;
            dgvResults.ColumnCount = _dgv.ColumnCount;
            dgvResults.DataSource = _dgv.DataSource;
            this.Show();
        }
    }
}
