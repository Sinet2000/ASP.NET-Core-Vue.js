using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace WebSite.Core.Extensions
{
    public static class DatabaseFacadeExtensions
    {
        public static Task<bool> ExistsAsync(this DatabaseFacade source)
        {
            return source.GetService<IRelationalDatabaseCreator>().ExistsAsync();
        }
    }
}
