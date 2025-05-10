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

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Data Source=YunusEmresPC;Initial Catalog=windowsformapp;Integrated Security=True";
        private int selectedId = -1;

        public Form1()
        {
            InitializeComponent();
            SetupDataGridView();
            CreateDatabaseIfNotExists();
            btnEkle.Click += BtnEkle_Click;
            btnGuncelle.Click += BtnGuncelle_Click;
            btnListele.Click += BtnListele_Click;
            btnSil.Click += BtnSil_Click;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            RefreshDataGridView();
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string createTableQuery = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contacts')
                        BEGIN
                            CREATE TABLE Contacts (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                İsim NVARCHAR(50),
                                Soyisim NVARCHAR(50),
                                Email NVARCHAR(100),
                                Telefon NVARCHAR(20)
                            )
                        END";
                    using (SqlCommand cmd = new SqlCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı oluşturma hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            dataGridView1.Columns.Add("Id", "Id");
            dataGridView1.Columns.Add("İsim", "İsim");
            dataGridView1.Columns.Add("Soyisim", "Soyisim");
            dataGridView1.Columns.Add("Email", "Email");
            dataGridView1.Columns.Add("Telefon", "Telefon");
            dataGridView1.Columns["Id"].Visible = false;
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string insertQuery = @"INSERT INTO Contacts (İsim, Soyisim, Email, Telefon) 
                                             VALUES (@İsim, @Soyisim, @Email, @Telefon)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@İsim", txtİsim.Text);
                            cmd.Parameters.AddWithValue("@Soyisim", txtSoyisim.Text);
                            cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("@Telefon", txtTelefon.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    RefreshDataGridView();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kayıt ekleme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (selectedId >= 0 && ValidateInputs())
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string updateQuery = @"UPDATE Contacts 
                                             SET İsim = @İsim, Soyisim = @Soyisim, 
                                                 Email = @Email, Telefon = @Telefon 
                                             WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", selectedId);
                            cmd.Parameters.AddWithValue("@İsim", txtİsim.Text);
                            cmd.Parameters.AddWithValue("@Soyisim", txtSoyisim.Text);
                            cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("@Telefon", txtTelefon.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    RefreshDataGridView();
                    ClearInputs();
                    selectedId = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Güncelleme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellenecek bir kayıt seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnListele_Click(object sender, EventArgs e)
        {
            RefreshDataGridView();
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (selectedId >= 0)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM Contacts WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", selectedId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    RefreshDataGridView();
                    ClearInputs();
                    selectedId = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Silme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Lütfen silinecek bir kayıt seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                selectedId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);
                txtİsim.Text = dataGridView1.SelectedRows[0].Cells["İsim"].Value.ToString();
                txtSoyisim.Text = dataGridView1.SelectedRows[0].Cells["Soyisim"].Value.ToString();
                txtEmail.Text = dataGridView1.SelectedRows[0].Cells["Email"].Value.ToString();
                txtTelefon.Text = dataGridView1.SelectedRows[0].Cells["Telefon"].Value.ToString();
            }
        }

        private void RefreshDataGridView()
        {
            try
            {
                dataGridView1.Rows.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string selectQuery = "SELECT * FROM Contacts";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dataGridView1.Rows.Add(
                                    reader["Id"],
                                    reader["İsim"],
                                    reader["Soyisim"],
                                    reader["Email"],
                                    reader["Telefon"]
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri listeleme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInputs()
        {
            txtİsim.Clear();
            txtSoyisim.Clear();
            txtEmail.Clear();
            txtTelefon.Clear();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtİsim.Text) ||
                string.IsNullOrWhiteSpace(txtSoyisim.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtTelefon.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
