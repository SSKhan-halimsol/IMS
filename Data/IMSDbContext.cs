using IMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Data
{
    public class IMSDbContext : DbContext
    {
        public IMSDbContext() : base("name=IMSConnection") { }
        public DbSet<User> User { get; set; }
        public DbSet<IMS.Models.Applicant> Applicants { get; set; }
        public DbSet<QuizResult> QuizResult { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
    }

}
