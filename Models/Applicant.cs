using System;
namespace IMS.Models
{
    public class Applicant
    {
        public int Id { get; set; }
        public string ApplicantName { get; set; }
        public string FatherName { get; set; }
        public string CNIC { get; set; }
        public string FatherProfession { get; set; }
        public string ContactNo { get; set; }
        public DateTime? Date { get; set; }
        public string MaritalStatus { get; set; }
        public int Age { get; set; }
        public string AppliedFor { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        // Technical 
        public string CurrentPositionReason { get; set; }
        public int OOPRating { get; set; }
        public int SQLRating { get; set; }
        public int UIRating { get; set; }
        public string IsCurrentlyEmployed { get; set; }
        public string CurrentCompany { get; set; }
        public string WhyThisJob { get; set; }
        public string WatchCheck { get; set; }
        public string Challenges { get; set; }
        public string Strengths { get; set; }
        public string Weaknesses { get; set; }

        // Work Experience 
        public string Frameworks { get; set; }
        public string ExperienceDuration { get; set; }
        public string Goals { get; set; }
        public string CurrentSalary { get; set; }
        public string EducationInstitute { get; set; }
        public string CGPA { get; set; }
        public string Willingness { get; set; }

        // Domain 
        public string Domain { get; set; }
        public string Status { get; set; }
        public string AdminComment { get; set; }
        public string ResumePath { get; set; }

    }
}