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
        double fi_clone = 0; // for iterations
        double[] fi_array = new double[13];
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
        double[] L = new double[13];
        double[] cos_fi3 = new double[13];
        double[] sin_fi3 = new double[13];
        double[] fi3 = new double[13];
        double[] Xc = new double[13];
        double[] Yc = new double[13];
        double[] Xs3 = new double[13];
        double[] Ys3 = new double[13];

        // second step
        double[] Xa_dash = new double[13]; // Uax 
        double[] Ya_dash = new double[13]; // Uay 
        double[] Ua3a2 = new double[13];
        double[] i31 = new double[13];
        double[] Xc_dash = new double[13];  // Ucx
        double[] Yc_dash = new double[13];  // Ucy 
        double[] Uc = new double[13];
        double[] Xs3_dash = new double[13]; // Us3x 
        double[] Ys3_dash = new double[13]; // Us3y
        double[] Us3 = new double[13];
        double[] Us5 = new double[13];
        double[] i31_dash = new double[13];

        // added
        double[] Xs3_doubledash = new double[13];
        double[] Ys3_doubledash = new double[13];
        double[] is51_dash = new double[13];

        private void forDebug()
        {
            textBox1.Text = "0,25";
            textBox2.Text = "0,1";
            textBox3.Text = "0,2";
            textBox4.Text = "0,4";
            textBox6.Text = "0,5";
            textBox7.Text = "0,5";
            textBox8.Text = "10";
            textBox9.Text = "100";
            textBox10.Text = "0,05";
            textBox11.Text = "0,1";
        }

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

            forDebug();

            // run animation
            //this.tpAnimation.Paint += new PaintEventHandler(TabPage_Paint);

            // tabPage2

            // tabPage3
            tbResTitle1.Enabled = tbResTitle2.Enabled = tbResTitle3.Enabled = false;

            // tabPage5
            fillDgvLegend();            
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
                fillColumnHeaderResult();

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

                int i = 0;
                double fi_1 = 0F;
                for (fi_1 = fi_clone; i < 13; ++i)
                {
                    if (fi_1 < 0)
                    {
                        fi_1 = fi_1 + 360;
                    }

                    if (fi_1 > 360)
                    {
                        fi_1 = fi_1 - 360;
                    }

// =============== first part of calculating =================
                    try
                    {
                        dgvResults.Rows[i].HeaderCell.Value = String.Format("{0:0.####}", fi_1);
                        fi_array[i] = fi_1;

                        Xa[i] = L1 * Math.Cos(fi_1);
                        Ya[i] = L1 * Math.Sin(fi_1);

                        L[i] = Math.Sqrt(Math.Pow(L0, 2) + Math.Pow(L1, 2) + 2 * L0 * L1 * Math.Sin(fi_1));

                        cos_fi3[i] = (L1 * Math.Cos(fi_1)) / L[i];
                        sin_fi3[i] = (L0 + L1 * Math.Sin(fi_1)) / L[i];

                        fi3[i] = Math.Acos((L1 * Math.Cos(fi_1)) / L[i]);

                        Xc[i] = L3 * cos_fi3[i];
                        Yc[i] = L3 * sin_fi3[i];
                        Xs3[i] = L5 * cos_fi3[i];
                        Ys3[i] = L5 * sin_fi3[i];
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message); }

// =============== second part of calculating =================
                    try
                    {
                        Xa_dash[i] = -L1 * Math.Sin(fi_1);
                        Ya_dash[i] = L1 * Math.Cos(fi_1);

                        Ua3a2[i] = -L1 * Math.Sin(fi_1 - fi3[i]);
                        i31[i] = (L1 / L[i]) * Math.Cos(fi_1 - fi3[i]);
                        dgvResults.Rows[i].Cells[2].Value = String.Format("{0:0.####}", i31[i]);

                        Xc_dash[i] = (-i31[i]) * L3 * sin_fi3[i];
                        Yc_dash[i] = i31[i] * L3 * cos_fi3[i];
                        Uc[i] = i31[i] * L5;

                        Xs3_dash[i] = (-i31[i]) * L5 * sin_fi3[i];
                        dgvResults.Rows[i].Cells[4].Value = String.Format("{0:0.####}", Xs3_dash[i]);
                        Ys3_dash[i] = i31[i] * L5 * cos_fi3[i];
                        dgvResults.Rows[i].Cells[5].Value = String.Format("{0:0.####}", Ys3_dash[i]);
                        Us3[i] = i31[i] * L5;

                        Us5[i] = Xc_dash[i];

                        i31_dash[i] = (Math.Pow(i31[i], 2) * sin_fi3[i] - (L1 * Math.Sin(fi_1) / L[i])) / cos_fi3[i];
                        dgvResults.Rows[i].Cells[3].Value = String.Format("{0:0.####}", i31_dash[i]);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message); }

