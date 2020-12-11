using System;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using DataTableQueryBuilder.DataTables;
using AutoMapper;

using BusinessLogic.Services;
using BusinessLogic.Models;

namespace WebSite.Controllers
{
    using ViewModels.Admin;
    using ViewModels.Mapping;
    using WebSite.Core.ActionAttributes;

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        private readonly IUserService userService;
        private readonly ICompanyService companyService;
        private readonly IUserOrderService orderService;
        readonly IMapper mapper;

        public AdminController(UserManager<User> userManager, RoleManager<Role> roleManager, IUserService userService, ICompanyService companyService, IUserOrderService orderService, IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.userService = userService;
            this.companyService = companyService;
            this.orderService = orderService;

            this.mapper = mapper;
        }

        public IActionResult UserListData()
        {
            var model = new UserListViewModel(roleManager.Roles.OrderBy(r => r.Name).ToList());

            return Json(model);
        }

        public IActionResult UserListAjax(DataTablesRequest request)
        {
            var qb = new DataTablesQueryBuilder<UserDataTableFields, User>(request, o =>
            {
                o.ForField(f => f.Email, o => o.EnableGlobalSearch());
                o.ForField(f => f.FullName, o => o.EnableGlobalSearch());
                o.ForField(f => f.CompanyName, o =>
                {
                    o.SetEntityProperty(u => u.Company!.Name);
                    o.EnableGlobalSearch();
                });
                o.ForField(f => f.Role, o => o.SetSearchExp((u, val) => u.Roles.Any(r => r.RoleId == int.Parse(val))));
                o.ForField(f => f.CreateDate, o => o.EnableGlobalSearch());
            });

            var query = qb.Build(userService.GetAllWithRolesAndCompanies());
            
            return query.MapToResponse(mapper);
        }

        public IActionResult CompanyListAjax(DataTablesRequest request)
        {
            var qb = new DataTablesQueryBuilder<CompanyDataTableFields, Company>(request, o => o.ForField(f => f.Name, o => o.EnableGlobalSearch()));

            var query = qb.Build(companyService.GetAll());
            
            return query.MapToResponse(mapper);
        }

        public IActionResult DisableOrEnableUser(int id)
        {
            var user = userService.GetById(id);

            userService.DisableOrEnable(user);

            return Json(new { success = true });
        }

        public IActionResult DeleteUser(int id)
        {
            userService.Delete(userService.GetById(id));

            return Json(new { success = true });
        }

        public IActionResult CreateUser()
        {
            var companies = companyService.GetAll().ToList();

            var model = new UserFormViewModel
            {
                Companies = companies.ToSelectListItems(r => r.Name ?? string.Empty, r => r.Id, true)
            };

            return Json(model);
        }

        [HttpPost]
        [ValidateModel]
        public IActionResult CreateUser([FromBody]UserFormViewModel model)
        {
            var newUser = new User
            {
                UserName = model.Email,
                CompanyId = model.CompanyId,
                FullName = model.FullName,
                Email = model.Email,
                Disabled = model.Disabled,
                CreateDate = DateTime.Now
            };

            var result = userManager.CreateAsync(newUser, model.Password).Result;

            if (result.Succeeded)
            {
                result = userManager.AddToRoleAsync(newUser, RoleNames.User).Result;

                if (result.Succeeded)
                    return Ok();
            }

            return BadRequest(new { error = result.Errors.Select(e => e.Description) });
        }

        public IActionResult EditUser(int id)
        {
            var user = userService.GetByIdWithCompany(id);

            var model = mapper.Map<UserFormViewModel>(user);
            model.Companies = companyService.GetAll().ToList().ToSelectListItems(r => r.Name ?? string.Empty, r => r.Id, true);

            return Json(model);
        }

        [HttpPost]
        [ValidateModel]
        public IActionResult EditUser([FromBody]UserFormViewModel model)
        {
            var userToEdit = userManager.FindByIdAsync(model.Id.ToString()).Result;

            userToEdit.UserName = model.Email;
            userToEdit.CompanyId = model.CompanyId;
            userToEdit.FullName = model.FullName;
            userToEdit.Email = model.Email;
            userToEdit.Disabled = model.Disabled;

            var result = userManager.UpdateAsync(userToEdit).Result;

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Password))
                {
                    string resetToken = userManager.GeneratePasswordResetTokenAsync(userToEdit).Result;
                    result = userManager.ResetPasswordAsync(userToEdit, resetToken, model.Password).Result;

                    if (result.Succeeded)
                        return Ok();
                }
                else
                    return Ok();
            }

            return BadRequest(new { error = result.Errors.Select(e => e.Description) });
        }

        [HttpPost]
        [ValidateModel]
        public IActionResult CreateCompany([FromBody]CompanyFormViewModel model)
        {
            var newCompany = new Company() { Name = model.Name };

            companyService.Add(newCompany);

            return Ok();
        }

        public IActionResult EditCompany(int id)
        {
            var company = companyService.GetById(id);

            var model = mapper.Map<CompanyFormViewModel>(company);

            return Json(model);
        }

        [HttpPost]
        [ValidateModel]
        public IActionResult EditCompany([FromBody]CompanyFormViewModel model)
        {
            var companyToEdit = companyService.GetById(model.Id.GetValueOrDefault());

            companyToEdit.Name = model.Name;

            companyService.Update(companyToEdit);

            return Ok();
        }

        public IActionResult DeleteCompany(int id)
        {
            if (userService.GetMany(u => u.CompanyId == id).Count() > 0)
                return BadRequest(new { error = "Cannot delete company because company has users." });

            companyService.Delete(companyService.GetById(id));

            return Ok();
        }
    }
}