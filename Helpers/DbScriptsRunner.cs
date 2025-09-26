using System;
using System.Data.SqlClient;

namespace Interview_Management_System.Helpers
{
    public static class DbScriptsRunner
    {
        public static void Initialize()
        {
            using (var conn = new SqlConnection(IMS.Helpers.DbChecker.ConnectionString))
            {
                conn.Open();

                // ✅ Create Applicants table
                string createApplicantsTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Applicants' AND xtype='U')
                    CREATE TABLE Applicants (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FullName NVARCHAR(100) NOT NULL,
                        CNIC NVARCHAR(20) NOT NULL,
                        ApplicationDate DATE NOT NULL,
                        Status NVARCHAR(20) NOT NULL
                    )";
                ExecuteSql(conn, createApplicantsTable);

                // ✅ Create Admin table
                string createAdminTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Admins' AND xtype='U')
                    CREATE TABLE Admins (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        UserName NVARCHAR(50) NOT NULL,
                        Password NVARCHAR(50) NOT NULL
                    )";
                ExecuteSql(conn, createAdminTable);

                // ✅ Seed default admin if not exists
                string insertAdmin = @"
                    IF NOT EXISTS (SELECT * FROM Admins WHERE UserName='admin')
                    INSERT INTO Admins (UserName, Password) VALUES ('admin', 'admin123')";
                ExecuteSql(conn, insertAdmin);
            }
        }

        private static void ExecuteSql(SqlConnection conn, string sql)
        {
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}