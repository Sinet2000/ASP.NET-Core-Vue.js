using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using AspNet.Security.OpenIdConnect.Extensions;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;

using BusinessLogic.Helpers;
using BusinessLogic.Models;
using BusinessLogic.Services;

namespace WebSite.Controllers
{
    using Services;
    using Core.Config;
    using Core.Helpers;
    using Core.ActionAttributes;
    using Core.ExternalAuthentication;
    using Core.Security;
    using ViewModels.Account;

    public class AuthController : Controller
    {
        private readonly IOptions<IdentityOptions> identityOptions;
        private readonly ILogger _logger;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IEmailSender emailSender;
        private readonly ISmsSender smsSender;
        private readonly ICompanyService companyService;
        private readonly ISiteUrlHelper siteUrlHelper;
        private readonly IEmailService emailService;
        private readonly IAssertionGrantHandlerProvider assertionGrantHandlerProvider;
        private readonly LoginProvidersSettings loginProvidersSettings;

        public AuthController(
            IOptions<IdentityOptions> identityOptions,
            ILoggerFactory loggerFactory,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ICompanyService companyService,
            ISiteUrlHelper siteUrlHelper,
            IEmailService emailService,
            IAssertionGrantHandlerProvider assertionGrantHandlerProvider,
            IOptions<LoginProvidersSettings> loginProvidersSettings)
        {
            this.identityOptions = identityOptions;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.smsSender = smsSender;
            this.companyService = companyService;
            this.siteUrlHelper = siteUrlHelper;
            this.emailService = emailService;
            this.assertionGrantHandlerProvider = assertionGrantHandlerProvider;
            this.loginProvidersSettings = loginProvidersSettings.Value;

            _logger = loggerFactory.CreateLogger<AuthController>();
        }

        public async Task<IActionResult> Login(OpenIdConnectRequest openIdRequest)
        {
            if (openIdRequest.IsPasswordGrantType())
                return await HandlePasswordGrantType(openIdRequest);

            if (openIdRequest.IsRefreshTokenGrantType())
                return await HandleRefreshTokenGrantType(openIdRequest);

            if (openIdRequest.GrantType == CustomGrantTypes.LoginAsUser)
                return await HandleLoginAsUserGrantType(openIdRequest);

            if (openIdRequest.GrantType == CustomGrantTypes.LoginBackAsAdmin)
                return await HandleLoginBackAsAdminGrantType(openIdRequest);

            var assertionGrantHandler = this.assertionGrantHandlerProvider.GetHandler(openIdRequest.GrantType);
            if (assertionGrantHandler != null)
            {
                return await HangleProviderLogin(openIdRequest, assertionGrantHandler);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        public async Task<IActionResult> HangleProviderLogin(OpenIdConnectRequest openIdRequest, IAssertionGrantHandler assertionGrantHandler)
        {
            // Reject the request if the "assertion" parameter is missing.
            if (string.IsNullOrEmpty(openIdRequest.Assertion))
            {
                return this.BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "The mandatory 'assertion' parameter was missing.",
                });
            }

            var validationResult = await assertionGrantHandler.ValidateAsync(openIdRequest.Assertion);
            if (!validationResult.IsSuccessful)
            {
                return this.BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = validationResult.Error,
                });
            }

            var user = await this.userManager.FindByLoginAsync(assertionGrantHandler.Name, validationResult.UserId);
            if (user == null)
            {
                if (!string.IsNullOrEmpty(validationResult.Email))
                {
                    var fullname = openIdRequest.GetParameter(Constants.ProviderResponse.Name)?.ToString();

                    // They provided a user name, so try to implicitly create an account for them
                    user = new User { UserName = validationResult.Email, Email = validationResult.Email, FullName = fullname };
                    var creationResult = await this.userManager.CreateAsync(user);
                    if (!creationResult.Succeeded)
                    {
                        return this.BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = string.Join(" ", creationResult.Errors.Select(error => error.Description)),
                        });
                    }

