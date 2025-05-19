using FluencyHub.ContentManagement.Domain;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.StudentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using FluencyHub.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using FluencyHub.PaymentProcessing.Domain.Common;
using FluencyHub.StudentManagement.Domain.Common;
using FluencyHub.ContentManagement.Domain.Common;
using System.Collections.Generic;

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
            
        // Processar entidades de todos os domínios
        await DispatchDomainEventsForEntities<PaymentProcessing.Domain.Common.BaseEntity, PaymentProcessing.Domain.Common.DomainEvent>(cancellationToken);
        await DispatchDomainEventsForEntities<StudentManagement.Domain.Common.BaseEntity, StudentManagement.Domain.Common.DomainEvent>(cancellationToken);
        await DispatchDomainEventsForEntities<ContentManagement.Domain.Common.BaseEntity, ContentManagement.Domain.Common.DomainEvent>(cancellationToken);
    }
    
    private async Task DispatchDomainEventsForEntities<TEntity, TDomainEvent>(CancellationToken cancellationToken)
        where TEntity : class
        where TDomainEvent : class
    {
        var entities = ChangeTracker
            .Entries<TEntity>()
            .Where(e => e.Entity is { } entity && entity.GetType().GetProperty("DomainEvents") != null && 
                       entity.GetType().GetMethod("ClearDomainEvents") != null)
            .Select(e => e.Entity)
            .ToList();
            
        foreach (var entity in entities)
        {
            var domainEventsProperty = entity.GetType().GetProperty("DomainEvents");
            if (domainEventsProperty?.GetValue(entity) is IEnumerable<TDomainEvent> domainEvents)
            {
                foreach (var domainEvent in domainEvents.ToList())
                {
                    await _domainEventService.PublishAsync(domainEvent);
                }
                
                var clearMethod = entity.GetType().GetMethod("ClearDomainEvents");
                clearMethod?.Invoke(entity, null);
            }
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ignorar todos os tipos de DomainEvent de todos os domínios
        modelBuilder.Ignore<FluencyHub.PaymentProcessing.Domain.Common.DomainEvent>();
        modelBuilder.Ignore<FluencyHub.StudentManagement.Domain.Common.DomainEvent>();
        modelBuilder.Ignore<FluencyHub.ContentManagement.Domain.Common.DomainEvent>();
        
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