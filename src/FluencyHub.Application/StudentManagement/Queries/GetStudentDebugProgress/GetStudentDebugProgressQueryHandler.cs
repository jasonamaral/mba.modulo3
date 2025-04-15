using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentDebugProgress;

public class GetStudentDebugProgressQueryHandler : IRequestHandler<GetStudentDebugProgressQuery, StudentDebugProgressViewModel>
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _dbContext;

    public GetStudentDebugProgressQueryHandler(
        IMediator mediator,
        IApplicationDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    public async Task<StudentDebugProgressViewModel> Handle(GetStudentDebugProgressQuery request, CancellationToken cancellationToken)
    {
        // Verificar se o estudante existe
        var student = await _mediator.Send(new GetStudentByIdQuery(request.StudentId), cancellationToken);
        if (student == null)
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        var viewModel = new StudentDebugProgressViewModel();

        // Buscar os CourseProgresses do estudante com informações completas
        var courseProgresses = await _dbContext.CourseProgresses
            .AsNoTracking()
            .Where(cp => cp.LearningHistoryId == request.StudentId)
            .Include(cp => cp.CompletedLessons)
            .ToListAsync(cancellationToken);

        foreach (var cp in courseProgresses)
        {
            // Serializar as lições completadas para JSON para depuração
            var completedLessons = cp.CompletedLessons.Select(cl => new
            {
                cl.Id,
                cl.LessonId,
                cl.CompletedAt
            }).ToList();

            var completedLessonsJson = JsonSerializer.Serialize(completedLessons, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            viewModel.DebugInfo.Add(new CourseProgressDebugInfo
            {
                CourseProgressId = cp.Id,
                CourseId = cp.CourseId,
                IsCompleted = cp.IsCompleted,
                LastUpdated = cp.LastUpdated,
                CompletedLessonsRaw = completedLessonsJson
            });
        }

        return viewModel;
    }
}

// Classe auxiliar para criar parâmetros de banco de dados de forma genérica
public class DbParameter
{
    public string ParameterName { get; set; }
    public object Value { get; set; }

    public DbParameter(string parameterName, object value)
    {
        ParameterName = parameterName;
        Value = value;
    }
} 