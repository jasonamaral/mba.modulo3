using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FluencyHub.PaymentProcessing.Infrastructure.Persistence.Repositories;

public class EnrollmentRepositoryAdapter : FluencyHub.PaymentProcessing.Application.Common.Interfaces.IEnrollmentRepository
{
    private readonly IServiceProvider _serviceProvider;

    public EnrollmentRepositoryAdapter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private IStudentManagementEnrollmentRepository GetRepository()
    {
        // Obter o serviço do StudentManagement de forma lazy para evitar dependências circulares
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IStudentManagementEnrollmentRepository>();
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await GetRepository().GetByIdAsync(id);
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
    {
        return await GetRepository().GetByCourseIdAsync(courseId);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
    {
        return await GetRepository().GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await GetRepository().GetAllAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsAsync()
    {
        return await GetRepository().GetActiveEnrollmentsAsync();
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await GetRepository().GetByStudentAndCourseAsync(studentId, courseId);
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await GetRepository().AddAsync(enrollment);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await GetRepository().SaveChangesAsync(cancellationToken);
    }
} 