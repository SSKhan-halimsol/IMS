using IMS.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace IMS.Views.Admin
{
    public partial class ApplicantsAdminControl : UserControl
    {
        private readonly AdminDashboard _dashboard;

        public ApplicantsAdminControl(AdminDashboard dashboard)
        {
            InitializeComponent();
            _dashboard = dashboard;
        }

        private void ApplicantsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is Applicant selectedApplicant)
            {
                using (var db = new IMS.Data.IMSDbContext())
                {
                    // Fetch the complete applicant record from database using its Id
                    var fullApplicant = db.Applicants.Find(selectedApplicant.Id);

                    if (fullApplicant != null)
                    {
                        _dashboard.ShowApplicantDetails(fullApplicant);
                    }
                }
            }
        }
    }
}