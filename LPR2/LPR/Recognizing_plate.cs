using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.CvEnum;
using tesseract;

namespace LPR
{
    public class Recognizing_plate
    {
        TesseractProcessor full_tesseract = null;
        TesseractProcessor ch_tesseract = null;
        TesseractProcessor num_tesseract = null;
        private string m_path = Application.StartupPath + @"\data\";
        private List<string> lstimages = new List<string>();
        private const string m_lang = "eng";
        public Recognizing_plate()
        {
            full_tesseract = new TesseractProcessor();
            bool succeed = full_tesseract.Init(m_path, m_lang, 3);
            if (!succeed)
            {
                MessageBox.Show("Tesseract initialization failed. The application will exit.");
                Application.Exit();
            }
            full_tesseract.SetVariable("tessedit_char_whitelist", "ABCDEFHKLMNPRSTVXY1234567890").ToString();

            ch_tesseract = new TesseractProcessor();
            succeed = ch_tesseract.Init(m_path, m_lang, 3);
            if (!succeed)
            {
                MessageBox.Show("Tesseract initialization failed. The application will exit.");
                Application.Exit();
            }
            ch_tesseract.SetVariable("tessedit_char_whitelist", "ABCDEFHKLMNPSTUVXY").ToString();

            num_tesseract = new TesseractProcessor();
            succeed = num_tesseract.Init(m_path, m_lang, 3);
            if (!succeed)
            {
                MessageBox.Show("Tesseract initialization failed. The application will exit.");
                Application.Exit();
            }
            num_tesseract.SetVariable("tessedit_char_whitelist", "1234567890").ToString();
        }
        public static List<Image<Bgr, byte>> FindLicensePlate(Bitmap image, out Image plateDraw)
        {
            plateDraw = image;
            List<Image<Bgr, byte>> PlateImagesList = new List<Image<Bgr, byte>>();
            Image<Bgr, byte> frame = new Image<Bgr, byte>(image);
            bool isface = false;
            using (Image<Gray, byte> grayframe = new Image<Gray, byte>(image))
            {
                var faces =
                       grayframe.DetectHaarCascade(
                               new HaarCascade(Application.StartupPath + "\\output-hv-33-x25.xml"), 1.1, 8,
                               HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                               new Size(0, 0)
                               )[0];
                foreach (var face in faces)
                {
                    Image<Bgr, byte> tmp = frame.Copy();
                    tmp.ROI = face.rect;

                    frame.Draw(face.rect, new Bgr(Color.Blue), 2);

                    PlateImagesList.Add(tmp);

                    isface = true;
                }
                if (isface)
                {
                    plateDraw = frame.Clone().ToBitmap();
                    for (int i = 0; i < PlateImagesList.Count; i++)
                    {
                        PlateImagesList[i] = PlateImagesList[i].Resize(400, 400, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                    }                   
                    return PlateImagesList;
                }
                return null;
            }
        }
        public static int IdentifyContours(Bitmap colorImage, int thresholdValue, bool invert, out Bitmap processedGray, out Bitmap processedColor, out List<Rectangle> list)
        {
            List<Rectangle> listR = new List<Rectangle>();
            #region Conversion To grayscale
            Image<Gray, byte> grayImage = new Image<Gray, byte>(colorImage);
            Image<Gray, byte> bi = new Image<Gray, byte>(grayImage.Width, grayImage.Height);
            Image<Bgr, byte> color = new Image<Bgr, byte>(colorImage);

            #endregion

            #region tim gia tri thresh de co so ky tu lon nhat

            //double thr = 0;
            //if (thr == 0)
            //{
            //    thr = grayImage.GetAverage().Intensity;
            //}
            Rectangle[] li = new Rectangle[9];
            Image<Bgr, byte> color_b = new Image<Bgr, byte>(colorImage); ;
            Image<Gray, byte> src_b = grayImage.Clone();
            Image<Gray, byte> bi_b = bi.Clone();
            Image<Bgr, byte> color2 = null;
            Image<Gray, byte> src = null;
            Image<Gray, byte> bi2 = null;
            int c = 0, c_best = 0;
            for (double value = 0; value <= 64; value += 3)
            //for (double value = 0; value == 0; value += 3)
            {
                for (int s = -1; s <= 1 && s + value != 1; s += 2)
                //for (int s = -1; s == -1 ; s += 2)
                {
                    color2 = new Image<Bgr, byte>(colorImage);
                    //src = grayImage.Clone();
                    bi2 = bi.Clone();
                    listR.Clear();
                    //list_best.Clear();
                    c = 0;
                    double t = 127 + value * s;
                    //double t = thr;
                    src = grayImage.ThresholdBinary(new Gray(t), new Gray(255));

                    using (MemStorage storage = new MemStorage())
                    {

                        Contour<Point> contours = src.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);
                        while (contours != null)
                        {

                            Rectangle rect = contours.BoundingRectangle;
                            CvInvoke.cvDrawContours(color2, contours, new MCvScalar(255, 255, 0), new MCvScalar(0), -1, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, new Point(0, 0));
                            double ratio = (double)rect.Width / rect.Height;
                            if (rect.Width > 20 && rect.Width < 150
                                && rect.Height > 80 && rect.Height < 180
                                && ratio > 0.1 && ratio < 1.1 && rect.X > 20)
                            {
                                c++;
                                CvInvoke.cvDrawContours(color2, contours, new MCvScalar(0, 255, 255), new MCvScalar(255), -1, 3, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, new Point(0, 0));

                                color2.Draw(contours.BoundingRectangle, new Bgr(0, 255, 0), 2);
                                bi2.Draw(contours, new Gray(255), -1);
                                listR.Add(contours.BoundingRectangle);


                            }

                            contours = contours.HNext;

                        }
                    }
                    //IntPtr a = color_b.Ptr;
                    //CvInvoke.cvReleaseImage(ref a);
                    double avg_h = 0;
                    double dis = 0;
                    for (int i = 0; i < c; i++)
                    {
                        avg_h += listR[i].Height;
                        for (int j = i + 1; j < c; j++)
                        {
                            if ((listR[j].X < (listR[i].X + listR[i].Width) && listR[j].X > listR[i].X)
                                && (listR[j].Y < (listR[i].Y + listR[i].Width) && listR[j].Y > listR[i].Y))
                            {
                                listR.RemoveAt(j);
                                c--;
                                j--;
                            }
                            else if ((listR[i].X < (listR[j].X + listR[j].Width) && listR[i].X > listR[j].X)
                                && (listR[i].Y < (listR[j].Y + listR[j].Width) && listR[i].Y > listR[j].Y))
                            {
                                avg_h -= listR[i].Height;
                                listR.RemoveAt(i);
                                c--;
                                i--;
                                break;
                            }

                        }
                    }
                    avg_h = avg_h / c;
                    for (int i = 0; i < c; i++)
                    {
                        dis += Math.Abs(avg_h - listR[i].Height);
                    }

                    if (c <= 8 && c > 1 && c > c_best && dis <= c * 8)
                    {
                        listR.CopyTo(li);
                        c_best = c;
                        color_b = color2;
                        bi_b = bi2;
                        src_b = src;
                    }
                }
                if (c_best == 8) break;
            }

            int count = c_best;
            grayImage = src_b;
            color = color_b;
            bi = bi_b;
            listR.Clear();
            for (int i = 0; i < li.Length; i++)
            {
                if (li[i].Height != 0) listR.Add(li[i]);
            }

            #endregion

            #region Asigning output
            processedColor = color.ToBitmap();
            processedGray = grayImage.ToBitmap();
            color_b.Dispose();
            src_b.Dispose();
            bi_b.Dispose();
            color2.Dispose();
            src.Dispose();
            bi2.Dispose();
            grayImage.Dispose();
            bi.Dispose();
            color.Dispose();
            list = listR;
            #endregion
            return count;
        }
        public string[] Reconize(Image<Bgr, byte> plate_img, out string bienso, out string bienso_text)
        {
            List<Rectangle> listRect;
            bienso = "";
            bienso_text = "";
            if (plate_img != null)
            {
                Image<Bgr, byte> src = new Image<Bgr, byte>(plate_img.ToBitmap());
                Bitmap grayframe;
                Bitmap color;
                int c = IdentifyContours(src.ToBitmap(), 50, false, out grayframe, out color, out listRect);
                Image<Gray, byte> dst = new Image<Gray, byte>(grayframe);
                grayframe = dst.ToBitmap();

                // lọc và sắp xếp số
                List<Bitmap> bmp = new List<Bitmap>();
                List<int> erode = new List<int>();
                List<Rectangle> up = new List<Rectangle>();
                List<Rectangle> dow = new List<Rectangle>();
                int up_y = 0, dow_y = 0;
                bool flag_up = false;

                int di = 0;

                if (listRect == null) return new string[] { "", "" };

                for (int i = 0; i < listRect.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(listRect[i], grayframe.PixelFormat);
                    int cou = 0;
                    full_tesseract.Clear();
                    full_tesseract.ClearAdaptiveClassifier();
                    string temp = full_tesseract.Apply(ch);
                    while (temp.Length > 3)
                    {
                        Image<Gray, byte> temp2 = new Image<Gray, byte>(ch);
                        temp2 = temp2.Erode(2);
                        ch = temp2.ToBitmap();
                        full_tesseract.Clear();
                        full_tesseract.ClearAdaptiveClassifier();
                        temp = full_tesseract.Apply(ch);
                        cou++;
                        if (cou > 10)
                        {
                            listRect.RemoveAt(i);
                            i--;
                            di = 0;
                            break;
                        }
                        di = cou;
                    }
                }

                for (int i = 0; i < listRect.Count; i++)
                {
                    for (int j = i; j < listRect.Count; j++)
                    {
                        if (listRect[i].Y > listRect[j].Y + 100)
                        {
                            flag_up = true;
                            up_y = listRect[j].Y;
                            dow_y = listRect[i].Y;
                            break;
                        }
                        else if (listRect[j].Y > listRect[i].Y + 100)
                        {
                            flag_up = true;
                            up_y = listRect[i].Y;
                            dow_y = listRect[j].Y;
                            break;
                        }
                        if (flag_up == true) break;
                    }
                }

                for (int i = 0; i < listRect.Count; i++)
                {
                    if (listRect[i].Y < up_y + 50 && listRect[i].Y > up_y - 50)
                    {
                        up.Add(listRect[i]);
                    }
                    else if (listRect[i].Y < dow_y + 50 && listRect[i].Y > dow_y - 50)
                    {
                        dow.Add(listRect[i]);
                    }
                }

                if (flag_up == false) dow = listRect;

                for (int i = 0; i < up.Count; i++)
                {
                    for (int j = i; j < up.Count; j++)
                    {
                        if (up[i].X > up[j].X)
                        {
                            Rectangle w = up[i];
                            up[i] = up[j];
                            up[j] = w;
                        }
                    }
                }
                for (int i = 0; i < dow.Count; i++)
                {
                    for (int j = i; j < dow.Count; j++)
                    {
                        if (dow[i].X > dow[j].X)
                        {
                            Rectangle w = dow[i];
                            dow[i] = dow[j];
                            dow[j] = w;
                        }
                    }
                }

                string up_str = "", dow_str = "";
                for (int i = 0; i < up.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(up[i], grayframe.PixelFormat);
                    Bitmap o = ch;
                    //ch = con.Erodetion(ch);
                    string temp;
                    if (i < 2)
                    {
                        temp = Ocr(ch, false, true); // nhan dien so
                    }
                    else
                    {
                        temp = Ocr(ch, false, false);// nhan dien chu
                    }

                    up_str += temp;
                }
                up_str = up_str.Replace("\n", "");
                up_str = up_str.Replace("\r", "");
                //up_str += "\r\n";
                for (int i = 0; i < dow.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(dow[i], grayframe.PixelFormat);
                    //ch = con.Erodetion(ch);
                    string temp = Ocr(ch, false, true); // nhan dien so
                    dow_str += temp;
                }
                dow_str = dow_str.Replace("\n", "");
                dow_str = dow_str.Replace("\r", "");
                bienso = up_str + dow_str;
                bienso_text = up_str + "\r\n" + dow_str;
                return new string[] { up_str, dow_str };
            }
            return new string[] { "", "" };
        }
        private string Ocr(Bitmap image_s, bool isFull, bool isNum = false)
        {
            string temp = "";
            Image<Gray, byte> src = new Image<Gray, byte>(image_s);
            double ratio = 1;
            while (true)
            {
                ratio = (double)CvInvoke.cvCountNonZero(src) / (src.Width * src.Height);
                if (ratio > 0.5) break;
                src = src.Dilate(2);
            }
            Bitmap image = src.ToBitmap();

            TesseractProcessor ocr;
            if (isFull)
                ocr = full_tesseract;
            else if (isNum)
                ocr = num_tesseract;
            else
                ocr = ch_tesseract;

            int cou = 0;
            ocr.Clear();
            ocr.ClearAdaptiveClassifier();
            temp = ocr.Apply(image);
            while (temp.Length > 3)
            {
                Image<Gray, byte> temp2 = new Image<Gray, byte>(image);
                temp2 = temp2.Erode(2);
                image = temp2.ToBitmap();
                ocr.Clear();
                ocr.ClearAdaptiveClassifier();
                temp = ocr.Apply(image);
                cou++;
                if (cou > 10)
                {
                    temp = "";
                    break;
                }
            }
            return temp;

        }
    }
}
