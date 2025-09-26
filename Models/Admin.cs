using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // optional but recommended
    }
}