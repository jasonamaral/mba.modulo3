using FluencyHub.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;

namespace FluencyHub.API.Adapters;

public class LessonRepositoryAdapter : ILessonRepository
{
    private readonly LessonRepository _repository;

    public LessonRepositoryAdapter(LessonRepository repository)
    {
        _repository = repository;
    }

    public async Task<Lesson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByCourseIdAsync(courseId, cancellationToken);
    }

    public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(lesson, cancellationToken);
    }

    public Task UpdateAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        return _repository.UpdateAsync(lesson, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _repository.SaveChangesAsync(cancellationToken);
    }
} 