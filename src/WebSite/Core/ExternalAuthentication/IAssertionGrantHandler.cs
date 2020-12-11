namespace WebSite.Core.ExternalAuthentication
{
    using System.Threading.Tasks;

    public interface IAssertionGrantHandler
    {
        string? Name { get; }

        Task<AssertionGrantResult> ValidateAsync(string assertion);
    }
}
