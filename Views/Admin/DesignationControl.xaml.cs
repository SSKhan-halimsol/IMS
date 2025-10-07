using IMS.Data;
using IMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IMS.Views.Admin
{
    public partial class DesignationControl : UserControl
    {
        public DesignationControl()
        {
            InitializeComponent();
            LoadDesignations();
        }

        private async void LoadDesignations()
        {
            var list = await DesignationRepository.GetAllAsync();
            DesignationGrid.ItemsSource = list;
        }

        private async void AddDesignation_Click(object sender, RoutedEventArgs e)
        {
            string title = DesignationInput.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a designation name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool added = await DesignationRepository.AddAsync(title);
            if (added)
            {
                MessageBox.Show("Designation added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DesignationInput.Clear();
                LoadDesignations();
            }
            else
            {
                MessageBox.Show("Failed to add designation.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteDesignation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int id = (int)button.Tag;
            var result = MessageBox.Show("Are you sure you want to delete this designation?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                bool deleted = await DesignationRepository.DeleteAsync(id);
                if (deleted)
                {
                    MessageBox.Show("Designation deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDesignations();
                }
                else
                {
                    MessageBox.Show("Failed to delete designation.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}