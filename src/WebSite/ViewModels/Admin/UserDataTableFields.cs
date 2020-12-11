using System.ComponentModel.DataAnnotations;

namespace WebSite.ViewModels.Admin
{
    public class UserDataTableFields
    {
        public int Id { get; set; }

        [Display(Name = "Company")]
        
        public string CompanyName { get; set; } = string.Empty;
        
        public string FullName { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string Role { get; set; } = string.Empty;
        
        public bool Disabled { get; set; }
        
        [Display(Name = "Created")]
        public string CreateDate { get; set; } = string.Empty;
    }
}