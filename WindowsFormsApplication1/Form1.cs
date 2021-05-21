using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        bool br;
        const int WM_POWERBROADCAST = 0x218;
        const int PbtApmsuspend = 0x0004;
        const int PBT_APMRESUMESUSPEND = 0x0007;
        protected override void WndProc(ref Message m)
        {
            // изменение состояния Windows
            if (m.Msg == WM_POWERBROADCAST)
            {
                switch (m.WParam.ToInt32())
                {
                    case PbtApmsuspend: //вход в спящий режим
                        {
                            if (sp.IsOpen) //если подключение открыто
                            {

                                this.pictureBox1_Click(null, null); //кнопка подключиться/отключиться
                                //MessageBox.Show("ком закрыт");

                            }
                            break;
                        }
                    case PBT_APMRESUMESUSPEND: // выход из спящего режима
                        {
                            if (!sp.IsOpen) //если подключение закрыто
                            {
                                this.pictureBox1_Click(null, null);
                                //  MessageBox.Show("Попытка открыть порт");

                            }
                            break;
                        }
                }
            }
            base.WndProc(ref m);
        }
        public Form1()
        {
            InitializeComponent();
        }

        string[] mass = File.ReadAllLines("ДУТ\\file1.txt");//Файл с тарировкой
        bool cont = true;
        SerialPort sp = new SerialPort();
        DateTime dold = new DateTime();
        bool stop;
        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.Legends.Clear();
            chart2.Legends.Clear();
            label1.Font = new Font("Tahoma", 20.0F);
            label2.Font = new Font("Tahoma", 20.0F);
            label3.Font = new Font("Tahoma", 20.0F);
            label4.Font = new Font("Tahoma", 20.0F);
            label5.Font = new Font("Tahoma", 20.0F);
            label6.Font = new Font("Tahoma", 20.0F);
            label7.Font = new Font("Tahoma", 20.0F);
            label8.Font = new Font("Tahoma", 20.0F);
            label9.Font = new Font("Tahoma", 20.0F);
            label10.Font = new Font("Tahoma", 20.0F);
            label11.Font = new Font("Tahoma", 20.0F);
            label12.Font = new Font("Tahoma", 20.0F);
            label13.Font = new Font("Tahoma", 20.0F);
            label14.Font = new Font("Tahoma", 20.0F);
            label15.Font = new Font("Tahoma", 20.0F);
            label16.Font = new Font("Tahoma", 20.0F);
            string secondLine = File.ReadLines("ДУТ\\config.txt").Skip(1).First(); //файл с настройками com порта
            string name = File.ReadLines("ДУТ\\config.txt").Skip(0).First();
            string name1 = name.Substring(6, name.Length - 6);
            string speed = secondLine.Substring(10, secondLine.Length - 10);
            int speed1 = Convert.ToInt32(speed);
            sp = new SerialPort(name1, speed1, Parity.None, 8, StopBits.One);
            chart1.ChartAreas[0].AxisY.Interval = Convert.ToInt32(File.ReadLines("ДУТ\\config.txt").Skip(2).First().Substring(11, File.ReadLines("ДУТ\\config.txt").Skip(2).First().Length - 11));
            chart2.ChartAreas[0].AxisY.Interval = Convert.ToInt32(File.ReadLines("ДУТ\\config.txt").Skip(3).First().Substring(11, File.ReadLines("ДУТ\\config.txt").Skip(3).First().Length - 11));
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            chart1.Legends.Clear();
            chart2.Legends.Clear();

            cont = true;

            if (sp.IsOpen)
            {
                cont = false;
                sp.Close();
                br = true;
            }

            else if (!sp.IsOpen)
            {
                Thread.Sleep(2000);

                sp.Open();




                // br = true;
                //sp.Open();
            }
            label16.Visible = true;
            label13.Visible = true;
            label1.Visible = true;
            label7.Visible = true;
            label2.Visible = true;
            label10.Visible = true;
            label11.Visible = true;
            label15.Visible = true;
            label10.Text = "0";
            label11.Text = "0";
            label13.Text = "0";
            label15.Text = "0";

            if (sp.IsOpen)
            {
                double res;  //текущий уровень в литрах
                double oldres = 0; // уровень в литрах в предыдущем шаге цикла
                long L1; //текущий уровень в у.е
                long L2 = 0; //уровень  в у.е. в предыдущем шаге цикла
                double sliv = 0;
                double delta = 30; // ислользуется для расчета значения слито
                double doliv = 0;
                double rashod = 0; //общий расход
                double level = 0;  // ислользуется для расчета значения слито
                DateTime t2 = DateTime.Now; DateTime t1 = new DateTime(); TimeSpan tSpan = new TimeSpan(0, 0, 0, 5);
                double sum = 0;// потребление
                while (cont)
                {
                    byte[] query = { 0x31, 0x01, 0x06, 0x6C };
                    sp.Write(query, 0, 4);
                    try
                    {
                        Thread.Sleep(200);
                        sp.WriteTimeout = 100;
                        sp.ReadTimeout = 100;
                        byte[] answer = new byte[(int)sp.BytesToRead];
                        sp.Read(answer, 0, sp.BytesToRead);
                        string hex = "";

                        foreach (char c in answer)
                        {
                            int tmp = c;
                            hex += String.Format("{0:x2} ", (uint)System.Convert.ToUInt32(tmp.ToString()));
                        }
                        richTextBox1.Text = hex;
                        if (hex.Substring(15, 1) == "0")
                        {
                            textBox4.Text = String.Format("{0:10}", hex.Substring(16, 1) + hex.Substring(12, 2));
                        }
                        else
                        {
                            textBox4.Text = String.Format("{0:10}", hex.Substring(16, 1) + hex.Substring(12, 2) + hex.Substring(15, 1));
                        }
                        L1 = long.Parse(textBox4.Text, System.Globalization.NumberStyles.HexNumber);
                        label2.Text = Convert.ToString(L1) + " у.е.";
                        chart1.ChartAreas[0].AxisY.Interval = Convert.ToInt32(File.ReadLines("ДУТ\\config.txt").Skip(2).First().Substring(11, File.ReadLines("ДУТ\\config.txt").Skip(2).First().Length - 11));
                        chart2.ChartAreas[0].AxisY.Interval = Convert.ToInt32(File.ReadLines("ДУТ\\config.txt").Skip(3).First().Substring(11, File.ReadLines("ДУТ\\config.txt").Skip(3).First().Length - 11));
                        int numberOfPointsInChart = 200;
                        int numberOfPointsAfterRemoval = 199;
                        while (chart1.Series["Уровень"].Points.Count > numberOfPointsInChart)
                        {
                            //Удаление данных вышедших за пределы графика слева
                            //оставляем точек на графике не более чем задано переменной numberOfPointsAfterRemoval
                            while (chart1.Series["Уровень"].Points.Count > numberOfPointsAfterRemoval)
                            {
                                //каждая точка удаляется индивидуально, с начала графика. После удаления нолевой
                                //точки - RemoveAt(0). Следующая за ней встает на ее место и так до тех пор,
                                //пока не выполнится условие цикла
                                chart1.Series["Уровень"].Points.RemoveAt(0);
                            }

                            //Масштаб оси Х
                            //chart1.ChartAreas["ChartArea1"].AxisX.Minimum = pointIndex - numberOfPointsAfterRemoval;
                            chart1.ChartAreas["ChartArea1"].AxisX.Maximum = chart1.ChartAreas["ChartArea1"].AxisX.Minimum + numberOfPointsInChart;
                        }

                        chart2.Series[0].Points.Add(L1);
                        int numberOfPointsInChart2 = 3000;

                        int numberOfPointsAfterRemoval2 = 2090;
                        while (chart2.Series["Уровень"].Points.Count > numberOfPointsInChart2)
                        {
                            //Удаление данных вышедших за пределы графика слева
                            //оставляем точек на графике не более чем задано переменной numberOfPointsAfterRemoval
                            while (chart2.Series["Уровень"].Points.Count > numberOfPointsAfterRemoval2)
                            {
                                //каждая точка удаляется индивидуально, с начала графика. После удаления нолевой
                                //точки - RemoveAt(0). Следующая за ней встает на ее место и так до тех пор,
                                //пока не выполнится условие цикла
                                chart2.Series["Уровень"].Points.RemoveAt(0);
                            }

                            //Масштаб оси Х
                            //chart1.ChartAreas["ChartArea1"].AxisX.Minimum = pointIndex - numberOfPointsAfterRemoval;
                            chart2.ChartAreas["ChartArea2"].AxisX.Maximum = chart2.ChartAreas["ChartArea2"].AxisX.Minimum + numberOfPointsInChart2;
                        }



                        double[] val_usl = mass[0].Split(new char[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries).Select(s => double.Parse(s)).ToArray();
                        double[] val_lit = mass[1].Split(new char[] { ' ' },
                          StringSplitOptions.RemoveEmptyEntries).Select(s => double.Parse(s)).ToArray();
                        long x = L1;
                        //double x = Convert.ToDouble(label2.Text);
                        for (int i = 0; i < 27; i++)
                        {

                            //double y = 0;

                            if (val_usl[i] > x && val_usl[i + 1] < x)// переводим условные единицы в литры
                            {
                                double d = val_usl[i];
                                double z = val_usl[i + 1];
                                double f = z;
                                double a = val_lit[i + 1];
                                double b = val_lit[i];
                                res = (x - z) * (b - a) / (d - f) + a;

                                chart1.Series[0].Points.Add(res);

                                double end = res / 1000; //миллилитры -> литры

                                label7.Text = Convert.ToString(Math.Round(end, 3)) + " л";

                                if (L2 != 0 && L2 - L1 > 1)//Условие для общего рассхода
                                {

                                    rashod += oldres - res;
                                    label15.Text = Convert.ToString(Math.Round(rashod, 0)) + " мл";
                                    oldres = res;
                                    L2 = L1;
                                    if (DateTime.Now >= t2)
                                    {
                                        t1 = DateTime.Now;
                                        t2 = t1 + tSpan;// текущее + 5 сек
                                        level = res;// переменная level  в течение 5 сек не будет обновляться
                                    }
                                    if (level - res > delta)//условие для слива.
                                    {
                                        sliv += level - res;
                                        label13.Text = Convert.ToString(Math.Round(sliv, 0)) + " мл";
                                        t2 = DateTime.Now + tSpan;//текущее время + 5 сек
                                        level = res;
                                    }
                                    sum = rashod - sliv;  //Считаем разрешенное потребление
                                    label10.Text = Convert.ToString(Math.Round(sum, 0)) + " мл";
                                }
                                if (L2 != 0 && L1 - L2 > 1)//условие для заправки
                                {
                                    doliv += res - oldres;
                                    label11.Text = Convert.ToString(Math.Round(doliv, 0)) + " мл";
                                    oldres = res;
                                    L2 = L1;
                                }
                                if (oldres == 0)
                                {
                                    oldres = res;
                                }
                                if (L2 == 0)
                                {
                                    L2 = L1;
                                }
                                chart1.Series[0].BorderWidth = 2;
                                chart2.Series[0].BorderWidth = 2;
                            }
                        }

                        int DecimalVal = Convert.ToInt32(hex.Substring(9, 2), 16); //температура датчика
                        label1.Text = Convert.ToString(DecimalVal) + "\u00B0C";

                        Application.DoEvents(); //выход из бесконечного цикла
                        if (stop)
                            break;
                    }
                    catch (TimeoutException)
                    {

                    }
                }
            }
        }
    }
}

