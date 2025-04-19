using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            
        builder.HasMany(lh => lh.CourseProgress)
            .WithOne()
            .HasForeignKey("LearningHistoryId")
            .OnDelete(DeleteBehavior.Cascade);
    }
} 