using System.ComponentModel.DataAnnotations;

namespace TripGenius.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User"; // Admin / User

        public string? Phone { get; set; }

        public string Status { get; set; } = "Active";

        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TripsCount { get; set; }
        public decimal TotalSpent { get; set; }

        // Navigation
        public List<Booking> Bookings { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