// =============== added part of calculating =================
                    try
                    {
                        Xs3_doubledash[i] = (-L5) * (Math.Pow(i31[i], 2) * cos_fi3[i] + i31_dash[i] * sin_fi3[i]);
                        dgvResults.Rows[i].Cells[6].Value = String.Format("{0:0.####}", Xs3_doubledash[i]);
                        Ys3_doubledash[i] = L5 * (i31_dash[i] * cos_fi3[i] - Math.Pow(i31[i], 2) * sin_fi3[i]);
                        dgvResults.Rows[i].Cells[7].Value = String.Format("{0:0.####}", Ys3_doubledash[i]);
                        is51_dash[i] = (-L3) * (Math.Pow(i31[i], 2) * cos_fi3[i] + i31_dash[i] * sin_fi3[i]);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message); }

                    if (cbDirection.SelectedIndex == 0)
                        fi_1 -= 30;
                    else
                        fi_1 += 30;
                }

                tabControl1.SelectedIndex = 2;
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

        private void fillColumnHeaderResult()
        {
            dgvResults.ColumnCount = 8;
            dgvResults.RowCount = 13;
            dgvResults.RowHeadersWidth = 90;
            for (int i = 0; i < dgvResults.ColumnCount; ++i)
                dgvResults.Columns[i].Width = 60;

            dgvResults.Columns[0].HeaderText = "φo";
            dgvResults.Columns[1].HeaderCell.Value = "Sd";
            dgvResults.Columns[2].HeaderCell.Value = "i31";
            dgvResults.Columns[3].HeaderCell.Value = "i31p";
            dgvResults.Columns[4].HeaderCell.Value = "x3p";
            dgvResults.Columns[5].HeaderCell.Value = "y3p";
            dgvResults.Columns[6].HeaderCell.Value = "x3pp";
            dgvResults.Columns[7].HeaderCell.Value = "y3pp";
        }
        
        private void легендаОбозначенийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
        }

