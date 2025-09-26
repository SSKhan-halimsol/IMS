using System;
using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class Interview
    {
        [Key] // this marks Id as primary key
        public int Id { get; set; }

        [Required]
        public int ApplicantId { get; set; }

        [Required]
        public int InterviewerId { get; set; }

        public DateTime InterviewDate { get; set; }

        public string Status { get; set; } = string.Empty; // e.g., Pending, Completed
    }
}