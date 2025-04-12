using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class LearningHistoryConfiguration : IEntityTypeConfiguration<LearningHistory>
{
    public void Configure(EntityTypeBuilder<LearningHistory> builder)
    {
        builder.ToTable("LearningHistories");
        
        builder.HasKey(lh => lh.Id);
        
        builder.Property(lh => lh.Id)
            .ValueGeneratedNever();
            
        builder.Property(lh => lh.CreatedAt)
            .IsRequired();
            
        builder.Property(lh => lh.UpdatedAt);
            
        // Configurar a propriedade CourseProgress como JSON
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        builder.Property(lh => lh.CourseProgress)
            .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<Dictionary<Guid, CourseProgress>>(v, options)
            );
    }
} 