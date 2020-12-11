using System.Linq;
using Microsoft.Extensions.Options;

namespace WebSite.Core.ExternalAuthentication
{
    using Config;

    public abstract class BaseAssertionGrantHandler
    {
        protected readonly LoginProviderSettings settings;

        public string? Name => settings.Name;

        public BaseAssertionGrantHandler(IOptions<LoginProvidersSettings> loginProvidersSettings)
        {
            this.settings = loginProvidersSettings.Value.LoginProviders.FirstOrDefault(x => x.AssertionGrantHandlerType == this.GetType().FullName);
        }
    }
}
