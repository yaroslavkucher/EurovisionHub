using System.Security.Claims;

namespace EurovisionHub.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("SuperAdmin") || user.IsInRole("Admin");
        }
    }
}