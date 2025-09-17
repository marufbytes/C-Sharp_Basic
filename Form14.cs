﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rent_dibo
{
    public partial class RentPostForm2 : Form
    {
        public  string cs = "data source=DESKTOP-EDVHTJF\\SQLEXPRESS; database=Home; integrated security=SSPI";

        public string LoggedInUsername { get; set; }
        public int LoggedInuserId { get; set; }
        public RentPostForm2()
        {
            InitializeComponent();
        }

        private void RentPostForm2_Load(object sender, EventArgs e)
        {
            textBox1.ReadOnly = true; 
            textBox4.ReadOnly = true;    
            textBox4.Text = "Vacant";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox2.Text.Trim(), out int flatId) || flatId <= 0)
            {
                MessageBox.Show("Enter a valid FlatID.", "Post Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string roomNumber = textBox5.Text.Trim();
            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                MessageBox.Show("Enter a Room Number.", "Post Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBox3.Text.Trim(), out decimal rentAmount) || rentAmount <= 0)
            {
                MessageBox.Show("Enter a valid Rent Amount.", "Post Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool assigned = false;
            using (SqlConnection conn = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM BachelorFlatManager WHERE UserID=@UID AND FlatID=@F", conn))
            {
                cmd.Parameters.AddWithValue("@UID", LoggedInuserId);
                cmd.Parameters.AddWithValue("@F", flatId);
                conn.Open();
                assigned = ((int)cmd.ExecuteScalar()) > 0;
            }

            if (!assigned)
            {
                MessageBox.Show("You are not assigned to manage this flat.", "Post Room",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

         
            string flatType = null;
            using (SqlConnection conn = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand("SELECT FlatType FROM Flats WHERE FlatID=@F", conn))
            {
                cmd.Parameters.AddWithValue("@F", flatId);
                conn.Open();
                object o = cmd.ExecuteScalar();
                flatType = o?.ToString();
            }

            if (!string.Equals(flatType, "Bachelor", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Rooms can only be added to Bachelor flats.", "Post Room",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

         
            const string sqlInsert = @"INSERT INTO Rooms (FlatID, RoomNumber, RentAmount, Status) OUTPUT INSERTED.RoomIDVAL UES (@FlatID, @RoomNumber, @RentAmount, 'Vacant');";

            int newRoomId;
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sqlInsert, conn))
            {
                cmd.Parameters.AddWithValue("@FlatID", flatId);
                cmd.Parameters.AddWithValue("@RoomNumber", roomNumber);
                cmd.Parameters.AddWithValue("@RentAmount", rentAmount);

                conn.Open();
                newRoomId = (int)cmd.ExecuteScalar();
            }

          
            textBox1.Text = newRoomId.ToString();
            MessageBox.Show("Room posted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void profileBackButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
    
}
