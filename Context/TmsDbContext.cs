using Microsoft.EntityFrameworkCore;
using Tms.Api.Models;

namespace Tms.Api.Data;

public class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseCalendar> CourseCalendar => Set<CourseCalendar>();
    public DbSet<Batch> Batch => Set<Batch>();
    public DbSet<Enrollment> Enrollment => Set<Enrollment>();
    public DbSet<Feedback> Feedback => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        // --- Roles ---
        model.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleId).UseIdentityColumn();
            e.Property(x => x.RoleName).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        // --- Users ---
        model.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).UseIdentityColumn();

            e.Property(x => x.Username).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.Username).IsUnique();

            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
            e.Property(x => x.Email).HasMaxLength(100);

            e.Property(x => x.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Manager)
                .WithMany(m => m.Employees)
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Courses ---
        model.Entity<Course>(e =>
        {
            e.ToTable("Courses");
            e.HasKey(x => x.CourseId);
            e.Property(x => x.CourseId).UseIdentityColumn();

            e.Property(x => x.CourseName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasColumnType("text");
            e.Property(x => x.DurationDays);

            e.Property(x => x.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");
        });

        // --- CourseCalendar ---
        model.Entity<CourseCalendar>(e =>
        {
            e.ToTable("CourseCalendar");
            e.HasKey(x => x.CalendarId);
            e.Property(x => x.CalendarId).UseIdentityColumn();

            e.Property(x => x.StartDate).IsRequired().HasColumnType("date");
            e.Property(x => x.EndDate).IsRequired().HasColumnType("date");

            e.HasOne(x => x.Course)
                .WithMany(c => c.Calendars)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // --- Batch ---
        model.Entity<Batch>(e =>
        {
            e.ToTable("Batch");
            e.HasKey(x => x.BatchId);
            e.Property(x => x.BatchId).UseIdentityColumn();

            e.Property(x => x.BatchName).IsRequired().HasMaxLength(50);

            e.Property(x => x.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            e.Property(x => x.IsActive)
                .HasDefaultValue(true); // ensures new batches are active by default

            e.HasOne(x => x.Calendar)
                .WithMany(cal => cal.Batches)
                .HasForeignKey(x => x.CalendarId)
                .OnDelete(DeleteBehavior.Restrict); // changed from NoAction
        });

        // Global query filter for active batches
        model.Entity<Batch>()
            .HasQueryFilter(b => b.IsActive);
        


        // --- Enrollment ---
        model.Entity<Enrollment>(e =>
        {
            e.ToTable("Enrollment");
            e.HasKey(x => x.EnrollmentId);
            e.Property(x => x.EnrollmentId).UseIdentityColumn();

            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RequestedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            // CHECK (Status IN ('Requested','Approved','Rejected'))
            e.HasCheckConstraint("CK_Enrollment_Status",
                "[Status] IN ('Requested','Approved','Rejected')");

            e.HasOne(x => x.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(x => x.Manager)
                .WithMany(u => u.ManagedEnrollments)
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(x => x.Batch)
                .WithMany(b => b.Enrollments)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // --- Feedback ---
        model.Entity<Feedback>(e =>
        {
            e.ToTable("Feedback");
            e.HasKey(x => x.FeedbackId);
            e.Property(x => x.FeedbackId).UseIdentityColumn();

            e.Property(x => x.FeedbackText).HasColumnType("text");
            e.Property(x => x.Rating);

            // CHECK (Rating BETWEEN 1 AND 5)
            e.HasCheckConstraint("CK_Feedback_Rating", "[Rating] BETWEEN 1 AND 5");

            e.Property(x => x.SubmittedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(x => x.Batch)
                .WithMany(b => b.Feedbacks)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
