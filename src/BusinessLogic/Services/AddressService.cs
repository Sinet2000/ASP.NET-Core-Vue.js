using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Services
{
    public class AddressService : BaseService<Address>, IAddressService
    {
        public AddressService(IDataContext dataContext)
            : base(dataContext)
        {
        }
    }

    public interface IAddressService : IService<Address>
    {
    }
}
