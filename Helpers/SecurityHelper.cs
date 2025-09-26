using System.Data.SqlClient;
using System.Threading.Tasks;

namespace IMS.Helpers
{
    public static class SecurityHelper
    {
        // ✅ update with your real SQL Server details
        private static readonly string connectionString =
            "Server=192.168.1.188;Database=IMS;User Id=sa;Password=123456;";

        public static async Task<bool> ValidateAdminAsync(string username, string password)
        {
            bool isAuthenticated = false;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username.Trim());
                    cmd.Parameters.AddWithValue("@Password", password.Trim());

                    int count = (int)await cmd.ExecuteScalarAsync();
                    if (count > 0)
                        isAuthenticated = true;
                }
            }

            return isAuthenticated;
        }
    }
}