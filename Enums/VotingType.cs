using System.ComponentModel.DataAnnotations;

namespace EurovisionHub.Enums
{
    public enum VotingType
    {
        [Display(Name ="Jury")]
        Jury = 0,
        [Display(Name = "Televote")]
        Televote
    }
}
