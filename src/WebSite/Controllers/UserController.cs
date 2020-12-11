using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using BusinessLogic.Services;

namespace WebSite.Controllers
{
    [Authorize(Roles="User, Admin")]
    public class UserController : Controller
    {
        private IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        public ActionResult Account()
        {
            return View();
        }
    }
}
