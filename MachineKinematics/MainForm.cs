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
        const double delta_fi = 2F;
        const UInt16 dimension = 181;
        double[] fi_array = new double[dimension];
        double m3 = 0;
        double m4 = 0;
        double Is4 = 0;
        double omega_1cp = 0;
        double delta = 0;
        double I_0_p = 0;

        double[] Fpc = new double[dimension]; // the forces of useful resistance

        // first step
        double[] Xa = new double[dimension];
        double[] Ya = new double[dimension];
        double[] L = new double[dimension];
        double[] cos_fi3 = new double[dimension];
        double[] sin_fi3 = new double[dimension];
        double[] fi3 = new double[dimension];
        double[] Xc = new double[dimension];
        double[] Yc = new double[dimension];
        double[] Xs3 = new double[dimension];
        double[] Ys3 = new double[dimension];

        // second step
        double[] Xa_dash = new double[dimension]; // Uax 
        double[] Ya_dash = new double[dimension]; // Uay 
        double[] Ua3a2 = new double[dimension];
        double[] i31 = new double[dimension];
        double[] Sd = new double[dimension]; // SD 'перемещение ползуна'
        double[] Xc_dash = new double[dimension];  // Ucx
        double[] Yc_dash = new double[dimension];  // Ucy 
        double[] Uc = new double[dimension];
        double[] Xs3_dash = new double[dimension]; // Us3x 
        double[] Ys3_dash = new double[dimension]; // Us3y
        double[] Us3 = new double[dimension];
        double[] Us5 = new double[dimension];
        double[] i31_dash = new double[dimension];

        // added
        double[] Xs3_doubledash = new double[dimension];
        double[] Ys3_doubledash = new double[dimension];
        double[] is51_dash = new double[dimension];

        private void forDebug()
        {
            textBox1.Text = "0,3457";
            textBox2.Text = "0,15";
            textBox3.Text = "0,6914";
            textBox4.Text = "0,6";
            textBox6.Text = "1";
            textBox7.Text = "1";
            textBox8.Text = "10";
            textBox9.Text = "9,42";
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
            cbDirection.SelectedIndex = 1;

            lbl1.Text = lbl2.Text = lbl3.Text = lbl4.Text = lbl5.Text = lbl6.Text = lbl7.Text =
                lbl8.Text = lbl9.Text = lbl10.Text = lbl11.Text = "\u2715";

            fillDgvInput();
            forDebug();

            // tabPage2

            // tabPage3
            tbResTitle1.Enabled = tbResTitle2.Enabled = tbResTitle3.Enabled = false;

            // tabPage5
            fillDgvLegend();
        }

        protected void TabPage_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen arrow = new Pen(Brushes.Black, 1);
            arrow.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            int scale = 400;
            int scale_L0_OB = (int)(L0 * (double)scale);
            int scale_L1_OA = (int)(L1 * (double)scale);
            int scale_L3_BC = (int)(L3 * (double)scale);
            int scale_L3_BS3 = (int)(L5 * (double)scale);

            int x_mid = gbAnimation.Width / 2;
            int x_left = 20;
            int x_right = gbAnimation.Width - 20;
            int y_top = 20;
            int y_bottom = gbAnimation.Height - 20;

            int fix_x_bottom = x_mid;
            int fix_y_bottom = y_bottom - 10;

            int fix_x_mid = x_mid;
            int fix_y_mid = y_bottom - 10 - scale_L0_OB;


            // draw bottom fix point
            e.Graphics.DrawLine(arrow, x_mid - 15, y_bottom + 10, x_mid + 15, y_bottom + 10);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10, x_mid - 9, y_bottom + 10);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10, x_mid + 9, y_bottom + 10);
            e.Graphics.DrawEllipse(arrow, x_mid - 3, y_bottom - 10 - 3, 6, 6);
            for (int i = (x_mid - 15); i <= (x_mid + 15 - 5); i = i + 5)
                e.Graphics.DrawLine(arrow, i, y_bottom + 15, 5 + i, y_bottom + 10);

            // draw fixed point for top slider

            // left top line
            e.Graphics.DrawLine(arrow, x_left + 200, y_top + 10, x_left + 215, y_top + 10);
            // left bottom line
            e.Graphics.DrawLine(arrow, x_left + 200, y_top + 20, x_left + 215, y_top + 20);
            // left top dash
            for (int i = (x_left + 200); i <= (x_left + 215 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, i, y_top + 10, 4 + i, y_top + 5);
            // left bottom dash
            for (int i = (x_left + 200); i <= (x_left + 215 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, 4 + i, y_top + 20, i, y_top + 25);

            // right top line
            e.Graphics.DrawLine(arrow, x_right - 200, y_top + 10, x_right - 215, y_top + 10);
            // right bottom line
            e.Graphics.DrawLine(arrow, x_right - 200, y_top + 20, x_right - 215, y_top + 20);
            // right top dash
            for (int i = (x_right - 215); i <= (x_right - 200 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, i, y_top + 10, 4 + i, y_top + 5);
            // right bottom dash
            for (int i = (x_right - 215); i <= (x_right - 200 - 3); i = i + 4)
                e.Graphics.DrawLine(arrow, 4 + i, y_top + 20, i, y_top + 25);

            // draw top slider
            e.Graphics.DrawLine(arrow, x_left + 50, y_top + 15, x_right - 50, y_top + 15);

            // draw mid fix point
            e.Graphics.DrawLine(arrow, x_mid - 15, y_bottom + 10 - scale_L0_OB, x_mid + 15, y_bottom + 10 - scale_L0_OB);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10 - scale_L0_OB, x_mid - 9, y_bottom + 10 - scale_L0_OB);
            e.Graphics.DrawLine(arrow, x_mid, y_bottom - 10 - scale_L0_OB, x_mid + 9, y_bottom + 10 - scale_L0_OB);
            e.Graphics.DrawEllipse(arrow, x_mid - 3, y_bottom - 10 - 3 - scale_L0_OB, 6, 6);
            for (int i = (x_mid - 15); i <= (x_mid + 15 - 5); i = i + 5)
                e.Graphics.DrawLine(arrow, i, y_bottom + 15 - scale_L0_OB, 5 + i, y_bottom + 10 - scale_L0_OB);

            // draw bottom rocker
            e.Graphics.DrawLine(arrow, fix_x_bottom, fix_y_bottom, fix_x_bottom, fix_y_bottom - scale_L3_BC);

            // draw mid rocker
            e.Graphics.DrawLine(arrow, fix_x_mid, fix_y_mid, fix_x_mid + scale_L1_OA, fix_y_mid);

            // draw mid 'поршень'
            e.Graphics.DrawRectangle(arrow, fix_x_mid + scale_L1_OA, fix_y_mid, 10, 20);

            /*Thread.Sleep(500);*/
            arrow.Dispose();
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

                Double.TryParse(textBox1.Text, out L0); // Lob
                Double.TryParse(textBox2.Text, out L1); // Loa
                Double.TryParse(textBox3.Text, out L3); // Lbc
                Double.TryParse(textBox4.Text, out L5); // Lbs3

                Double.TryParse(textBox5.Text, out fi_zero); // pseudo-constant
                fi_clone = fi_zero;                           // for iterations

                Double.TryParse(textBox6.Text, out m3);
                Double.TryParse(textBox7.Text, out m4);
                Double.TryParse(textBox8.Text, out Is4);

                Double.TryParse(textBox9.Text, out omega_1cp);

                Double.TryParse(textBox10.Text, out delta);

                Double.TryParse(textBox11.Text, out I_0_p);

                int i = 0, j = 0;
                double fi_1 = 0F;
                for (fi_1 = fi_clone; i < dimension; ++i)
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

                        fi_array[i] = fi_1;

                        Xa[i] = L1 * Math.Cos(degToRad(fi_1));
                        Ya[i] = L1 * Math.Sin(degToRad(fi_1));

                        L[i] = Math.Sqrt(Math.Pow(L0, 2) + Math.Pow(L1, 2) + 2 * L0 * L1 * Math.Sin(degToRad(fi_1)));

                        cos_fi3[i] = (L1 * Math.Cos(degToRad(fi_1))) / L[i];
                        /*for debug*/
                        tbResValue3.Text = Convert.ToString(cos_fi3[0]);
                        sin_fi3[i] = (L0 + L1 * Math.Sin(degToRad(fi_1))) / L[i];

                        fi3[i] = radToDeg(Math.Acos((L1 * Math.Cos(degToRad(fi_1))) / L[i]));

                        Xc[i] = L3 * cos_fi3[i];
                        Yc[i] = L3 * sin_fi3[i];
                        Xs3[i] = L5 * cos_fi3[i];
                        Ys3[i] = L5 * sin_fi3[i];
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message); }

