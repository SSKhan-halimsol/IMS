using IMS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace IMS.Data
{
    public static class DesignationRepository
    {
        private static readonly string connectionString = "Server=192.168.1.188;Database=IMS;User Id=sa;Password=123456;";

        public static async Task<List<Designation>> GetAllAsync()
        {
            var list = new List<Designation>();

            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                string query = @"
                SELECT D.Id, D.Title, 
                       COUNT(A.Id) AS ApplicantCount
                FROM Designations D
                LEFT JOIN Applicants A ON A.AppliedFor = D.Title
                GROUP BY D.Id, D.Title
                ORDER BY D.Title ASC";

                using (var cmd = new SqlCommand(query, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Designation
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            ApplicantCount = reader.GetInt32(2)
                        });
                    }
                }
            }

            return list;
        }

        public static async Task<bool> AddAsync(string title)
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                string query = "INSERT INTO Designations (Title) VALUES (@Title)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public static async Task<bool> DeleteAsync(int id)
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                string query = "DELETE FROM Designations WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
    }
}