                    var rolesResult = await userManager.AddToRoleAsync(user, RoleNames.User);
                    if (!rolesResult.Succeeded)
                        return BadRequest(new { error = rolesResult.Errors.Select(x => x.Description) });
                }

                if (user == null)
                {
                    // If the user is already logged in, use the current user
                    user = await this.userManager.GetUserAsync(this.User);
                }

                // Add the login if we found a user
                if (user != null)
                {
                    var login = new UserLoginInfo(assertionGrantHandler.Name, validationResult.UserId, assertionGrantHandler.Name);
                    var addLoginResult = await this.userManager.AddLoginAsync(user, login);
                    if (!addLoginResult.Succeeded)
                    {
                        return this.BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = string.Join(" ", addLoginResult.Errors.Select(error => error.Description)),
                        });
                    }
                }
                else
                {
                    // Ask the user to create an account.
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.AccountSelectionRequired,
                    });
                }
            }

            // Ensure the user is allowed to sign in.
            if (!await this.signInManager.CanSignInAsync(user))
            {
                return this.BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user is not allowed to sign in.",
                });
            }

            var ticket = await CreateTicketAsync(openIdRequest, user);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            return SignOut(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        [Authorize]
        public async Task<IActionResult> UserInfo()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user profile is no longer available."
                });
            }

            var claims = new JObject();

            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            claims[OpenIdConnectConstants.Claims.Subject] = await userManager.GetUserIdAsync(user);
            claims[CustomClaims.Account.HasLocalAccount] = await userManager.HasPasswordAsync(user);

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Email))
            {
                claims[OpenIdConnectConstants.Claims.Email] = user.Email;
                claims[OpenIdConnectConstants.Claims.EmailVerified] = user.EmailConfirmed;
            }

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Profile))
            {
                SetProfileClaims(claims, user);
            }

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Phone))
            {
                claims[OpenIdConnectConstants.Claims.PhoneNumber] = user.PhoneNumber;
                claims[OpenIdConnectConstants.Claims.PhoneNumberVerified] = user.PhoneNumberConfirmed;
            }

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles))
            {
                claims[OpenIddictConstants.Claims.Roles] = JArray.FromObject(await userManager.GetRolesAsync(user));
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Json(claims);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ConfirmEmail([FromBody]ConfirmEmailPostViewModel model)
        {
            User user = await userManager.FindByIdAsync(model.UserId);

            IdentityResult confirmResult = await userManager.ConfirmEmailAsync(user, model.Token);

            if (confirmResult.Succeeded)
                return Ok();

            return BadRequest(new { error = "Failed to confirm email. Please check your link and try again." });
        }

        [ValidateModel]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            var requireConfirmedEmail = signInManager.Options.SignIn.RequireConfirmedEmail;

            var user = new User() { UserName = model.Email, FullName = model.FullName, Email = model.Email, EmailConfirmed = !requireConfirmedEmail, CreateDate = DateTime.Now };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(new { error = result.Errors.Select(x => x.Description) });

            result = await userManager.AddToRoleAsync(user, RoleNames.User);

            if (!result.Succeeded)
                return BadRequest(new { error = result.Errors.Select(x => x.Description) });

            _logger.LogInformation($"New user registered (id: {user.Id})");

            if (!requireConfirmedEmail)
                return Ok(new { canLogin = true });

            // Send email confirmation email
            string confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await emailService.SendConfirmAccountEmail(siteUrlHelper.GetConfirmEmail(user.Id, confirmToken), model.Email);

            _logger.LogInformation($"Sent email confirmation email (id: {user.Id})");

            return Ok(new { canLogin = false, requireConfirmedEmail = true });
        }

        [HttpPost]
        public IActionResult DoesUserNameExist(string email)
        {
            return Json(userManager.FindByNameAsync(email).Result == null);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordViewModel model)
        {
            User user = await userManager.FindByNameAsync(model.Email);

            if (user == null || (signInManager.Options.SignIn.RequireConfirmedEmail && !await userManager.IsEmailConfirmedAsync(user)))
                return BadRequest(new { error = "User does not exists or is not confirmed." });

            string code = await userManager.GeneratePasswordResetTokenAsync(user);

            await emailService.SendForgotPasswordEmail(siteUrlHelper.GetResetPassword(code), model.Email);

            return Ok();
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordViewModel model)
        {
            User user = await userManager.FindByNameAsync(model.Email);
            if (user == null)
                return BadRequest(new { error = "User does not exists." });

            IdentityResult result = await userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
                return Ok();

            return BadRequest(new { error = result.Errors.Select(e => e.Description) });
        }

        public IActionResult LoginProviders()
        {
            var providers = loginProvidersSettings.LoginProviders;

            return Json(providers);
        }

        private async Task<IActionResult> HandlePasswordGrantType(OpenIdConnectRequest request)
        {
            var user = await userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The username/password couple is invalid."
                });
            }

            // Validate the username/password parameters and ensure the account is not locked out.
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The account is locked out."
                    });
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The username/password couple is invalid."
                });
            }

            var ticket = await CreateTicketAsync(request, user);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> HandleRefreshTokenGrantType(OpenIdConnectRequest request)
        {
            // Retrieve the claims principal stored in the refresh token.
            var info = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

            // Retrieve the user profile corresponding to the refresh token.
            // Note: if you want to automatically invalidate the refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            var user = await userManager.GetUserAsync(info.Principal);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The refresh token is no longer valid."
                });
            }

            // Ensure the user is still allowed to sign in.
            if (!await signInManager.CanSignInAsync(user))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user is no longer allowed to sign in."
                });
            }

            // Create a new authentication ticket, but reuse the properties stored
            // in the refresh token, including the scopes originally granted.
            var ticket = await CreateTicketAsync(request, user, info.Properties);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> HandleLoginAsUserGrantType(OpenIdConnectRequest request)
        {
            if (!(User != null && User.Identity.IsAuthenticated))
                return Forbid();

            var loggedInUser = await userManager.GetUserAsync(User);

            if (!await userManager.IsInRoleAsync(loggedInUser, RoleNames.Admin))
                return Forbid();

            var userToLoginAs = await userManager.FindByNameAsync(request.Username);

            var ticket = await CreateTicketAsync(request, userToLoginAs);

            var identity = (ClaimsIdentity)ticket.Principal.Identity;

            identity.AddClaim(CustomClaims.Admin.LoginAsUserAdminAccountUsername, loggedInUser.UserName, OpenIdConnectConstants.Destinations.AccessToken);
            identity.AddClaim(CustomClaims.Admin.LoginAsUserReturnTo, request.GetParameter(CustomClaims.Admin.LoginAsUserReturnTo)?.Value.ToString(), OpenIdConnectConstants.Destinations.AccessToken);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> HandleLoginBackAsAdminGrantType(OpenIdConnectRequest request)
        {
            if (!(User != null && User.Identity.IsAuthenticated))
                return Forbid();

            var adminUsername = User.FindFirstValue(CustomClaims.Admin.LoginAsUserAdminAccountUsername);

            if (string.IsNullOrEmpty(adminUsername))
                return Forbid();

            var userToLoginAs = await userManager.FindByNameAsync(adminUsername);

            var ticket = await CreateTicketAsync(request, userToLoginAs);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        #region Helpers
        private Task<User> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, User user, AuthenticationProperties? properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal,
                properties,
                OpenIdConnectServerDefaults.AuthenticationScheme);

            var requestScopes = request != null ? request.GetScopes() : new string[] { };

            if (!request.IsRefreshTokenGrantType())
            {
                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                ticket.SetScopes(request.GetScopes());
                ticket.SetResources("resource-server");
            }

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.
            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return ticket;
        }

        private void SetProfileClaims(JObject claims, User user)
        {
            claims[OpenIdConnectConstants.Claims.Name] = user.FullName;

            // Set some custom claims
            //if (await userManager.IsInRoleAsync(user, RoleNames.Admin))
            //{
            //    claims[CustomClaims.Admin.CustomClaim] = 1;
            //}
        }
        #endregion
    }
}
