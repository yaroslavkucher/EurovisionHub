namespace EurovisionHub.Models.ViewModels
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string CurrentRole { get; set; }
    }

    public class ChangeRoleViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string SelectedRole { get; set; }
        public string Comment { get; set; }
        public List<string> AllRoles { get; set; }
    }
}