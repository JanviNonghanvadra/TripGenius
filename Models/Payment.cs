using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TripGenius.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } // UPI / Card

        public string Status { get; set; } // Success / Failed

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}
