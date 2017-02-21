using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace MachineKinematics
{
    public partial class MainForm : Form
    {
        public MainForm() { InitializeComponent(); }

        double L0 = 0; // Lob
        double L1 = 0; // Loa
        double L3 = 0; // Lbc
        double L5 = 0; // Lbs3

        // first step
        double Xa = 0;
        double Ya = 0;
        double l = 0;
        double cos_fi3 = 0;
        double sin_fi3 = 0;
        double fi3 = 0;
        double Xc = 0;
        double Yc = 0;
        double Xs3 = 0;
        double Ys3 = 0;

        // second step
        double Uax = 0; // Xa_dash
        double Uay = 0; // Ya_dash
        double Ua3a2 = 0;
        double i31 = 0;
        double Ucx = 0;  // Xc_dash
        double Ucy = 0;  // Yc_dash
        double Uc = 0;
        double Us3x = 0; // Xs3_dash
        double Us3y = 0; // Ys3_dash
        double Us3 = 0;
        double Us5 = 0;
        double i31_dash = 0;


        private void Form1_Load(object sender, EventArgs e)
        {
            dgvTitles.RowCount = dgvInput.RowCount = 13;
            dgvTitles.ColumnCount = dgvInput.ColumnCount = 1;
            dgvTitles.ColumnHeadersVisible =
                dgvInput.ColumnHeadersVisible =
                dgvTitles.RowHeadersVisible =
                dgvInput.RowHeadersVisible = false;
            dgvTitles.Width = dgvInput.Width = 80;
            dgvTitles.Columns[0].Width = dgvInput.Columns[0].Width = 77;
            dgvTitles.DefaultCellStyle.Font = new Font("Lucida Calligraphy", 12);
            for (int i = 0; i < 13; ++i)
                dgvTitles.Rows[i].Cells[0].Value = "Fpc" + (i + 1) + " =";
            dgvTitles.Enabled = false;

            dgvLegend.AutoSize = true;

            // run animation
            //this.tpAnimation.Paint += new PaintEventHandler(TabPage_Paint);

            //tabPage3
            tbResTitle1.Enabled = tbResTitle2.Enabled = tbResTitle3.Enabled = false;
        }

        protected void TabPage_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen arrow = new Pen(Brushes.Black, 2);
            arrow.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            for (int i = 0; i < 10; i += 5)
            {
                e.Graphics.DrawLine(arrow, 200 + i, 200 + i, 100 + i, 100 + i);
                Thread.Sleep(500);
            }
            arrow.Dispose();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void начатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Boolean flag = true;
            foreach (Control c in this.Controls)
            {
                if (c is TextBox)
                {
                    TextBox textBox = c as TextBox;
                    if (textBox.Text == string.Empty)
                    {
                        break;
                    }
                }
            }
            
            if(this.Controls.OfType<TextBox>().Any(t => string.IsNullOrEmpty(t.Text)))  
            {
                MessageBox.Show("Вы ");
            }
            else
            {
                MessageBox.Show("Вы ввели недостаточно данных.");
            }
        }

        private static Boolean checkInput()
        {
            //Boolean flag = true
            foreach (Control c in Form.ActiveForm.Controls)
            {
                if (c is TextBox)
                {
                    TextBox textBox = c as TextBox;
                    if (textBox.Text == string.Empty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void легендаОбозначенийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double OB = 0;
                double OA = 0;
                if (Double.TryParse(textBox2.Text, out OA) && Double.TryParse(textBox1.Text, out OB))
                {
                    double fi = Math.Acos(OA / OB) * (180 / Math.PI);
                    if (Double.IsNaN(fi) || Double.IsInfinity(fi))
                    {
                        throw new OutOfMemoryException("Угол φ не найден!");
                    }
                    else
                    {
                        textBox5.Text = Convert.ToString(fi);
                    }
                }
            }
            catch (Exception x) { }
        }

    }
}
