using System;
using System.Linq;

using AutoMapper;

using BusinessLogic.Models;

namespace WebSite.ViewModels.Mapping
{
    public class DateTimeToFormattedStringResolver : IMemberValueResolver<object, object, DateTime?, string>
    {
        public string Resolve(object source, object destination, DateTime? sourceMember, string destMember, ResolutionContext context)
        {
            if (!sourceMember.HasValue)
                return "";

            return sourceMember.Value.ToString("MM/dd/yyyy h:mm tt");
        }
    }

    public class DateToFormattedStringResolver : IMemberValueResolver<object, object, DateTime?, string>
    {
        public string Resolve(object source, object destination, DateTime? sourceMember, string destMember, ResolutionContext context)
        {
            if (!sourceMember.HasValue)
                return "";

            return sourceMember.Value.ToString("MM/dd/yyyy");
        }
    }

    public class UserRoleListToStringResolver : IValueResolver<User, object, string>
    {
        public string Resolve(User source, object destination, string member, ResolutionContext context)
        {
            var list = source.Roles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name);

            return string.Join(", ", list);
        }
    }

    public class CompanyIdResolver : IMemberValueResolver<object, object, Company?, int?>
    {
        public int? Resolve(object source, object destination, Company? sourceMember, int? destMember, ResolutionContext context)
        {
            return sourceMember?.Id ?? default;
        }
    }

    public class CompanyNameResolver : IMemberValueResolver<object, object, Company?, string>
    {
        public string Resolve(object source, object destination, Company? sourceMember, string destMember, ResolutionContext context)
        {
            return sourceMember?.Name ?? string.Empty;
        }
    }
}