/*=================================================================================================== 
 *=== Build charts  
 *===================================================================================================*/

        private void btnChart_sd_i51_i51P_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_i21_i21P_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_xs2p_ys2p_xs2pp_ys2pp_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();

            chart1.Series.Add("Xs2p").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ys2p").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Xs2pp").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ys2pp").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

            //chart1.DataSource = dgvResults.DataSource;
            for (int i = 0; i < dgvResults.RowCount; ++i)
            {
                chart1.Series["Xs2p"].Points.AddXY(fi_array[i], Xs3_dash[i]);
                chart1.Series["Ys2p"].Points.AddXY(fi_array[i], Ys3_dash[i]);
                chart1.Series["Xs2pp"].Points.AddXY(fi_array[i], Xs3_doubledash[i]);
                chart1.Series["Ys2pp"].Points.AddXY(fi_array[i], Ys3_doubledash[i]);
                //chart1.Series["Xs2p"].Points.AddXY(Xs3_dash[i], Ys3_dash[i]);
                /*
                chart1.Series["Xs2p"].Points.AddXY(dgvResults.Rows[i].Cells[4].Value, dgvResults.Rows[i].Cells[5].Value);
                chart1.Series["Xs2pp"].Points.AddXY(dgvResults.Rows[i].Cells[6].Value, dgvResults.Rows[i].Cells[7].Value);
                */
                //chart1.Series["Xs2p"].Points.Add(Xs3_dash);
                //chart1.Series["Ys2p"].Points.Add(Ys3_dash);
                //chart1.Series["Xs2pp"].Points.AddXY(dgvResults.Rows[i].Cells[6].Value, dgvResults.Rows[i].Cells[7].Value);
            }
            
            //chart1.Series["Xs2p"].Points.Add(Xs3_dash);
            //chart1.Series["Ys2p"].Points.Add(Ys3_dash);
        }

        private void btnChart_i2p_A_B_C_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_Mcp_Mdp_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_dT_dTi_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_T2_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_e1_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_omega1_Click(object sender, EventArgs e)
        {

        }

        private void btnChart_Ac_Ad_Click(object sender, EventArgs e)
        {

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
                        MessageBox.Show("Допускаются лишь следующие символы: {0-9}, {,}.");
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
                if(temp >= 0 && temp < 1)
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

/*=================================================================================================== 
 *=== fill dgvLegend 
 *===================================================================================================*/

        private void fillDgvLegend()
        {
            // ₁ ₂ ₃ ₄ ₅ φ ′ ᴵ Δ ε ω ᶜ
            dgvLegend.Font = new Font("Consolas", 12);
            dgvLegend.RowCount = 30;

            dgvLegend.Rows[0].Cells[0].Value = "Кинематические характеристики исполнительного механизма";

            dgvLegend.Rows[1].Cells[0].Value = "Обобщённая координата";
            dgvLegend.Rows[1].Cells[1].Value = "φ₁";
            dgvLegend.Rows[1].Cells[2].Value = "Fi";

            dgvLegend.Rows[2].Cells[0].Value = "Перемещение ползуна";
            dgvLegend.Rows[2].Cells[1].Value = "Sь";
            dgvLegend.Rows[2].Cells[2].Value = "SB";

            dgvLegend.Rows[3].Cells[0].Value = "Передаточная функция кулисы";
            dgvLegend.Rows[3].Cells[1].Value = "i₃₁";
            dgvLegend.Rows[3].Cells[2].Value = "i31";

            dgvLegend.Rows[4].Cells[0].Value = "Передаточная функция шатуна";
            dgvLegend.Rows[4].Cells[1].Value = "i₄₁";
            dgvLegend.Rows[4].Cells[2].Value = "i41";

            dgvLegend.Rows[5].Cells[0].Value = "Передаточная функция ползуна";
            dgvLegend.Rows[5].Cells[1].Value = "i₅₁";
            dgvLegend.Rows[5].Cells[2].Value = "i51";

            dgvLegend.Rows[6].Cells[0].Value = "Проекция аналога скорости точки S₃ на ось x";
            dgvLegend.Rows[6].Cells[1].Value = "X′s₃";
            dgvLegend.Rows[6].Cells[2].Value = "xS3P";

            dgvLegend.Rows[7].Cells[0].Value = "Проекция аналога скорости точки S₃ на ось y";
            dgvLegend.Rows[7].Cells[1].Value = "Y′s₃";
            dgvLegend.Rows[7].Cells[2].Value = "yS3P";

            dgvLegend.Rows[8].Cells[0].Value = "Проекция аналога скорости точки S₄ на ось x";
            dgvLegend.Rows[8].Cells[1].Value = "X′s₄";
            dgvLegend.Rows[8].Cells[2].Value = "yS4P";

            dgvLegend.Rows[9].Cells[0].Value = "Проекция аналога скорости точки S₄ на ось y";
            dgvLegend.Rows[9].Cells[1].Value = "Y′s₄";
            dgvLegend.Rows[9].Cells[2].Value = "yS4P";

            dgvLegend.Rows[10].Cells[0].Value = "Производная передаточной функции шатуна";
            dgvLegend.Rows[10].Cells[1].Value = "i′₃₁ + i′₄₁";
            dgvLegend.Rows[10].Cells[2].Value = "i31P + i41P";

            dgvLegend.Rows[11].Cells[0].Value = "Производная передаточной функции ползуна";
            dgvLegend.Rows[11].Cells[1].Value = "i′₅₁ + i′₅₁";
            dgvLegend.Rows[11].Cells[2].Value = "i51P + i51";

            dgvLegend.Rows[12].Cells[0].Value = "Проекция аналога ускорения точки S₂ на ось x";
            dgvLegend.Rows[12].Cells[1].Value = "X′′s₂";
            dgvLegend.Rows[12].Cells[2].Value = "yS2PP";

            dgvLegend.Rows[13].Cells[0].Value = "Проекция аналога ускорения точки S₂ на ось y";
            dgvLegend.Rows[13].Cells[1].Value = "Y′′s₂";
            dgvLegend.Rows[13].Cells[2].Value = "yS2PP";

            dgvLegend.Rows[14].Cells[0].Value = "Переменная составляющая приведенного момента инерции";

            dgvLegend.Rows[15].Cells[0].Value = "Часть Iɪɪᴵᴵ от массы шатуна";
            dgvLegend.Rows[15].Cells[1].Value = "A";
            dgvLegend.Rows[15].Cells[2].Value = "A";

            dgvLegend.Rows[16].Cells[0].Value = "Часть Iɪɪᴵᴵ от момента инерции шатуна";
            dgvLegend.Rows[16].Cells[1].Value = "B";
            dgvLegend.Rows[16].Cells[2].Value = "B";

            dgvLegend.Rows[17].Cells[0].Value = "Часть Iɪɪᴵᴵ от массы ползуна";
            dgvLegend.Rows[17].Cells[1].Value = "C";
            dgvLegend.Rows[17].Cells[2].Value = "C";

            dgvLegend.Rows[18].Cells[0].Value = "Часть Iɪɪᴵᴵ от массы кулисы";
            dgvLegend.Rows[18].Cells[1].Value = "D";
            dgvLegend.Rows[18].Cells[2].Value = "D";

            dgvLegend.Rows[19].Cells[0].Value = "Часть Iɪɪᴵᴵ от момента инерции кулисы";
            dgvLegend.Rows[19].Cells[1].Value = "E";
            dgvLegend.Rows[19].Cells[2].Value = "E";

            dgvLegend.Rows[20].Cells[0].Value = "Переменная составляющая приведенного момента инерции";
            dgvLegend.Rows[20].Cells[1].Value = "Iɪɪᴵᴵ";
            dgvLegend.Rows[20].Cells[2].Value = "I2p";

            dgvLegend.Rows[21].Cells[0].Value = "Производная произведённого момента инерции";
            dgvLegend.Rows[21].Cells[1].Value = "dIɪɪ / dφ₁";
            dgvLegend.Rows[21].Cells[2].Value = "dIpdFi";

            dgvLegend.Rows[22].Cells[0].Value = "Определение закона движения звена произведения";

            dgvLegend.Rows[23].Cells[0].Value = "Приведённый момент сил сопротивления";
            dgvLegend.Rows[23].Cells[1].Value = "Mɪɪᶜ";
            dgvLegend.Rows[23].Cells[2].Value = "Mcp";

            dgvLegend.Rows[24].Cells[0].Value = "Работа сил сопротивления";
            dgvLegend.Rows[24].Cells[1].Value = "Ac";
            dgvLegend.Rows[24].Cells[2].Value = "Ac";

            dgvLegend.Rows[25].Cells[0].Value = "Работа движущих сил";
            dgvLegend.Rows[25].Cells[1].Value = "Aд";
            dgvLegend.Rows[25].Cells[2].Value = "Ad";

            dgvLegend.Rows[26].Cells[0].Value = "Изменение кинеической энергии машины";
            dgvLegend.Rows[26].Cells[1].Value = "ΔT";
            dgvLegend.Rows[26].Cells[2].Value = "dT";

            dgvLegend.Rows[27].Cells[0].Value = "Изменение кинеической энергии пост. сост-ей прив-го момента инерции";
            dgvLegend.Rows[27].Cells[1].Value = "ΔTɪ";
            dgvLegend.Rows[27].Cells[2].Value = "dTI";

            dgvLegend.Rows[28].Cells[0].Value = "Угловая скорость кривошипа";
            dgvLegend.Rows[28].Cells[1].Value = "ω₁";
            dgvLegend.Rows[28].Cells[2].Value = "w1";

            dgvLegend.Rows[29].Cells[0].Value = "Угловое ускорение кривошипа";
            dgvLegend.Rows[29].Cells[1].Value = "ε₁";
            dgvLegend.Rows[29].Cells[2].Value = "e1";
        }
    }
}
