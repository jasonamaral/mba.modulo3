using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class CompletedLessonConfiguration : IEntityTypeConfiguration<CompletedLesson>
{
    public void Configure(EntityTypeBuilder<CompletedLesson> builder)
    {
        builder.ToTable("CompletedLessons");
        
        builder.HasKey(cl => cl.Id);
        
        builder.Property(cl => cl.LessonId)
            .IsRequired();
            
        builder.Property(cl => cl.CourseProgressId)
            .IsRequired();
            
        builder.Property(cl => cl.CompletedAt)
            .IsRequired();
            
        // Índice para melhorar a performance de buscas por CourseProgressId
        builder.HasIndex(cl => cl.CourseProgressId);
        
        // Índice para verificar rapidamente se uma lição específica foi concluída
        builder.HasIndex(cl => new { cl.CourseProgressId, cl.LessonId }).IsUnique();
    }
} 