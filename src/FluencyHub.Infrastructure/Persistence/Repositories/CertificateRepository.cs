using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Infrastructure.Persistence.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly FluencyHubDbContext _context;

    public CertificateRepository(FluencyHubDbContext context)
    {
        _context = context;
    }

    public async Task<Certificate?> GetByIdAsync(Guid id)
    {
        return await _context.Certificates
            .Include(c => c.Student)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Certificate>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.Certificates
            .Include(c => c.Course)
            .Where(c => c.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certificate>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.Certificates
            .Include(c => c.Student)
            .Where(c => c.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<Certificate?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _context.Certificates
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);
    }

    public async Task<IEnumerable<Certificate>> GetAllAsync()
    {
        return await _context.Certificates
            .Include(c => c.Student)
            .Include(c => c.Course)
            .ToListAsync();
    }

    public async Task AddAsync(Certificate certificate)
    {
        await _context.Certificates.AddAsync(certificate);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}