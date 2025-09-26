using IMS.Helpers;
using System.Windows.Input;

namespace IMS.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ICommand NavigateCommand { get; }
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainWindowViewModel()
        {
            // Default dashboard view
            CurrentView = new DashboardViewModel();

            NavigateCommand = new RelayCommand(Navigate);
        }

        private void Navigate(object parameter)
        {
            switch (parameter?.ToString())
            {
                case "Applicants":
                    CurrentView = new ApplicantsViewModel();
                    break;
                default:
                    CurrentView = new DashboardViewModel();
                    break;
            }
        }
    }
}