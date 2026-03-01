using System;
using System.Collections.Generic;

namespace EurovisionHub.Models;

public partial class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    private DateTime? _date;
    public DateTime? Date
    {
        get => _date;
        set => _date = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
    }

    public string Type { get; set; } = null!;

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
}
