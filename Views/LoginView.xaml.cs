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

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }
    }
}