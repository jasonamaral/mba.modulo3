using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;

public class CourseRepository : 
    FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository,
    FluencyHub.ContentManagement.Domain.ICourseRepository
{
    private readonly ContentDbContext _dbContext;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;
    
    public CourseRepository(ContentDbContext dbContext, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }
    
    public async Task<Course> GetByIdAsync(Guid id)
    {
        var course = await _dbContext.Courses
            .Include(c => c.Content)
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), id);
        }
        
        return course;
    }
    
    async Task<Course?> FluencyHub.ContentManagement.Domain.ICourseRepository.GetByIdAsync(Guid id)
    {
        return await _dbContext.Courses
            .Include(c => c.Content)
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Course> GetByIdWithLessonsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _dbContext.Courses
            .Include(c => c.Content)
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), id);
        }
        
        return course;
    }
    
    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _dbContext.Courses
            .Include(c => c.Content)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
    {
        return await _dbContext.Courses
            .Include(c => c.Content)
            .Where(c => c.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Course>> GetByLanguageAsync(string language)
    {
        return await _dbContext.Courses
            .Include(c => c.Content)
            .Where(c => c.Content.Language == language)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Course>> GetByLevelAsync(string level)
    {
        return await _dbContext.Courses
            .Include(c => c.Content)
            .Where(c => c.Content.Level == level)
            .ToListAsync();
    }
    
    public async Task<Course> AddAsync(Course course)
    {
        await _dbContext.Courses.AddAsync(course);
        await _dbContext.SaveChangesAsync();
        return course;
    }
    
    async Task FluencyHub.ContentManagement.Domain.ICourseRepository.AddAsync(Course course)
    {
        await _dbContext.Courses.AddAsync(course);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Course course)
    {
        _dbContext.Courses.Update(course);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var course = await _dbContext.Courses.FindAsync(id);
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), id);
        }
        
        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Lesson> GetLessonByIdAsync(Guid lessonId)
    {
        var lesson = await _dbContext.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);
            
        if (lesson == null)
        {
            throw new NotFoundException(nameof(Lesson), lessonId);
        }
        
        return lesson;
    }
    
    public async Task<int> GetLessonsCountByCourseId(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons
            .CountAsync(l => l.CourseId == courseId, cancellationToken);
    }
    
    public async Task<IEnumerable<CourseProgressInfo>> GetCourseProgressesForStudent(Guid studentId, CancellationToken cancellationToken = default)
    {
        // Como estamos desacoplando os contextos, este método retornará uma lista vazia
        // Em uma implementação real, precisaríamos usar eventos para manter essa informação sincronizada
        return new List<CourseProgressInfo>();
    }
    
    public async Task<bool> ExistsAsync(Guid courseId)
    {
        return await _dbContext.Courses.AnyAsync(c => c.Id == courseId);
    }
    
    public async Task<string> GetNameAsync(Guid courseId)
    {
        var course = await _dbContext.Courses.FindAsync(courseId);
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), courseId);
        }
        
        return course.Name;
    }

    async Task<bool> FluencyHub.ContentManagement.Domain.ICourseRepository.DeleteAsync(Guid id)
    {
        try
        {
            await DeleteAsync(id);
            return true;
        }
        catch
        {
            return false;
        }
    }
} 