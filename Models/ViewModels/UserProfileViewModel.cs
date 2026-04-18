using EurovisionHub.Models;

namespace EurovisionHub.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime RegistrationDate { get; set; }
        public RoleRequest? LatestRoleRequest { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool ShowRoleChangeNotification { get; set; }
        public string? RoleChangeComment { get; set; }
    }
}