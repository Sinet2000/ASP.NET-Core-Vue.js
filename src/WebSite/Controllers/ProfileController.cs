using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AspNet.Security.OpenIdConnect.Primitives;

using AutoMapper;
using DataTableQueryBuilder.DataTables;

using BusinessLogic.Models;

using WebSite.Services;
using WebSite.ViewModels.Profile;

namespace WebSite.Controllers
{
    using Core.Config;
    using Core.ActionAttributes;
    using Core.ExternalAuthentication;
    using Core.Helpers;

    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IEmailSender emailSender;
        private readonly ISmsSender smsSender;
        private readonly LoginProvidersSettings loginProvidersSettings;
        private readonly IAssertionGrantHandlerProvider assertionGrantHandlerProvider;

        private readonly IMapper mapper;

        public ProfileController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IMapper mapper,
            IOptions<LoginProvidersSettings> loginProvidersSettings,
            IAssertionGrantHandlerProvider assertionGrantHandlerProvider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.smsSender = smsSender;
            this.mapper = mapper;
            this.loginProvidersSettings = loginProvidersSettings.Value;
            this.assertionGrantHandlerProvider = assertionGrantHandlerProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            var statusMessage = GetMessage(message);

            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel
            {
                HasLocalPassword = await userManager.HasPasswordAsync(user),
                PhoneNumber = await userManager.GetPhoneNumberAsync(user),
                TwoFactor = await userManager.GetTwoFactorEnabledAsync(user),
                Logins = await userManager.GetLoginsAsync(user),
                BrowserRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
                StatusMessage = statusMessage,
                ChangeBaseInfo = new ChangeBaseInfoViewModel { Email = user.Email, FullName = user.FullName ?? string.Empty }
            };

