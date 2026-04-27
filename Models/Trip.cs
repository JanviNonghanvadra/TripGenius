using System.ComponentModel.DataAnnotations;

namespace TripGenius.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Destination { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int DurationDays { get; set; }

        public string? ImageUrl { get; set; }

        public string Status { get; set; } = "Active";

        // Fields from TripGeniusNew for user saved trips
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public decimal Budget { get; set; }
        public int UserId { get; set; }
        public bool IsSaved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public List<Booking> Bookings { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
