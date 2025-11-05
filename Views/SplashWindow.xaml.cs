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
                StatusText.Text = "Please provide database credentials...";

                // Always ask for credentials at startup
                string connStr = null;

                var dlg = new DatabaseConfigDialog();
                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    connStr = dlg.ConnectionString;
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }

                // Validate connection
                StatusText.Text = "Validating connection...";
                bool dbConnected = await Task.Run(() => CheckDatabaseConnection(connStr));

                if (dbConnected)
                {
                    StatusText.Text = "Database connected. Initializing...";
                    await Task.Delay(1500);

                    // Open Main Window
                    new MainWindow().Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Database connection failed. Please try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Reopen Splash (re-ask credentials)
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