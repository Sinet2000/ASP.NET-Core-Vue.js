using System.ComponentModel.DataAnnotations;

namespace WebSite.ViewModels.Profile
{
    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
