using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class QuizAnswer
    {
        [Key]
        public int AnswerID { get; set; }
        public int QuestionID { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }  // Indicates whether this option is correct

        // Navigation property
        public virtual QuizQuestion Question { get; set; }
    }
}