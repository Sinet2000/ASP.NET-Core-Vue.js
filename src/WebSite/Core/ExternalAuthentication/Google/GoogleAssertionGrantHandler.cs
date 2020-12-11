using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace WebSite.Core.ExternalAuthentication.Google
{
    using ExternalAuthentication;
    using Config;

    public sealed class GoogleAssertionGrantHandler : BaseAssertionGrantHandler, IAssertionGrantHandler
    {
        public GoogleAssertionGrantHandler(IOptions<LoginProvidersSettings> loginProvidersSettings) : base(loginProvidersSettings)
        { }

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            GoogleWebToken googleToken;

            var validationEndpoint = "https://www.googleapis.com/oauth2/v3/tokeninfo?access_token=" + assertion;
            using (var httpClient = new HttpClient())
            {
                using var response = await httpClient.GetAsync(validationEndpoint);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new JsonTextReader(new StreamReader(stream));
                var serializer = new JsonSerializer();
                googleToken = serializer.Deserialize<GoogleWebToken>(reader);
            }

            var clientId = settings.Id;

            if (string.IsNullOrEmpty(clientId) || !clientId.Equals(googleToken.Aud, StringComparison.OrdinalIgnoreCase))
            {
                return new AssertionGrantResult { Error = "The token was for the wrong audience" };
            }

            return new AssertionGrantResult { UserId = googleToken.Sub, Email = googleToken.Email };
        }
    }
}
