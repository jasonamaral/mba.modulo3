using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries;

public record GetCourseProgressQuery(Guid CourseId, Guid LearningHistoryId) : IRequest<CourseProgress>;

public class GetCourseProgressQueryHandler : IRequestHandler<GetCourseProgressQuery, CourseProgress>
{
    private readonly ILearningRepository _learningRepository;

    public GetCourseProgressQueryHandler(ILearningRepository learningRepository)
    {
        _learningRepository = learningRepository;
    }

    public async Task<CourseProgress> Handle(GetCourseProgressQuery request, CancellationToken cancellationToken)
    {
        return await _learningRepository.GetCourseProgressAsync(request.CourseId, request.LearningHistoryId);
    }
} 