            return View(model);
        }

        public async Task<IActionResult> GetBaseInfo()
        {
            var user = await GetCurrentUserAsync();

            if (user != null)
            {
                ChangeBaseInfoViewModel model = mapper.Map<ChangeBaseInfoViewModel>(user);
                return Json(model);
            }

            return BadRequest(new { error = GetMessage(ManageMessageId.Error) });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ChangeBaseInfo([FromBody]ChangeBaseInfoViewModel model)
        {
            var user = await GetCurrentUserAsync();

            if (user != null)
            {
                user.Email = model.Email;
                user.FullName = model.FullName;
                user.UserName = model.Email;

                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    CollectIdentityErrors(result);

                    return BadRequest(new { error = ModelState.ExtractErrorMessages() });
                }

                return Ok(new { message = GetMessage(ManageMessageId.ChangeBaseInfoSuccess) });
            }

            return BadRequest(new { error = GetMessage(ManageMessageId.Error) });
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await GetCurrentUserAsync();
            var model = new ChangePasswordViewModel() { HasLocalPassword = await userManager.HasPasswordAsync(user) };

            return View(model);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel model)
        {
            var user = await GetCurrentUserAsync();

            if (user != null)
            {
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return Ok(new { message = GetMessage(ManageMessageId.ChangePasswordSuccess) });
                }

                CollectIdentityErrors(result);

                return BadRequest(new { error = ModelState.ExtractErrorMessages() });
            }

            return BadRequest(new { error = GetMessage(ManageMessageId.Error) });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> SetPassword([FromBody]SetPasswordViewModel model)
        {
            var user = await GetCurrentUserAsync();

            if (user != null)
            {
                var result = await userManager.AddPasswordAsync(user, model.NewPassword);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return Ok(new { message = GetMessage(ManageMessageId.SetPasswordSuccess) });
                }

                CollectIdentityErrors(result);

                return BadRequest(new { error = ModelState.ExtractErrorMessages() });
            }

            return BadRequest(new { error = GetMessage(ManageMessageId.Error) });
        }

        public async Task<IActionResult> LoginProvidersListAjax(DataTablesRequest request)
        {
            var user = await GetCurrentUserAsync();
            var userLogins = (await userManager.GetLoginsAsync(user)).AsQueryable();

            var qb = new DataTablesQueryBuilder<UserLoginInfo, UserLoginInfo>(request);

            var query = qb.Build(userLogins);

            return query.MapToResponse(mapper);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLoginProvider([FromBody]UserLoginInfoViewModel account)
        {
            var user = await GetCurrentUserAsync();
            var userLogins = user != null ? await userManager.GetLoginsAsync(user) : null;

            if (user != null && (!string.IsNullOrEmpty(user.PasswordHash) || userLogins?.Count > 1))
            {
                var result = await userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return Ok(new { message = GetMessage(ManageMessageId.RemoveLoginSuccess) });
                }
            }

            return BadRequest(new { message = GetMessage(ManageMessageId.Error) });
        }

        public async Task<IActionResult> GetAvailableLoginProviders()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var userLogins = await userManager.GetLoginsAsync(user);

                var providers = loginProvidersSettings.LoginProviders;

                if (userLogins?.Count > 0)
                {
                    providers = providers.Where(x => !userLogins.Any(l => l.LoginProvider == x.Name)).ToList();
                }

                return Json(providers);
            }

            return BadRequest(new { message = GetMessage(ManageMessageId.Error) });
        }

        public async Task<IActionResult> AddLogin([FromForm(Name = "grant_type")] string grantType, string assertion)
        {
            var assertionGrantHandler = this.assertionGrantHandlerProvider.GetHandler(grantType);
            if (assertionGrantHandler != null)
            {
                if (string.IsNullOrEmpty(assertion))
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidRequest,
                        ErrorDescription = "The mandatory 'assertion' parameter was missing.",
                    });
                }

                var validationResult = await assertionGrantHandler.ValidateAsync(assertion);
                if (!validationResult.IsSuccessful)
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = validationResult.Error,
                    });
                }

                var existingUser = User != null ? await userManager.GetUserAsync(User) : null;

                if (existingUser != null)
                {
                    var login = new UserLoginInfo(assertionGrantHandler.Name, validationResult.UserId, assertionGrantHandler.Name);
                    var existingLogin = await userManager.FindByLoginAsync(login.LoginProvider, login.ProviderKey);

                    if (existingLogin != null)
                        return BadRequest(new { error = GetMessage(ManageMessageId.LoginAlreadyAssociated) });

                    var addLoginResult = await userManager.AddLoginAsync(existingUser, login);
                    if (!addLoginResult.Succeeded)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = string.Join(" ", addLoginResult.Errors.Select(error => error.Description)),
                        });
                    }
                    else
                    {
                        return Ok(new { message = GetMessage(ManageMessageId.AddLoginSuccess) });
                    }
                }
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        #region Helpers
        private void CollectIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private string GetMessage(ManageMessageId? id)
        {
            var message = string.Empty;
            if (!id.HasValue)
                return message;

            switch (id)
            {
                case ManageMessageId.AddLoginSuccess:
                    message = "The external login was added.";
                    break;

                case ManageMessageId.ChangeBaseInfoSuccess:
                    message = "Your profile has been updated.";
                    break;

                case ManageMessageId.ChangePasswordSuccess:
                    message = "Your password has been changed.";
                    break;

                case ManageMessageId.Error:
                    message = "An error has occurred.";
                    break;

                case ManageMessageId.LoginAlreadyAssociated:
                    message = "A user with this external login already exists.";
                    break;

                case ManageMessageId.RemoveLoginSuccess:
                    message = "The external login was removed.";
                    break;

                case ManageMessageId.SetPasswordSuccess:
                    message = "Your password has been set.";
                    break;

                case ManageMessageId.SetTwoFactorSuccess:
                    message = "Your two-factor authentication provider has been set.";
                    break;
            }

            return message;
        }

        public enum ManageMessageId
        {
            AddLoginSuccess,
            ChangeBaseInfoSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error,
            LoginAlreadyAssociated
        }

        private Task<User> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}
