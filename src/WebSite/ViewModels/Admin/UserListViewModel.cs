using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

using BusinessLogic.Models;

namespace WebSite.ViewModels.Admin
{
    using Mapping;

    public class UserListViewModel
    {
        public int? Role { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }

        public UserListViewModel(IList<Role> roles)
        {
            Roles = roles.ToSelectListItems(r => r.Name, r => r.Id, true);
        }
    }
}