using IMS.ViewModels;
using System.Windows;

namespace IMS.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

    public void SettingBtn_Click(object sender, RoutedEventArgs e)
    {
        var set = new DatabaseConfigDialog();
        bool? result = set.ShowDialog();

        if (result == true)
        {
            // ✅ Update connection string in App.config
            string connStr = set.ConnectionString;
            IMS.Helpers.DbConfigManager.SaveConnectionString(connStr);
            IMS.Helpers.DbConfigManager.ApplyToAppConfig(connStr);
            IMS.Helpers.DbConfigManager.SetIsConfigured(true);

            MessageBox.Show("Database configuration updated successfully!\nThe application will now restart to apply the new settings.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // ✅ Restart the application so the new connection is used
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }
        else
        {
            MessageBox.Show("Database configuration not changed.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }

        private void back_click(object sender , RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}