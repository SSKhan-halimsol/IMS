using System;
using System.Data.SqlClient;
using System.Windows;

namespace IMS.Views
{
    public partial class DatabaseConfigDialog : Window
    {
        public string ConnectionString { get; private set; }

        public DatabaseConfigDialog()
        {
            InitializeComponent();
        }

        //private void Cancel_Click(object sender, RoutedEventArgs e)
        //{
        //    Application.Current.Shutdown();
        //}

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string server = TxtServer.Text.Trim();
            string db = TxtDatabase.Text.Trim();
            string user = TxtUser.Text.Trim();
            string pass = TxtPassword.Password.Trim();

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(db) ||
                string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Please fill in all fields.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connStr =
                $"Data Source={server};Initial Catalog={db};User ID={user};Password={pass};MultipleActiveResultSets=True";

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                }

                ConnectionString = connStr;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}