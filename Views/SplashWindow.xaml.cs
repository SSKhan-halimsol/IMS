using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace IMS.Views
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            Loaded += SplashWindow_Loaded;
        }

        private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Checking database configuration...";

                bool isConfigured = IMS.Helpers.DbConfigManager.IsConfigured();
                string connStr = IMS.Helpers.DbConfigManager.GetConnectionString();

                // Step 1: If not configured, show dialog
                if (!isConfigured || string.IsNullOrEmpty(connStr))
                {
                    var dlg = new DatabaseConfigDialog();
                    bool? result = dlg.ShowDialog();

                    if (result != true)
                    {
                        Application.Current.Shutdown();
                        return;
                    }

                    connStr = dlg.ConnectionString;
                }

                // Step 2: Validate connection
                StatusText.Text = "Validating connection...";
                bool dbConnected = await Task.Run(() => CheckDatabaseConnection(connStr));

                if (dbConnected)
                {
                    StatusText.Text = "Database connected. Initializing...";
                    await Task.Delay(1500);
                    new MainWindow().Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Database connection failed. Please reconfigure.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    IMS.Helpers.DbConfigManager.ClearConnectionString();
                    IMS.Helpers.DbConfigManager.SetIsConfigured(false);

                    // Restart splash to retry
                    new SplashWindow().Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private bool CheckDatabaseConnection(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}