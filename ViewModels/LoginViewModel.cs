using System.ComponentModel.DataAnnotations;

namespace InkVault.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Optional: When true, creates a persistent authentication cookie
        /// that survives browser closure. When false, session-based auth only.
        /// </summary>
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;
    }
}
