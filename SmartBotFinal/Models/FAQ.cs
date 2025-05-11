namespace SmartBotFinal.Models
{
    public class FAQ
    {
        public int Id { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public FAQCategory? Category { get; set; } // E.g., "Admissions", "Housing"
        public string? Keywords { get; set; } // For NLP intent matching
    }

    public enum FAQCategory
    {
        Admissions,
        Housing,
        Academics,
        FinancialAid,
        CampusLife,
        TechnicalSupport,
        GeneralInquiry,
        Other
    }
}
