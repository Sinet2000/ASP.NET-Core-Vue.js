using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSite.Core.Security
{
    public static class CustomClaims
    {
        public static class Account
        {
            public const string HasLocalAccount = "has_local_account";
        }

        public static class Admin
        {
            public const string LoginAsUserAdminAccountUsername = "admin_username";
            public const string LoginAsUserReturnTo = "return_to";
        }
    }
}
