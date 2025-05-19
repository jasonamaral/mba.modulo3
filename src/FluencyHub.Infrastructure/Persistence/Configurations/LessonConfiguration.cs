using FluencyHub.ContentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Title)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(l => l.Content)
            .HasMaxLength(10000)
            .IsRequired();
            
        builder.Property(l => l.MaterialUrl)
            .HasMaxLength(500);
            
        builder.Property(l => l.Order)
            .HasDefaultValue(0);
    }
} 