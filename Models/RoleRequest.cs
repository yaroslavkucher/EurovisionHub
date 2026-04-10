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

        [Required(ErrorMessage = "Будь ласка, напишіть, чому ви хочете стати адміністратором")]
        [StringLength(500, ErrorMessage = "Мотивація не може перевищувати 500 символів")]
        [Display(Name = "Мотивація")]
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