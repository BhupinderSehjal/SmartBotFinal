using Microsoft.EntityFrameworkCore;
using SmartBotFinal.Models;

namespace SmartBotFinal.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<BotResponse> BotResponses { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<CampusLocation> CampusLocations { get; set; }
        public DbSet<ChatbotMessage> ChatbotMessages { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<SystemIntegration> SystemIntegrations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
    }
    
}
