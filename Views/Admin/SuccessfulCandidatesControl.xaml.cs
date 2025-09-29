using IMS.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace IMS.Views.Admin
{
    public partial class SuccessfulCandidatesControl : UserControl
    {
        private readonly AdminDashboard _dashboard;
        public SuccessfulCandidatesControl(AdminDashboard dashboard)
        {

            InitializeComponent();
            _dashboard = dashboard;
            DataContext = new ViewModels.SuccessfulCandidatesViewModel();
        }
        private void ApplicantsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is Applicant selectedApplicant)
            {
                _dashboard.ShowApplicantDetails(selectedApplicant);
            }
        }
    }
}
