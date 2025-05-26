using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using FluencyHub.StudentManagement.Infrastructure.Exceptions;
using FluencyHub.SharedKernel.Events;
using FluencyHub.SharedKernel.Domain;
using FluencyHub.StudentManagement.Domain.Common;
using IDomainEventService = FluencyHub.SharedKernel.Events.IDomainEventService;
using IStudentManagementEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentManagementEnrollmentRepository;
using IApplicationEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IEnrollmentRepository;
using ISharedEnrollmentRepository = FluencyHub.SharedKernel.Contracts.IEnrollmentRepository;
using ISharedEnrollment = FluencyHub.SharedKernel.Contracts.IEnrollment;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : IStudentManagementEnrollmentRepository, IApplicationEnrollmentRepository, ISharedEnrollmentRepository
{
    private readonly StudentDbContext _context;
    private readonly IDomainEventService _eventService;

    public EnrollmentRepository(StudentDbContext context, IDomainEventService eventService)
    {
        _context = context;
        _eventService = eventService;
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
    }

    public void Update(Enrollment enrollment)
    {
        _context.Entry(enrollment).State = EntityState.Modified;
    }

    public void Delete(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
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

    public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsAsync()
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.Status == StatusMatricula.Ativa || e.Status == StatusMatricula.AguardandoPagamento)
            .ToListAsync();
    }

    public async Task<bool> HasActiveEnrollmentAsync(Guid studentId, Guid courseId)
    {
        return await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId && 
                     (e.Status == StatusMatricula.Ativa || e.Status == StatusMatricula.AguardandoPagamento));
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsWithStudentAndCourseAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null)
        {
            throw new NotFoundException(nameof(Enrollment), id);
        }

        _context.Enrollments.Remove(enrollment);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await SaveChangesAsync();
    }

    // Implementação da interface IEnrollmentRepository do SharedKernel
    async Task<ISharedEnrollment?> ISharedEnrollmentRepository.GetByIdAsync(Guid id)
    {
        return await GetByIdAsync(id);
    }

    async Task<ISharedEnrollment?> ISharedEnrollmentRepository.GetByPaymentIdAsync(Guid paymentId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.PaymentId == paymentId);
    }

    async Task ISharedEnrollmentRepository.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }
} 