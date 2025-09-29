using IMS.Data;
using IMS.Helpers;        // make sure this exists and contains RelayCommand
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

        public ICollectionView ApplicantsView { get; }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                UpdatePagedApplicants();   // re-apply search when text changes
            }
        }

        // Pagination fields
        private int _currentPage = 1;
        private int _itemsPerPage = 10;

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

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand DeleteApplicantCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public ApplicantsAdminViewModel()
        {
            ApplicantsView = CollectionViewSource.GetDefaultView(_pagedApplicants);

            RefreshCommand = new RelayCommand(async _ => await SafeLoadApplicantsAsync());

            DeleteApplicantCommand = new RelayCommand(async param =>
            {
                var applicant = param as Applicant;
                if (applicant == null) return;
                await SafeDeleteApplicantAsync(applicant);
            });

            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CanGoNext);
            PrevPageCommand = new RelayCommand(_ => PrevPage(), _ => CanGoPrev);

            _ = SafeLoadApplicantsAsync();
        }

        public IEnumerable<Applicant> FilteredApplicants =>
            _allApplicants.Where(a =>
                string.IsNullOrWhiteSpace(SearchText) ||
                (a.ApplicantName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.FatherName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.CNIC?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.Email?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.ContactNo?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.ExperienceDuration?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.AppliedFor?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0));

        private void UpdatePagedApplicants()
        {
            _pagedApplicants.Clear();

            var items = FilteredApplicants
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage);

            foreach (var a in items)
                _pagedApplicants.Add(a);

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrev));
        }

        private async Task SafeLoadApplicantsAsync()
        {
            try
            {
                var list = await ApplicantRepository.GetAllAsync();
                _allApplicants.Clear();
                foreach (var a in list) _allApplicants.Add(a);

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

        private async Task SafeDeleteApplicantAsync(Applicant applicant)
        {
            if (applicant == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete {applicant.ApplicantName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool deleted = await ApplicantRepository.DeleteAsync(applicant.Id);

                if (deleted)
                {
                    _allApplicants.Remove(applicant);
                    UpdatePagedApplicants();

                    MessageBox.Show("Applicant deleted successfully.",
                                    "Deleted",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
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
                MessageBox.Show($"Error deleting applicant: {ex.Message}",
                                "SQL Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // Pagination helpers
        public bool CanGoNext => CurrentPage < TotalPages;
        public bool CanGoPrev => CurrentPage > 1;

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