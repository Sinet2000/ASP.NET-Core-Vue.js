using System.ComponentModel.DataAnnotations;

namespace WebSite.ViewModels.Account
{
    public class ExternalLoginConfirmationViewModel
    {
        public string? LoginProvider { get; set; }
        public string? ReturnUrl { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
    }
}
