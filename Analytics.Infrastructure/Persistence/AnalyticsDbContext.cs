using Analytics.Domain;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    public DbSet<UserEvent> UserEvents => Set<UserEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEvent>(builder =>
        {
            builder.ToTable("user_events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.TrackId)
                .HasColumnName("track_id");

            builder.Property(e => e.EventType)
                .HasColumnName("event_type")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(e => e.ContextType)
                .HasColumnName("context_type")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(e => e.ContextId)
                .HasColumnName("context_id");

            builder.Property(e => e.PositionMs)
                .HasColumnName("position_ms");

            builder.Property(e => e.DurationMs)
                .HasColumnName("duration_ms");

            builder.Property(e => e.TimestampUtc)
                .HasColumnName("timestamp_utc")
                .IsRequired();

            builder.Property(e => e.PayloadJson)
                .HasColumnName("payload")
                .HasColumnType("jsonb");

            builder.HasIndex(e => e.UserId).HasDatabaseName("idx_user_events_user");
            builder.HasIndex(e => e.TrackId).HasDatabaseName("idx_user_events_track");
            builder.HasIndex(e => e.EventType).HasDatabaseName("idx_user_events_event_type");
            builder.HasIndex(e => e.TimestampUtc).HasDatabaseName("idx_user_events_ts");
        });
    }
}

