using MediatR;

namespace FluencyHub.Application.ContentManagement.Commands.CompleteEnrollment;

public record CompleteEnrollmentCommand(Guid EnrollmentId) : IRequest; 