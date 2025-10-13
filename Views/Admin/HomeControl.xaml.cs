using IMS.Data;
using IMS.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace IMS.Views.Admin
{
    public partial class HomeControl : UserControl
    {
            private readonly DispatcherTimer _refreshTimer;

            public HomeControl()
            {
                InitializeComponent();
            DataContext = new HomeControlViewModel();
            LoadApplicantStats();

                // Optional: Refresh every 30 seconds automatically
                _refreshTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(30)
                };
                _refreshTimer.Tick += (s, e) => LoadApplicantStats();
                _refreshTimer.Start();
            }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logged out successfully!", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
            LoginView login = new LoginView();
            login.Show();
            Window.GetWindow(this)?.Close();
        }

        private void LoadApplicantStats()
            {
                try
                {
                    using (var context = new IMSDbContext())
                    {
                        var applicants = context.Applicants.ToList();
                        var today = DateTime.Today;

                        // 1️⃣ Total applicants
                        int total = applicants.Count;

                        // 2️⃣ Today’s applicants
                        int todayCount = applicants.Count(a => a.Date.HasValue && a.Date.Value.Date == today);

                        // 3️⃣ This month’s applicants
                        int monthCount = applicants.Count(a =>
                            a.Date.HasValue &&
                            a.Date.Value.Month == today.Month &&
                            a.Date.Value.Year == today.Year);

                        // 4️⃣ Pending applicants (status not Accepted or Rejected)
                        int pending = applicants.Count(a =>
                            !string.Equals(a.Status, "Accepted", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(a.Status, "Rejected", StringComparison.OrdinalIgnoreCase));

                        // Update UI
                        TotalApplicantsText.Text = total.ToString();
                        TodayApplicantsText.Text = todayCount.ToString();
                        MonthApplicantsText.Text = monthCount.ToString();
                        PendingReviewsText.Text = pending.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading applicant stats: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
 }
