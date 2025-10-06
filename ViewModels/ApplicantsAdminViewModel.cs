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
    public class ApplicantsAdminViewModel : BaseViewModel
    {
        private readonly ObservableCollection<Applicant> _allApplicants = new ObservableCollection<Applicant>();
        private readonly ObservableCollection<Applicant> _pagedApplicants = new ObservableCollection<Applicant>();

        // FILTER PROPERTIES
        private string _searchName;
        public string SearchName
        {
            get => _searchName;
            set
            {
                if (_searchName != value)
                {
                    _searchName = value;
                    OnPropertyChanged();
                    UpdatePagedApplicants();
                }
            }
        }

        private string _searchCNIC;
        public string SearchCNIC
        {
            get => _searchCNIC;
            set
            {
                if (_searchCNIC != value)
                {
                    _searchCNIC = value;
                    OnPropertyChanged();
                    UpdatePagedApplicants();
                }
            }
        }

        private string _searchEmail;
        public string SearchEmail
        {
            get => _searchEmail;
            set
            {
                if (_searchEmail != value)
                {
                    _searchEmail = value;
                    OnPropertyChanged();
                    UpdatePagedApplicants();
                }
            }
        }

        private string _searchContact;
        public string SearchContact
        {
            get => _searchContact;
            set
            {
                if (_searchContact != value)
                {
                    _searchContact = value;
                    OnPropertyChanged();
                    UpdatePagedApplicants();
                }
            }
        }

        private DateTime? _searchDate;
        public DateTime? SearchDate
        {
            get => _searchDate;
            set
            {
                if (_searchDate != value)
                {
                    _searchDate = value;
                    OnPropertyChanged();
                    UpdatePagedApplicants();
                }
            }
        }

        public ICommand ClearFiltersCommand { get; }

        public ICollectionView ApplicantsView { get; }

        private int _currentPage = 1;
        private readonly int _itemsPerPage = 10;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    UpdatePagedApplicants();
                }
            }
        }

        public int TotalPages => (int)Math.Ceiling((double)FilteredApplicants.Count() / _itemsPerPage);
        public bool CanGoNext => CurrentPage < TotalPages;
        public bool CanGoPrev => CurrentPage > 1;

        public ICommand RefreshCommand { get; }
        public ICommand DeleteApplicantCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public ApplicantsAdminViewModel()
        {
            ApplicantsView = CollectionViewSource.GetDefaultView(_pagedApplicants);

            RefreshCommand = new RelayCommand(async _ => await SafeLoadApplicantsAsync());
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CanGoNext);
            PrevPageCommand = new RelayCommand(_ => PrevPage(), _ => CanGoPrev);
            DeleteApplicantCommand = new RelayCommand(async param =>
            {
                if (param is Applicant applicant)
                    await SafeDeleteApplicantAsync(applicant);
            });

            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

            _ = SafeLoadApplicantsAsync();
        }

        public IEnumerable<Applicant> FilteredApplicants =>
            _allApplicants.Where(a =>
                (string.IsNullOrWhiteSpace(SearchName) || a.ApplicantName?.IndexOf(SearchName, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(SearchCNIC) || a.CNIC?.IndexOf(SearchCNIC, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(SearchEmail) || a.Email?.IndexOf(SearchEmail, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(SearchContact) || a.ContactNo?.IndexOf(SearchContact, StringComparison.OrdinalIgnoreCase) >= 0)
            );

        private void UpdatePagedApplicants()
        {
            _pagedApplicants.Clear();

            var items = FilteredApplicants
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage);

            foreach (var applicant in items)
                _pagedApplicants.Add(applicant);

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrev));
        }

        private void ClearFilters()
        {
            SearchName = string.Empty;
            SearchCNIC = string.Empty;
            SearchEmail = string.Empty;
            SearchContact = string.Empty;
            SearchDate = null;

            CurrentPage = 1;
            UpdatePagedApplicants();
        }

        // ✅ LOAD FROM DATABASE
        private async Task SafeLoadApplicantsAsync()
        {
            try
            {
                var list = await ApplicantRepository.GetAllAsync();

                _allApplicants.Clear();
                foreach (var a in list)
                    _allApplicants.Add(a);

                CurrentPage = 1;
                UpdatePagedApplicants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading applicants: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ✅ DELETE APPLICANT
        private async Task SafeDeleteApplicantAsync(Applicant applicant)
        {
            if (applicant == null) return;

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete '{applicant.ApplicantName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                bool deleted = await ApplicantRepository.DeleteAsync(applicant.Id);
                if (deleted)
                {
                    _allApplicants.Remove(applicant);
                    UpdatePagedApplicants();
                    MessageBox.Show("Applicant deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Delete failed. Applicant may have dependent records (QuizResult).",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting applicant: {ex.Message}", "SQL Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ PAGINATION CONTROLS
        private void NextPage()
        {
            if (CanGoNext)
                CurrentPage++;
        }

        private void PrevPage()
        {
            if (CanGoPrev)
                CurrentPage--;
        }
    }
}
