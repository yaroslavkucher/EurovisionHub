using System.ComponentModel.DataAnnotations;

namespace EurovisionHub.Models
{
    public class WinnerOpinion
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int EventId { get; set; }

        public bool IsAgree { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}