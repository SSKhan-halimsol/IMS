using System.Windows;

namespace IMS.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void btnApplicant_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Applicant Form",
                Content = new ApplicantsControl(),
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Show();
            this.Close();
        }
        private void Login_click(object sender, RoutedEventArgs e)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }
    }
}