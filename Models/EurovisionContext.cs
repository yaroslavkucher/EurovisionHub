using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EurovisionHub.Models;

public partial class EurovisionContext : IdentityDbContext<ApplicationUser>
{
    public EurovisionContext()
    {
    }

    public EurovisionContext(DbContextOptions<EurovisionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Participation> Participations { get; set; }

    public virtual DbSet<Song> Songs { get; set; }

    public virtual DbSet<Vote> Votes { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<RoleRequest> RoleRequests { get; set; }

    public virtual DbSet<WinnerOpinion> WinnerOpinions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Country_pkey");

            entity.ToTable("Country");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(4);
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Event_pkey");

            entity.ToTable("Event");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.HasOne(d => d.Type).WithMany(p => p.Events)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EventType_fkey");
            entity.HasOne(d => d.HostCountry).WithMany(p => p.HostedEvents)
                .HasForeignKey(d => d.HostCountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("HostCountry_fkey");
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("EventType");

            entity.HasKey(e => e.Id).HasName("EventType_pkey");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");

            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Participation_pkey");

            entity.ToTable("Participation");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");

            entity.HasOne(d => d.Country).WithMany(p => p.Participations)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Country_fkey");

            entity.HasOne(d => d.Event).WithMany(p => p.Participations)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Event_fkey");

            entity.HasOne(d => d.Song).WithMany(p => p.Participations)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Song_fkey");
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Song_pkey");

            entity.ToTable("Song");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Artist).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Vote_pkey");

            entity.ToTable("Vote");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");

            entity.HasOne(d => d.FromCountry).WithMany(p => p.Votes)
                .HasForeignKey(d => d.FromCountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FromCountry_fkey");

            entity.HasOne(d => d.ToParticipation).WithMany(p => p.Votes)
                .HasForeignKey(d => d.ToParticipationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ToParticipation_fkey");
            entity.HasOne(d => d.Event).WithMany(p => p.Votes)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Event_fkey");
        });

        modelBuilder.Entity<RoleRequest>()
        .HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
