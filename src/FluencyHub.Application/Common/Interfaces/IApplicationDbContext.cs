using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FluencyHub.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<FluencyHub.Domain.StudentManagement.CourseProgress> CourseProgresses { get; }
        DbSet<FluencyHub.Domain.StudentManagement.CompletedLesson> CompletedLessons { get; }
        DbSet<FluencyHub.Domain.StudentManagement.LearningHistory> LearningHistories { get; }
        DbSet<FluencyHub.Domain.ContentManagement.Course> Courses { get; }
        DbSet<FluencyHub.Domain.ContentManagement.Lesson> Lessons { get; }
        
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
} 