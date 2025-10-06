using IMS.Models;
using IMS.Views.Admin;
using System.Windows;
using System.Windows.Controls;

namespace IMS.Views.Admin
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard()
        {
            InitializeComponent();
            MainContent.Content = new HomeControl();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnHome);
            MainContent.Content = new HomeControl();
        }

        private void BtnApplicants_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnApplicants);
            MainContent.Content = new ApplicantsAdminControl(this);
        }

        private void Btnsucc_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnSelected);
            MainContent.Content = new SuccessfulCandidatesControl(this);
        }

        private void BtnDesignations_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnDesignations);
            MainContent.Content = new DesignationAdminControl();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logged out successfully!", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
            LoginView login = new LoginView();
            login.Show();
            this.Close();
        }
        private void SetActiveButton(Button activeButton)
        {
            // Reset all
            BtnHome.Tag = null;
            BtnApplicants.Tag = null;
            BtnSelected.Tag = null;

            // Highlight only the active one
            activeButton.Tag = "Active";
        }


        public void ShowApplicantDetails(Applicant selectedApplicant)
        {
            MainContent.Content = new ApplicantDetailControl(selectedApplicant, this);
        }
    }
}