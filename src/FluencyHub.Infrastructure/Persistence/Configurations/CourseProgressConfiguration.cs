using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Collections.Generic;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class CourseProgressConfiguration : IEntityTypeConfiguration<CourseProgress>
{
    public void Configure(EntityTypeBuilder<CourseProgress> builder)
    {
        builder.ToTable("CourseProgresses");
        
        builder.HasKey(cp => cp.Id);
        
        builder.Property(cp => cp.CourseId)
            .IsRequired();
            
        builder.Property(cp => cp.LearningHistoryId)
            .IsRequired();
            
        builder.Property(cp => cp.IsCompleted)
            .IsRequired();
            
        builder.Property(cp => cp.LastUpdated)
            .IsRequired();
            
        // Configurar a relação com CompletedLessons
        builder.HasMany(cp => cp.CompletedLessons)
            .WithOne(cl => cl.CourseProgress)
            .HasForeignKey(cl => cl.CourseProgressId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 