using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class QuizQuestion
    {
        [Key]
        public int QuestionID { get; set; }
        public int QuizID { get; set; }
        public string QuestionText { get; set; }

        // Navigation property
        public virtual Quiz Quiz { get; set; }

        // Each question has multiple possible answers
        public virtual ICollection<QuizAnswer> Answers { get; set; }
    }
}