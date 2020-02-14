using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace LPR
{
    public class SQL_helper
    {
        private MySqlConnection connection;
        public string server = "localhost";
        public string database = "local";
        public string uid = "root";
        public string password = "1234";
        public string connectionString;

        string query;
        public SQL_helper()
        {
            connectionString = "SERVER=" + server + ";PORT=3306;DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }

        public MySqlConnection connect()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open(); //if you have this problem, try changing the Port to 3333 of connectString
                return connection;
            }
            else
            {
                connection.Close();
                connection.Open();
                return connection;
            }
        }
        public static byte[] imageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }
        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        public void sql_insert_raw(string number, Image plate, Image car, string cam)
        {
            query = "insert into local.parking (plate_number,time,plate,car,camera_name) value(@num,now(),@plate,@car,@cam)";
            MySqlCommand command = new MySqlCommand("", connect());
            command.CommandText = query;

            command.Parameters.AddWithValue("@num", number);
            command.Parameters.Add(new MySqlParameter("@plate", imageToByteArray(plate)));
            command.Parameters.Add(new MySqlParameter("@car", imageToByteArray(car)));
            command.Parameters.AddWithValue("@cam", cam);

            command.ExecuteNonQuery();
        }
        public bool sql_insert_unique(SQL_Data DTO)
        {
            query = "SELECT ID,plate_number FROM local.parking where camera_name = '" + DTO.camera_name +
                "' ORDER BY ID DESC LIMIT 1";
            MySqlCommand command = new MySqlCommand("", connect());
            command.CommandText = query;
            MySqlDataReader read = command.ExecuteReader();
            string last_plate_number = "";
            int id = 0;
            if (read.Read())
            {
                last_plate_number = read.GetString(1);
                id = read.GetInt32(0);
            }                
            if (CalculateSimilarity(last_plate_number, DTO.plate_number) >= 0.50)
            {
                if(DTO.plate_number.Length > last_plate_number.Length)
                {
                    query = "update local.parking set plate_number = '" + DTO.plate_number + "' where ID = " + id.ToString();
                    command.Parameters.AddWithValue("@num", DTO.plate_number);
                    command.Parameters.AddWithValue("@id", id);
                    command = new MySqlCommand("", connect());
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                    return true;
                }                    
                return false;
            }
                
            query = "insert into local.parking (plate_number,time,plate,car,camera_name) value(@num,now(),@plate,@car,@cam)";
            command = new MySqlCommand("", connect());
            command.CommandText = query;
            
            command.Parameters.AddWithValue("@num", DTO.plate_number);
            command.Parameters.Add(new MySqlParameter("@plate", imageToByteArray(ScaleImage(DTO.plate, 100, 100))));
            command.Parameters.Add(new MySqlParameter("@car", imageToByteArray(ScaleImage(DTO.car, 200, 200))));
            command.Parameters.AddWithValue("@cam", DTO.camera_name);

            command.ExecuteNonQuery();

            return true;
        }
        public DataTable sql_select_all()
        {
            try
            {
                DataTable dt = new DataTable();
                query = "SELECT * FROM local.parking";
                MySqlDataAdapter da = new MySqlDataAdapter(query, connect());
                da.Fill(dt);
                return dt;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Select All \n\r\n" + e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        public SQL_Data sql_select_one(int id)
        {
            try
            {
                DataTable dt = new DataTable();
                query = "SELECT * FROM local.parking where ID = " + id.ToString();
                DataTable result = new DataTable();
                MySqlCommand cmd = new MySqlCommand(query, connect());
                MySqlDataReader read = cmd.ExecuteReader();
                if (read.Read())
                {
                    SQL_Data dto = new SQL_Data();
                    dto.id = read.GetInt32(0);
                    dto.plate_number = read.GetString(1);
                    dto.datetime = read.GetDateTime(2);
                    dto.plate = byteArrayToImage((byte[])read.GetValue(3));
                    dto.car = byteArrayToImage((byte[])read.GetValue(4));
                    dto.camera_name = read.GetString(5);
                    return dto;               
                }
                throw new Exception("Can't read data");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error sql_select_one \n\r\n" + e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        public DataTable sql_query_custumor(string commandText)
        {
            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(commandText, connect());
            da.Fill(dt);
            return dt;
            //var command = new MySqlCommand(commandText, connect());
            //command.ExecuteNonQuery();
        }
        public void sql_reset()
        {
            var command = new MySqlCommand("Truncate table local.parking", connect());
            command.ExecuteNonQuery();
        }
        public static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
    }
}
