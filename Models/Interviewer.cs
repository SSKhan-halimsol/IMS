using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class Interviewer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
    }
}