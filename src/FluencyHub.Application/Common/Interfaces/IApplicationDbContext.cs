using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.StudentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace FluencyHub.Application.Common.Interfaces
{
    /// <summary>
    /// Esta interface está obsoleta e será removida em versões futuras.
    /// Cada bounded context deve usar seu próprio contexto de banco de dados específico.
    /// </summary>
    [Obsolete("Esta interface está obsoleta. Cada bounded context deve usar seu próprio contexto de banco de dados específico.")]
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