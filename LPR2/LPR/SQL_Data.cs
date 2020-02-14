using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LPR
{
    public class SQL_Data
    {
        public int id { get; set;}
        public string plate_number { get; set; }
        public DateTime datetime { get; set; }
        public Image plate { get; set; }
        public Image car { get; set; }
        public string camera_name { get; set; }
    }
}
