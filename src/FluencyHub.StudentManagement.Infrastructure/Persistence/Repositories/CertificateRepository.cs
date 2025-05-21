using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly StudentDbContext _dbContext;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;

    public CertificateRepository(StudentDbContext dbContext, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }

    public async Task<Certificate?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Certificates
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Certificate>> GetByStudentIdAsync(Guid studentId)
    {
        return await _dbContext.Certificates
            .Include(c => c.Student)
            .Where(c => c.StudentId == studentId)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Certificate>> GetByCourseIdAsync(Guid courseId)
    {
        return await _dbContext.Certificates
            .Include(c => c.Student)
            .Where(c => c.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<Certificate?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _dbContext.Certificates
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);
    }

    public async Task<IEnumerable<Certificate>> GetAllAsync()
    {
        return await _dbContext.Certificates
            .Include(c => c.Student)
            .ToListAsync();
    }

    public async Task AddAsync(Certificate certificate)
    {
        await _dbContext.Certificates.AddAsync(certificate);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Certificate certificate)
    {
        _dbContext.Certificates.Update(certificate);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var certificate = await _dbContext.Certificates.FindAsync(id);
        if (certificate == null)
        {
            throw new NotFoundException(nameof(Certificate), id);
        }

        _dbContext.Certificates.Remove(certificate);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 