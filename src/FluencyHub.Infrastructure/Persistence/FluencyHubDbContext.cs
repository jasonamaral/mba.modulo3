using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.PaymentProcessing;
using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using FluencyHub.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FluencyHub.Infrastructure.Persistence;

public class FluencyHubDbContext : DbContext, IApplicationDbContext
{
    public FluencyHubDbContext(DbContextOptions<FluencyHubDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<LearningHistory> LearningHistories { get; set; }
    public DbSet<CourseProgress> CourseProgresses { get; set; }
    public DbSet<CompletedLesson> CompletedLessons { get; set; }
    
    public DatabaseFacade Database => base.Database;
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => base.SaveChangesAsync(cancellationToken);
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FluencyHubDbContext).Assembly);

        // Configure CourseContent as owned entity
        modelBuilder.Entity<Course>()
            .OwnsOne(c => c.Content, content =>
            {
                content.ToTable("CourseContents");
            });
            
        // Configure CardDetails as owned entity
        modelBuilder.Entity<Payment>()
            .OwnsOne(p => p.CardDetails, details =>
            {
                details.ToTable("CardDetails");
            });

        // Course
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Lessons)
            .WithOne(l => l.Course)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student
        modelBuilder.Entity<Student>()
            .HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
            .HasMany(s => s.Certificates)
            .WithOne(c => c.Student)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrollment
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Certificate
        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Enrollment)
            .WithMany()
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 