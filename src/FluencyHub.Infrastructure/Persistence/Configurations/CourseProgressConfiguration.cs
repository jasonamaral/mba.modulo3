using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.HasMany(cp => cp.CompletedLessons)
            .WithOne(cl => cl.CourseProgress)
            .HasForeignKey(cl => cl.CourseProgressId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}