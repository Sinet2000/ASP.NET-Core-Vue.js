using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services
{
    using Models;

    public class UserService : BaseService<User>, IUserService
    {
        public UserService(IDataContext dataContext)
            : base(dataContext)
        {
        }

        public void DisableOrEnable(User user)
        {
            user.Disabled = !user.Disabled;

            dataContext.SaveChanges();
        }

        public User GetByIdWithCompany(int id)
        {
            return dataContext.Users.Where(u => u.Id == id).Include(u => u.Company).Single();
        }

        public IQueryable<User> GetAllWithRolesAndCompanies()
        {
            return dataContext.Users
                .Include(u => u.Roles).ThenInclude(r => r.Role)
                .Include(u => u.Company);
        }   
    }

    public interface IUserService : IService<User>
    {
        void DisableOrEnable(User user);

        User GetByIdWithCompany(int id);

        IQueryable<User> GetAllWithRolesAndCompanies();
    }
}
