using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

namespace WebSite.ViewModels.Profile
{
    public class ManageLoginsViewModel
    {
        public bool HasLocalPassword { get; set; }

        public IList<UserLoginInfo> CurrentLogins { get; set; } = new List<UserLoginInfo>();

        public IList<AuthenticationScheme> OtherLogins { get; set; } = new List<AuthenticationScheme>();

        public StatusMessageViewModel? StatusMessage { get; set; }
        
        public bool ShowRemoveButton { get; set; }
    }

    public class UserLoginInfoViewModel
    {
        public UserLoginInfoViewModel() { }

        public string? LoginProvider { get; set; }

        public string? ProviderKey { get; set; }
    }

    public class StatusMessageViewModel
    {
        public string? Text { get; set; }
        public TypeEnum Type { get; set; }

        public enum TypeEnum
        {
            Error,
            Success
        }
    }
}
