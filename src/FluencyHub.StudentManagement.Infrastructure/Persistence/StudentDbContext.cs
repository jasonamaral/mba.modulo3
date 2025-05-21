using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Common;
using FluencyHub.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence;

public class StudentDbContext : DbContext
{
    private readonly IDomainEventService _domainEventService;

    public StudentDbContext(
        DbContextOptions<StudentDbContext> options,
        IDomainEventService domainEventService = null) 
        : base(options)
    {
        _domainEventService = domainEventService;
    }
    
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<LearningHistory> LearningHistories => Set<LearningHistory>();
    public DbSet<CourseProgress> CourseProgresses => Set<CourseProgress>();
    public DbSet<CompletedLesson> CompletedLessons => Set<CompletedLesson>();
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchEvents(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private async Task DispatchEvents(CancellationToken cancellationToken)
    {
        if (_domainEventService == null)
            return;
            
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        domainEntities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _domainEventService.PublishAsync(domainEvent);
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Ignore<DomainEvent>();

        // Estudante
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

        // Matrícula - referência CourseId diretamente sem navegação
        modelBuilder.Entity<Enrollment>()
            .Property(e => e.CourseId);

        // Certificado - referência CourseId diretamente sem navegação
        modelBuilder.Entity<Certificate>()
            .Property(c => c.CourseId);

        // Histórico de aprendizado
        modelBuilder.Entity<LearningHistory>()
            .HasMany(lh => lh.CourseProgresses)
            .WithOne()
            .HasForeignKey("LearningHistoryId")
            .OnDelete(DeleteBehavior.Cascade);

        // Progresso do curso
        modelBuilder.Entity<CourseProgress>()
            .HasMany(cp => cp.CompletedLessons)
            .WithOne()
            .HasForeignKey("CourseProgressId")
            .OnDelete(DeleteBehavior.Cascade);
    }
} 