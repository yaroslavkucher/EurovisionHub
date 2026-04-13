using Microsoft.AspNetCore.Identity;

namespace EurovisionHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;

        public string? RoleChangeComment { get; set; }

        public bool ShowRoleChangeNotification { get; set; }

    }
}