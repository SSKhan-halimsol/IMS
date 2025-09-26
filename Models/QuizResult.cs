using System;

public class QuizResult
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public DateTime SubmittedOn { get; set; }
}