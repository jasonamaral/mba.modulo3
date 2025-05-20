using FluencyHub.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

namespace FluencyHub.API.Adapters;

public class CertificateRepositoryAdapter : ICertificateRepository
{
    private readonly CertificateRepository _repository;

    public CertificateRepositoryAdapter(CertificateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Certificate?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Certificate>> GetByStudentIdAsync(Guid studentId)
    {
        return await _repository.GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Certificate>> GetByCourseIdAsync(Guid courseId)
    {
        return await _repository.GetByCourseIdAsync(courseId);
    }

    public async Task<Certificate?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _repository.GetByStudentAndCourseAsync(studentId, courseId);
    }

    public async Task<IEnumerable<Certificate>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task AddAsync(Certificate certificate)
    {
        await _repository.AddAsync(certificate);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _repository.SaveChangesAsync(cancellationToken);
    }
} 