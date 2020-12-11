using System.ComponentModel.DataAnnotations;

namespace WebSite.ViewModels.Profile
{
    public class VerifyPhoneNumberViewModel
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Status { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
