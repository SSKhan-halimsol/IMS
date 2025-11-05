using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace IMS.Helpers
{
    public static class DbChecker
    {
        public static string ConnectionString =>
    IMS.Helpers.DbConfigManager.GetConnectionString() ??
                @"Data Source=localhost;Initial Catalog=IMS;Integrated Security=True;MultipleActiveResultSets=True";
        private const string TargetDatabaseName = "IMS";

        public static bool EnsureDatabase()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string checkDbQuery = $@"
                IF DB_ID('{TargetDatabaseName}') IS NULL
                BEGIN
                    CREATE DATABASE [{TargetDatabaseName}];
                END";

                    using (var command = new SqlCommand(checkDbQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Database '{TargetDatabaseName}' is ready.");
                    }
                }

                return true; // ✅ Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while ensuring database: " + ex.Message);
                return false; // ❌ Failure
            }
        }

        public static void SaveQuizResult(int applicantId, int score, int totalQuestions)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DbChecker.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                        INSERT INTO QuizResults (ApplicantId, Score, TotalQuestions, Passed)
                        VALUES (@ApplicantId, @Score, @TotalQuestions, @Passed)";

                        cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                        cmd.Parameters.AddWithValue("@Score", score);
                        cmd.Parameters.AddWithValue("@TotalQuestions", totalQuestions);
                        cmd.Parameters.AddWithValue("@Passed", score >= (totalQuestions / 2)); // pass if >=50%

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error saving quiz result: " + ex.Message);
            }
        }
        public static void EnsureApplicantsTable()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Drop if exists
                string drop = @"IF OBJECT_ID('dbo.Applicants', 'U') IS NOT NULL DROP TABLE dbo.Applicants";
                using (var cmd = new SqlCommand(drop, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Create
                string create = @"
                CREATE TABLE Applicants(
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ApplicantName NVARCHAR(100),
                    FatherName NVARCHAR(100),
                    CNIC NVARCHAR(20) UNIQUE,
                    FatherProfession NVARCHAR(100),
                    ContactNo NVARCHAR(50),
                    Date DATE,
                    MaritalStatus NVARCHAR(20),
                    Age INT,
                    AppliedFor NVARCHAR(100),
                    Address NVARCHAR(200),
                    Email NVARCHAR(100) UNIQUE,

                    CurrentPositionReason NVARCHAR(500),
                    OOPRating INT,
                    SQLRating INT,
                    UIRating INT,
                    IsCurrentlyEmployed NVARCHAR(10),
                    CurrentCompany NVARCHAR(100),
                    WhyThisJob NVARCHAR(500),
                    WatchCheck NVARCHAR(100),
                    Challenges NVARCHAR(500),
                    Strengths NVARCHAR(500),
                    Weaknesses NVARCHAR(500),

                    Frameworks NVARCHAR(200),
                    ExperienceDuration NVARCHAR(50),
                    Goals NVARCHAR(200),
                    CurrentSalary NVARCHAR(50),
                    EducationInstitute NVARCHAR(200),
                    CGPA NVARCHAR(20),
                    Willingness NVARCHAR(50)
                )";
                using (var cmd = new SqlCommand(create, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}