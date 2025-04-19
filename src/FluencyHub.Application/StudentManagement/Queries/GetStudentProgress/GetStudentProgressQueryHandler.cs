using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentProgress;

public class GetStudentProgressQueryHandler : IRequestHandler<GetStudentProgressQuery, Dictionary<Guid, Dictionary<Guid, bool>>>
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

    public async Task<Dictionary<Guid, Dictionary<Guid, bool>>> Handle(GetStudentProgressQuery request, CancellationToken cancellationToken)
    {
        var student = await _mediator.Send(new GetStudentByIdQuery(request.StudentId), cancellationToken) ?? throw new NotFoundException("Student", request.StudentId);

        var learningHistory = await _learningRepository.GetByStudentIdAsync(request.StudentId, cancellationToken);

        if (learningHistory == null || learningHistory.CourseProgress.Count == 0)
        {
            return [];
        }

        var progressDict = new Dictionary<Guid, Dictionary<Guid, bool>>();

        foreach (var courseProgress in learningHistory.CourseProgress)
        {
            var course = await _courseRepository.GetByIdWithLessonsAsync(courseProgress.CourseId, cancellationToken);
            if (course == null) continue;

            var lessonProgressDict = new Dictionary<Guid, bool>();

            foreach (var lesson in course.Lessons)
            {
                bool isCompleted = courseProgress.CompletedLessons.Any(cl => cl.LessonId == lesson.Id);

                lessonProgressDict[lesson.Id] = isCompleted;
            }

            progressDict[courseProgress.CourseId] = lessonProgressDict;
        }

        return progressDict;
    }
}