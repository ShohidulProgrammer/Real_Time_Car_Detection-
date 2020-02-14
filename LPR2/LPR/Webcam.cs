using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Windows.Forms;

namespace LPR
{
    class Webcam
    {
        private bool DeviceExist = false;
        private FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        private VideoCaptureDevice videoSource = null;
        public List<string> list_cam = new List<string>();
        public Bitmap bitmap;
        public Image image;
        public string status = "Non!";
        public string pb = "";

        public static List<string> get_all_cam()
        {
            List<string> ls = new List<string>();
            try
            {
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new ApplicationException();
                foreach (FilterInfo device in videoDevices)
                {
                    ls.Add(device.Name);
                }
            }
            catch (ApplicationException)
            {
                ls.Clear();
            }
            return ls;
        }
        public void refesh()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                list_cam.Clear();
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    list_cam.Add(device.Name);
                }
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                list_cam.Clear();
                list_cam.Add("No capture device on your system");
            }
        }

        public void Start(int index, int quality)
        {
            refesh();
            if (DeviceExist)
            {
                videoSource = new VideoCaptureDevice(videoDevices[index].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                VideoCapabilities[] v_res = videoSource.VideoCapabilities;
                videoSource.VideoResolution = v_res[quality];
                CloseVideoSource();
                videoSource.Start();
                status = "run";
            }
        }
        public void Stop()
        {
            if (videoSource.IsRunning)
            {
                CloseVideoSource();
                status = "stop";
            }
        }
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bitmap = (Bitmap)eventArgs.Frame.Clone();
            image = bitmap;
        }
        private void CloseVideoSource()
        {
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                }
        }
        public void Save()
        {
            string path = Application.StartupPath + @"\data\" + "aa.bmp";
            image.Save(path);
        }
        public void put_picturebox(string name)
        {
            pb = name;
        }
        public List<string> get_all_res(int index)
        {
            List<string> result = new List<string>();
            VideoCaptureDevice video = new VideoCaptureDevice(videoDevices[index].MonikerString);
            foreach(VideoCapabilities v in video.VideoCapabilities)
            {
                string s = v.FrameSize.ToString();
                string w = s.Split(',')[0].Split('=')[1];
                string h = s.Split(',')[1].Split('=')[1].Split('}')[0];
                result.Add(w + "x" + h);
            }
            return result;
        }
    }
}
