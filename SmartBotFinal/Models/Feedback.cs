namespace SmartBotFinal.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public User? Name { get; set; } // Optional
        public string? Message { get; set; }
        public int Rating { get; set; } // 1-5 scale
        public DateTime Timestamp { get; set; }
    }
}
