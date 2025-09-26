using IMS.Helpers;
using IMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace IMS.ViewModels
{
    public class QuizViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Question> Questions { get; set; }
        private int _currentQuestionIndex;
        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set { _currentQuestionIndex = value; OnPropertyChanged(); }
        }

        public Question CurrentQuestion =>
            Questions != null && Questions.Count > 0
            ? Questions[CurrentQuestionIndex]
            : null;

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand SubmitCommand { get; }

        public QuizViewModel(List<Question> quizQuestions)
        {
            Questions = new ObservableCollection<Question>(quizQuestions);
            CurrentQuestionIndex = 0;

            NextCommand = new RelayCommand(_ => CurrentQuestionIndex++, _ => CurrentQuestionIndex < Questions.Count - 1);
            PreviousCommand = new RelayCommand(_ => CurrentQuestionIndex--, _ => CurrentQuestionIndex > 0);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}