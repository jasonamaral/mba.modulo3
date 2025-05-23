using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly StudentDbContext _context;

    public CertificateRepository(StudentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Certificate>> GetAllAsync()
    {
        return await _context.Certificates
            .Include(c => c.Student)
            .ToListAsync();
    }

    public async Task<Certificate?> GetByIdAsync(Guid id)
    {
        return await _context.Certificates
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Certificate>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.Certificates
            .Include(c => c.Student)
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
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);
    }

    public async Task AddAsync(Certificate certificate)
    {
        await _context.Certificates.AddAsync(certificate);
    }

    public void Update(Certificate certificate)
    {
        _context.Entry(certificate).State = EntityState.Modified;
    }

    public void Delete(Certificate certificate)
    {
        _context.Certificates.Remove(certificate);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
} 