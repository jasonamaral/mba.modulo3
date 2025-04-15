using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        // Verificar se o estudante existe
        var student = await _mediator.Send(new GetStudentByIdQuery(request.StudentId), cancellationToken);
        if (student == null)
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        // Buscar os progressos do estudante
        var courseProgressList = await _courseRepository.GetCourseProgressesForStudent(request.StudentId, cancellationToken);
        
        // Se não houver progressos, retornar um dicionário vazio
        if (courseProgressList == null || !courseProgressList.Any())
        {
            return new StudentProgressViewModel();
        }

        var viewModel = new StudentProgressViewModel();

        // Preencher o dicionário de progresso
        foreach (var progress in courseProgressList)
        {
            // Obter a contagem de lições completadas
            var completedLessonsCount = await _learningRepository.GetCompletedLessonsCountAsync(
                request.StudentId, progress.CourseId, cancellationToken);

            // Obter o total de lições do curso
            var totalLessons = await _courseRepository.GetLessonsCountByCourseId(progress.CourseId, cancellationToken);

            viewModel.Progress[progress.CourseId] = new CourseProgressDto
            {
                CompletedLessons = completedLessonsCount,
                TotalLessons = totalLessons,
                IsCompleted = progress.IsCompleted,
                LastUpdated = progress.LastUpdated
            };
        }

        return viewModel;
    }
} 