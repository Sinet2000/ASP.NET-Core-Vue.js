using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WebSite.ViewModels.Profile
{
    public class RemoveLoginViewModel
    {
        public RemoveLoginViewModel(bool showRemoveButton, IList<UserLoginInfo> linkedAccounts)
        {
            ShowRemoveButton = showRemoveButton;
            LinkedAccounts = linkedAccounts;
        }

        public bool ShowRemoveButton { get; set; }

        public IList<UserLoginInfo> LinkedAccounts { get; set; }
    }
}
