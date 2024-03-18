using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace YamoriMovieApis.Models.Domain
{
    // our Database context class will inherit from IdentityDbContext class of the Application user type
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
                
        }

        public DbSet<TokenInfo> TokenInfos { get; set; }
    }
}
