using System;
using System.Linq;
using Microsoft.Extensions.Options;

namespace WebSite.Core.ExternalAuthentication
{
    using Core.Config;

    public sealed class AssertionGrantHandlerProvider : IAssertionGrantHandlerProvider
    {
        private readonly LoginProvidersSettings settings;
        private readonly IServiceProvider serviceProvider;

        public AssertionGrantHandlerProvider(IOptions<LoginProvidersSettings> loginProvidersSettings, IServiceProvider serviceProvider)
        {
            this.settings = loginProvidersSettings.Value;
            this.serviceProvider = serviceProvider;
        }

        public IAssertionGrantHandler? GetHandler(string grantType)
        {
            var provider = this.settings.LoginProviders.FirstOrDefault(p => p.GrantType == grantType);

            if (provider != null && !string.IsNullOrWhiteSpace(provider.AssertionGrantHandlerType))
                return (IAssertionGrantHandler)this.serviceProvider.GetService(Type.GetType(provider.AssertionGrantHandlerType));

            return null;
        }
    }

    public interface IAssertionGrantHandlerProvider
    {
        IAssertionGrantHandler? GetHandler(string grantType);
    }
}
