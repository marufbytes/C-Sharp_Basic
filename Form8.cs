using System;
using System.Collections;
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
    public partial class FlatDetailsForm : Form
    {
        public string LoggedInUsername { get; set; }
        public int LoggedInuserId { get; set; }
        private readonly int _flatId;

        public string connectionString ="data source=DESKTOP-EDVHTJF\\SQLEXPRESS; database=Home; integrated security=SSPI";

        public FlatDetailsForm(int flatId)
        {
            InitializeComponent();
            _flatId = flatId;
        }

        private void FlatDetailsForm_Load(object sender, EventArgs e)
        {
            if (_flatId <= 0)
            {
                MessageBox.Show("No FlatID was provided.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            LoadFlatDetails();
        }

        private void LoadFlatDetails()
        {
            string query = @"SELECT FlatID, LandlordID, Location, RentAmount, FlatType, Status, BachelorCategory 
                             FROM Flats WHERE FlatID = @FlatID";
            try 
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FlatID", _flatId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            FlatIdtextBox1.Text = reader["FlatID"].ToString();
                            LandlordIdtextBox2.Text = reader["LandlordID"].ToString();
                            LocationtextBox3.Text = reader["Location"].ToString();
                            RentAmounttextBox4.Text = reader["RentAmount"].ToString();
                            FlatTypetextBox5.Text = reader["FlatType"].ToString();
                            StatustextBox6.Text = reader["Status"].ToString();
                            BachelorCategorytextBox7.Text = reader["BachelorCategory"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No details found for this flat.", "Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Close();
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Failed to load flat details.\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
        

        public FlatDetailsForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TenantDashboard form3 = new TenantDashboard();
            form3.LoggedInUsername = LoggedInUsername;
            form3.LoggedInuserId = LoggedInuserId;
            form3.StartPosition = FormStartPosition.CenterScreen;
            form3.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FlatIdtextBox1.Text))
            {
                MessageBox.Show("No Flat selected.", "Book Flat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(FlatIdtextBox1.Text, out int flatIdParsed)) 
            {
                MessageBox.Show("Invalid FlatID.", "Book Flat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string flatType = FlatTypetextBox5.Text?.Trim();
            if (string.IsNullOrWhiteSpace(flatType))
            {
                MessageBox.Show("Flat type is missing.", "Book Flat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

         
            if (flatType.Equals("Bachelor", StringComparison.OrdinalIgnoreCase))
            {
                RoomBookDetails roomForm = new RoomBookDetails(flatIdParsed);
                roomForm.LoggedInUsername = this.LoggedInUsername; 
                roomForm.LoggedInuserId = this.LoggedInuserId;  
                roomForm.StartPosition = FormStartPosition.CenterParent;
                roomForm.ShowDialog(this);
                return;
               
            }

            int tenantId = 0;
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT TenantID FROM Tenants WHERE UserID = @UserID";
                using (SqlCommand getTenant = new SqlCommand(query ,conn))
                {
                    getTenant.Parameters.AddWithValue("@UserID", LoggedInuserId);
                    object t = getTenant.ExecuteScalar();
                    if (t == null || t == DBNull.Value)
                    {
                        MessageBox.Show("Your tenant profile was not found. Complete your tenant profile before booking.","Book Flat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    tenantId = (int)t;
                }

                using (var dup = new SqlCommand(
                    "SELECT COUNT(*) FROM Bookings WHERE TenantID=@TID AND FlatID=@FID AND Status IN ('Pending','Approved')", conn))
                {
                    dup.Parameters.AddWithValue("@TID", tenantId);
                    dup.Parameters.AddWithValue("@FID", flatIdParsed);
                    int exists = (int)dup.ExecuteScalar();
                    if (exists > 0)
                    {
                        MessageBox.Show("You already have a booking for this flat (Pending/Approved).",
                                        "Book Flat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                using (var ins = new SqlCommand(
                    "INSERT INTO Bookings (TenantID, FlatID, BookingDate, Status) VALUES (@TID, @FID, GETDATE(), 'Pending')", conn))
                {
                    ins.Parameters.AddWithValue("@TID", tenantId);
                    ins.Parameters.AddWithValue("@FID", flatIdParsed);
                    int rows = ins.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Booking request submitted (Pending).","Book Flat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Booking failed. Please try again.","Book Flat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 form1 = new Form1();
            form1.Show();
        }
    }
}
