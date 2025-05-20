using FluencyHub.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using FluencyHub.StudentManagement.Application.Common.Interfaces;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class StudentRepository : FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository
{
    private readonly FluencyHubDbContext _context;

    public StudentRepository(FluencyHubDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _context.Students
            .Include(s => s.Enrollments)
            .Include(s => s.Certificates)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        return await _context.Students
            .Include(s => s.Enrollments)
            .Include(s => s.Certificates)
            .FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<IEnumerable<Student>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Students.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
    {
        return await _context.Students
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByStudentIdAsync(Guid studentId)
    {
        return await _context.Certificates
            .Include(c => c.Course)
            .Where(c => c.StudentId == studentId)
            .ToListAsync();
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }
} 