using System;
using System.Collections.Generic;

namespace EurovisionHub.Models;

public partial class Participation
{
    public int Id { get; set; }

    public int CountryId { get; set; }

    public int SongId { get; set; }

    public int EventId { get; set; }

    public int? OrderNumber { get; set; }

    public virtual Country? Country { get; set; }

    public virtual Event? Event { get; set; }

    public virtual Song? Song { get; set; }

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
