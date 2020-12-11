using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace WebSite.ViewModels.Profile
{
    public class IndexViewModel
    {
        public bool HasLocalPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; } = new List<UserLoginInfo>();

        public string? PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }

        public string? StatusMessage { get; set; }

        public ChangeBaseInfoViewModel? ChangeBaseInfo { get; set; }
    }
}
