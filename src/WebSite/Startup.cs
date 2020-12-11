using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using AspNet.Security.OpenIdConnect.Primitives;

using AutoMapper;
using DataTableQueryBuilder.DataTables;
using OpenIddict.Abstractions;

using BusinessLogic.Config;
using BusinessLogic.Helpers;
using BusinessLogic.Models;
using BusinessLogic.Services;

namespace WebSite
{
    using Core.Config;
    using Core.ExternalAuthentication;
    using Core.Helpers;
    using Core.Security;
    using Services;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Read settings from config
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<WebpackTagHelpersSettings>(Configuration.GetSection("WebpackTagHelpersSettings"));
            services.Configure<DevelopmentSettings>(Configuration.GetSection("DevelopmentSettings"));
            services.Configure<SmtpConfig>(Configuration.GetSection("SmtpConfig"));
            services.Configure<LoginProvidersSettings>(options => Configuration.GetSection("Authentication").Bind(options));

            // Add Entity Framework services to the services container.
            services.AddDbContext<DataContext>(o =>
            {
                //o.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"], b => b.MigrationsAssembly("BusinessLogic"));

                o.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"], b => b.MigrationsAssembly("BusinessLogic"));

                o.UseOpenIddict<int>();
            });

            services.AddIdentity<User, Role>(o =>
            {
                // Configure Identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireDigit = false;
                o.Password.RequiredLength = 6;

                o.SignIn.RequireConfirmedEmail = false;

                // Configure Identity to use the same JWT claims as OpenIddict instead of the legacy WS-Federation claims it uses by default (ClaimTypes),
                // which saves you from doing the mapping in your authorization controller.
                o.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                o.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                o.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddUserStore<UserStore<User, Role, DataContext, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityUserToken<int>, IdentityRoleClaim<int>>>()
            .AddRoleStore<RoleStore<Role, DataContext, int, UserRole, IdentityRoleClaim<int>>>()
            .AddDefaultTokenProviders();

            var jwtTokenSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("dfdasfsjafhjasfkghas;fjaFJKKASDLGHDFJGHASLFJL;SADFKLDSA;FKLSADFJKLFDGHJSDFHGK;JFDSG"));

            //Disable automatic JWT->WS - Federation claims mapping used by the JWT middleware.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            // Register the OpenIddict services.
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and entities.
                    options.UseEntityFrameworkCore()
                    .UseDbContext<DataContext>()
                    .ReplaceDefaultEntities<int>();
                })
                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.UseMvc();

                    // Enable the token endpoint (required to use the password flow).
                    options.EnableTokenEndpoint("/api/auth/login");
                    options.EnableLogoutEndpoint("/api/auth/logout");
                    options.EnableUserinfoEndpoint("/api/auth/userinfo");

                    options.AllowPasswordFlow();

                    if (!bool.Parse(Configuration["Authentication:RequireHttps"]))
                        options.DisableHttpsRequirement();

                    options.AllowRefreshTokenFlow();

                    options.RegisterScopes(OpenIdConnectConstants.Scopes.OpenId,
                        OpenIdConnectConstants.Scopes.Email,
                        OpenIdConnectConstants.Scopes.Profile,
                        OpenIdConnectConstants.Scopes.Phone,
                        OpenIdConnectConstants.Scopes.Address,
                        OpenIddictConstants.Scopes.Roles);

                    // Accept token requests that don't specify a client_id.
                    options.AcceptAnonymousClients();

                    var loginProvidersSettings = Configuration.GetSection("Authentication").Get<LoginProvidersSettings>();
                    foreach (var provider in loginProvidersSettings.LoginProviders)
                    {
                        options.AllowCustomFlow(provider.GrantType);

                        if (!string.IsNullOrEmpty(provider.AssertionGrantHandlerType))
                        {
                            services.AddSingleton(Type.GetType(provider.AssertionGrantHandlerType));
                        }
                    }

                    options.AllowCustomFlow(CustomGrantTypes.LoginAsUser);
                    options.AllowCustomFlow(CustomGrantTypes.LoginBackAsAdmin);

                    //Use JWT token format
                    options.UseJsonWebTokens();

                    options.AddSigningKey(jwtTokenSigningKey);
                });

            //Add JWT Bearer as default authentication scheme
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            //Add validation rules for JWT tokens issued by OpenIddict
            .AddJwtBearer(o =>
            {
                o.Audience = Configuration["Authentication:JwtBearer:audience"];
                o.RequireHttpsMetadata = bool.Parse(Configuration["Authentication:RequireHttps"]);

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //ClockSkew = TimeSpan.FromSeconds(0), // for testing
                    ValidateIssuer = false,
                    IssuerSigningKey = jwtTokenSigningKey,
                    NameClaimType = OpenIdConnectConstants.Claims.Subject,
                    RoleClaimType = OpenIdConnectConstants.Claims.Role
                };
            });

            // Add framework services.
            services.AddMvc(option =>
            {
                option.EnableEndpointRouting = false;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson();

            services.AddCors();

            // Add application services.
            AddApplicationServices(services);

            // Add AutoMapper.
            AddAutoMapper(services);

            services.RegisterDataTables();

            // Add logging
            services.AddLogging(builder => builder
                .AddConfiguration(Configuration.GetSection("Logging"))
                .AddConsole()
                .AddDebug());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, DevelopmentDefaultData defaultData)
        {
            if (env.IsDevelopment())
            {
                // Create roles and admin user
                defaultData.CreateIfNoDatabaseExists().Wait();

                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        private void AddApplicationServices(IServiceCollection services)
        {
            services.AddScoped<DevelopmentDefaultData>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            //Business Logic
            services.AddScoped<IDataContext, DataContext>(serviceProvider => serviceProvider.GetService<DataContext>());
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserOrderService, UserOrderService>();

            //Helpers
            services.AddScoped<ISiteUrlHelper, SiteUrlHelper>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddSingleton<IAssertionGrantHandlerProvider, AssertionGrantHandlerProvider>();
        }

        private void AddAutoMapper(IServiceCollection services)
        {
            var webSiteAssebly = Assembly.GetExecutingAssembly();
            var profiles = webSiteAssebly.DefinedTypes
                .Where(ti => ti.Name.EndsWith("MappingProfile"))
                .Select(ti => ti.AsType()).ToArray();

            services.AddAutoMapper(profiles);
        }
    }
}