// =============== second part of calculating =================
                    try
                    {
                        Xa_dash[i] = -L1 * Math.Sin(degToRad(fi_1));
                        Ya_dash[i] = L1 * Math.Cos(degToRad(fi_1));

                        Ua3a2[i] = -L1 * Math.Sin(degToRad(fi_1 - fi3[i]));
                        i31[i] = (L1 / L[i]) * Math.Cos(degToRad(fi_1 - fi3[i]));
                        /*for debug*/
                        tbResValue1.Text = Convert.ToString(fi3[0]);
                        /*for debug*/
                        tbResValue2.Text = Convert.ToString(L[0]);

                        Sd[i] = Xc[i] - L3 * cos_fi3[0];
                        
                        Xc_dash[i] = (-i31[i]) * L3 * sin_fi3[i];
                        Yc_dash[i] = i31[i] * L3 * cos_fi3[i];
                        
                        /*my dif*/
                        //Xc_dash[i] = -L3 * sin_fi3[i];
                        //Yc_dash[i] = L3 * cos_fi3[i];
                        /*end my dif*/
                        
                        Uc[i] = i31[i] * L3;

                        Xs3_dash[i] = (-i31[i]) * L5 * sin_fi3[i];
                        Ys3_dash[i] = i31[i] * L5 * cos_fi3[i];

                        Us3[i] = i31[i] * L5;

                        Us5[i] = Xc_dash[i];

                        //i31_dash[i] = (Math.Pow(i31[i], 2) * sin_fi3[i] - (L1 * Math.Sin(degToRad(fi_1)) / L[i])) / cos_fi3[i];
                        /*my dif*/
                        i31_dash[i] = L1 / L[i];
                        /*end my dif*/
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message); }

