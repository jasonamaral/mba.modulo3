using FluencyHub.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

namespace FluencyHub.API.Adapters;

public class EnrollmentRepositoryAdapter : IEnrollmentRepository
{
    private readonly EnrollmentRepository _repository;

    public EnrollmentRepositoryAdapter(EnrollmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
    
    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _repository.GetByStudentAndCourseAsync(studentId, courseId);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _repository.GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
    {
        return await _repository.GetByCourseIdAsync(courseId);
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsAsync()
    {
        return await _repository.GetActiveEnrollmentsAsync();
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _repository.AddAsync(enrollment);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _repository.SaveChangesAsync(cancellationToken);
    }
} 