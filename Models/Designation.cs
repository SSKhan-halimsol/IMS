namespace IMS.Models
{
    public class Designation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ApplicantCount { get; set; } // computed later
    }
}