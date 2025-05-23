using System;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Common;
using FluencyHub.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence;

public class StudentDbContext : DbContext
{
    private readonly IDomainEventService? _domainEventService;

    public StudentDbContext(
        DbContextOptions<StudentDbContext> options,
        IDomainEventService? domainEventService = null) 
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
        
        modelBuilder.Ignore<IDomainEvent>();

        modelBuilder.Entity<Student>(ConfigureStudent);
        modelBuilder.Entity<Enrollment>(ConfigureEnrollment);
        modelBuilder.Entity<Certificate>(ConfigureCertificate);
        modelBuilder.Entity<LearningHistory>(ConfigureLearningHistory);
        modelBuilder.Entity<CourseProgress>(ConfigureCourseProgress);
        modelBuilder.Entity<CompletedLesson>(ConfigureCompletedLesson);

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
            .WithOne(cl => cl.CourseProgress)
            .HasForeignKey(cl => cl.CourseProgressId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureStudent(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(s => s.Email).IsUnique();

        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Certificates)
            .WithOne(c => c.Student)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.LearningHistory)
            .WithOne()
            .HasForeignKey<LearningHistory>(lh => lh.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureEnrollment(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.EnrollmentDate).IsRequired();

        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureCertificate(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CertificateNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.IssueDate).IsRequired();

        builder.HasOne(c => c.Student)
            .WithMany(s => s.Certificates)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureLearningHistory(EntityTypeBuilder<LearningHistory> builder)
    {
        builder.HasKey(lh => lh.Id);

        builder.HasOne<Student>()
            .WithOne(s => s.LearningHistory)
            .HasForeignKey<LearningHistory>(lh => lh.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureCourseProgress(EntityTypeBuilder<CourseProgress> builder)
    {
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.CourseId).IsRequired();
        builder.Property(cp => cp.IsCompleted).IsRequired();
        builder.Property(cp => cp.LastUpdated).IsRequired();
    }

    private void ConfigureCompletedLesson(EntityTypeBuilder<CompletedLesson> builder)
    {
        builder.HasKey(cl => cl.Id);
        builder.Property(cl => cl.LessonId).IsRequired();
        builder.Property(cl => cl.CompletedAt).IsRequired();
    }
} 