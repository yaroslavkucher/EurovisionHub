using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EurovisionHub.Models;

public partial class EurovisionContext : DbContext
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=eurovision_db;Username=postgres;Password=YAROSLAV0401");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.Type).HasMaxLength(255);
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
