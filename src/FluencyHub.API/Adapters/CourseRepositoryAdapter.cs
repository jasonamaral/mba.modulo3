using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Models;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.API.Adapters;

public class CourseRepositoryAdapter : FluencyHub.Application.Common.Interfaces.ICourseRepository
{
    private readonly CourseRepository _repository;

    public CourseRepositoryAdapter(CourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Course> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
    
    public async Task<Course> GetByIdWithLessonsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdWithLessonsAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
    
    public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
    {
        return await _repository.GetActiveCoursesAsync();
    }
    
    public async Task<IEnumerable<Course>> GetByLanguageAsync(string language)
    {
        return await _repository.GetByLanguageAsync(language);
    }
    
    public async Task<IEnumerable<Course>> GetByLevelAsync(string level)
    {
        return await _repository.GetByLevelAsync(level);
    }

    public async Task<Course> AddAsync(Course course)
    {
        return await _repository.AddAsync(course);
    }

    public Task UpdateAsync(Course course)
    {
        return _repository.UpdateAsync(course);
    }

    public Task DeleteAsync(Guid id)
    {
        return _repository.DeleteAsync(id);
    }

    public async Task<Lesson> GetLessonByIdAsync(Guid lessonId)
    {
        return await _repository.GetLessonByIdAsync(lessonId);
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _repository.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<FluencyHub.Application.Common.Interfaces.CourseProgressInfo>> GetCourseProgressesForStudent(Guid studentId, CancellationToken cancellationToken = default)
    {
        var progresses = await _repository.GetCourseProgressesForStudent(studentId, cancellationToken);
        return progresses.Select(p => new FluencyHub.Application.Common.Interfaces.CourseProgressInfo
        {
            CourseId = p.CourseId,
            IsCompleted = p.IsCompleted,
            LastUpdated = p.LastUpdated
        }).ToList();
    }
    
    public async Task<int> GetLessonsCountByCourseId(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetLessonsCountByCourseId(courseId, cancellationToken);
    }
} 