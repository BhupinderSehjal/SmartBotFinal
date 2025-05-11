namespace SmartBotFinal.Models
{
    public class SystemIntegration
    {
        public int Id { get; set; }
        public string SystemType { get; set; } // E.g., "LMS", "Library"
        public string ApiEndpoint { get; set; }
        public string ApiKey { get; set; } // Securely encrypt this in practice
        public string Description { get; set; }
    }
}
