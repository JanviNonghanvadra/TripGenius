using System.ComponentModel.DataAnnotations;
using TripGenius.Models;

namespace TripGenius.Models.ViewModels
{
    public class SettingsViewModel
    {
        // Password section - validation attributes
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm the new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirmation password does not match the new password")]
        public string? ConfirmPassword { get; set; }

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

        // Payment / currency
        public string? Currency { get; set; }
        public string? CurrencySymbol { get; set; }
        public decimal? ProcessingFee { get; set; }
        public decimal? FixedFee { get; set; }

        // Preferences UI fields
        public string? Language { get; set; }
        public string? TravelStyle { get; set; }
        public string? TimeZone { get; set; }

        // Optional back-reference
        public User? User { get; set; }
        public Setting? Settings { get; set; }
    }
}