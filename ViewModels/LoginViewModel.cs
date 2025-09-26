using IMS.Helpers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IMS.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async (param) => await ExecuteLogin(param));
        }

        private async Task ExecuteLogin(object param)
        {
            bool isValid = await SecurityHelper.ValidateAdminAsync(Username, Password);

            if (isValid)
            {
                // ✅ Open Admin Dashboard
                var adminDashboard = new Views.Admin.AdminDashboard();
                adminDashboard.Show();

                // Close login window
                if (param is Window loginWindow)
                    loginWindow.Close();
            }
            else
            {
                ErrorMessage = "Invalid username or password!";
                MessageBox.Show(ErrorMessage, "Login Failed",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}