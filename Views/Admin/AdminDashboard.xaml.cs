using IMS.Models;
using IMS.Views.Admin;
using System;
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

        private void BtnQuiz_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnQuiz);
            MainContent.Content = new QuizManagementControl();
        }

        private void BtnDesignations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetActiveButton(BtnDesignations);
                MainContent.Content = new DesignationControl();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Designations module: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SetActiveButton(Button activeButton)
        {
            // Reset all
            BtnHome.Tag = null;
            BtnApplicants.Tag = null;
            BtnSelected.Tag = null;
            BtnDesignations.Tag = null;
            BtnQuiz.Tag = null;

            // Highlight only the active one
            activeButton.Tag = "Active";
        }


        public void ShowApplicantDetails(Applicant selectedApplicant)
        {
            MainContent.Content = new ApplicantDetailControl(selectedApplicant, this);
        }
    }
}