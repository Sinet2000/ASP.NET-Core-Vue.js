using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebSite.Core.Config
{
    public class LoginProviderSettings
    {
        public string? Name { get; set; }

        public string? Key { get { return  Name?.ToLower(); } }

        public string? Id { get; set; }

        public string? GrantType { get; set; }

        [JsonIgnore]
        public string? AssertionGrantHandlerType { get; set; }
    }

    public class LoginProvidersSettings
    {
        public List<LoginProviderSettings> LoginProviders { get; set; } = new List<LoginProviderSettings>();
    }
}
