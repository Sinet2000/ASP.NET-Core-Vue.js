using System.ComponentModel.DataAnnotations;

namespace WebSite.ViewModels.Account
{
    public class ConfirmEmailPostViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}