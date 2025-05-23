using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using FluencyHub.SharedKernel.Events;
using FluencyHub.SharedKernel.Domain;
using FluencyHub.StudentManagement.Domain.Common;
using IApplicationStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IDomainStudentRepository = FluencyHub.StudentManagement.Domain.IStudentRepository;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class StudentRepository : IApplicationStudentRepository, IDomainStudentRepository
{
    private readonly StudentDbContext _context;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;

    public StudentRepository(StudentDbContext context, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _context = context;
        _eventService = eventService;
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
            .Include(s => s.LearningHistory)
            .Include(s => s.Enrollments)
            .Include(s => s.Certificates)
            .FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<IEnumerable<Student>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Students.AsQueryable();

        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        return await query
            .Include(s => s.Enrollments)
            .Include(s => s.Certificates)
            .ToListAsync(cancellationToken);
    }

    async Task<IEnumerable<Student>> IDomainStudentRepository.GetAllAsync()
    {
        return await GetAllAsync();
    }

    public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
    {
        return await _context.Students
            .Where(s => s.IsActive)
            .Include(s => s.Enrollments)
            .Include(s => s.Certificates)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByStudentIdAsync(Guid studentId)
    {
        return await _context.Certificates
            .Where(c => c.StudentId == studentId)
            .ToListAsync();
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
    }

    async Task IDomainStudentRepository.UpdateAsync(Student student)
    {
        if (student == null) throw new ArgumentNullException(nameof(student));
        _context.Entry(student).State = EntityState.Modified;
        await SaveChangesAsync();
    }

    async Task<bool> IDomainStudentRepository.DeleteAsync(Guid id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return false;

        _context.Students.Remove(student);
        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Students.AnyAsync(s => s.Id == id);
    }

    public async Task<bool> ExistsEmailAsync(string email)
    {
        return await _context.Students.AnyAsync(s => s.Email == email);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var events = _context.ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var @event in events)
        {
            await _eventService.PublishAsync(@event);
        }
    }
} 