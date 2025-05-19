using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.StudentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FluencyHub.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Student> Students { get; }
        DbSet<Enrollment> Enrollments { get; }
        DbSet<Course> Courses { get; }
        DbSet<Lesson> Lessons { get; }
        DbSet<Certificate> Certificates { get; }
        DbSet<Payment> Payments { get; }
        
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 