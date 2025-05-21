using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using FluencyHub.SharedKernel.Events;
using System.Collections.Generic;
using System.Linq;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : IEnrollmentRepository, IStudentManagementEnrollmentRepository
{
    private readonly StudentDbContext _dbContext;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;

    public EnrollmentRepository(StudentDbContext dbContext, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsAsync()
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .Where(e => e.Status == StatusMatricula.Ativa || e.Status == StatusMatricula.AguardandoPagamento)
            .ToListAsync();
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _dbContext.Enrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();
        
        // Não publicar eventos aqui, pois o tipo de evento é incompatível
    }

    public async Task<bool> HasActiveEnrollmentAsync(Guid studentId, Guid courseId)
    {
        return await _dbContext.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId && 
                     (e.Status == StatusMatricula.Ativa || e.Status == StatusMatricula.AguardandoPagamento));
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsWithStudentAndCourseAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Enrollments
            .Include(e => e.Student)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Enrollment enrollment)
    {
        _dbContext.Enrollments.Update(enrollment);
        await _dbContext.SaveChangesAsync();
        
        // Não publicar eventos aqui, pois o tipo de evento é incompatível
    }

    public async Task DeleteAsync(Guid id)
    {
        var enrollment = await _dbContext.Enrollments.FindAsync(id);
        if (enrollment == null)
        {
            throw new NotFoundException(nameof(Enrollment), id);
        }

        _dbContext.Enrollments.Remove(enrollment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 