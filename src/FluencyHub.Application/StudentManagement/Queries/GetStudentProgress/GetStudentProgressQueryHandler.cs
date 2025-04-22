using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentProgress;

public class GetStudentProgressQueryHandler : IRequestHandler<GetStudentProgressQuery, StudentProgressViewModel>
{
    private readonly IMediator _mediator;
    private readonly ILearningRepository _learningRepository;
    private readonly ICourseRepository _courseRepository;

    public GetStudentProgressQueryHandler(
        IMediator mediator,
        ILearningRepository learningRepository,
        ICourseRepository courseRepository)
    {
        _mediator = mediator;
        _learningRepository = learningRepository;
        _courseRepository = courseRepository;
    }

    public async Task<StudentProgressViewModel> Handle(GetStudentProgressQuery request, CancellationToken cancellationToken)
    {
        var student = await _mediator.Send(new GetStudentByIdQuery(request.StudentId), cancellationToken) ?? throw new NotFoundException("Student", request.StudentId);

        var learningHistory = await _learningRepository.GetByStudentIdAsync(request.StudentId, cancellationToken);

        var viewModel = new StudentProgressViewModel
        {
            Progress = new Dictionary<Guid, CourseProgressDto>()
        };

        if (learningHistory == null || learningHistory.CourseProgress.Count == 0)
        {
            return viewModel;
        }

        foreach (var courseProgress in learningHistory.CourseProgress)
        {
            var course = await _courseRepository.GetByIdWithLessonsAsync(courseProgress.CourseId, cancellationToken);
            if (course == null) continue;

            int totalLessons = course.Lessons.Count;
            int completedLessons = courseProgress.CompletedLessons.Count;
            bool isCompleted = courseProgress.IsCompleted;

            viewModel.Progress[courseProgress.CourseId] = new CourseProgressDto
            {
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons,
                IsCompleted = isCompleted,
                LastUpdated = courseProgress.LastUpdated
            };
        }

        return viewModel;
    }
}