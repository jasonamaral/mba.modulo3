using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly ContentDbContext _dbContext;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;

    public LessonRepository(ContentDbContext dbContext, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }

    public async Task<Lesson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        await _dbContext.Lessons.AddAsync(lesson, cancellationToken);
    }

    public Task UpdateAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        _dbContext.Lessons.Update(lesson);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var lesson = await _dbContext.Lessons.FindAsync(id);
        if (lesson == null)
        {
            throw new NotFoundException(nameof(Lesson), id);
        }

        _dbContext.Lessons.Remove(lesson);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 