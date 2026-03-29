using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Persistence.Contexts
{
    //dotnet ef migrations add ImagesAdd --project Persistence --startup-project ClimateMonitorAPI
    //dotnet ef database update --project Persistence --startup-project ClimateMonitorAPI
    public class ClimateMonitorDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Building> Buildings { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<Measurement> Measurements { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<AccessRight> AccessRights { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<AccessInvite> AccessInvites { get; set; } = null!;

        public ClimateMonitorDbContext(DbContextOptions<ClimateMonitorDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Building>()
                .HasMany(b => b.Rooms)
                .WithOne(r => r.Building)
                .HasForeignKey(r => r.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Room>()
                .HasMany(r => r.Devices)
                .WithOne(d => d.Room)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Device>()
                .HasMany(d => d.Measurements)
                .WithOne(m => m.Device)
                .HasForeignKey(m => m.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Device>()
                .HasIndex(d => d.MacAddress)
                .IsUnique();

            builder.Entity<Measurement>()
                .HasIndex(m => new { m.DeviceId, m.Timestamp });

            builder.Entity<AccessRight>()
                .HasKey(ar => new { ar.UserId, ar.BuildingId });

            builder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AuditLog>()
                .HasOne(al => al.Room)
                .WithMany()
                .HasForeignKey(al => al.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AuditLog>()
                .HasOne(al => al.Device)
                .WithMany()
                .HasForeignKey(al => al.DeviceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AuditLog>()
                .HasIndex(al => new { al.UserId, al.Timestamp });

            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            builder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccessInvite>()
                .HasOne(ai => ai.Building)
                .WithMany()
                .HasForeignKey(ai => ai.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccessInvite>()
                .HasOne(ai => ai.CreatedByUser)
                .WithMany()
                .HasForeignKey(ai => ai.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccessInvite>()
                .HasOne(ai => ai.UsedByUser)
                .WithMany()
                .HasForeignKey(ai => ai.UsedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccessInvite>()
                .HasIndex(ai => ai.Token)
                .IsUnique();
        }
    }
}
