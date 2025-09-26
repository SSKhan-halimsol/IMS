using System.Windows.Controls;

namespace IMS.Views.Admin
{
    public partial class SuccessfulCandidatesControl : UserControl
    {
        public SuccessfulCandidatesControl()
        {
            InitializeComponent();
            DataContext = new ViewModels.SuccessfulCandidatesViewModel();
        }
    }
}
