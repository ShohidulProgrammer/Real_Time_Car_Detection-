using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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
using System.Threading;
using System.Windows.Forms;

namespace LPR
{
    public partial class window_cam : UserControl
    {
        public window_cam()
        {
            InitializeComponent();
        }
        Webcam wc = new Webcam();
        List<string> lb;
        //List<mini_thread> ls_timer = new List<mini_thread>();
        //Recognizing_plate rec_p = new Recognizing_plate();
        //SQL_Data data = new SQL_Data();
        //SQL_helper sql_help = new SQL_helper();
        //bool success = true;
        private void window_cam_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            lb = Webcam.get_all_cam();
            foreach (string s in lb)
            {
                comboBox1.Items.Add(s);
                comboBox1.SelectedIndex = 0;
                button1.Enabled = true;
            }
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            new Thread(threading_recognize).Start();
        }
        public void destroy()
        {
            if (button1.Text == "Stop")
            {
                timer1.Enabled = false;
                wc.Stop();
                button1.Text = "Start";
            }
        }
        private void window_cam_Resize(object sender, EventArgs e)
        {
            PictureBox picbox = (PictureBox)tableLayoutPanel1.GetControlFromPosition(0, 0);
            int[] size = new int[2];
            size[0] = tableLayoutPanel1.GetRowHeights()[0];
            size[1] = tableLayoutPanel1.GetColumnWidths()[0];
            int h = 0, w = 0;
            try
            {
                w = int.Parse(comboBox2.Text.Split('x')[0]);
                h = int.Parse(comboBox2.Text.Split('x')[1]);
            }
            catch (Exception)
            {
                h = 480;
                w = 640;
            }
            if ((double)size[0]/size[1] < (double)h / w)
            {
                picbox.Size = new Size(size[0] * w / h, size[0]);
            }
            else
            {
                picbox.Size = new Size(size[1], size[1] * h / w);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                wc.Start(lb.IndexOf(comboBox1.Text), comboBox2.SelectedIndex);
                timer1.Enabled = true;
                button1.Text = "Stop";
            }
            else
            {
                //SQL_helper sql = new SQL_helper();
                //sql.sql_insert(pictureBox1);
                timer1.Enabled = false;
                wc.Stop();
                button1.Text = "Start";
                pictureBox2.Image = null;
                pictureBox1.Image = null;
                pictureBox3.Image = null;
                textBox1.Text = "";
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            foreach(string s in wc.get_all_res(lb.IndexOf(comboBox1.Text)))
            {
                comboBox2.Items.Add(s);
                comboBox2.SelectedIndex = 0;
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            window_cam_Resize(sender, new EventArgs());
            if (button1.Text == "Stop")
            {
                timer1.Enabled = false;
                wc.Stop();
                wc.Start(lb.IndexOf(comboBox1.Text), comboBox2.SelectedIndex);
                timer1.Enabled = true;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Image = wc.image;
        }
        public static void my_invoke(Control invo_res, Delegate dele)
        {
            if (invo_res.InvokeRequired)
                invo_res.Invoke(dele);
        }
        private void threading_recognize()
        {
            Recognizing_plate rec = new Recognizing_plate();
            SQL_helper helper = new SQL_helper();
            SQL_Data dto = new SQL_Data();
            while (true)
            {
                Thread.Sleep(100);
                if (timer1.Enabled)
                {
                    if (pictureBox1.Image != null)
                    {
                        Image pic = null;
                        my_invoke(pictureBox1, (MethodInvoker)delegate { pic = (Image)pictureBox1.Image.Clone(); });
                        Image plateDraw = null;
                        try
                        {
                            var im = Recognizing_plate.FindLicensePlate((Bitmap)pic, out plateDraw);
                            string bienso, bienso_text;
                            bool flag = false;
                            foreach(var pl in im)
                            {
                                if (pl != null)
                                {
                                    Image plate = pl.ToBitmap();
                                    string[] result = rec.Reconize(pl, out bienso, out bienso_text);                                    

                                    if (result[0].Length != 3 || result[1].Length < 4)
                                    {
                                        if (!flag)
                                        {
                                            my_invoke(pictureBox2, (MethodInvoker)delegate { pictureBox2.Image = plateDraw; });
                                            my_invoke(textBox1, (MethodInvoker)delegate { textBox1.Text = bienso_text; });
                                            my_invoke(pictureBox3, (MethodInvoker)delegate { pictureBox3.Image = plate; });
                                            my_invoke(textBox1, (MethodInvoker)delegate { textBox1.BackColor = Color.Red; });
                                        }                                        
                                        continue;
                                    }
                                    
                                    dto.plate_number = bienso;
                                    dto.plate = plate;
                                    dto.car = plateDraw;
                                    my_invoke(comboBox1, (MethodInvoker)delegate { dto.camera_name = comboBox1.Text; });
                                    if (helper.sql_insert_unique(dto))
                                    {
                                        flag = true;
                                        my_invoke(pictureBox2, (MethodInvoker)delegate { pictureBox2.Image = plateDraw; });
                                        my_invoke(textBox1, (MethodInvoker)delegate { textBox1.Text = bienso_text; });
                                        my_invoke(pictureBox3, (MethodInvoker)delegate { pictureBox3.Image = plate; });
                                        my_invoke(textBox1, (MethodInvoker)delegate { textBox1.BackColor = Color.Lime; });
                                    }
                                    
                                }
                            }
                            
                        }
                        catch (Exception) { }
                    }
                }
            }

        }        
    }
}
