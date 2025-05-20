using FluencyHub.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

namespace FluencyHub.API.Adapters;

public class LearningRepositoryAdapter : ILearningRepository
{
    private readonly LearningRepository _repository;

    public LearningRepositoryAdapter(LearningRepository repository)
    {
        _repository = repository;
    }

    public async Task<LearningHistory?> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByStudentIdAsync(studentId, cancellationToken);
    }

    public async Task<CourseProgress?> GetCourseProgressAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetCourseProgressAsync(studentId, courseId, cancellationToken);
    }

    public async Task<bool> HasCompletedLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _repository.HasCompletedLessonAsync(studentId, lessonId, cancellationToken);
    }

    public async Task<int> GetCompletedLessonsCountAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetCompletedLessonsCountAsync(studentId, courseId, cancellationToken);
    }

    public async Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetCompletedLessonIdsAsync(studentId, courseId, cancellationToken);
    }

    public async Task AddAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(learningHistory, cancellationToken);
    }

    public async Task UpdateAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(learningHistory, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        await _repository.CompleteLessonAsync(studentId, courseId, lessonId, cancellationToken);
    }

    public async Task UncompleteLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        await _repository.UncompleteLessonAsync(studentId, lessonId, cancellationToken);
    }
} 