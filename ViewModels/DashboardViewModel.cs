using IMS.Data;
using IMS.Models;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.Linq;

namespace IMS.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private ObservableCollection<Applicant> _recentApplicants;
        public ObservableCollection<Applicant> RecentApplicants
        {
            get => _recentApplicants;
            set { _recentApplicants = value; OnPropertyChanged(); }
        }

        public PlotModel ApplicantsChart { get; private set; }

        public DashboardViewModel()
        {
            
        }
    }
}