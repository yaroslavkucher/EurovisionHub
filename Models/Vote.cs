using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EurovisionHub.Models;

public partial class Vote : IValidatableObject
{
    public int Id { get; set; }

    public int FromCountryId { get; set; }

    public int ToParticipationId { get; set; }

    [Required(ErrorMessage = "Points is required")]
    [Range(1, 12, ErrorMessage = "Points must be between 1 and 12, but without 11.")]
    public int Points { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Points == 11)
        {
            yield return new ValidationResult("This number of points does not correspond to the participant evaluation system", new[] { nameof(Points) });
        }
    }

    [Display(Name = "Jury/Televote")]
    public bool IsJury { get; set; }
    
    public int EventId { get; set; }

    public virtual Event? Event { get; set; } = null!;
    
    public virtual Country? FromCountry { get; set; } = null!;

    [Display(Name = "ToCountry")]
    public virtual Participation? ToParticipation { get; set; } = null!;
}
