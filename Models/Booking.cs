using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TripGenius.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TripId { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public int NumberOfPeople { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } // Pending / Confirmed / Cancelled

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("TripId")]
        public Trip Trip { get; set; }

        public Payment Payment { get; set; }
    }
}
