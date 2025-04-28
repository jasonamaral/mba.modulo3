using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.PaymentProcessing;
using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using FluencyHub.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using FluencyHub.Domain.Common;
using System.Reflection;

namespace FluencyHub.Infrastructure.Persistence;

public class FluencyHubDbContext : DbContext, IApplicationDbContext
{
    private readonly IDomainEventService _domainEventService;

    public FluencyHubDbContext(
        DbContextOptions<FluencyHubDbContext> options,
        IDomainEventService domainEventService = null) 
        : base(options)
    {
        _domainEventService = domainEventService;
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
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private async Task DispatchDomainEvents(CancellationToken cancellationToken)
    {
        if (_domainEventService == null)
            return;
            
        var entities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
            
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();
            
        entities.ForEach(e => e.ClearDomainEvents());
        
        foreach (var domainEvent in domainEvents)
        {
            await _domainEventService.PublishAsync(domainEvent);
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ignorar DomainEvent e classes derivadas para evitar o mapeamento pelo EF Core
        modelBuilder.Ignore<DomainEvent>();
        
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