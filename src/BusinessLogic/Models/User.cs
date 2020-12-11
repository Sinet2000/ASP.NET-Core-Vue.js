using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Models
{
    public class User: IdentityUser<int>
    {
        public bool Disabled { get; set; }

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public string FullName { get; set; } = "";

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public DateTime CreateDate { get; set; }

        public ICollection<UserOrder> Orders { get; set; } = new List<UserOrder>();

        public ICollection<Address> Addresses { get; set; } = new List<Address>();

        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<UserRole> Roles { get; } = new List<UserRole>();

        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<int>> Claims { get; } = new List<IdentityUserClaim<int>>();

        /// <summary>
        /// Navigation property for this users login accounts.
        /// </summary>
        public virtual ICollection<IdentityUserLogin<int>> Logins { get; } = new List<IdentityUserLogin<int>>();

        public User() : base()
        {
        }

        public User(string userName) : base()
        {
            this.UserName = userName;
        }
    }

    public class UserRole : IdentityUserRole<int>
    {
        public Role? Role { get; set; }
        public User? User { get; set; }
    }

    public class Role : IdentityRole<int>
    {
        public ICollection<UserRole> Users { get; set; } = new List<UserRole>();

        public Role() : base()
        {
        }

        public Role(string roleName) : base()
        {
            this.Name = roleName;
        }
    }
}
