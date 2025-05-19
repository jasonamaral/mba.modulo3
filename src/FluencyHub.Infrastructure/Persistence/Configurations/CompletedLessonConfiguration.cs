using FluencyHub.StudentManagement.Domain;
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
            
        builder.HasIndex(cl => cl.CourseProgressId);
        
        builder.HasIndex(cl => new { cl.CourseProgressId, cl.LessonId }).IsUnique();
    }
} 