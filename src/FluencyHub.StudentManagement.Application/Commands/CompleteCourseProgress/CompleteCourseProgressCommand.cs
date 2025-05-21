using FluencyHub.StudentManagement.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands;

public record CompleteCourseProgressCommand(Guid CourseProgressId) : IRequest<Unit>;

public class CompleteCourseProgressCommandHandler : IRequestHandler<CompleteCourseProgressCommand, Unit>
{
    private readonly ILearningRepository _learningRepository;

    public CompleteCourseProgressCommandHandler(ILearningRepository learningRepository)
    {
        _learningRepository = learningRepository;
    }

    public async Task<Unit> Handle(CompleteCourseProgressCommand request, CancellationToken cancellationToken)
    {
        var courseProgress = await _learningRepository.GetCourseProgressByIdAsync(request.CourseProgressId);
        
        if (courseProgress != null)
        {
            courseProgress.CompleteCourse();
            await _learningRepository.SaveChangesAsync(cancellationToken);
        }
        
        return Unit.Value;
    }
} 