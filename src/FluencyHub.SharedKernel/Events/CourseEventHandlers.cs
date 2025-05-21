using MediatR;
using FluencyHub.SharedKernel.Queries;
using Microsoft.Extensions.Logging;

namespace FluencyHub.SharedKernel.Events;

/// <summary>
/// Manipulador para a consulta de existência de cursos
/// </summary>
public class CourseExistsHandler : IRequestHandler<CourseExists, bool>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CourseExistsHandler> _logger;

    public CourseExistsHandler(IMediator mediator, ILogger<CourseExistsHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<bool> Handle(CourseExists request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verificando existência do curso {CourseId}", request.CourseId);

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
    private readonly ILogger<GetCourseNameHandler> _logger;

    public GetCourseNameHandler(IMediator mediator, ILogger<GetCourseNameHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<string> Handle(GetCourseName request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo nome do curso {CourseId}", request.CourseId);

        var query = new GetCourseById { CourseId = request.CourseId };
        var course = await _mediator.Send(query, cancellationToken);

        return course?.Name ?? string.Empty;
    }
} 