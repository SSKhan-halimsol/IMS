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
        private readonly ObservableCollection<Applicant> _allAcceptedApplicants = new ObservableCollection<Applicant>();
        private readonly ObservableCollection<Applicant> _pagedApplicants = new ObservableCollection<Applicant>();

        public ObservableCollection<Applicant> PagedApplicants => _pagedApplicants;

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
                    CurrentPage = 1;
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
                    CurrentPage = 1;
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
                    CurrentPage = 1;
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
                    CurrentPage = 1;
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
                    CurrentPage = 1;
                    UpdatePagedApplicants();
                }
            }
        }

        public ICommand ClearFiltersCommand { get; }
        public ICommand ViewApplicantDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        // PAGINATION PROPERTIES
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
                    OnPropertyChanged(nameof(PageInfo));
                    UpdatePagedApplicants();
                }
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (_totalPages != value)
                {
                    _totalPages = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PageInfo));
                }
            }
        }

        public string PageInfo => $"Page {CurrentPage} of {TotalPages}";

        public bool CanGoNext => CurrentPage < TotalPages;
        public bool CanGoPrev => CurrentPage > 1;

        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        // Statistics Properties
        private int _totalAccepted;
        public int TotalAccepted
        {
            get => _totalAccepted;
            set
            {
                if (_totalAccepted != value)
                {
                    _totalAccepted = value;
                    OnPropertyChanged();
                }
            }
        }

        public SuccessfulCandidatesViewModel()
        {
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CanGoNext);
            PrevPageCommand = new RelayCommand(_ => PrevPage(), _ => CanGoPrev);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            RefreshCommand = new RelayCommand(async _ => await LoadAcceptedApplicantsAsync());

            _ = LoadAcceptedApplicantsAsync();
        }

        private IEnumerable<Applicant> FilteredApplicants
        {
            get
            {
                return _allAcceptedApplicants.Where(a =>
                    (string.IsNullOrWhiteSpace(SearchName) ||
                     (a.ApplicantName != null && a.ApplicantName.IndexOf(SearchName, StringComparison.OrdinalIgnoreCase) >= 0)) &&

                    (string.IsNullOrWhiteSpace(SearchCNIC) ||
                     (a.CNIC != null && a.CNIC.IndexOf(SearchCNIC, StringComparison.OrdinalIgnoreCase) >= 0)) &&

                    (string.IsNullOrWhiteSpace(SearchEmail) ||
                     (a.Email != null && a.Email.IndexOf(SearchEmail, StringComparison.OrdinalIgnoreCase) >= 0)) &&

                    (string.IsNullOrWhiteSpace(SearchContact) ||
                     (a.ContactNo != null && a.ContactNo.IndexOf(SearchContact, StringComparison.OrdinalIgnoreCase) >= 0)) &&

                    (!SearchDate.HasValue ||
                     (a.Date.HasValue && a.Date.Value.Date == SearchDate.Value.Date))
                );
            }
        }

        private void UpdatePagedApplicants()
        {
            var filtered = FilteredApplicants.ToList();

            // Calculate total pages
            TotalPages = filtered.Count == 0 ? 1 : (int)Math.Ceiling((double)filtered.Count / _itemsPerPage);

            // Ensure current page is valid
            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            // Clear and populate paged items
            _pagedApplicants.Clear();

            var items = filtered
                .Skip((CurrentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage);

            foreach (var applicant in items)
                _pagedApplicants.Add(applicant);

            // Notify UI of changes
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

        private async Task LoadAcceptedApplicantsAsync()
        {
            try
            {
                var list = await ApplicantRepository.GetAllAsync();

                var accepted = list
                    .Where(a => !string.IsNullOrWhiteSpace(a.Status) &&
                                a.Status.Trim().Equals("Accepted", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(a => a.Date) // Most recent first
                    .ToList();

                _allAcceptedApplicants.Clear();
                foreach (var a in accepted)
                    _allAcceptedApplicants.Add(a);

                // Update statistics
                TotalAccepted = accepted.Count;

                UpdatePagedApplicants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading accepted applicants: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

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

        // Public method to refresh data from external sources
        public async Task RefreshDataAsync()
        {
            await LoadAcceptedApplicantsAsync();
        }
    }
}