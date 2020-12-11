using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace WebSite.Core.ExternalAuthentication.Facebook
{
    using Config;

    public sealed class FacebookAssertionGrantHandler : BaseAssertionGrantHandler, IAssertionGrantHandler
    {
        public FacebookAssertionGrantHandler(IOptions<LoginProvidersSettings> loginProvidersSettings) : base(loginProvidersSettings)
        { }

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            // Verify the token is for our app. This also indirectly verifies the token is valid since it's useable.
            var appEndpoint = "https://graph.facebook.com/app/?access_token=" + assertion;
            var httpClient = new HttpClient();

            using (var response = await httpClient.GetAsync(appEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    var serializer = new JsonSerializer();
                    var facebookApp = serializer.Deserialize<FacebookApp>(reader);

                    var appId = settings.Id;

                    if (String.IsNullOrEmpty(appId) || !appId.Equals(facebookApp.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return new AssertionGrantResult { Error = "The token was for the wrong app" };
                    }
                }
            }

            // Get the facebook user id. We also have it on the client already but we can't trust clients to tell us who they are.
            FacebookUser facebookUser;
            var userEndpoint = "https://graph.facebook.com/me?fields=id,email&access_token=" + assertion;

            using (var response = await httpClient.GetAsync(userEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    var serializer = new JsonSerializer();
                    facebookUser = serializer.Deserialize<FacebookUser>(reader);
                }
            }

            if (string.IsNullOrEmpty(facebookUser.Id))
            {
                return new AssertionGrantResult { Error = "The token does not belong to a valid user" };
            }

            return new AssertionGrantResult { UserId = facebookUser.Id, Email = facebookUser.Email };
        }
    }
}
