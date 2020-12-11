using System.ComponentModel.DataAnnotations;

namespace WebSite.ViewModels.Account
{
    public class BaseLoginViewModel
    {
        public string? ReturnUrl { get; set; }
    }

    public class LoginViewModel : BaseLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
