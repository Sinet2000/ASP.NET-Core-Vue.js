using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebSite.ViewModels.Admin
{
    public class UserFormViewModel
    {
        public int? Id { get; set; }

        public IEnumerable<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [Display(Name = "Full Name")]
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        public virtual string Email { get; set; } = string.Empty;

        public bool Disabled { get; set; }
    }
}