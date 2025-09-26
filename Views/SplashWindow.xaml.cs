using System.Threading.Tasks;
using System.Windows;
using Interview_Management_System.Helpers;

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
                // Your DB connection logic here (e.g. SQL connection test)
                System.Threading.Thread.Sleep(2000); // simulate delay
                return true; // return false if failed
            }
            catch
            {
                return false;
            }
        }
    }
}