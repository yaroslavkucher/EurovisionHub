using System;
using System.Collections.Generic;

namespace EurovisionHub.Models;

public partial class Song
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Artist { get; set; } = null!;

    public string? VideoUrl { get; set; }

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
}
