using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebSite.ViewModels.Profile;

namespace WebSite.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IAddressService addressService;
        readonly IMapper mapper;

        public AddressController(
            IAddressService addressService,
            IMapper mapper)
        {
            this.addressService = addressService;

            this.mapper = mapper;
        }

        public IActionResult GetUserAddresses(int userId)
        {
            var userAddresses = addressService.GetMany(i => i.UserId == userId);

            return Json(mapper.Map<List<AddressViewModel>>(userAddresses));
        }
    }
}
