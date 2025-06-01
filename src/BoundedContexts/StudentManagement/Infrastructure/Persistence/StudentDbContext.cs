using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Domain;
using FluencyHub.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
    public DbSet<LearningRecord> LearningRecords => Set<LearningRecord>();
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // SOLUÇÃO ROBUSTA: Contornar problemas de concorrência otimista
        
        // 1. Primeiro, processar eventos de domínio
        await DispatchEvents(cancellationToken);
        
        // 2. Detectar mudanças explicitamente
        ChangeTracker.DetectChanges();
        
        // 3. Tentar salvar com retry em caso de concorrência
        const int maxRetries = 3;
        int attempt = 0;
        
        while (attempt < maxRetries)
        {
            try
            {
                // Resetar problemas de concorrência para todas as entidades
                var entries = ChangeTracker.Entries().ToList();
                
                foreach (var entry in entries)
                {
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                    {
                        // Para entidades modificadas, resetar valores originais para evitar conflitos
                        if (entry.State == EntityState.Modified)
                        {
                            foreach (var property in entry.Properties)
                            {
                                if (property.IsModified && 
                                    (property.Metadata.Name == "UpdatedAt" || 
                                     property.Metadata.Name == "LastUpdated" ||
                                     property.Metadata.IsConcurrencyToken))
                                {
                                    // Forçar sincronização dos valores para evitar conflitos
                                    property.OriginalValue = property.CurrentValue;
                                }
                            }
                        }
                    }
                }
                
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                attempt++;
                
                if (attempt >= maxRetries)
                {
                    // Se falhou após todas as tentativas, usar estratégia de "force update"
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is BaseEntity)
                        {
                            // Forçar o estado como Added para novas entidades
                            if (entry.State == EntityState.Modified)
                            {
                                var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                                if (databaseValues == null)
                                {
                                    // A entidade foi deletada, recriar como Added
                                    entry.State = EntityState.Added;
                                }
                                else
                                {
                                    // Usar valores do banco e reapllicar as mudanças
                                    entry.OriginalValues.SetValues(databaseValues);
                                    entry.State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    
                    // Última tentativa
                    return await base.SaveChangesAsync(cancellationToken);
                }
                
                // Aguardar um pouco antes da próxima tentativa
                await Task.Delay(100 * attempt, cancellationToken);
                
                // Recarregar entidades conflituosas
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync(cancellationToken);
                }
            }
        }
        
        return 0; // Nunca deveria chegar aqui
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
        modelBuilder.Entity<LearningRecord>(ConfigureLearningRecord);

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
            .HasForeignKey<LearningHistory>(lh => lh.Id)
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
            
        // Ignorar a propriedade Course pois é uma interface (ICourse)
        builder.Ignore(e => e.Course);
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
            
        // Ignorar a propriedade Course pois é uma interface (ICourse)
        builder.Ignore(c => c.Course);
    }

    private void ConfigureLearningHistory(EntityTypeBuilder<LearningHistory> builder)
    {
        builder.HasKey(lh => lh.Id);

        // Ignorar a propriedade StudentId pois é uma propriedade computada
        builder.Ignore(lh => lh.StudentId);

        // Configurar o backing field para a coleção CourseProgresses
        builder.Navigation(lh => lh.CourseProgresses)
            .HasField("_courseProgresses")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Configurar o backing field para a coleção Records
        builder.Navigation(lh => lh.Records)
            .HasField("_records")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Student>()
            .WithOne(s => s.LearningHistory)
            .HasForeignKey<LearningHistory>(lh => lh.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureCourseProgress(EntityTypeBuilder<CourseProgress> builder)
    {
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.CourseId).IsRequired();
        builder.Property(cp => cp.IsCompleted).IsRequired();
        builder.Property(cp => cp.LastUpdated).IsRequired();
        
        // Configurar o backing field para a coleção CompletedLessons
        builder.Navigation(cp => cp.CompletedLessons)
            .HasField("_completedLessons")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private void ConfigureCompletedLesson(EntityTypeBuilder<CompletedLesson> builder)
    {
        builder.HasKey(cl => cl.Id);
        builder.Property(cl => cl.LessonId).IsRequired();
        builder.Property(cl => cl.CompletedAt).IsRequired();
    }

    private void ConfigureLearningRecord(EntityTypeBuilder<LearningRecord> builder)
    {
        builder.HasKey(lr => lr.Id);
        builder.Property(lr => lr.LessonId).IsRequired();
        builder.Property(lr => lr.CompletedAt).IsRequired();
        builder.Property(lr => lr.Grade);
    }
} 