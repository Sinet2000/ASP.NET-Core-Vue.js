using Microsoft.AspNetCore.Http;
using System.Web;

namespace WebSite.Core.Helpers
{
    public class SiteUrlHelper : ISiteUrlHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SiteUrlHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetSiteUrl()
        {
            HttpRequest request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}";
        }

        private string ConvertToAbsoluteUrl(string relativeUrl)
        {
            return GetSiteUrl() + relativeUrl;
        }

        public string GetResetPassword(string code)
        {
            code = HttpUtility.UrlEncode(code);
            return ConvertToAbsoluteUrl($"/auth/reset-password?code={code}");
        }

        public string GetConfirmEmail(int userId, string token)
        {
            token = HttpUtility.UrlEncode(token);
            return ConvertToAbsoluteUrl($"/auth/confirm-email?userId={userId}&token={token}");
        }
    }

    public interface ISiteUrlHelper
    {
        string GetResetPassword(string code);
        string GetConfirmEmail(int userId, string token);
    }
}