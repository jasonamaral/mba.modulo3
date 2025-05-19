using FluencyHub.StudentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.DateOfBirth)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20);

        builder.Ignore(s => s.LearningHistory);

        builder.HasMany(s => s.Enrollments)
            .WithOne()
            .HasForeignKey("StudentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Certificates)
            .WithOne()
            .HasForeignKey("StudentId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}