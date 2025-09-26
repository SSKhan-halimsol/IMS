using IMS.ViewModels;
using System.Windows.Controls;

namespace IMS.Views.Admin
{
    /// <summary>
    /// Interaction logic for HomeControl.xaml
    /// </summary>
    public partial class HomeControl : UserControl
    {
        public HomeControl()
        {
            InitializeComponent();
            DataContext = new HomeControlViewModel(); // Bind VM
        }
    }
}
