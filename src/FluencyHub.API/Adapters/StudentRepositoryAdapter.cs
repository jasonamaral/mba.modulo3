using FluencyHub.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

namespace FluencyHub.API.Adapters;

public class StudentRepositoryAdapter : FluencyHub.Application.Common.Interfaces.IStudentRepository
{
    private readonly StudentRepository _repository;

    public StudentRepositoryAdapter(StudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Student>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(includeInactive, cancellationToken);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        return await _repository.GetByEmailAsync(email);
    }
    
    public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
    {
        return await _repository.GetActiveStudentsAsync();
    }
    
    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
    {
        return await _repository.GetEnrollmentsByStudentIdAsync(studentId);
    }
    
    public async Task<IEnumerable<Certificate>> GetCertificatesByStudentIdAsync(Guid studentId)
    {
        return await _repository.GetCertificatesByStudentIdAsync(studentId);
    }

    public async Task AddAsync(Student student)
    {
        await _repository.AddAsync(student);
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Student student)
    {
        return _repository.UpdateAsync(student);
    }

    public Task DeleteAsync(Guid id)
    {
        return _repository.DeleteAsync(id);
    }
} 