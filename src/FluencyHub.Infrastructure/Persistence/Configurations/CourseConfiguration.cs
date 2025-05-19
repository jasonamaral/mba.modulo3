using FluencyHub.ContentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(c => c.Description)
            .HasMaxLength(2000)
            .IsRequired();
            
        builder.OwnsOne(c => c.Content, contentBuilder =>
        {
            contentBuilder.Property(cc => cc.Syllabus)
                .HasMaxLength(5000)
                .IsRequired();
                
            contentBuilder.Property(cc => cc.LearningObjectives)
                .HasMaxLength(2000)
                .IsRequired();
                
            contentBuilder.Property(cc => cc.PreRequisites)
                .HasMaxLength(1000);
                
            contentBuilder.Property(cc => cc.TargetAudience)
                .HasMaxLength(1000);
                
            contentBuilder.Property(cc => cc.Language)
                .HasMaxLength(50)
                .IsRequired();
                
            contentBuilder.Property(cc => cc.Level)
                .HasMaxLength(50)
                .IsRequired();
        });
        
        builder.HasMany(c => c.Lessons)
            .WithOne()
            .HasForeignKey("CourseId")
            .OnDelete(DeleteBehavior.Cascade);
    }
} 