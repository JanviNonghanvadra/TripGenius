using System.ComponentModel.DataAnnotations;

namespace TripGenius.Models
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        // General
        public string? SiteName { get; set; }
        public string? SiteDescription { get; set; }
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }

        // Notifications
        public bool EmailNotifications { get; set; }
        public bool NewUserRegistration { get; set; }
        public bool BookingConfirmations { get; set; }
        public bool ReviewNotifications { get; set; }
        public bool PaymentAlerts { get; set; }

        // Email
        public string? SMTPHost { get; set; }
        public int? SMTPPort { get; set; }
        public string? Encryption { get; set; }
        public string? SMTPUsername { get; set; }
        public string? SMTPPassword { get; set; }

        // Security
        public bool TwoFactorAuth { get; set; }
        public bool LoginAlerts { get; set; }

        // Payment
        public string? Currency { get; set; }
        public string? CurrencySymbol { get; set; }
        public decimal? ProcessingFee { get; set; }
        public decimal? FixedFee { get; set; }
    }
}