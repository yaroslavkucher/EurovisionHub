using System;
using System.Collections.Generic;

namespace EurovisionHub.Models;

public partial class Vote
{
    public int Id { get; set; }

    public int FromCountryId { get; set; }

    public int ToParticipationId { get; set; }

    public int Points { get; set; }

    public bool IsJury { get; set; }

    public virtual Country FromCountry { get; set; } = null!;

    public virtual Participation ToParticipation { get; set; } = null!;
}
