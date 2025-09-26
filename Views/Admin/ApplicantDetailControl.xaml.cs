using IMS.Data;
using IMS.Models;
using System;
using System.Data.SqlClient;
using System.IdentityModel;
using System.Windows;
using System.Windows.Controls;

namespace IMS.Views.Admin
{
    public partial class ApplicantDetailControl : UserControl
    {
        private readonly Applicant _applicant;
        private readonly AdminDashboard _dashboard;

        public ApplicantDetailControl(Applicant applicant, AdminDashboard dashboard)
        {
            InitializeComponent();
            _applicant = applicant;
            _dashboard = dashboard;
            DataContext = applicant;
        }

        private void SaveComment_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Applicant applicant)
            {
                try
                {
                    using (var db = new IMSDbContext())
                    {
                        // Get the applicant from the database
                        var dbApplicant = db.Applicants.Find(applicant.Id);
                        if (dbApplicant != null)
                        {
                            // Save the comment
                            dbApplicant.AdminComment = CommentBox.Text.Trim();
                            db.SaveChanges();

                            // Update UI
                            applicant.AdminComment = dbApplicant.AdminComment;

                            MessageBox.Show("Comment saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Applicant not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving comment: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete {_applicant.ApplicantName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool deleted = await ApplicantRepository.DeleteAsync(_applicant.Id);
                if (deleted)
                {
                    MessageBox.Show("Applicant deleted successfully.",
                                    "Deleted",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    // Go back to applicant list after delete
                    _dashboard.MainContent.Content = new ApplicantsAdminControl(_dashboard);
                }
                else
                {
                    MessageBox.Show("Delete failed. Applicant may have dependent records (QuizResult).",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting applicant: {ex.Message}",
                                "SQL Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
        private void Accepted_Click(object sender, RoutedEventArgs e)
        {
            UpdateApplicantStatus("Accepted");
        }

        private void Rejected_Click(object sender, RoutedEventArgs e)
        {
            UpdateApplicantStatus("Rejected");
        }

        private void UpdateApplicantStatus(string status)
        {
            try
            {
                using (var db = new IMSDbContext())
                {
                    var dbApplicant = db.Applicants.Find(_applicant.Id);
                    if (dbApplicant != null)
                    {
                        // If already marked, just inform user and disable buttons
                        if (dbApplicant.Status == "Accepted" || dbApplicant.Status == "Rejected")
                        {
                            MessageBox.Show($"This applicant is already marked as {dbApplicant.Status}.",
                                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            DisableStatusButtons();
                            return;
                        }

                        // Otherwise, update status
                        dbApplicant.Status = status;
                        db.SaveChanges();

                        // Update UI model
                        _applicant.Status = status;

                        MessageBox.Show($"Applicant marked as {status}.",
                                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Disable after marking
                        DisableStatusButtons();
                    }
                    else
                    {
                        MessageBox.Show("Applicant not found in database.",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating status: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisableStatusButtons()
        {
            AcceptedBtn.IsEnabled = false;
            RejectedBtn.IsEnabled = false;
        }

        private void BackToApplicants_Click(object sender, RoutedEventArgs e)
        {
            _dashboard.MainContent.Content = new ApplicantsAdminControl(_dashboard);
        }

        // Call this when loading the control to disable if already marked
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_applicant.Status == "Accepted" || _applicant.Status == "Rejected")
            {
                DisableStatusButtons();
            }
        }
    }
}