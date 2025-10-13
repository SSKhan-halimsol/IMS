using IMS.Data;
using IMS.Models;
using System;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateResumeDisplay();

            if (!string.IsNullOrEmpty(_applicant.Status) &&
                (_applicant.Status == "Accepted" || _applicant.Status == "Rejected"))
            {
                DisableActionButtons();
                UpdateStatusDisplay();
            }
        }

        /// <summary>
        /// ✅ Checks if the applicant has uploaded a resume, and updates the display text accordingly.
        /// </summary>
        private void UpdateResumeDisplay()
        {
            try
            {
                if (!string.IsNullOrEmpty(_applicant.ResumePath) && File.Exists(_applicant.ResumePath))
                {
                    ResumeFileNameText.Text = Path.GetFileName(_applicant.ResumePath);
                    ResumeFileNameText.Foreground = new SolidColorBrush(Colors.LightGreen);
                    ResumeFileNameText.FontStyle = FontStyles.Normal;
                }
                else
                {
                    ResumeFileNameText.Text = "No resume uploaded";
                    ResumeFileNameText.Foreground = new SolidColorBrush(Colors.LightGray);
                    ResumeFileNameText.FontStyle = FontStyles.Italic;
                }
            }
            catch (Exception ex)
            {
                ResumeFileNameText.Text = "Error checking resume";
                MessageBox.Show($"Error checking resume: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewResume_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is Applicant applicant && !string.IsNullOrEmpty(applicant.ResumePath))
                {
                    if (File.Exists(applicant.ResumePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = applicant.ResumePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("Resume file not found at the specified location.",
                            "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No resume uploaded for this applicant.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening resume: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadResume_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is Applicant applicant && !string.IsNullOrEmpty(applicant.ResumePath))
                {
                    if (File.Exists(applicant.ResumePath))
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            FileName = Path.GetFileName(applicant.ResumePath),
                            Filter = "All Files (*.*)|*.*|PDF Files (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx",
                            Title = "Save Resume As"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            File.Copy(applicant.ResumePath, saveFileDialog.FileName, true);
                            MessageBox.Show("Resume downloaded successfully!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Resume file not found at the specified location.",
                            "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No resume uploaded for this applicant.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading resume: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveComment_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Applicant applicant)
            {
                try
                {
                    using (var db = new IMSDbContext())
                    {
                        var dbApplicant = db.Applicants.Find(applicant.Id);
                        if (dbApplicant != null)
                        {
                            dbApplicant.AdminComment = CommentBox.Text.Trim();
                            db.SaveChanges();
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

                        var confirmResult = MessageBox.Show(
                            $"Are you sure you want to mark '{_applicant.ApplicantName}' as {status}?",
                            "Confirm Status Update",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (confirmResult != MessageBoxResult.Yes)
                            return;

                        dbApplicant.Status = status;
                        db.SaveChanges();

                        _applicant.Status = status;

                        DisableActionButtons();
                        UpdateStatusDisplay();

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
                AcceptedBtn.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                AcceptedBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                AcceptedBtn.BorderThickness = new Thickness(2);
            }
            else if (_applicant.Status == "Rejected")
            {
                RejectedBtn.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                RejectedBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                RejectedBtn.BorderThickness = new Thickness(2);
            }
        }

        private void BackToApplicants_Click(object sender, RoutedEventArgs e)
        {
            _dashboard.MainContent.Content = new ApplicantsAdminControl(_dashboard);
        }
    }
}