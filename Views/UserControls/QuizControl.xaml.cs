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
        private List<QuizQuestion> _questions;
        private readonly Dictionary<int, int> _answers = new Dictionary<int, int>();
        private int _currentIndex = 0;
        private readonly int _applicantId;
        private int _quizId;
        private int _quizDurationInMinutes = 5; // default fallback

        // Timers
        private DispatcherTimer _questionTimer;
        private DispatcherTimer _totalTimer;

        // Time remaining
        private int _questionTimeRemaining = 20;
        private int _totalTimeRemaining; // in seconds

        public QuizControl(int applicantId, string experienceLevel, string designation)
        {
            InitializeComponent();
            _applicantId = applicantId;

            designation = (designation ?? "").Trim().ToLower();
            experienceLevel = (experienceLevel ?? "").Trim().ToLower();

            _questions = LoadQuizFromDatabase(experienceLevel, designation);

            if (_questions == null || _questions.Count == 0)
            {
                MessageBox.Show("No quiz available for your designation and experience level.",
                                "Quiz", MessageBoxButton.OK, MessageBoxImage.Warning);

                Loaded += (s, e) =>
                {
                    var parentWindow = Window.GetWindow(this);
                    if (parentWindow != null)
                        parentWindow.Close();
                };

                return;
            }

            // Shuffle question order
            Random rnd = new Random();
            _questions = _questions.OrderBy(q => rnd.Next()).ToList();

            _totalTimeRemaining = _quizDurationInMinutes * 60;
            WelcomeText.Text = "Welcome! Ready to take your quiz?";
        }

        private List<QuizQuestion> LoadQuizFromDatabase(string experienceLevel, string designation)
        {
            var questions = new List<QuizQuestion>();

            try
            {
                using (SqlConnection con = new SqlConnection(DbChecker.ConnectionString))
                {
                    con.Open();

                    // Get Quiz info (including duration)
                    string quizQuery = @"
                        SELECT TOP 1 QuizID, DurationInMinutes
                        FROM Quiz
                        WHERE LOWER(LTRIM(RTRIM(Designation))) = LOWER(LTRIM(RTRIM(@d)))
                          AND LOWER(LTRIM(RTRIM(ExperienceLevel))) = LOWER(LTRIM(RTRIM(@e)))
                          AND IsActive = 1
                        ORDER BY CreatedAt DESC";

                    using (SqlCommand getQuiz = new SqlCommand(quizQuery, con))
                    {
                        getQuiz.Parameters.AddWithValue("@d", designation);
                        getQuiz.Parameters.AddWithValue("@e", experienceLevel);

                        using (var reader = getQuiz.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                _quizId = Convert.ToInt32(reader["QuizID"]);
                                _quizDurationInMinutes = Convert.ToInt32(reader["DurationInMinutes"]);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }

                    // Load questions
                    using (SqlCommand qCmd = new SqlCommand(
                        "SELECT QuestionID, QuizID, QuestionText FROM QuizQuestions WHERE QuizID=@id ORDER BY QuestionID", con))
                    {
                        qCmd.Parameters.AddWithValue("@id", _quizId);

                        using (SqlDataReader qReader = qCmd.ExecuteReader())
                        {
                            while (qReader.Read())
                            {
                                var q = new QuizQuestion
                                {
                                    QuestionID = Convert.ToInt32(qReader["QuestionID"]),
                                    QuizID = Convert.ToInt32(qReader["QuizID"]),
                                    QuestionText = qReader["QuestionText"].ToString(),
                                    Answers = new List<QuizAnswer>()
                                };
                                questions.Add(q);
                            }
                        }
                    }

                    // Load answers
                    foreach (var q in questions)
                    {
                        using (SqlCommand aCmd = new SqlCommand(
                            "SELECT AnswerID, QuestionID, AnswerText, IsCorrect FROM QuizAnswers WHERE QuestionID=@qid ORDER BY AnswerID", con))
                        {
                            aCmd.Parameters.AddWithValue("@qid", q.QuestionID);
                            using (SqlDataReader aReader = aCmd.ExecuteReader())
                            {
                                while (aReader.Read())
                                {
                                    q.Answers.Add(new QuizAnswer
                                    {
                                        AnswerID = Convert.ToInt32(aReader["AnswerID"]),
                                        QuestionID = Convert.ToInt32(aReader["QuestionID"]),
                                        AnswerText = aReader["AnswerText"].ToString(),
                                        IsCorrect = Convert.ToBoolean(aReader["IsCorrect"])
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading quiz: " + ex.Message);
            }

            return questions;
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            QuizScreen.Visibility = Visibility.Visible;

            DisplayQuestion(_currentIndex);
            UpdateNavigation();

            // Start timers
            _questionTimeRemaining = 20;
            _totalTimeRemaining = _quizDurationInMinutes * 60;

            _questionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _totalTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

            _questionTimer.Tick += QuestionTimer_Tick;
            _totalTimer.Tick += TotalTimer_Tick;

            _questionTimer.Start();
            _totalTimer.Start();

            UpdateTimerUI();
        }

        private void QuestionTimer_Tick(object sender, EventArgs e)
        {
            if (_questionTimeRemaining > 0)
            {
                _questionTimeRemaining--;
                UpdateTimerUI();
            }
            else
            {
                MoveToNextQuestionOrSubmit();
                _questionTimeRemaining = 20; // reset for next question
            }
        }

        private void TotalTimer_Tick(object sender, EventArgs e)
        {
            if (_totalTimeRemaining > 0)
            {
                _totalTimeRemaining--;
                UpdateTimerUI();
            }
            else
            {
                _totalTimer.Stop();
                _questionTimer.Stop();
                MessageBox.Show("Quiz time is up!");
                SubmitQuiz(true);
            }
        }

        private void UpdateTimerUI()
        {
            TotalTimeText.Text = $"Total Time Left: {_totalTimeRemaining / 60:D2}:{_totalTimeRemaining % 60:D2}";
            TimerBar.Maximum = 20;
            TimerBar.Value = _questionTimeRemaining;
            ProgressText.Text = $"Question {_currentIndex + 1} of {_questions.Count}";
        }

        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= _questions.Count) return;

            var q = _questions[index];
            QuestionText.Text = q.QuestionText ?? "";
            OptionsPanel.Children.Clear();

            foreach (var a in q.Answers)
            {
                var rb = new RadioButton
                {
                    Content = a.AnswerText,
                    Tag = a.AnswerID,
                    GroupName = "QuizOptions",
                    Style = (Style)FindResource("OptionButtonStyle")
                };

                if (_answers.ContainsKey(index) && _answers[index] == a.AnswerID)
                    rb.IsChecked = true;

                rb.Checked += (s, e) => _answers[index] = a.AnswerID;
                OptionsPanel.Children.Add(rb);
            }

            _questionTimeRemaining = 20;
            UpdateTimerUI();
        }

        private void SaveCurrentSelection()
        {
            var selected = OptionsPanel.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);
            if (selected != null)
                _answers[_currentIndex] = (int)selected.Tag;
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
                SubmitQuiz(true);
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

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextQuestionOrSubmit();
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            _questionTimer.Stop();
            _totalTimer.Stop();
            SubmitQuiz(false);
        }

        private void SubmitQuiz(bool autoSubmit)
        {
            SaveCurrentSelection();

            int score = 0;
            foreach (var i in Enumerable.Range(0, _questions.Count))
            {
                if (_answers.ContainsKey(i))
                {
                    int selectedAnswerId = _answers[i];
                    var selectedAnswer = _questions[i].Answers.FirstOrDefault(a => a.AnswerID == selectedAnswerId);
                    if (selectedAnswer != null && selectedAnswer.IsCorrect)
                        score++;
                }
            }

            bool passed = score >= (_questions.Count / 2);

            try
            {
                using (SqlConnection conn = new SqlConnection(DbChecker.ConnectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO QuizResult (ApplicantId, Score, TotalQuestions, Passed, SubmittedOn)
                                     VALUES (@ApplicantId, @Score, @Total, @Passed, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ApplicantId", _applicantId);
                        cmd.Parameters.AddWithValue("@Score", score);
                        cmd.Parameters.AddWithValue("@Total", _questions.Count);
                        cmd.Parameters.AddWithValue("@Passed", passed);
                        cmd.ExecuteNonQuery();
                    }
                }

                MarksBox.Text = $"Your Score: {score}/{_questions.Count}";
                MessageBox.Show(
                    autoSubmit
                        ? $"Time’s up! Quiz auto-submitted.\nScore: {score}/{_questions.Count}"
                        : $"Quiz completed!\nScore: {score}/{_questions.Count}",
                    "Quiz Result", MessageBoxButton.OK, MessageBoxImage.Information);

                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving quiz result: " + ex.Message);
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