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
        
        // input data
        double L0 = 0; // Lob
        double L1 = 0; // Loa
        double L3 = 0; // Lbc
        double L5 = 0; // Lbs3
        double fi_zero = 0; // pseudo-constant
        double fi_clone; // for iterations
        double m3 = 0;
        double m4 = 0;
        double Is4 = 0;
        double omega_1cp = 0;
        double delta = 0;
        double I_0_p = 0;

        double[] Fpc = new double[13]; // the forces of useful resistance

        // first step
        double[] Xa = new double[13];
        double[] Ya = new double[13];
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
            // tabPage1
            dgvTitles.RowCount = dgvInput.RowCount = 13;
            dgvTitles.ColumnCount = dgvInput.ColumnCount = 1;
            dgvTitles.ColumnHeadersVisible =
                dgvInput.ColumnHeadersVisible =
                dgvTitles.RowHeadersVisible =
                dgvInput.RowHeadersVisible = false;
            dgvTitles.Width = dgvInput.Width = 80;
            dgvTitles.Columns[0].Width = dgvInput.Columns[0].Width = 77;
            dgvTitles.DefaultCellStyle.Font = dgvInput.DefaultCellStyle.Font = new Font("Lucida Calligraphy", 12);
            for (int i = 0; i < 13; ++i)
                dgvTitles.Rows[i].Cells[0].Value = "Fpc" + (i + 1) + " =";
            dgvTitles.Enabled = false;
            cbDirection.SelectedIndex = 0;

            lbl1.Text = lbl2.Text = lbl3.Text = lbl4.Text = lbl5.Text = lbl6.Text = lbl7.Text =
                lbl8.Text = lbl9.Text = lbl10.Text = lbl11.Text = "\u2715";

            for (int i = 0; i <= 12; ++i)
                if (i > 0 && i < 6)
                {
                    dgvInput.Rows[i].Cells[0].Value = 2000;
                    Fpc[i] = 2000F;
                }
                else
                {
                    dgvInput.Rows[i].Cells[0].Value = 100;
                    Fpc[i] = 100F;
                }

            // run animation
            //this.tpAnimation.Paint += new PaintEventHandler(TabPage_Paint);

            // tabPage2

            // tabPage3
            tbResTitle1.Enabled = tbResTitle2.Enabled = tbResTitle3.Enabled = false;

            // tabPage5
            dgvLegend.AutoSize = true;
        }

        protected void TabPage_Paint(object sender, PaintEventArgs e)
        {
            /*
            base.OnPaint(e);
            Pen arrow = new Pen(Brushes.Black, 2);
            arrow.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            for (int i = 0; i < 10; i += 5)
            {
                e.Graphics.DrawLine(arrow, 200 + i, 200 + i, 100 + i, 100 + i);
                Thread.Sleep(500);
            }
            arrow.Dispose();
            */
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void начатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnFi_Click(this, e);

            if (checkInput())
            {
                MessageBox.Show("Поехали!");

                Double.TryParse(textBox1.Text , out L0); // Lob
                Double.TryParse(textBox2.Text , out L1); // Loa
                Double.TryParse(textBox3.Text , out L3); // Lbc
                Double.TryParse(textBox4.Text , out L5); // Lbs3

                Double.TryParse(textBox5.Text , out fi_zero); // pseudo-constant
                fi_clone = fi_zero;                           // for iterations
                
                Double.TryParse(textBox6.Text , out m3);
                Double.TryParse(textBox7.Text , out m4);
                Double.TryParse(textBox8.Text , out Is4);
                
                Double.TryParse(textBox9.Text , out omega_1cp);
                
                Double.TryParse(textBox10.Text , out delta);
                
                Double.TryParse(textBox11.Text , out I_0_p);

                dgvResults.ColumnCount = 3;
                dgvResults.RowCount = 15;
                int i = 0;
                double fi_1 = 0F;

                switch (cbDirection.SelectedIndex)
                { 
                    case 0:
                        {
                            MessageBox.Show("По часовой стрелке пока не реализовано. Вебертие \"Против...\"");

                            break;
                        }
                    case 1:
                        {
                            for (fi_1 = fi_clone; i < 13; fi_1 += 30F, ++i)
                            {
                                if (fi_1 > 360)
                                {
                                    fi_1 = 330 - fi_clone;
                                }

                                dgvResults.Rows[i].Cells[0].Value = fi_1;
                                dgvResults.Rows[i].Cells[1].Value = Xa[i] = L1 * Math.Cos(fi_1);
                                dgvResults.Rows[i].Cells[2].Value = Ya[i] = L1 * Math.Sin(fi_1);
                            }

                            break;
                        }

                    default: MessageBox.Show(""); break;
                }

                




            }
            else
            {
                MessageBox.Show("Что-то пошло не так!");
            }
        }
        
        private Boolean checkInput()
        {
            if (lbl1.Text == "\u2714" && lbl2.Text == "\u2714" && lbl3.Text == "\u2714"
                && lbl4.Text == "\u2714" && lbl5.Text == "\u2714" && lbl6.Text == "\u2714"
                && lbl7.Text == "\u2714" && lbl8.Text == "\u2714" && lbl9.Text == "\u2714"
                && lbl10.Text == "\u2714" && lbl11.Text == "\u2714")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        
        private void легендаОбозначенийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
        }