// =============== added part of calculating =================
                    try
                    {
                        Xs3_doubledash[i] = (-L5) * (Math.Pow(i31[i], 2) * cos_fi3[i] + i31_dash[i] * sin_fi3[i]);
                        Ys3_doubledash[i] = L5 * (i31_dash[i] * cos_fi3[i] - Math.Pow(i31[i], 2) * sin_fi3[i]);
                        is51_dash[i] = (-L3) * (Math.Pow(i31[i], 2) * cos_fi3[i] + i31_dash[i] * sin_fi3[i]);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Data + "\n" + ex.Message); }

                    if ((i % 15 == 0) || (i == 0))
                    {
                        dgvResults.Rows[j].HeaderCell.Value = String.Format("{0:0.#####}", j + 1);
                        dgvResults.Rows[j].Cells[0].Value = String.Format("{0:0.#####}", fi_1);
                        dgvResults.Rows[j].Cells[1].Value = String.Format("{0:0.#####}", Sd[i]);
                        dgvResults.Rows[j].Cells[2].Value = String.Format("{0:0.#####}", i31[i]);
                        dgvResults.Rows[j].Cells[3].Value = String.Format("{0:0.#####}", i31_dash[i]);
                        dgvResults.Rows[j].Cells[4].Value = String.Format("{0:0.#####}", Xs3_dash[i]);
                        dgvResults.Rows[j].Cells[5].Value = String.Format("{0:0.#####}", Ys3_dash[i]);
                        dgvResults.Rows[j].Cells[6].Value = String.Format("{0:0.#####}", Xs3_doubledash[i]);
                        dgvResults.Rows[j].Cells[7].Value = String.Format("{0:0.#####}", Ys3_doubledash[i]);
                        ++j;
                    }

                    if (cbDirection.SelectedIndex == 0)
                        fi_1 -= delta_fi;
                    else
                        fi_1 += delta_fi;
                }

                tabControl1.SelectedIndex = 2;

                // run animation
                this.gbAnimation.Paint += new PaintEventHandler(TabPage_Paint);
            }
            else
            {
                MessageBox.Show("Что-то пошло не так!");
            }
        }

        private void легендаОбозначенийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
        }

