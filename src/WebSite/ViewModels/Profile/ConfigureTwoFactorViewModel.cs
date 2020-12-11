using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace WebSite.ViewModels.Profile
{
    public class ConfigureTwoFactorViewModel
    {
        public string? SelectedProvider { get; set; }

        public ICollection<SelectListItem> Providers { get; set; } = new List<SelectListItem>();
    }
}
