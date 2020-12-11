using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessLogic.Models.Maps
{
    public class UserRoleMap : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasOne(e => e.Role).WithMany(e => e.Users).HasForeignKey(e => e.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.User).WithMany(e => e.Roles).HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
