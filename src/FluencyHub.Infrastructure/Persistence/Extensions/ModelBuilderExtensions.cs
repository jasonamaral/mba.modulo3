using FluencyHub.PaymentProcessing.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Infrastructure.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void ConfigureDomainEventAsKeyless(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DomainEvent>().HasNoKey().ToTable("DomainEvents", t => t.ExcludeFromMigrations());
    }
} 