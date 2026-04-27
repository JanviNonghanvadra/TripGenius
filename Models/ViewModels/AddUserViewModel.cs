using System.ComponentModel.DataAnnotations;

namespace TripGenius.Models.ViewModels
{
    public class AddUserViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; } = "User";

        [Required]
        public string Status { get; set; } = "Active";

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}