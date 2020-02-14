using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using Emgu.CV.UI;
using Emgu.Util;
using Emgu.CV.CvEnum;
using tesseract;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace LPR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            var cam = new window_cam();
            cam.Dock = DockStyle.Fill;
            tableLayoutPanel_main.Controls.Add(cam, 0, 0);
            //double a = SQL_helper.CalculateSimilarity("29A15390", "295539");
            //var aa = tableLayoutPanel_main.ColumnStyles;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            sql s = new sql();
            s.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tableLayoutPanel_main.ColumnCount++;
            tableLayoutPanel_main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < tableLayoutPanel_main.RowCount; i++)
            {
                var cam = new window_cam();
                cam.Dock = DockStyle.Fill;
                tableLayoutPanel_main.Controls.Add(cam, tableLayoutPanel_main.ColumnCount - 1, i);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (tableLayoutPanel_main.ColumnCount <= 1)
                return;
            for (int i = 0; i < tableLayoutPanel_main.RowCount; i++)
            {
                window_cam wc = (window_cam)tableLayoutPanel_main.GetControlFromPosition(tableLayoutPanel_main.ColumnCount - 1, i);
                wc.destroy();
                tableLayoutPanel_main.Controls.Remove(wc);
                wc.Dispose();
            }
            tableLayoutPanel_main.ColumnCount--;
            //tableLayoutPanel_main.ColumnStyles.Remove(tableLayoutPanel_main.ColumnStyles[tableLayoutPanel_main.ColumnCount]);
            Thread.Sleep(100);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tableLayoutPanel_main.RowCount++;
            tableLayoutPanel_main.RowStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < tableLayoutPanel_main.ColumnCount; i++)
            {
                var cam = new window_cam();
                cam.Dock = DockStyle.Fill;
                tableLayoutPanel_main.Controls.Add(cam, i, tableLayoutPanel_main.RowCount - 1);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tableLayoutPanel_main.RowCount <= 1)
                return;
            for (int i = 0; i < tableLayoutPanel_main.ColumnCount; i++)
            {
                window_cam wc = (window_cam)tableLayoutPanel_main.GetControlFromPosition(i, tableLayoutPanel_main.RowCount - 1);
                wc.destroy();
                tableLayoutPanel_main.Controls.Remove(wc);
                wc.Dispose();
            }
            tableLayoutPanel_main.RowCount--;
            //tableLayoutPanel_main.RowStyles.Remove(tableLayoutPanel_main.RowStyles[tableLayoutPanel_main.RowCount]);
            Thread.Sleep(100);
        }
    }
}
