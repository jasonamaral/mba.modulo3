using FluencyHub.Domain.PaymentProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.StudentId)
            .IsRequired();
            
        builder.Property(p => p.EnrollmentId)
            .IsRequired();
            
        builder.Property(p => p.Amount)
            .HasPrecision(10, 2)
            .IsRequired();
            
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);
            
        builder.Property(p => p.PaymentDate)
            .IsRequired();
            
        builder.OwnsOne(p => p.CardDetails, cdBuilder =>
        {
            cdBuilder.Property(cd => cd.CardHolderName)
                .HasMaxLength(100)
                .IsRequired();
                
            cdBuilder.Property(cd => cd.MaskedCardNumber)
                .HasMaxLength(20)
                .IsRequired();
                
            cdBuilder.Property(cd => cd.ExpiryMonth)
                .HasMaxLength(2)
                .IsRequired();
                
            cdBuilder.Property(cd => cd.ExpiryYear)
                .HasMaxLength(4)
                .IsRequired();
        });
    }
}