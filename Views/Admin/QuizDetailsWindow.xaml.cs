using System;
using System.Windows;
using System.Windows.Media;

namespace IMS.Views.Admin
{
    public partial class QuizDetailsWindow : Window
    {
        public QuizDetailsWindow(int score, int total, bool passed, DateTime submittedOn)
        {
            InitializeComponent();

            ScoreText.Text = $"Score: {score} / {total}";
            PassedText.Text = passed ? "Result: Passed ✅" : "Result: Failed ❌";
            PassedText.Foreground = new SolidColorBrush(passed ? Colors.LightGreen : Colors.IndianRed);
            DateText.Text = $"Submitted On: {submittedOn:g}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}