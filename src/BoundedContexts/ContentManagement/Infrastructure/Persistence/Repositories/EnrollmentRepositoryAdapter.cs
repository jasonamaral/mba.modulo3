using System;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.ContentManagement.Application.Commands.CompleteEnrollment;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.SharedKernel.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;

// Implementa a interface do ContentManagement
public class EnrollmentRepositoryAdapter : FluencyHub.ContentManagement.Application.Commands.CompleteEnrollment.IEnrollmentRepository
{
    private readonly IServiceProvider _serviceProvider;

    public EnrollmentRepositoryAdapter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private FluencyHub.SharedKernel.Contracts.IEnrollmentRepository GetRepository()
    {
        // Obter o serviço do SharedKernel de forma lazy para evitar dependências circulares
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<FluencyHub.SharedKernel.Contracts.IEnrollmentRepository>();
    }

    public async Task<EnrollmentInfo?> GetByIdAsync(Guid id)
    {
        var enrollment = await GetRepository().GetByIdAsync(id);
        if (enrollment == null)
            return null;

        // Mapeia da interface compartilhada para o modelo específico do ContentManagement
        return new EnrollmentInfo
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            Status = enrollment.Status,
            CreatedAt = enrollment.EnrollmentDate,
            CompletionDate = enrollment.CompletionDate
        };
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await GetRepository().SaveChangesAsync(cancellationToken);
    }
} 