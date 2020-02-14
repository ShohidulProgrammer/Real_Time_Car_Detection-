using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LPR
{
    public partial class sql : Form
    {
        public sql()
        {
            InitializeComponent();
        }
        SQL_helper sql_help = new SQL_helper();
        private void sql_Load(object sender, EventArgs e)
        {
            hour.DropDownStyle = ComboBoxStyle.DropDownList;
            min.DropDownStyle = ComboBoxStyle.DropDownList;
            sec.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int t = 0; t < 60; t++)
            {
                min.Items.Add(t.ToString("D2"));
                sec.Items.Add(t.ToString("D2"));
            }
            for (int t = 0; t < 24; t++)
            {
                hour.Items.Add(t.ToString("D2"));
            }
            update_datagrid(true);
        }
        int seleted_row = 0;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                seleted_row = e.RowIndex;
                DataGridView grid = (DataGridView)sender;
                id.Text = grid.Rows[e.RowIndex].Cells[0].Value.ToString();
                plate_num.Text = grid.Rows[e.RowIndex].Cells[1].Value.ToString();
                cam_name.Text = grid.Rows[e.RowIndex].Cells[5].Value.ToString();
                byte[] img = (byte[])grid.Rows[e.RowIndex].Cells[3].Value;
                pictureBox2.Image = SQL_helper.byteArrayToImage(img);
                img = (byte[])grid.Rows[e.RowIndex].Cells[4].Value;
                pictureBox1.Image = SQL_helper.byteArrayToImage(img);
                DateTime d = (DateTime)grid.Rows[e.RowIndex].Cells[2].Value;

                hour.SelectedIndex = d.Hour;
                min.SelectedIndex = d.Minute;
                sec.SelectedIndex = d.Second;

                dateTimePicker1.Value = d;
            }
            catch (Exception) { }
        }
        int time = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(time <   10)
                update_datagrid(false);
            else
            {
                update_datagrid(true);
                time = 0;
            }
            time++;
            
        }
        private void update_datagrid(bool isfull)
        {
            DataTable dt;
            int p = dataGridView1.FirstDisplayedScrollingRowIndex;
            if (isfull)
            {
                dt = sql_help.sql_select_all();
                dataGridView1.DataSource = dt;
                dataGridView1.Columns[0].Width = 40;
                dataGridView1.Columns[2].Width = 120;
                try
                {
                    dataGridView1.Rows[seleted_row].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[seleted_row].Cells[0];
                }
                catch (Exception) { }
                try
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = p;
                }
                catch (Exception) { }
            }
            else
            {
                dt = sql_help.sql_query_custumor("SELECT ID FROM local.parking");
                if (dataGridView1.RowCount - 1 > dt.Rows.Count)
                {
                    update_datagrid(true);
                    return;
                }
                else if (dataGridView1.RowCount - 1 < dt.Rows.Count)
                {
                    int count = dt.Rows.Count - dataGridView1.RowCount + 1;
                    for (int i = 0; i < count; i++)
                    {
                        SQL_Data dto = sql_help.sql_select_one(Convert.ToInt32((UInt32)dt.Rows[dt.Rows.Count - count + i].ItemArray[0]));

                        DataRow new_row = ((DataTable)dataGridView1.DataSource).NewRow();
                        new_row[0] = dto.id;
                        new_row[1] = dto.plate_number;
                        new_row[2] = dto.datetime;
                        new_row[3] = SQL_helper.imageToByteArray(dto.plate);
                        new_row[4] = SQL_helper.imageToByteArray(dto.car);
                        new_row[5] = dto.camera_name;
                        ((DataTable)dataGridView1.DataSource).Rows.Add(new_row);
                        ((DataTable)dataGridView1.DataSource).AcceptChanges();
                    }
                }
                else
                {
                    return;
                }
            }

            dataGridView1.Columns[0].Width = 40;
            dataGridView1.Columns[2].Width = 120;
            try
            {
                dataGridView1.Rows[seleted_row].Selected = true;
                dataGridView1.CurrentCell = dataGridView1.Rows[seleted_row].Cells[0];
            }
            catch (Exception) { }
            try
            {
                dataGridView1.FirstDisplayedScrollingRowIndex = p;
            }
            catch (Exception) { }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Be careful: \n\r\n " + "Do you want to RESET and DELETE all data"
                                                , "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                sql_help.sql_reset();
                seleted_row = 0;
                update_datagrid(true);
                id.Text = "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime d = dateTimePicker1.Value;
            string query = "update local.parking set plate_number = '" + plate_num.Text +
                "',time = '" + d.Year.ToString() + "-" + d.Month.ToString() +
                "-" + d.Day + " " + hour.Text + ":" + min.Text + ":" + sec.Text +
                "',camera_name = '" + cam_name.Text +
                "' where ID = " + id.Text;
            try
            {
                sql_help.sql_query_custumor(query);
            }
            catch (Exception)
            {
                MessageBox.Show("Update Error !!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show("Update Done !!!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            update_datagrid(true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (id.Text == "")
                return;
            DialogResult result = MessageBox.Show("Do you want to DELETE this Row"
                                                , "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                string query = "DELETE FROM `local`.`parking` WHERE `ID`='" + id.Text+"'";
                try
                {
                    sql_help.sql_query_custumor(query);
                }
                catch (Exception)
                {
                    MessageBox.Show("DELETE Error !!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MessageBox.Show("DELETE Done !!!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                seleted_row = 0;
                id.Text = "";
                update_datagrid(true);
            }
        }
    }
}
