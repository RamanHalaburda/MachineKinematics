using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;

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

        public void ShowReportForm_Idif(String _title, DataGridView _dgv, double[] _first, double[] _second)
        {
            // for DataGridView
            this.Text = _title;
            dgvResults.RowCount = _dgv.RowCount;
            dgvResults.ColumnCount = 2;
            dgvResults.DataSource = _dgv.DataSource;
            for (int i = 0; i < 13; ++i)
            {
                dgvResults.Rows[i].Cells[0].Value = _dgv.Rows[i].Cells[9].Value;
                dgvResults.Rows[i].Cells[1].Value = _dgv.Rows[i].Cells[10].Value;
            }

            // style of DataGridView
            for (int i = 0; i < dgvResults.ColumnCount; ++i)
            {
                dgvResults.Columns[i].Width = 60;
            }
            string title_first = (string)_dgv.Columns[9].HeaderCell.Value;
            string title_second = (string)_dgv.Columns[10].HeaderCell.Value;
            dgvResults.Columns[0].HeaderCell.Value = title_first;
            dgvResults.Columns[1].HeaderCell.Value = title_second;
            dgvResults.RowHeadersVisible = false;

            // for Chart
            //chart1.Series.Dispose();
            chart1.Series.Clear();
            chart1.Series.Add(title_first).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add(title_second).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < 181; ++i)
            {
                chart1.Series[title_first].Points.AddXY(i, _first[i]);
                chart1.Series[title_second].Points.AddXY(i, _second[i]);
            }

            // show ReportForm
            this.Show();
        }

        public void ShowReportForm(String _title, DataGridView _dgv, double[] _first, double[] _second, double[] _third)
        {
            // for DataGridView
            this.Text = _title;
            dgvResults.RowCount = _dgv.RowCount;
            dgvResults.ColumnCount = 3;
            dgvResults.DataSource = _dgv.DataSource;
            for (int i = 0; i < 13; ++i)
            {
                dgvResults.Rows[i].Cells[0].Value = _dgv.Rows[i].Cells[11].Value;
                dgvResults.Rows[i].Cells[1].Value = _dgv.Rows[i].Cells[8].Value;
                dgvResults.Rows[i].Cells[2].Value = _dgv.Rows[i].Cells[12].Value;
            }

            // style of DataGridView
            for (int i = 0; i < dgvResults.ColumnCount; ++i)
            {
                dgvResults.Columns[i].Width = 60;
            }
            string title_first = (string)_dgv.Columns[11].HeaderCell.Value;
            string title_second = (string)_dgv.Columns[8].HeaderCell.Value;
            string title_third = (string)_dgv.Columns[12].HeaderCell.Value;
            dgvResults.Columns[0].HeaderCell.Value = title_first;
            dgvResults.Columns[1].HeaderCell.Value = title_second;
            dgvResults.Columns[2].HeaderCell.Value = title_third;
            dgvResults.RowHeadersVisible = false;

            // for Chart
            //chart1.Series.Dispose();
            chart1.Series.Clear();
            chart1.Series.Add(title_first).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add(title_second).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add(title_third).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < 181; ++i)
            {
                chart1.Series[title_first].Points.AddXY(i, _first[i]);
                chart1.Series[title_second].Points.AddXY(i, _second[i]);
                chart1.Series[title_third].Points.AddXY(i, _third[i]);
            }

            // show ReportForm
            this.Show();
        }

        public void ShowReportForm_OmegaEps(String _title, DataGridView _dgv, double[] _first, double[] _second)
        {
            // for DataGridView
            this.Text = _title;
            dgvResults.RowCount = _dgv.RowCount;
            dgvResults.ColumnCount = 2;
            dgvResults.DataSource = _dgv.DataSource;
            for (int i = 0; i < 13; ++i)
            {
                dgvResults.Rows[i].Cells[0].Value = _dgv.Rows[i].Cells[13].Value;
                dgvResults.Rows[i].Cells[1].Value = _dgv.Rows[i].Cells[14].Value;
            }

            // style of DataGridView
            for (int i = 0; i < dgvResults.ColumnCount; ++i)
            {
                dgvResults.Columns[i].Width = 60;
            }
            string title_first = (string)_dgv.Columns[13].HeaderCell.Value;
            string title_second = (string)_dgv.Columns[14].HeaderCell.Value;
            dgvResults.Columns[0].HeaderCell.Value = title_first;
            dgvResults.Columns[1].HeaderCell.Value = title_second;
            dgvResults.RowHeadersVisible = false;

            // for Chart
            //chart1.Series.Dispose();
            chart1.Series.Clear();
            chart1.Series.Add(title_first).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add(title_second).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            for (int i = 0; i < 181; ++i)
            {
                chart1.Series[title_first].Points.AddXY(i, _first[i]);
                chart1.Series[title_second].Points.AddXY(i, _second[i]);
            }

            // show ReportForm
            this.Show();
        }

        // print
        private PrintDocument printDocument = new PrintDocument();
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 312 438
                button1.Visible = false;
                dgvResults.Height = 438;

                CaptureScreen();
                printDocument.Print();
                
                button1.Visible = true;
                dgvResults.Height = 312;
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "У вас нет доступных принтеров.",
                    "Ошибка подключения к принтеру",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification);
            }
        }

        private void CaptureScreen()
        {
            Graphics myGraphics = this.CreateGraphics();
            Size s = this.Size;
            Bitmap memoryImage = new Bitmap(s.Width, s.Height, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, s);
        }
    }
}
