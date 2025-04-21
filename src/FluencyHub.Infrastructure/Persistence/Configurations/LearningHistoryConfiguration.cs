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
            
        // Configure LearningRecord as a value object
        builder.OwnsMany(lh => lh.Records, recordBuilder =>
        {
            recordBuilder.ToTable("LearningRecords");
            
            recordBuilder.WithOwner().HasForeignKey("LearningHistoryId");
            
            recordBuilder.Property<int>("Id").ValueGeneratedOnAdd();
            recordBuilder.HasKey("Id");
            
            recordBuilder.Property(lr => lr.LessonId)
                .IsRequired();
                
            recordBuilder.Property(lr => lr.CompletedAt)
                .IsRequired();
                
            recordBuilder.Property(lr => lr.Grade);
        });
    }
} 