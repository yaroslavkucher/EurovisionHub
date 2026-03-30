using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace EurovisionHub.Models;

public partial class Country
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Code is required")]
    [RegularExpression(@"^.{1,3}$", ErrorMessage = "Country code must be 3 characters or less.")]
    public string Code { get; set; } = null!;

    public string? FlagUrl { get; set; }

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public virtual ICollection<Event> HostedEvents { get; set; } = new List<Event>();
}
