using IMS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace IMS.Data
{
    public static class ApplicantRepository
    {
        private static readonly string connectionString =
            "Server=192.168.1.188;Database=IMS;User Id=sa;Password=123456;";

        // Fetch all applicants
        public static async Task<List<Applicant>> GetAllAsync()
        {
            var list = new List<Applicant>();

            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                string query = @"
            SELECT Id, ApplicantName, FatherName, CNIC, Email, ContactNo, 
                   ExperienceDuration, Date, Status
            FROM Applicants
            ORDER BY Id ASC";

                using (var cmd = new SqlCommand(query, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var a = new Applicant
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ApplicantName = reader["ApplicantName"] as string ?? "",
                            FatherName = reader["FatherName"] as string ?? "",
                            CNIC = reader["CNIC"] as string ?? "",
                            Email = reader["Email"] as string ?? "",
                            ContactNo = reader["ContactNo"] as string ?? "",
                            ExperienceDuration = reader["ExperienceDuration"] as string ?? "",
                            Date = reader["Date"] != DBNull.Value
                                ? Convert.ToDateTime(reader["Date"])
                                : DateTime.MinValue,
                            Status = reader["Status"] as string ?? ""
                        };
                        list.Add(a);
                    }
                }
            }

            return list;
        }

        // Fetch a single applicant by Id
        public static async Task<Applicant> GetByIdAsync(int id)
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                string query = @"
                    SELECT Id, ApplicantName, FatherName, CNIC, Email, ContactNo, ExperienceDuration, Date, Status
                    FROM Applicants
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Applicant
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ApplicantName = reader["ApplicantName"] as string ?? "",
                                FatherName = reader["FatherName"] as string ?? "",
                                CNIC = reader["CNIC"] as string ?? "",
                                Email = reader["Email"] as string ?? "",
                                ContactNo = reader["ContactNo"] as string ?? "",
                                ExperienceDuration = reader["ExperienceDuration"] as string ?? "",
                                Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : DateTime.MinValue,
                                Status = reader["Status"] as string ?? ""
                            };
                        }
                    }
                }
            }

            return null;
        }
        // Delete applicant
        public static async Task<bool> DeleteAsync(int id)
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                string query = "DELETE FROM Applicants WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
    }
}