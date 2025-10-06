using IMS.Helpers;
using IMS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace IMS.Views
{
    public partial class QuizControl : UserControl
    {
        private readonly List<Question> _questions;
        private readonly Dictionary<int, int> _answers;
        private int _currentIndex;
        private readonly int _applicantId;
        private readonly DispatcherTimer _questionTimer;
        private readonly DispatcherTimer _totalTimer;
        private int _questionTimeRemaining = 10;
        private int _totalTimeRemaining;

        public QuizControl()
        {
            InitializeComponent();
        }

        public QuizControl(int applicantId, string experienceLevel, string designation)
        {
            InitializeComponent();
            _applicantId = applicantId;
            var allQuestions = QuizLoader.GetQuestions(experienceLevel ?? string.Empty, designation ?? string.Empty);

            if (allQuestions != null && allQuestions.Count > 0)
            {
                var rnd = new Random();
                _questions = allQuestions.OrderBy(q => rnd.Next()).Take(10).ToList();
            }
            else
            {
                _questions = new List<Question>();
            }

            _answers = new Dictionary<int, int>();
            _currentIndex = 0;
            WelcomeText.Text = $"Welcome!! Ready to take your {designation} quiz?";

            if (_questions.Count == 0)
            {
                MessageBox.Show("No quiz questions available for this designation.", "Quiz", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Visibility = Visibility.Collapsed;
                return;
            }

            // Initialize timers
            _totalTimeRemaining = _questions.Count * 15;

            _questionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _questionTimer.Tick += QuestionTimer_Tick;

            _totalTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _totalTimer.Tick += TotalTimer_Tick;

            DisplayQuestion(_currentIndex);
            UpdateNavigation();
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            QuizScreen.Visibility = Visibility.Visible;

            _questionTimer.Start();
            _totalTimer.Start();
        }

        private void QuestionTimer_Tick(object sender, EventArgs e)
        {
            _questionTimeRemaining--;
            ProgressText.Text = $"Question {_currentIndex + 1} of {_questions.Count}  |  Time Left: {_questionTimeRemaining}s";

            if (_questionTimeRemaining <= 0)
            {
                MoveToNextQuestionOrSubmit();
            }
        }

        private void TotalTimer_Tick(object sender, EventArgs e)
        {
            _totalTimeRemaining--;

            if (_totalTimeRemaining <= 0)
            {
                _questionTimer.Stop();
                _totalTimer.Stop();
                SubmitQuiz(autoSubmit: true);
            }
        }

        private void MoveToNextQuestionOrSubmit()
        {
            SaveCurrentSelection();
            if (_currentIndex < _questions.Count - 1)
            {
                _currentIndex++;
                DisplayQuestion(_currentIndex);
                UpdateNavigation();
            }
            else
            {
                _questionTimer.Stop();
                _totalTimer.Stop();
                SubmitQuiz(autoSubmit: true);
            }
        }

        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= _questions.Count) return;

            _questionTimeRemaining = 15; // reset per-question timer

            Question q = _questions[index];
            ProgressText.Text = $"Question {index + 1} of {_questions.Count}  |  Time Left: {_questionTimeRemaining}s";
            QuestionText.Text = q.Text ?? string.Empty;

            OptionsPanel.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                if (q.Options != null && q.Options.Length > i && !string.IsNullOrEmpty(q.Options[i]))
                {
                    RadioButton rb = new RadioButton
                    {
                        Content = q.Options[i],
                        Tag = i,
                        GroupName = "QuizOptions",
                        Margin = new Thickness(0, 6, 0, 6),
                        Foreground = System.Windows.Media.Brushes.White
                    };
                    OptionsPanel.Children.Add(rb);
                }
            }

            if (_answers.TryGetValue(index, out int saved))
            {
                foreach (RadioButton rb in OptionsPanel.Children.OfType<RadioButton>())
                {
                    if ((int)rb.Tag == saved)
                    {
                        rb.IsChecked = true;
                        break;
                    }
                }
            }
        }

        private void SaveCurrentSelection()
        {
            int foundIndex = -1;
            foreach (RadioButton rb in OptionsPanel.Children.OfType<RadioButton>())
            {
                if (rb.IsChecked == true)
                {
                    foundIndex = (int)rb.Tag;
                    break;
                }
            }

            if (foundIndex >= 0)
                _answers[_currentIndex] = foundIndex;
            else
                _answers.Remove(_currentIndex);
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentSelection();
            MoveToNextQuestionOrSubmit();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentSelection();
            if (_currentIndex > 0)
            {
                _currentIndex--;
                DisplayQuestion(_currentIndex);
                UpdateNavigation();
            }
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            _questionTimer.Stop();
            _totalTimer.Stop();
            SubmitQuiz(autoSubmit: false);
        }

        private void SubmitQuiz(bool autoSubmit)
        {
            SaveCurrentSelection();

            int score = 0;
            for (int i = 0; i < _questions.Count; i++)
            {
                if (_answers.TryGetValue(i, out int selected))
                {
                    if (selected == _questions[i].CorrectOptionIndex)
                        score++;
                }
            }

            bool passed = score >= (_questions.Count / 2);
            DateTime submittedOn = DateTime.Now;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbChecker.ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO QuizResult (ApplicantId, Score, TotalQuestions, Passed, SubmittedOn)
                        VALUES (@ApplicantId, @Score, @TotalQuestions, @Passed, @SubmittedOn)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ApplicantId", _applicantId);
                        cmd.Parameters.AddWithValue("@Score", score);
                        cmd.Parameters.AddWithValue("@TotalQuestions", _questions.Count);
                        cmd.Parameters.AddWithValue("@Passed", passed);
                        cmd.Parameters.AddWithValue("@SubmittedOn", submittedOn);
                        cmd.ExecuteNonQuery();
                    }
                }

                MarksBox.Text = $"Your Score: {score}/{_questions.Count}";
                MessageBox.Show(
                    autoSubmit
                        ? $"Time’s up! Quiz auto-submitted.\nScore: {score}/{_questions.Count}"
                        : $"Quiz completed!\nScore: {score}/{_questions.Count}",
                    "Quiz Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving quiz result: " + ex.Message,
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateNavigation()
        {
            BackBtn.IsEnabled = _currentIndex > 0;
            bool isLast = (_currentIndex == _questions.Count - 1);
            NextBtn.Visibility = isLast ? Visibility.Collapsed : Visibility.Visible;
            SubmitBtn.Visibility = isLast ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}