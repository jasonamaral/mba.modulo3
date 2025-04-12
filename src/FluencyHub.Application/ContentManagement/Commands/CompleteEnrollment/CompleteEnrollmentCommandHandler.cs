using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Application.ContentManagement.Commands.CompleteEnrollment;

public class CompleteEnrollmentCommandHandler : IRequestHandler<CompleteEnrollmentCommand>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public CompleteEnrollmentCommandHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task Handle(CompleteEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId);

        if (enrollment == null)
        {
            throw new NotFoundException("Matrícula não encontrada");
        }

        enrollment.CompleteEnrollment();
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
    }
} 