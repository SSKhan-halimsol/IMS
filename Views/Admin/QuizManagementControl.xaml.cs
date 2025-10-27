using IMS.Data;
using IMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace IMS.Views.Admin
{
    public partial class QuizManagementControl : UserControl
    {
        private int currentQuizId = 0;
        private QuizQuestion selectedQuestion = null;
        private ObservableCollection<QuizAnswer> currentAnswers = new ObservableCollection<QuizAnswer>();

        private readonly string connectionString = @"Data Source=192.168.1.188;Initial Catalog=IMS;User ID=sa;Password=123456;MultipleActiveResultSets=True";

        public QuizManagementControl()
        {
            InitializeComponent();
            LoadDesignations();
            Loaded += QuizManagementControl_Loaded;
        }

        private async void LoadDesignations()
        {
            var list = await DesignationRepository.GetAllAsync();
            DesignationBox.ItemsSource = list;
        }

        private void QuizManagementControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadQuizzes();
            lstAnswers.ItemsSource = currentAnswers;
        }

        // ================================================
        // 🔹 LOAD ALL QUIZZES
        // ================================================
        private void LoadQuizzes()
        {
            try
            {
                List<Quiz> quizzes = new List<Quiz>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT QuizID, QuizTitle, Designation, ExperienceLevel, 
                                    DurationInMinutes, IsActive, CreatedAt 
                                    FROM Quiz 
                                    ORDER BY CreatedAt DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        quizzes.Add(new Quiz
                        {
                            QuizID = (int)reader["QuizID"],
                            QuizTitle = reader["QuizTitle"].ToString(),
                            Designation = reader["Designation"].ToString(),
                            ExperienceLevel = reader["ExperienceLevel"].ToString(),
                            DurationInMinutes = (int)reader["DurationInMinutes"],
                            IsActive = (bool)reader["IsActive"],
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        });
                    }
                }

                lstQuizzes.ItemsSource = quizzes;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"SQL Error loading quizzes:\n{ex.Message}\n\nError Number: {ex.Number}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quizzes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================
        // 🔹 QUIZ SELECTED FROM LIST
        // ================================================
        private void lstQuizzes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstQuizzes.SelectedItem is Quiz quiz)
            {
                currentQuizId = quiz.QuizID;
                txtTitle.Text = quiz.QuizTitle;
                DesignationBox.Text = quiz.Designation;
                ExperienceBox.Text = quiz.ExperienceLevel;
                txtDuration.Text = quiz.DurationInMinutes.ToString();

                LoadQuestions();
                ClearAnswerSection();
            }
        }

        // ================================================
        // 🔹 NEW QUIZ BUTTON
        // ================================================
        private void NewQuiz_Click(object sender, RoutedEventArgs e)
        {
            currentQuizId = 0;
            txtTitle.Text = "";
            DesignationBox.Text = "";
            ExperienceBox.Text = "";
            txtDuration.Text = "30";
            lstQuestions.ItemsSource = null;
            ClearAnswerSection();
            lstQuizzes.SelectedItem = null;

            UpdateQuestionCount(0);
            UpdateAnswerCount(0);

            MessageBox.Show("Ready to create a new quiz. Fill in the details and click 'Save Quiz'.",
                "New Quiz", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ================================================
        // 🔹 SAVE QUIZ
        // ================================================
        private void SaveQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("Please enter a quiz title.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTitle.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(DesignationBox.Text))
                {
                    MessageBox.Show("Please enter a designation.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    DesignationBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(ExperienceBox.Text))
                {
                    MessageBox.Show("Please enter an experience level.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ExperienceBox.Focus();
                    return;
                }

                if (!int.TryParse(txtDuration.Text, out int duration) || duration <= 0)
                {
                    MessageBox.Show("Please enter a valid duration in minutes (greater than 0).", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtDuration.Focus();
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    if (currentQuizId == 0)
                    {
                        // Insert new quiz
                        string insertQuery = @"INSERT INTO Quiz 
                            (QuizTitle, Designation, ExperienceLevel, DurationInMinutes, IsActive, CreatedAt)
                            VALUES (@Title, @Designation, @Experience, @Duration, 1, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        SqlCommand cmd = new SqlCommand(insertQuery, con);
                        cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Designation", DesignationBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Experience", ExperienceBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Duration", duration);

                        currentQuizId = (int)cmd.ExecuteScalar();

                        MessageBox.Show($"Quiz created successfully!\n\nQuiz ID: {currentQuizId}\nDesignation: {DesignationBox.Text}\nExperience Level: {ExperienceBox.Text}\n\nYou can now add questions.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Update existing quiz
                        string updateQuery = @"UPDATE Quiz SET 
                            QuizTitle = @Title, 
                            Designation = @Designation, 
                            ExperienceLevel = @Experience, 
                            DurationInMinutes = @Duration
                            WHERE QuizID = @QuizID";

                        SqlCommand cmd = new SqlCommand(updateQuery, con);
                        cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Designation", DesignationBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Experience", ExperienceBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Duration", duration);
                        cmd.Parameters.AddWithValue("@QuizID", currentQuizId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Quiz updated successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No changes were made.", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

                LoadQuizzes();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error while saving quiz:\n{ex.Message}\n\nError Number: {ex.Number}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving quiz: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================
        // 🔹 DELETE QUIZ (Questions and Answers remain)
        // ================================================
        private void DeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuizId == 0)
            {
                MessageBox.Show("Please select a quiz to delete.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this quiz?\n\n⚠️ Note: Questions and answers will remain in the database and can be reused.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        // Only delete the quiz, not the questions or answers
                        SqlCommand delQuiz = new SqlCommand("DELETE FROM Quiz WHERE QuizID=@id", con);
                        delQuiz.Parameters.AddWithValue("@id", currentQuizId);
                        int rowsAffected = delQuiz.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Quiz deleted successfully!\n\nQuestions and answers have been preserved.",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    currentQuizId = 0;
                    txtTitle.Text = "";
                    DesignationBox.Text = "";
                    ExperienceBox.Text = "";
                    txtDuration.Text = "30";
                    lstQuestions.ItemsSource = null;
                    ClearAnswerSection();
                    UpdateQuestionCount(0);
                    UpdateAnswerCount(0);

                    LoadQuizzes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting quiz: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ================================================
        // 🔹 REFRESH QUIZZES
        // ================================================
        private void RefreshQuizzes_Click(object sender, RoutedEventArgs e)
        {
            LoadQuizzes();
            if (currentQuizId > 0)
            {
                LoadQuestions();
                if (selectedQuestion != null)
                {
                    LoadAnswers(selectedQuestion.QuestionID);
                }
            }
        }

        // ================================================
        // 🔹 ADD QUESTION
        // ================================================
        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuizId == 0)
            {
                MessageBox.Show("Please save the quiz details first before adding questions.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO QuizQuestions (QuizID, QuestionText) VALUES (@qid, @qt)", con);
                    cmd.Parameters.AddWithValue("@qid", currentQuizId);
                    cmd.Parameters.AddWithValue("@qt", "New Question - Click to edit");
                    cmd.ExecuteNonQuery();
                }

                LoadQuestions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding question: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================
        // 🔹 REMOVE QUESTION (Deletes related answers)
        // ================================================
        private void RemoveQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (lstQuestions.SelectedItem is QuizQuestion question)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this question?\n\n⚠️ This will also delete all answers associated with this question!",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();

                            // Delete answers first (cascading delete)
                            SqlCommand delAns = new SqlCommand("DELETE FROM QuizAnswers WHERE QuestionID=@id", con);
                            delAns.Parameters.AddWithValue("@id", question.QuestionID);
                            int answersDeleted = delAns.ExecuteNonQuery();

                            // Then delete question
                            SqlCommand delQ = new SqlCommand("DELETE FROM QuizQuestions WHERE QuestionID=@id", con);
                            delQ.Parameters.AddWithValue("@id", question.QuestionID);
                            delQ.ExecuteNonQuery();

                        }

                        LoadQuestions();
                        ClearAnswerSection();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting question: " + ex.Message, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a question to delete.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ================================================
        // 🔹 LOAD QUESTIONS
        // ================================================
        private void LoadQuestions()
        {
            if (currentQuizId == 0) return;

            try
            {
                List<QuizQuestion> questions = new List<QuizQuestion>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM QuizQuestions WHERE QuizID=@id ORDER BY QuestionID", con);
                    cmd.Parameters.AddWithValue("@id", currentQuizId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        questions.Add(new QuizQuestion
                        {
                            QuestionID = (int)reader["QuestionID"],
                            QuizID = currentQuizId,
                            QuestionText = reader["QuestionText"].ToString()
                        });
                    }
                }

                lstQuestions.ItemsSource = questions;
                UpdateQuestionCount(questions.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading questions: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================
        // 🔹 QUESTION SELECTED
        // ================================================
        private void lstQuestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstQuestions.SelectedItem is QuizQuestion q)
            {
                selectedQuestion = q;
                txtQuestionText.Text = q.QuestionText;
                LoadAnswers(q.QuestionID);
                txtQuestionText.Focus();

                txtQuestionText.CaretIndex = txtQuestionText.Text.Length;

                txtQuestionText.BringIntoView();
            }
            else
            {
                ClearAnswerSection();
            }
        }

        // ================================================
        // 🔹 LOAD ANSWERS
        // ================================================
        private void LoadAnswers(int questionId)
        {
            try
            {
                currentAnswers.Clear();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM QuizAnswers WHERE QuestionID=@id ORDER BY AnswerID", con);
                    cmd.Parameters.AddWithValue("@id", questionId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        currentAnswers.Add(new QuizAnswer
                        {
                            AnswerID = (int)reader["AnswerID"],
                            QuestionID = questionId,
                            AnswerText = reader["AnswerText"].ToString(),
                            IsCorrect = (bool)reader["IsCorrect"]
                        });
                    }
                }

                UpdateAnswerCount(currentAnswers.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading answers: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================
        // 🔹 UPDATE QUESTION TEXT
        // ================================================
        private void txtQuestionText_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedQuestion == null)
                    return;

                string newText = txtQuestionText.Text?.Trim() ?? "";

                if (string.IsNullOrEmpty(newText))
                {
                    MessageBox.Show("Question text cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtQuestionText.Text = selectedQuestion.QuestionText; // restore old text
                    return;
                }

                // Only update if text has changed
                if (newText != selectedQuestion.QuestionText)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand(
                            "UPDATE QuizQuestions SET QuestionText=@qt WHERE QuestionID=@id", con);
                        cmd.Parameters.AddWithValue("@qt", newText);
                        cmd.Parameters.AddWithValue("@id", selectedQuestion.QuestionID);
                        cmd.ExecuteNonQuery();
                    }

                    selectedQuestion.QuestionText = newText;

                    // Save the current selection
                    int currentQuestionID = selectedQuestion.QuestionID;

                    LoadQuestions();

                    // Reselect the question to maintain focus
                    if (lstQuestions.Items != null)
                    {
                        foreach (var item in lstQuestions.Items)
                        {
                            if (item is QuizQuestion q && q.QuestionID == currentQuestionID)
                            {
                                lstQuestions.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating question: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================
        // 🔹 UPDATE ANSWER TEXT
        // ================================================
        private void AnswerText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is QuizAnswer answer)
            {
                try
                {
                    string newText = answer.AnswerText?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(newText))
                    {
                        MessageBox.Show("Answer text cannot be empty.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (selectedQuestion != null)
                        {
                            LoadAnswers(selectedQuestion.QuestionID);
                        }
                        return;
                    }

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE QuizAnswers SET AnswerText=@txt WHERE AnswerID=@id", con);
                        cmd.Parameters.AddWithValue("@txt", newText);
                        cmd.Parameters.AddWithValue("@id", answer.AnswerID);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating answer: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ================================================
        // 🔹 ADD ANSWER
        // ================================================
        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedQuestion == null)
            {
                MessageBox.Show("Please select a question first!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO QuizAnswers (QuestionID, AnswerText, IsCorrect) VALUES (@qid, @txt, 0)", con);
                    cmd.Parameters.AddWithValue("@qid", selectedQuestion.QuestionID);
                    cmd.Parameters.AddWithValue("@txt", "New Answer - Click to edit");
                    cmd.ExecuteNonQuery();
                }

                LoadAnswers(selectedQuestion.QuestionID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding answer: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================
        // 🔹 REMOVE ANSWER
        // ================================================
        private void RemoveAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (lstAnswers.SelectedItem is QuizAnswer a)
            {
                var result = MessageBox.Show("Are you sure you want to delete this answer?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM QuizAnswers WHERE AnswerID=@id", con);
                            cmd.Parameters.AddWithValue("@id", a.AnswerID);
                            cmd.ExecuteNonQuery();
                        }

                        if (selectedQuestion != null)
                        {
                            LoadAnswers(selectedQuestion.QuestionID);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting answer: " + ex.Message, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an answer to delete.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ================================================
        // 🔹 MARK ANSWER AS CORRECT (FIXED)
        // ================================================
        private void Answer_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is QuizAnswer selected)
            {
                try
                {
                    bool isChecked = checkBox.IsChecked == true;

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        if (isChecked)
                        {
                            // Unmark all answers for this question first
                            SqlCommand clear = new SqlCommand("UPDATE QuizAnswers SET IsCorrect=0 WHERE QuestionID=@qid", con);
                            clear.Parameters.AddWithValue("@qid", selected.QuestionID);
                            clear.ExecuteNonQuery();

                            // Mark this one as correct
                            SqlCommand set = new SqlCommand("UPDATE QuizAnswers SET IsCorrect=1 WHERE AnswerID=@id", con);
                            set.Parameters.AddWithValue("@id", selected.AnswerID);
                            set.ExecuteNonQuery();
                        }
                        else
                        {
                            // Uncheck this answer
                            SqlCommand unset = new SqlCommand("UPDATE QuizAnswers SET IsCorrect=0 WHERE AnswerID=@id", con);
                            unset.Parameters.AddWithValue("@id", selected.AnswerID);
                            unset.ExecuteNonQuery();
                        }
                    }

                    // Reload answers to reflect changes in UI
                    if (selectedQuestion != null)
                    {
                        LoadAnswers(selected.QuestionID);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating answer status: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ================================================
        // 🔹 UPDATE COUNTERS
        // ================================================
        private void UpdateQuestionCount(int count)
        {
            if (txtQuestionCount != null)
            {
                txtQuestionCount.Text = $"{count} Question{(count != 1 ? "s" : "")}";
            }
        }

        private void UpdateAnswerCount(int count)
        {
            if (txtAnswerCount != null)
            {
                txtAnswerCount.Text = $"{count} Answer{(count != 1 ? "s" : "")}";
            }
        }

        // ================================================
        // 🔹 CLEAR ANSWER SECTION
        // ================================================
        private void ClearAnswerSection()
        {
            if (txtQuestionText != null)
            {
                txtQuestionText.Text = "";
            }

            currentAnswers.Clear();
            selectedQuestion = null;
            UpdateAnswerCount(0);
        }
    }
}