using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InkVault.ViewModels
{
    public class RegisterViewModel
    {
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        [Required] public string Username { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = null!;
        public IFormFile? ProfilePicture { get; set; }
    }
}


