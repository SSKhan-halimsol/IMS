using IMS.Data;
using IMS.Helpers;
using IMS.Models;
using IMS.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IMS.Views
{
    public partial class ApplicantsControl : UserControl
    {
        private int _step = 1;
        private string _resumeFilePath = null;

        public ApplicantsControl()
        {
            InitializeComponent();
            this.Loaded += ApplicantsControl_Loaded;
            LoadDesignations();
        }

        private async void LoadDesignations()
        {
            var list = await DesignationRepository.GetAllAsync();
            DesignationBox.ItemsSource = list;
        }

        private void ApplicantsControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.WindowState = WindowState.Maximized;
                parentWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                parentWindow.WindowStyle = WindowStyle.None;
                parentWindow.ResizeMode = ResizeMode.NoResize;
            }
        }

        private void UploadResumeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select Resume",
                    Filter = "PDF Files (*.pdf)|*.pdf|Word Documents (*.docx;*.doc)|*.docx;*.doc|All Files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFile = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(selectedFile);

                    // Check file size (limit to 10MB)
                    if (fileInfo.Length > 10 * 1024 * 1024)
                    {
                        MessageBox.Show("File size exceeds 10MB limit. Please select a smaller file.",
                            "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Create Resumes directory if it doesn't exist
                    string resumesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resumes");
                    if (!Directory.Exists(resumesFolder))
                    {
                        Directory.CreateDirectory(resumesFolder);
                    }

                    // Generate unique filename
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string safeFileName = Path.GetFileNameWithoutExtension(selectedFile)
                        .Replace(" ", "_")
                        .Replace(".", "_");
                    string extension = Path.GetExtension(selectedFile);
                    string newFileName = $"{safeFileName}_{timestamp}{extension}";
                    string destinationPath = Path.Combine(resumesFolder, newFileName);

                    // Copy file to Resumes folder
                    File.Copy(selectedFile, destinationPath, true);
                    _resumeFilePath = destinationPath;

                    // Update UI
                    ResumeFileNameText.Text = Path.GetFileName(destinationPath);
                    ResumeUploadStatus.Text = "✓ Resume uploaded successfully";
                    ResumeUploadStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
                    RemoveResumeBtn.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading resume: {ex.Message}",
                    "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveResumeBtn_Click(object sender, RoutedEventArgs e)
        {
            _resumeFilePath = null;
            ResumeFileNameText.Text = "No file selected";
            ResumeUploadStatus.Text = "";
            RemoveResumeBtn.Visibility = Visibility.Collapsed;
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_step == 1)
            {
                Step1Panel.Visibility = Visibility.Collapsed;
                Step2Panel.Visibility = Visibility.Visible;
                StepTitle.Text = "Step 2 of 3 - Technical Information";
                BackBtn.Visibility = Visibility.Visible;
            }
            else if (_step == 2)
            {
                Step2Panel.Visibility = Visibility.Collapsed;
                Step3Panel.Visibility = Visibility.Visible;
                StepTitle.Text = "Step 3 of 3 - Work Experience";
                NextBtn.Visibility = Visibility.Collapsed;
                SubmitBtn.Visibility = Visibility.Visible;
            }
            _step++;
            MainScrollViewer.ScrollToTop();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_step == 2)
            {
                Step2Panel.Visibility = Visibility.Collapsed;
                Step1Panel.Visibility = Visibility.Visible;
                StepTitle.Text = "Step 1 of 3 - Basic Information";
                BackBtn.Visibility = Visibility.Collapsed;
            }
            else if (_step == 3)
            {
                Step3Panel.Visibility = Visibility.Collapsed;
                Step2Panel.Visibility = Visibility.Visible;
                StepTitle.Text = "Step 2 of 3 - Technical Information";
                NextBtn.Visibility = Visibility.Visible;
                SubmitBtn.Visibility = Visibility.Collapsed;
            }
            _step--;
            MainScrollViewer.ScrollToTop();
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Applicant applicant = new Applicant
                {
                    ApplicantName = FullNameBox.Text,
                    FatherName = FatherNameBox.Text,
                    CNIC = CnicBox.Text,
                    FatherProfession = FatherProfessionBox.Text,
                    ContactNo = PhoneBox.Text,
                    Date = DateBox.SelectedDate,
                    MaritalStatus = MaritalStatusBox.SelectedItem?.ToString(),
                    Age = int.TryParse(AgeBox.Text, out int ageVal) ? ageVal : 0,
                    AppliedFor = DesignationBox.Text,
                    Address = AddressBox.Text,
                    Email = EmailBox.Text,
                    ResumePath = _resumeFilePath, // Add resume path

                    CurrentPositionReason = LeavingReasonBox.Text,
                    OOPRating = OopRatingBox.SelectedItem != null ? (int)OopRatingBox.SelectedItem : 0,
                    SQLRating = SqlRatingBox.SelectedItem != null ? (int)SqlRatingBox.SelectedItem : 0,
                    UIRating = UiRatingBox.SelectedItem != null ? (int)UiRatingBox.SelectedItem : 0,
                    IsCurrentlyEmployed = EmployedBox.SelectedItem?.ToString(),
                    CurrentCompany = CompanyBox.Text,
                    WhyThisJob = JobReasonBox.Text,
                    WatchCheck = WatchCountBox.Text,
                    Challenges = ChallengesBox.Text,
                    Strengths = StrengthsBox.Text,
                    Weaknesses = WeaknessesBox.Text,

                    Frameworks = FrameworkBox.Text,
                    ExperienceDuration = ExperienceBox.Text,
                    Goals = GoalsBox.Text,
                    CurrentSalary = SalaryBox.Text,
                    EducationInstitute = EducationBox.Text,
                    CGPA = CgpaBox.Text,
                    Willingness = WillingnessBox.SelectedItem?.ToString()
                };

                // Save to DB
                using (SqlConnection conn = new SqlConnection(DbChecker.ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Applicants WHERE Email = @Email", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", applicant.Email ?? (object)DBNull.Value);
                        int exists = (int)checkCmd.ExecuteScalar();
                    }

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                            INSERT INTO Applicants
                            (
                                ApplicantName, FatherName, CNIC, FatherProfession, ContactNo, Date, MaritalStatus,
                                Age, AppliedFor, Address, Email, ResumePath,
                                CurrentPositionReason, OOPRating, SQLRating, UIRating, IsCurrentlyEmployed, CurrentCompany,
                                WhyThisJob, WatchCheck, Challenges, Strengths, Weaknesses,
                                Frameworks, ExperienceDuration, Goals, CurrentSalary, EducationInstitute, CGPA, Willingness
                            )
                            OUTPUT INSERTED.Id
                            VALUES
                            (
                                @ApplicantName, @FatherName, @CNIC, @FatherProfession, @ContactNo, @Date, @MaritalStatus,
                                @Age, @AppliedFor, @Address, @Email, @ResumePath,
                                @CurrentPositionReason, @OOPRating, @SQLRating, @UIRating, @IsCurrentlyEmployed, @CurrentCompany,
                                @WhyThisJob, @WatchCheck, @Challenges, @Strengths, @Weaknesses,
                                @Frameworks, @ExperienceDuration, @Goals, @CurrentSalary, @EducationInstitute, @CGPA, @Willingness
                            )";

                        // Add parameters
                        cmd.Parameters.AddWithValue("@ApplicantName", applicant.ApplicantName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FatherName", applicant.FatherName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CNIC", applicant.CNIC ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FatherProfession", applicant.FatherProfession ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNo", applicant.ContactNo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Date", applicant.Date.HasValue ? (object)applicant.Date.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaritalStatus", applicant.MaritalStatus ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Age", applicant.Age);
                        cmd.Parameters.AddWithValue("@AppliedFor", applicant.AppliedFor ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", applicant.Address ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", applicant.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ResumePath", applicant.ResumePath ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@CurrentPositionReason", applicant.CurrentPositionReason ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@OOPRating", applicant.OOPRating);
                        cmd.Parameters.AddWithValue("@SQLRating", applicant.SQLRating);
                        cmd.Parameters.AddWithValue("@UIRating", applicant.UIRating);
                        cmd.Parameters.AddWithValue("@IsCurrentlyEmployed", applicant.IsCurrentlyEmployed ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CurrentCompany", applicant.CurrentCompany ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@WhyThisJob", applicant.WhyThisJob ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@WatchCheck", applicant.WatchCheck ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Challenges", applicant.Challenges ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Strengths", applicant.Strengths ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Weaknesses", applicant.Weaknesses ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@Frameworks", applicant.Frameworks ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExperienceDuration", applicant.ExperienceDuration ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Goals", applicant.Goals ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CurrentSalary", applicant.CurrentSalary ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EducationInstitute", applicant.EducationInstitute ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CGPA", applicant.CGPA ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Willingness", applicant.Willingness ?? (object)DBNull.Value);

                        object result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int newId))
                        {
                            applicant.Id = newId;
                        }
                    }
                }

                MessageBox.Show("Form Submitted and Data Saved Successfully!",
                       "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // After successful insert and applicant.Id is set:
                int applicantId = applicant.Id;
                string experience = applicant.ExperienceDuration ?? string.Empty;
                string designation = DesignationBox.SelectedItem?.ToString() ?? string.Empty;

                // Create the quiz control
                IMS.Views.QuizControl quiz = new IMS.Views.QuizControl(applicantId, experience, designation);
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    object named = parentWindow.FindName("MainContentArea");
                    if (named != null && named is ContentControl)
                    {
                        ((ContentControl)named).Content = quiz;
                        return;
                    }

                    // Fallback: set window content (replaces whole window content)
                    parentWindow.Content = quiz;
                    return;
                }

                // Final fallback: hide this control and add quiz to same parent panel
                this.Visibility = Visibility.Collapsed;
                Panel parentPanel = this.Parent as Panel;
                if (parentPanel != null)
                {
                    parentPanel.Children.Add(quiz);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}