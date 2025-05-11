namespace SmartBotFinal.Models
{
    public class CampusLocation
    {
        public int Id { get; set; }
        public string? BuildingName { get; set; }
        public string? RoomNumber { get; set; }
        public string? Department { get; set; } // E.g., "Computer Science"
        public string? Description { get; set; } // E.g., "Library, Cafeteria"
        public string? MapImageUrl { get; set; } // Link to map
    }
}
