using IMS.Data;
using IMS.Helpers;
using IMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace IMS.ViewModels
{
    public class SuccessfulCandidatesViewModel : BaseViewModel
    {
        public ObservableCollection<Applicant> AcceptedApplicants { get; } = new ObservableCollection<Applicant>();

        public SuccessfulCandidatesViewModel()
        {
            _ = LoadAcceptedApplicantsAsync();
        }

        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        
        private async Task LoadAcceptedApplicantsAsync()
        {
            try
            {
                var list = await ApplicantRepository.GetAllAsync();

                var accepted = list
                    .Where(a => !string.IsNullOrWhiteSpace(a.Status) &&
                                a.Status.Trim().Equals("Accepted", StringComparison.OrdinalIgnoreCase));

                AcceptedApplicants.Clear();
                foreach (var a in accepted)
                    AcceptedApplicants.Add(a);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading accepted applicants: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

    }
}