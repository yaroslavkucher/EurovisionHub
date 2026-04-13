using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EurovisionHub.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class RoleRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Please write why you want to become an administrator.")]
        [StringLength(500, ErrorMessage = "Motivation cannot exceed 500 characters.")]
        [Display(Name = "Motivation")]
        public string Motivation { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public string RequestedRole { get; set; } = "Admin";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public string? AdminComment { get; set; }
    }
}