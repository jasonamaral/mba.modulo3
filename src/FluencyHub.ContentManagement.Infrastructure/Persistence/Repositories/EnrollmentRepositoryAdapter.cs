using System;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.ContentManagement.Application.Commands.CompleteEnrollment;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;

public class EnrollmentRepositoryAdapter : FluencyHub.ContentManagement.Application.Commands.CompleteEnrollment.IEnrollmentRepository
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

    public async Task<EnrollmentInfo?> GetByIdAsync(Guid id)
    {
        var enrollment = await GetRepository().GetByIdAsync(id);
        if (enrollment == null)
            return null;

        return new EnrollmentInfo
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            Status = enrollment.Status.ToString(),
            CreatedAt = enrollment.CreatedAt,
            CompletionDate = enrollment.CompletionDate
        };
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await GetRepository().SaveChangesAsync(cancellationToken);
    }
} 