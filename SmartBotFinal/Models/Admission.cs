namespace SmartBotFinal.Models
{
    public class Admission
    {
        public int Id { get; set; }
        public string ProgramName { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public string Requirements { get; set; }
        public string ContactEmail { get; set; }
        public string ApplicationLink { get; set; }
    }
}
