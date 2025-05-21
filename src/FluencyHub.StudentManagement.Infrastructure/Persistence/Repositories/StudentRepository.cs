using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class StudentRepository : FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository
{
    private readonly StudentDbContext _dbContext;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;

    public StudentRepository(StudentDbContext dbContext, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Students
            .Include(s => s.LearningHistory)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        return await _dbContext.Students
            .Include(s => s.LearningHistory)
            .FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<IEnumerable<Student>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Students.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }
        
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
    {
        return await _dbContext.Students
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByStudentIdAsync(Guid studentId)
    {
        return await _dbContext.Certificates
            .Include(c => c.Student)
            .Where(c => c.StudentId == studentId)
            .ToListAsync();
    }

    public async Task AddAsync(Student student)
    {
        await _dbContext.Students.AddAsync(student);
    }

    public async Task UpdateAsync(Student student)
    {
        _dbContext.Students.Update(student);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student == null)
            return false;

        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 