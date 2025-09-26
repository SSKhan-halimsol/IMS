using IMS.Helpers;
using IMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IMS.Views
{
    public partial class QuizControl : UserControl
    {
        private readonly List<Question> _questions;
        private readonly Dictionary<int, int> _answers;
        private int _currentIndex;
        private readonly int _applicantId;
        public QuizControl()
        {
            InitializeComponent();

        }
        public QuizControl(int applicantId, string experienceLevel, string designation)
        {
            InitializeComponent();
            _applicantId = applicantId;
            var allQuestions = QuizLoader.GetQuestions(experienceLevel ?? string.Empty, designation ?? string.Empty);

            // Always pick up to 10 questions (randomized if more available)
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

            if (_questions == null || _questions.Count == 0)
            {
                MessageBox.Show("No quiz questions available for this designation.", "Quiz", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Visibility = Visibility.Collapsed;
                return;
            }

            DisplayQuestion(_currentIndex);
            UpdateNavigation();
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            QuizScreen.Visibility = Visibility.Visible;
        }
        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= _questions.Count) return;

            Question q = _questions[index];

            ProgressText.Text = string.Format("Question {0} of {1}", index + 1, _questions.Count);
            QuestionText.Text = q.Text ?? string.Empty;

            OptionsPanel.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                if (q.Options != null && q.Options.Length > i && !string.IsNullOrEmpty(q.Options[i]))
                {
                    RadioButton rb = new RadioButton();
                    rb.Content = q.Options[i];
                    rb.Tag = i; // store the option index
                    rb.GroupName = "QuizOptions";
                    rb.Margin = new Thickness(0, 6, 0, 6);
                    rb.Foreground = System.Windows.Media.Brushes.White;
                    OptionsPanel.Children.Add(rb);
                }
            }

            int saved;
            if (_answers.TryGetValue(index, out saved))
            {
                foreach (object child in OptionsPanel.Children)
                {
                    RadioButton rb = child as RadioButton;
                    if (rb != null && rb.Tag is int && (int)rb.Tag == saved)
                    {
                        rb.IsChecked = true;
                        break;
                    }
                }
            }

            MainScrollViewer.ScrollToTop();
        }

        private void SaveCurrentSelection()
        {
            int questionIndex = _currentIndex;
            int foundIndex = -1;
            foreach (object child in OptionsPanel.Children)
            {
                RadioButton rb = child as RadioButton;
                if (rb != null && rb.IsChecked == true)
                {
                    if (rb.Tag is int)
                        foundIndex = (int)rb.Tag;
                    break;
                }
            }

            if (foundIndex >= 0)
            {
                _answers[questionIndex] = foundIndex;
            }
            else
            {
                if (_answers.ContainsKey(questionIndex))
                    _answers.Remove(questionIndex);
            }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentSelection();

            if (_currentIndex < _questions.Count - 1)
            {
                _currentIndex++;
                DisplayQuestion(_currentIndex);
                UpdateNavigation();
            }
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

            bool passed = score >= (_questions.Count / 2); // pass if >=50%
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

                // ✅ Update TextBlock with marks
                MarksBox.Text = $"Your Score: {score}/{_questions.Count}";

                MessageBox.Show(
                    $"Quiz completed! Score: {score}/{_questions.Count}",
                    "Quiz Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // ✅ Use the existing MainWindow, do NOT create a new one
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    var contentControl = parentWindow.FindName("MainContent") as ContentControl;
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

        private void SaveResultToDatabase(object sender, RoutedEventArgs e)
        {
            SaveCurrentSelection();

            int score = 0;
            for (int i = 0; i < _questions.Count; i++)
            {
                int selected;
                if (_answers.TryGetValue(i, out selected))
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

                MessageBox.Show(
                    string.Format("Quiz completed! Score: {0}/{1}", score, _questions.Count),
                    "Quiz Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving quiz result: " + ex.Message,
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                object ctrl = parentWindow.FindName("MainContentArea");
                if (ctrl != null && ctrl is ContentControl)
                {
                    parentWindow.Close();
                }
                else
                {
                    this.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

    }
}