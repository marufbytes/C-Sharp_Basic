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
    public partial class RoomBookForms : Form
    {
        private readonly int _flatId;
        public string LoggedInUsername { get; set; }
        public int LoggedInuserId { get; set; }

        public string connectionString ="data source=DESKTOP-EDVHTJF\\SQLEXPRESS; database=Home; integrated security=SSPI";

        public RoomBookForms(int flatId)
        {
            InitializeComponent();

            _flatId = flatId; ;

            
            string startQuery = @"SELECT RoomID,FlatID,RoomNumber,RentAmount,Status FROM dbo.Rooms WHERE FlatID = @FlatID AND Status = 'Vacant' ORDER BY RoomNumber;";

            FillDataGridView(startQuery, new[] { new SqlParameter("@FlatID", _flatId) });
        }

        private void InitRoomStatusCombo()
        {
            if (comboStatus == null) return; 
            comboStatus.Items.Clear();
            comboStatus.Items.Add("All");
            comboStatus.Items.Add("Vacant");
            comboStatus.Items.Add("Occupied");
            comboStatus.SelectedIndex = 0;
        }

        private void FillDataGridView(string sql, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dataGridViewRooms.DataSource = dt; 
                }
            }
        }

       
        private void btnFilter_Click(object sender, EventArgs e)
        {
            string status = (comboStatus?.SelectedItem?.ToString() ?? "All").Trim();

            string q = @"SELECT RoomID,FlatID,RoomNumber, RentAmount, Status FROM dbo.Rooms WHERE FlatID = @FlatID AND (@S = 'All' OR Status = @S) ORDER BY RoomNumber;";

            FillDataGridView(q, new[]
            {
            new SqlParameter("@FlatID", _flatId),
            new SqlParameter("@S", status)
        });
        }

        private void Form10_Load(object sender, EventArgs e)
        {

        }

        private void dataGridViewRooms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; 

         
            DataGridViewRow row = dataGridViewRooms.Rows[e.RowIndex];

         
            int roomId = Convert.ToInt32(row.Cells["RoomID"].Value);
            int flatId = Convert.ToInt32(row.Cells["FlatID"].Value);
            string roomNumber = row.Cells["RoomNumber"].Value?.ToString();
            string rentAmount = row.Cells["RentAmount"].Value?.ToString();
            string status = row.Cells["Status"].Value?.ToString();

        
            var detailsForm = new RoomBookDetails(roomId, flatId, roomNumber, rentAmount, status);
            detailsForm.ShowDialog(this);
        }
        private void RoomBookDetails_Load(object sender, EventArgs e)
        {
        }

        private void bookthisRoom_Click(object sender, EventArgs e)
        {
            if (dataGridViewRooms.CurrentRow == null)
            {
                MessageBox.Show("Please select a room first.", "Book Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

    
            object val = dataGridViewRooms.CurrentRow.Cells["RoomID"].Value;
            if (val == null || val == DBNull.Value || !int.TryParse(val.ToString(), out int roomId))
            {
                MessageBox.Show("Invalid RoomID on the selected row.", "Book Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

     
            int tenantId;
            string tenantGender = null;
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT TenantID, Gender FROM Tenants WHERE UserID = @UID", conn))
            {
                cmd.Parameters.AddWithValue("@UID", LoggedInuserId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        MessageBox.Show("No tenant profile found for this user. Complete your tenant profile first.","Book Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    tenantId = (int)r["TenantID"];
                    tenantGender = r["Gender"]?.ToString();
                }
            }

       
            string flatType = null, bachelorCategory = null;
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(
                "SELECT FlatType, BachelorCategory FROM Flats WHERE FlatID = @FID", conn))
            {
                cmd.Parameters.AddWithValue("@FID", _flatId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        flatType = r["FlatType"].ToString();
                        bachelorCategory = r["BachelorCategory"]?.ToString();
                    }
                }
            }

            if (string.Equals(flatType, "Bachelor", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(tenantGender, bachelorCategory, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        $"This Bachelor flat is restricted to {bachelorCategory} tenants only.",
                        "Booking Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

           
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction tx = conn.BeginTransaction();

              
                const string updateSql = @"UPDATE Rooms SET Status = 'Pending'WHERE RoomID = @RoomID AND Status = 'Vacant';";
                int rows;
                using (var cmd = new SqlCommand(updateSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@RoomID", roomId);
                    rows = cmd.ExecuteNonQuery();
                }

                if (rows == 0)
                {
                    MessageBox.Show("This room is not available (already Pending/Occupied).","Book Room", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tx.Rollback();
                    return;
                }

               
                const string insertSql = @"INSERT INTO Bookings (TenantID, FlatID, RoomID, BookingDate, Status) VALUES (@TID, @FID, @RID, GETDATE(), 'Pending');";

                using (var cmd = new SqlCommand(insertSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@TID", tenantId);
                    cmd.Parameters.AddWithValue("@FID", _flatId);
                    cmd.Parameters.AddWithValue("@RID", roomId);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();

                MessageBox.Show("Room booking request submitted (status: Pending).",
                                "Book Room", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            
            string refreshQuery = @"SELECT RoomID, FlatID, RoomNumber, RentAmount, Status FROM dbo.Rooms WHERE FlatID = @FlatID ORDER BY RoomNumber;";

            FillDataGridView(refreshQuery, new[] { new SqlParameter("@FlatID", _flatId) });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FlatDetailsForm fdf = new FlatDetailsForm();
            fdf.LoggedInUsername=this.LoggedInUsername;
            fdf.LoggedInuserId=this.LoggedInuserId;
            fdf.Show();
            this.Hide();
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