/*=================================================================================================== 
 *=== Build charts  
 *===================================================================================================*/

        private void btnChart_sd_i31_i31P_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();

            chart1.Series.Add("Sd").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("i31").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("i31'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

            groupBox8.Text = btnChart_sd_i51_i51P.Text;
            tabControl1.SelectedIndex = 3;

            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Sd"].Points.AddXY(i, Sd[i]);
                chart1.Series["i31"].Points.AddXY(i, i31[i]);
                chart1.Series["i31'"].Points.AddXY(i, i31_dash[i]);
            }
        }

        private void btnChart_i21_i21P_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_i21_i21P.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_xs2p_ys2p_xs2pp_ys2pp_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();

            chart1.Series.Add("Xs2p").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ys2p").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Xs2pp").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Ys2pp").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

            //chart1.DataSource = dgvResults.DataSource;
            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Xs2p"].Points.AddXY(i, Xs3_dash[i]);
                chart1.Series["Ys2p"].Points.AddXY(i, Ys3_dash[i]);
                chart1.Series["Xs2pp"].Points.AddXY(i, Xs3_doubledash[i]);
                chart1.Series["Ys2pp"].Points.AddXY(i, Ys3_doubledash[i]);
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

            groupBox8.Text = btnChart_xs2p_ys2p_xs2pp_ys2pp.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_i2p_A_B_C_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_i2p_A_B_C.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_Mcp_Mdp_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_Mcp_Mdp.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_dT_dTi_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_dT_dTi.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_T2_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_T2.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_e1_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_e1.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_omega1_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_omega1.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_Ac_Ad_Click(object sender, EventArgs e)
        {
            groupBox8.Text = btnChart_Ac_Ad.Text;
            tabControl1.SelectedIndex = 3;
        }

        private void btnChart_xc_dash_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();

            chart1.Series.Add("Xc").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Yc").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Xc'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series.Add("Yc'").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

            groupBox8.Text = btnChart_xc_dash.Text;
            tabControl1.SelectedIndex = 3;

            for (int i = 0; i < dimension; ++i)
            {
                chart1.Series["Xc"].Points.AddXY(i, Xc[i]);
                chart1.Series["Yc"].Points.AddXY(i, Yc[i]);
                chart1.Series["Xc'"].Points.AddXY(i, Xc_dash[i]);
                chart1.Series["Yc'"].Points.AddXY(i, Yc_dash[i]);
            }
        }

/*=================================================================================================== 
 *=== Validate input data  
 *===================================================================================================*/

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
                double fi = 270 - Math.Acos(OA / OB) * (180 / Math.PI);
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
                if (temp >= 0 && temp < 1)
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
         *=== filling some fields  
         *===================================================================================================*/

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

        private void fillDgvInput()
        {
            for (int i = 0; i <= 12; ++i)
                if (i > 4 && i < 11)
                {
                    dgvInput.Rows[i].Cells[0].Value = 2500;
                    Fpc[i] = 2500F;
                }
                else
                {
                    dgvInput.Rows[i].Cells[0].Value = 5000;
                    Fpc[i] = 5000F;
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

/*=================================================================================================== 
 *=== some math  
 *===================================================================================================*/

        public double degToRad(double param)
        {
            return (param * Math.PI / 180F);
        }

        public double radToDeg(double param)
        {
            return (param * 180F / Math.PI);
        }
    }
}
