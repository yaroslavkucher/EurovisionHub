using System;
using System.Collections.Generic;

namespace EurovisionHub.Models;

public partial class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? Date { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
}
