using IMS.Data;
using IMS.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IMS.ViewModels
{
    public class SuccessfulCandidatesViewModel : BaseViewModel
    {
        public ObservableCollection<Applicant> AcceptedApplicants { get; } = new ObservableCollection<Applicant>();

        public SuccessfulCandidatesViewModel()
        {
            _ = LoadAcceptedApplicantsAsync();
        }

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