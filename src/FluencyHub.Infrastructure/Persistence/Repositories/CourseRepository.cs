using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using Microsoft.EntityFrameworkCore;
using FluencyHub.Application.Common.Exceptions;

namespace FluencyHub.Infrastructure.Persistence.Repositories;

public class CourseRepository : Application.Common.Interfaces.ICourseRepository
{
    private readonly FluencyHubDbContext _context;
    
    public CourseRepository(FluencyHubDbContext context)
    {
        _context = context;
    }
    
    public async Task<Course> GetByIdAsync(Guid id)
    {
        return await _context.Courses
            .Include(c => c.Content)
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Course> GetByIdWithLessonsAsync(Guid id)
    {
        var course = await _context.Courses
            .Include(c => c.Content)
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), id);
        }
        
        return course;
    }
    
    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses
            .Include(c => c.Content)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Content)
            .Where(c => c.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Course>> GetByLanguageAsync(string language)
    {
        return await _context.Courses
            .Include(c => c.Content)
            .Where(c => c.Content.Language == language)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Course>> GetByLevelAsync(string level)
    {
        return await _context.Courses
            .Include(c => c.Content)
            .Where(c => c.Content.Level == level)
            .ToListAsync();
    }
    
    public async Task<Course> AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();
        return course;
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), id);
        }
        
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }

    public async Task<Lesson> GetLessonByIdAsync(Guid lessonId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);
        
        if (lesson == null)
        {
            throw new NotFoundException(nameof(Lesson), lessonId);
        }
        
        return lesson;
    }
} 