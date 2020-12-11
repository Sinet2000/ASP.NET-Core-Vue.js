using System.ComponentModel.DataAnnotations;

namespace WebSite.ViewModels.Profile
{
    public class ChangeBaseInfoViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;
    }
}
