using FluencyHub.ContentManagement.Domain;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluencyHub.ContentManagement.Application.Common.Interfaces;

namespace FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;

public class LessonRepository : FluencyHub.ContentManagement.Application.Common.Interfaces.ILessonRepository
{
    private readonly FluencyHubDbContext _context;

    public LessonRepository(FluencyHubDbContext context)
    {
        _context = context;
    }

    public async Task<Lesson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        await _context.Lessons.AddAsync(lesson, cancellationToken);
    }

    public Task UpdateAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        _context.Lessons.Update(lesson);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
} 