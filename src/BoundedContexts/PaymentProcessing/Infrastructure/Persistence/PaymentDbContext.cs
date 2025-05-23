using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Domain.Common;
using FluencyHub.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace FluencyHub.PaymentProcessing.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    private readonly IDomainEventService? _domainEventService;

    public PaymentDbContext(
        DbContextOptions<PaymentDbContext> options,
        IDomainEventService? domainEventService = null) 
        : base(options)
    {
        _domainEventService = domainEventService;
    }
    
    public DbSet<Payment> Payments => Set<Payment>();
    
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
        
        // Configurações de entidades
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.TransactionId).HasMaxLength(100);
        });

        // Configurar CardDetails como entidade possuída
        modelBuilder.Entity<Payment>()
            .OwnsOne(p => p.CardDetails, details =>
            {
                details.ToTable("CardDetails");
            });

        // Pagamento não tem navegação - apenas IDs
        modelBuilder.Entity<Payment>()
            .Property(p => p.EnrollmentId);

        modelBuilder.Entity<Payment>()
            .Property(p => p.StudentId);
    }
} 