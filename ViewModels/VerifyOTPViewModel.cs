using System.ComponentModel.DataAnnotations;

namespace InkVault.ViewModels
{
    public class VerifyOTPViewModel
    {
        [Required(ErrorMessage = "OTP is required")]
        [Display(Name = "One-Time Password")]
        public string OTP { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string Purpose { get; set; } = "Registration"; // Registration, Login, or PasswordReset
    }
}
