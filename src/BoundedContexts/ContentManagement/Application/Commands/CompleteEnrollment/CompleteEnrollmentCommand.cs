using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.ContentManagement.Application.Commands.CompleteEnrollment;

public record CompleteEnrollmentCommand(Guid EnrollmentId) : IRequest<bool>;

public class CompleteEnrollmentCommandHandler : IRequestHandler<CompleteEnrollmentCommand, bool>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public CompleteEnrollmentCommandHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<bool> Handle(CompleteEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId);
        
        if (enrollment == null)
            throw new NotFoundException($"Enrollment with ID {request.EnrollmentId} not found");

        enrollment.Status = "Completed";
        enrollment.CompletionDate = DateTime.UtcNow;
        
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

public interface IEnrollmentRepository
{
    Task<EnrollmentInfo?> GetByIdAsync(Guid id);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class EnrollmentInfo
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletionDate { get; set; }
} 