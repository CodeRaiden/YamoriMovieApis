using Microsoft.AspNetCore.Identity;

namespace YamoriMovieApis.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
