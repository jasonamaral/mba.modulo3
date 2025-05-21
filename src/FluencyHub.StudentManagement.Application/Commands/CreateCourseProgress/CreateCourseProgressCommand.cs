using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands;

public record CreateCourseProgressCommand(CourseProgress CourseProgress) : IRequest<Unit>;

public class CreateCourseProgressCommandHandler : IRequestHandler<CreateCourseProgressCommand, Unit>
{
    private readonly ILearningRepository _learningRepository;

    public CreateCourseProgressCommandHandler(ILearningRepository learningRepository)
    {
        _learningRepository = learningRepository;
    }

    public async Task<Unit> Handle(CreateCourseProgressCommand request, CancellationToken cancellationToken)
    {
        await _learningRepository.AddCourseProgressAsync(request.CourseProgress);
        await _learningRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
} 