namespace WebSite.Core.ExternalAuthentication
{
    public sealed class AssertionGrantResult
    {
        public string? UserId { get; set; }

        public string? Email { get; set; }

        public string? Error { get; set; }

        public bool IsSuccessful => string.IsNullOrEmpty(this.Error) && !string.IsNullOrEmpty(this.UserId) && !string.IsNullOrEmpty(this.Email); }
}
