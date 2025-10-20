using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IMS.Models
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public string QuizTitle { get; set; }            // e.g. “.NET Developer Quiz”
        public string Designation { get; set; }          // Linked designation (e.g. “.NET Developer”)
        public string ExperienceLevel { get; set; }      // e.g. “Fresher”, “Less than 5 years”
        public int DurationInMinutes { get; set; }       // Timer duration for quiz
        public bool IsActive { get; set; }               // Whether this quiz is active or not
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property (1-to-many)
        public virtual ICollection<QuizQuestion> Questions { get; set; }
    }
}