using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebSite.ViewModels.Profile
{
    public class SideMenuViewModel
    {
        public bool HasLocalPassword { get; set; }

        public int ExternalLoginsCount { get; set; }
    }
}
