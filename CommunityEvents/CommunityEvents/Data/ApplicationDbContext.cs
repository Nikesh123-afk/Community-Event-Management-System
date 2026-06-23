using CommunityEvents.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Data;

/// <summary>
/// EF Core Code-First DbContext.
/// Inherits from IdentityDbContext to integrate ASP.NET Core Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Participant> Participants { get; set; } = null!;
    public DbSet<Venue> Venues { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;
    public DbSet<Registration> Registrations { get; set; } = null!;
    public DbSet<EventActivity> EventActivities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── EventActivity composite key ────────────────────────────────────
        builder.Entity<EventActivity>()
            .HasKey(ea => new { ea.EventId, ea.ActivityId });

        builder.Entity<EventActivity>()
            .HasOne(ea => ea.Event)
            .WithMany(e => e.EventActivities)
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventActivity>()
            .HasOne(ea => ea.Activity)
            .WithMany(a => a.EventActivities)
            .HasForeignKey(ea => ea.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Registration ──────────────────────────────────────────────────
        builder.Entity<Registration>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Registration>()
            .HasOne(r => r.Participant)
            .WithMany(p => p.Registrations)
            .HasForeignKey(r => r.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Event ──────────────────────────────────────────────────────────
        builder.Entity<Event>()
            .HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Event>()
            .Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Entity<Event>()
            .Property(e => e.Description).HasMaxLength(2000).IsRequired();

        // ── Indexes ────────────────────────────────────────────────────────
        builder.Entity<Participant>()
            .HasIndex(p => p.Email)
            .IsUnique();

        builder.Entity<Event>()
            .HasIndex(e => new { e.EventDate, e.VenueId });

        // ── Seed data ──────────────────────────────────────────────────────
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        builder.Entity<Venue>().HasData(
            new { Id = 1, Name = "City Hall Auditorium", Address = "1 Town Square, Sunderland, SR1 1AB", Capacity = 500, Description = "Main civic auditorium", ContactPhone = "01915550001", ContactEmail = "bookings@cityhall.example.com", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = 2, Name = "Roker Park Community Centre", Address = "45 Roker Avenue, Sunderland, SR6 0HB", Capacity = 200, Description = "Modern community centre with flexible spaces", ContactPhone = "01915550002", ContactEmail = "events@roker.example.com", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = 3, Name = "University of Sunderland — SU Building", Address = "Chester Rd, Sunderland, SR1 3SD", Capacity = 1000, Description = "Large lecture halls and event spaces", ContactPhone = "01915550003", ContactEmail = "events@sunderland.ac.uk", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        builder.Entity<Activity>().HasData(
            new { Id = 1, Name = "C# Advanced Workshop", Description = "Hands-on OOP and design patterns in C#", Type = ActivityType.Workshop, DurationMinutes = 120, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = 2, Name = "Keynote Talk: Future of AI", Description = "Visionary talk on AI and its societal impact", Type = ActivityType.Talk, DurationMinutes = 60, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = 3, Name = "Networking Mixer", Description = "Structured networking with icebreaker games", Type = ActivityType.Networking, DurationMinutes = 90, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = 4, Name = "Charity Quiz", Description = "Team-based quiz with prizes for local charities", Type = ActivityType.Game, DurationMinutes = 90, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = 5, Name = "Art Exhibition", Description = "Community artwork display from local artists", Type = ActivityType.Exhibition, DurationMinutes = 180, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );
    }
}
