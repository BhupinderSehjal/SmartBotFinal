namespace SmartBotFinal.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string? CourseCode { get; set; } // E.g., "CS-101"
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Credits { get; set; }
        public string? Department { get; set; }
        public string? Prerequisites { get; set; } // Comma-separated course codes
    }
}
