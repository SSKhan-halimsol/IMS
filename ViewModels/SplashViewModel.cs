using System.ComponentModel;
using IMS.Helpers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IMS.ViewModels
{
    public class SplashViewModel : INotifyPropertyChanged
    {
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public async Task<bool> InitializeDatabaseAsync()
        {
            StatusMessage = "Checking database connection...";
            bool result = IMS.Helpers.DbChecker.EnsureDatabase();
            StatusMessage = result ? "Database is ready." : "Database connection failed.";
            await Task.Delay(1000); // pause for UX
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}