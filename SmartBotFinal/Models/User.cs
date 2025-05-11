namespace SmartBotFinal.Models
{
    public class User
    {
        public string Id { get; set; } // Unique identifier
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; } // E.g., "Student", "Faculty"
        public string PhoneNumber { get; set; }
 
    }

    public enum UserRole
    {
        Student,
        Faculty,
        Admin,
        Guest
    }
}
