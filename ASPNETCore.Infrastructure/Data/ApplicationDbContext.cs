using ASPNETCore.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<VolunteerEvent> VolunteerEvents { get; set; }
        public DbSet<EventAttendance> EventAttendances { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<EventStatus> EventStatuses { get; set; }
        public DbSet<AttendanceStatus> AttendanceStatuses { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<VolunteerProfile> VolunteerProfiles { get; set; }
        public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }
        public DbSet<VolunteerRank> VolunteerRanks { get; set; }

        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<ReportStatus> ReportStatuses { get; set; }

        public DbSet<Ban> Bans { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // регистрация на мероприятие 1 раз
            modelBuilder.Entity<EventAttendance>()
                .HasIndex(a => new { a.EventId, a.UserId })
                .IsUnique();

            // EventAttendance - Event
            modelBuilder.Entity<EventAttendance>()
                .HasOne(a => a.VolunteerEvent)
                .WithMany(e => e.Attendees)
                .HasForeignKey(a => a.EventId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // EventAttendance - User
            modelBuilder.Entity<EventAttendance>()
                .HasOne(a => a.User)
                .WithMany(u => u.EventAttendance)
                .HasForeignKey(a => a.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // EventAttendance - Status
            modelBuilder.Entity<EventAttendance>()
                .HasOne(a => a.AttendanceStatus)
                .WithMany(s => s.Attendance)
                .HasForeignKey(a => a.AttendanceStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event - Organizer
            modelBuilder.Entity<VolunteerEvent>()
                .HasOne(e => e.User)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event - Moderator
            modelBuilder.Entity<VolunteerEvent>()
                .HasOne(e => e.ModeratedByUser)
                .WithMany()
                .HasForeignKey(e => e.ModeratedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event - Category
            modelBuilder.Entity<VolunteerEvent>()
                .HasOne(e => e.EventCategory)
                .WithMany(c => c.VEvents)
                .HasForeignKey(e => e.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event - Status
            modelBuilder.Entity<VolunteerEvent>()
                .HasOne(e => e.EventStatus)
                .WithMany(s => s.VEvents)
                .HasForeignKey(e => e.EventStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event - City
            modelBuilder.Entity<VolunteerEvent>()
                .HasOne(e => e.City)
                .WithMany(c => c.VEvents)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // VolunteerProfile 1:1
            modelBuilder.Entity<VolunteerProfile>()
                .HasOne(v => v.User)
                .WithOne(u => u.VolunteerProfile)
                .HasForeignKey<VolunteerProfile>(v => v.UserId);

            // OrganizerProfile 1:1
            modelBuilder.Entity<OrganizerProfile>()
                .HasOne(o => o.User)
                .WithOne(u => u.OrganizerProfile)
                .HasForeignKey<OrganizerProfile>(o => o.UserId);

            // VolunteerProfile - Rank
            modelBuilder.Entity<VolunteerProfile>()
                .HasOne(v => v.Rank)
                .WithMany()
                .HasForeignKey(v => v.RankId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ban - Moderator
            modelBuilder.Entity<Ban>()
                .HasOne(b => b.Moder)
                .WithMany()
                .HasForeignKey(b => b.ModerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ban - BannedUser
            modelBuilder.Entity<Ban>()
                .HasOne(b => b.BannedUser)
                .WithMany(u => u.Bans)
                .HasForeignKey(b => b.BannedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserReport - Sender
            modelBuilder.Entity<UserReport>()
                .HasOne(r => r.Sender)
                .WithMany()
                .HasForeignKey(r => r.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserReport - Reported
            modelBuilder.Entity<UserReport>()
                .HasOne(r => r.Reported)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserReport - Status
            modelBuilder.Entity<UserReport>()
                .HasOne(r => r.ReportStatus)
                .WithMany(s => s.UserReports)
                .HasForeignKey(r => r.ReportStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            //soft delete фильтры
            modelBuilder.Entity<VolunteerEvent>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<EventAttendance>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<EventCategory>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<EventStatus>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AttendanceStatus>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<City>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<VolunteerRank>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ReportStatus>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<UserReport>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Ban>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}