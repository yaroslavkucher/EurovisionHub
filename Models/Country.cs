using System;
using System.Collections.Generic;

namespace EurovisionHub.Models;

public partial class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? FlagUrl { get; set; }

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
