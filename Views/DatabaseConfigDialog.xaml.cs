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

            string masterConnStr =
                $"Data Source={server};Initial Catalog=master;User ID={user};Password={pass};MultipleActiveResultSets=True";

            try
            {
                using (var conn = new SqlConnection(masterConnStr))
                {
                    conn.Open();

                    // ✅ Check if database exists
                    string checkDb = $"SELECT db_id('{db}')";
                    using (var cmd = new SqlCommand(checkDb, conn))
                    {
                        object result = cmd.ExecuteScalar();

                        // ✅ If not exists, create it
                        if (result == DBNull.Value || result == null)
                        {
                            string createDb = $"CREATE DATABASE [{db}]";
                            using (var createCmd = new SqlCommand(createDb, conn))
                            {
                                createCmd.ExecuteNonQuery();
                            }
                            MessageBox.Show($"Database '{db}' created successfully.", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

                // ✅ Now connect to the newly created (or existing) database
                string connStr =
                    $"Data Source={server};Initial Catalog={db};User ID={user};Password={pass};MultipleActiveResultSets=True";

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                }

                // ✅ Save connection and apply config
                ConnectionString = connStr;
                IMS.Helpers.DbConfigManager.SaveConnectionString(connStr);
                IMS.Helpers.DbConfigManager.ApplyToAppConfig(connStr);
                IMS.Helpers.DbConfigManager.SetIsConfigured(true);

                MessageBox.Show("Connection successful and configuration saved!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

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