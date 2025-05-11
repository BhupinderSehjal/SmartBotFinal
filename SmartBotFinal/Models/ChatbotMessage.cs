namespace SmartBotFinal.Models
{
    public class ChatbotMessage
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // Optional (if authenticated)
        public string? MessageText { get; set; }
        public string? Response { get; set; }
        public DateTime Timestamp { get; set; }
        public ChatbotIntent? Intent { get; set; } // E.g., "Admissions", "Courses"
    }

    public enum ChatbotIntent
    {
        Admissions,
        Courses,
        Fees,
        ContactSupport,
        GeneralInquiry,
        Events
    }
}
