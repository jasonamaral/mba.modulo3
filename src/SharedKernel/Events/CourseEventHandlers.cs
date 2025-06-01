using MediatR;
using FluencyHub.SharedKernel.Queries;

namespace FluencyHub.SharedKernel.Events;

/// <summary>
/// Manipulador para a consulta de existência de cursos
/// </summary>
public class CourseExistsHandler : IRequestHandler<CourseExists, bool>
{
    private readonly IMediator _mediator;

    public CourseExistsHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<bool> Handle(CourseExists request, CancellationToken cancellationToken)
    {
        var query = new GetCourseById { CourseId = request.CourseId };
        var result = await _mediator.Send(query, cancellationToken);

        return result != null;
    }
}

/// <summary>
/// Manipulador para a consulta de nome de cursos
/// </summary>
public class GetCourseNameHandler : IRequestHandler<GetCourseName, string>
{
    private readonly IMediator _mediator;

    public GetCourseNameHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> Handle(GetCourseName request, CancellationToken cancellationToken)
    {
        var query = new GetCourseById { CourseId = request.CourseId };
        var course = await _mediator.Send(query, cancellationToken);

        return course?.Name ?? string.Empty;
    }
} 