/*=================================================================================================== 
 *=== Validate input data  
 *===================================================================================================*/

        private void dgvInput_CurrentCellChanged(object sender, EventArgs e)
        {
            double temp = 0;
            for (int i = 0; i <= 12; ++i)
                if ((Convert.ToString(dgvInput.Rows[i].Cells[0].Value) != ""))
                    if (Double.TryParse(Convert.ToString(dgvInput.Rows[i].Cells[0].Value), out temp))
                        Fpc[i] = Convert.ToDouble(dgvInput.Rows[i].Cells[0].Value);
                    else
                    {
                        dgvInput.Rows[i].Cells[0].Value = Convert.ToString(0);
                        Fpc[i] = 0;
                        MessageBox.Show("Допускаются лишь следующие символы: {0-9}, {.}, {-}.");
                    }
        }
        
        private void btnFi_Click(object sender, EventArgs e)
        {
            double OB = 0;
            double OA = 0;
            if (Double.TryParse(textBox2.Text, out OA) && Double.TryParse(textBox1.Text, out OB))
            {
                double fi = 270 + Math.Acos(OA / OB) * (180 / Math.PI);
                if (Double.IsNaN(fi) || Double.IsInfinity(fi))
                {
                    throw new ArgumentException("Угол φ не найден.\nПрограмма обнуляет значения.\nПопробуйте снова.");
                }
                else
                {
                    //textBox5.Text = Convert.ToString(fi);
                    textBox5.Text = String.Format("{0:0.##########}", fi); // ten character before point
                    lbl5.Text = "\u2714";
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox1.Text, out temp) && textBox1.Text.Length != 0)
                lbl1.Text = "\u2714";
            else
                lbl1.Text = "\u2715";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox2.Text, out temp) && textBox2.Text.Length != 0)
                lbl2.Text = "\u2714";
            else
                lbl2.Text = "\u2715";
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox3.Text, out temp) && textBox3.Text.Length != 0)
                lbl3.Text = "\u2714";
            else
                lbl3.Text = "\u2715";
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox4.Text, out temp) && textBox4.Text.Length != 0)
                lbl4.Text = "\u2714";
            else
                lbl4.Text = "\u2715";
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox5.Text, out temp) && textBox5.Text.Length != 0)
                lbl5.Text = "\u2714";
            else
                lbl5.Text = "\u2715";
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox6.Text, out temp) && textBox6.Text.Length != 0)
                lbl6.Text = "\u2714";
            else
                lbl6.Text = "\u2715";
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox7.Text, out temp) && textBox7.Text.Length != 0)
                lbl7.Text = "\u2714";
            else
                lbl7.Text = "\u2715";
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox8.Text, out temp) && textBox8.Text.Length != 0)
                lbl8.Text = "\u2714";
            else
                lbl8.Text = "\u2715";
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox9.Text, out temp) && textBox9.Text.Length != 0)
                lbl9.Text = "\u2714";
            else
                lbl9.Text = "\u2715";
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox10.Text, out temp) && textBox10.Text.Length != 0)
                lbl10.Text = "\u2714";
            else
                lbl10.Text = "\u2715";
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            double temp = 0;
            if (Double.TryParse(textBox11.Text, out temp) && textBox11.Text.Length != 0)
                lbl11.Text = "\u2714";
            else
                lbl11.Text = "\u2715";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || e.KeyChar == ',' || e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }
    }
}
