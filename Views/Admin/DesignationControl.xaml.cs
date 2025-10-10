using IMS.Data;
using IMS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace IMS.Views.Admin
{
    public partial class DesignationControl : UserControl
    {
        public DesignationControl()
        {
            InitializeComponent();
            LoadDesignationsAsync();

            // Add a subtle fade-in animation when control loads
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            this.BeginAnimation(OpacityProperty, fadeIn);
        }

        private async void LoadDesignationsAsync()
        {
            try
            {
                // Show loading state (optional - you can add a loading indicator)
                DesignationGrid.IsEnabled = false;
                DesignationGrid.Opacity = 0.5;

                var list = await DesignationRepository.GetAllAsync();
                DesignationGrid.ItemsSource = list;

                // Restore grid state
                DesignationGrid.IsEnabled = true;

                // Animate grid back to full opacity
                var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
                DesignationGrid.BeginAnimation(OpacityProperty, fadeIn);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading designations: {ex.Message}");
                DesignationGrid.IsEnabled = true;
                DesignationGrid.Opacity = 1;
            }
        }

        private async void AddDesignation_Click(object sender, RoutedEventArgs e)
        {
            string title = DesignationInput.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(title))
            {
                ShowWarningMessage("Please enter a designation name.", "Empty Field");
                DesignationInput.Focus();
                return;
            }

            if (title.Length < 3)
            {
                ShowWarningMessage("Designation name must be at least 3 characters long.", "Invalid Input");
                DesignationInput.Focus();
                return;
            }

            if (title.Length > 100)
            {
                ShowWarningMessage("Designation name must not exceed 100 characters.", "Invalid Input");
                DesignationInput.Focus();
                return;
            }

            try
            {
                // Disable button during operation
                var button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                    button.Content = "Adding...";
                }

                bool added = await DesignationRepository.AddAsync(title);

                if (added)
                {
                    ShowSuccessMessage("Designation added successfully!", "Success");
                    DesignationInput.Clear();
                    await Task.Delay(100); // Small delay for better UX
                    LoadDesignationsAsync();
                }
                else
                {
                    ShowErrorMessage("Failed to add designation. It may already exist.");
                }

                // Re-enable button
                if (button != null)
                {
                    button.IsEnabled = true;
                    button.Content = "➕  Add Designation";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding designation: {ex.Message}");

                // Re-enable button
                var button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                    button.Content = "➕  Add Designation";
                }
            }
        }

        private async void DeleteDesignation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int id = (int)button.Tag;

            // Custom styled confirmation dialog
            var result = MessageBox.Show(
                "Are you sure you want to delete this designation?\n\n" +
                "⚠️ Warning: This action cannot be undone.\n" +
                "All associated data may be affected.",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Disable button during operation
                    button.IsEnabled = false;

                    bool deleted = await DesignationRepository.DeleteAsync(id);

                    if (deleted)
                    {
                        ShowSuccessMessage("Designation deleted successfully!", "Deleted");
                        await Task.Delay(100); // Small delay for better UX
                        LoadDesignationsAsync();
                    }
                    else
                    {
                        ShowErrorMessage(
                            "Failed to delete designation.\n\n" +
                            "This designation may be associated with existing applicants.");
                    }

                    button.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Error deleting designation: {ex.Message}");
                    button.IsEnabled = true;
                }
            }
        }

        #region Helper Methods for Custom Messages

        private void ShowSuccessMessage(string message, string title = "Success")
        {
            var messageWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.Transparent,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true
            };

            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(26, 31, 58)),
                CornerRadius = new CornerRadius(16),
                BorderThickness = new Thickness(1),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(16, 185, 129)),
                Padding = new Thickness(30)
            };

            var stackPanel = new StackPanel();

            var icon = new TextBlock
            {
                Text = "✓",
                FontSize = 48,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(16, 185, 129)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var messageText = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(messageText);
            border.Child = stackPanel;
            messageWindow.Content = border;

            // Auto-close after 2 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                messageWindow.Close();
            };
            timer.Start();

            messageWindow.ShowDialog();
        }

        private void ShowErrorMessage(string message, string title = "Error")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void ShowWarningMessage(string message, string title = "Warning")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        #endregion

        // Public method to refresh data (can be called from parent window)
        public void RefreshData()
        {
            LoadDesignationsAsync();
        }
    }
}