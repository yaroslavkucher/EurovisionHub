using Microsoft.AspNetCore.Identity;

namespace EurovisionHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;

    }
}