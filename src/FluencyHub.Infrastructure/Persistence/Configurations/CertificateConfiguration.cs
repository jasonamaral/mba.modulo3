using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluencyHub.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.CourseId)
            .IsRequired();
            
        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(c => c.IssueDate)
            .IsRequired();
    }
} 