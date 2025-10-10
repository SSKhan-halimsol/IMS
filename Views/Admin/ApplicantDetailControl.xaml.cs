using IMS.Data;
using IMS.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            DataContext = _applicant;
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

                            MessageBox.Show("Comment saved successfully.",
                                          "Success",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Applicant not found in database.",
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving comment: {ex.Message}",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_applicant.ApplicantName}'?\n\n" +
                $"This action cannot be undone and will remove all associated data.",
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
                    MessageBox.Show("Delete failed. Applicant may have dependent records (QuizResult).\n\n" +
                                  "Please delete associated quiz results first.",
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
                        // If already marked, inform user
                        if (!string.IsNullOrEmpty(dbApplicant.Status) &&
                            (dbApplicant.Status == "Accepted" || dbApplicant.Status == "Rejected"))
                        {
                            MessageBox.Show($"This applicant is already marked as '{dbApplicant.Status}'.\n\n" +
                                          $"Current Status: {dbApplicant.Status}",
                                            "Already Processed",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                            return;
                        }

                        // Confirm the action
                        var confirmResult = MessageBox.Show(
                            $"Are you sure you want to mark '{_applicant.ApplicantName}' as {status}?",
                            "Confirm Status Update",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (confirmResult != MessageBoxResult.Yes)
                            return;

                        // Update status
                        dbApplicant.Status = status;
                        db.SaveChanges();

                        // Update UI model
                        _applicant.Status = status;

                        // Disable buttons after marking
                        DisableActionButtons();

                        // Show success with visual feedback
                        MessageBox.Show($"✓ Applicant successfully marked as '{status}'.\n\n" +
                                      $"Applicant: {_applicant.ApplicantName}\n" +
                                      $"New Status: {status}",
                                        "Success",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Applicant not found in database.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating status: {ex.Message}\n\n" +
                              $"Please try again or contact system administrator.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BackToApplicants_Click(object sender, RoutedEventArgs e)
        {
            _dashboard.MainContent.Content = new ApplicantsAdminControl(_dashboard);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_applicant.Status) &&
                (_applicant.Status == "Accepted" || _applicant.Status == "Rejected"))
            {
                DisableActionButtons();
                UpdateStatusDisplay();
            }
        }

        private void DisableActionButtons()
        {
            AcceptedBtn.IsEnabled = false;
            RejectedBtn.IsEnabled = false;

            AcceptedBtn.Opacity = 0.5;
            RejectedBtn.Opacity = 0.5;
        }
        private void UpdateStatusDisplay()
        {
            if (string.IsNullOrEmpty(_applicant.Status))
                return;
            if (_applicant.Status == "Accepted")
            {
                AcceptedBtn.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                AcceptedBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                AcceptedBtn.BorderThickness = new Thickness(2);
            }
            else if (_applicant.Status == "Rejected")
            {
                RejectedBtn.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                RejectedBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                RejectedBtn.BorderThickness = new Thickness(2);
            }
        }
    }
}