using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripGenius.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? AdminReply { get; set; }

        public DateTime? ReplyDate { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("TripId")]
        public Trip Trip { get; set; }
    }
}