using System.Threading.Tasks;
using System.Windows;
using System;
using System.Data.SqlClient;

namespace IMS.Views
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            Loaded += SplashWindow_Loaded;
        }

            // Run DB check in background
            private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Checking database connection...";

            bool dbConnected = await Task.Run(() => CheckDatabaseConnection());

            if (dbConnected)
            {
                StatusText.Text = "Database connected. Initializing...";
                await Task.Delay(1500); // give some time to show message
                // Open main window
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                StatusText.Text = "❌ Failed to connect with database";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private bool CheckDatabaseConnection()
        {
            try
            {
                string connectionString = @"Data Source=192.168.1.188;Initial Catalog=IMS;User ID=sa;Password=123456;MultipleActiveResultSets=True";
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database connection failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }
    }
}