using System;
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
    public partial class ManagerDashboard : Form
    {
        string connectionString = "data source=DESKTOP-EDVHTJF\\SQLEXPRESS; database=Home; integrated security=SSPI";
        public string LoggedInUsername { get; set; }
        public int LoggedInuserId { get; set; }
        public ManagerDashboard()
        {
            InitializeComponent();
        }

        private void ManagerDashboard_Load(object sender, EventArgs e)
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

            
                string query = @"SELECT m.ManagerID, m.FlatID, m.ManagerName, m.Phone, m.Email, u.UserID
                                FROM BachelorFlatManager m
                                INNER JOIN Users u ON m.UserID = u.UserID
                                  WHERE u.UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", LoggedInuserId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            textBox1.Text = reader["UserID"].ToString();
                            textBox2.Text = reader["ManagerID"].ToString();
                            textBox3.Text = reader["FlatID"]?.ToString();
                            textBox4.Text = reader["ManagerName"]?.ToString();
                            textBox5.Text = reader["Phone"]?.ToString();
                            textBox6.Text = reader["Email"]?.ToString();
                        }
                        else
                        {
                            MessageBox.Show("No manager profile found. Please check if you're assigned to a flat.","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RentPostForm2 roompostform = new RentPostForm2();
            roompostform.LoggedInUsername = LoggedInUsername;
            roompostform.LoggedInuserId = LoggedInuserId;
            roompostform.ShowDialog(this);
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text.Trim(), out int userId) || userId <= 0)
            {
                MessageBox.Show("Invalid UserID."); return;
            }
            if (!int.TryParse(textBox2.Text.Trim(), out int managerId) || managerId <= 0)
            {
                MessageBox.Show("Invalid ManagerID."); return;
            }

        
            object flatIdParam = DBNull.Value;
            string flatText = textBox3.Text.Trim();
            if (!string.IsNullOrEmpty(flatText))
            {
                if (!int.TryParse(flatText, out int flatId) || flatId <= 0)
                {
                    MessageBox.Show("FlatID must be a positive number or left blank."); return;
                }
                flatIdParam = flatId; 
            }

            string managerName = textBox4.Text.Trim();
            string phone = string.IsNullOrWhiteSpace(textBox5.Text) ? null : textBox5.Text.Trim();
            string email = string.IsNullOrWhiteSpace(textBox6.Text) ? null : textBox6.Text.Trim();

            if (string.IsNullOrWhiteSpace(managerName))
            {
                MessageBox.Show("Manager name is required."); return;
            }

       
            string sql = @"UPDATE dbo.BachelorFlatManager SET FlatID = @FlatID, ManagerName = @ManagerName, Phone = @Phone,Email = @Email
                            WHERE ManagerID = @ManagerID AND UserID = @UserID;";

            int rows;
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@FlatID", flatIdParam ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ManagerName", managerName);
                cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ManagerID", managerId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                rows = cmd.ExecuteNonQuery();
            }

            if (rows > 0)
                MessageBox.Show("Manager profile updated successfully.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("No row updated. Check ManagerID/UserID.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void profileBackButton_Click(object sender, EventArgs e)
        {
            var f1 = new Form1();
            f1.ShowDialog();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
               "Are you sure you want to exit the application?",
               "Confirm Exit",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Question
           );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            var approveForm = new Approve2();
            approveForm.LoggedInUsername = LoggedInUsername;
            approveForm.LoggedInuserId = LoggedInuserId;
            approveForm.ShowDialog(this);
           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FlatPaymentsForm form = new FlatPaymentsForm();
            form.LoggedInuserId = this.LoggedInuserId;
            form.LoggedInUsername = this.LoggedInUsername;
            form.LoggedInRole = "Manager"; 
            form.Show();
            this.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Notice notice = new Notice();
            notice.Show();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text.Trim(), out int userId) || userId <= 0)
            {
                MessageBox.Show("Invalid UserID."); return;
            }
            if (!int.TryParse(textBox2.Text.Trim(), out int managerId) || managerId <= 0)
            {
                MessageBox.Show("Invalid ManagerID."); return;
            }


            object flatIdParam = DBNull.Value;
            string flatText = textBox3.Text.Trim();
            if (!string.IsNullOrEmpty(flatText))
            {
                if (!int.TryParse(flatText, out int flatId) || flatId <= 0)
                {
                    MessageBox.Show("FlatID must be a positive number or left blank."); return;
                }
                flatIdParam = flatId;
            }

            string managerName = textBox4.Text.Trim();
            string phone = string.IsNullOrWhiteSpace(textBox5.Text) ? null : textBox5.Text.Trim();
            string email = string.IsNullOrWhiteSpace(textBox6.Text) ? null : textBox6.Text.Trim();

            if (string.IsNullOrWhiteSpace(managerName))
            {
                MessageBox.Show("Manager name is required."); return;
            }


            string sql = @"UPDATE dbo.BachelorFlatManager SET FlatID = @FlatID, ManagerName = @ManagerName, Phone = @Phone, Email = @Email
                   WHERE ManagerID = @ManagerID AND UserID = @UserID;";

            int rows;
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@FlatID", flatIdParam ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ManagerName", managerName);
                cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ManagerID", managerId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                rows = cmd.ExecuteNonQuery();
            }

            if (rows > 0)
                MessageBox.Show("Manager profile updated successfully.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("No row updated. Check ManagerID/UserID.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }
    }  
    
}
