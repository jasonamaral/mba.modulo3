using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.SharedKernel.Queries;
using Microsoft.Extensions.Logging;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IEnrollmentRepository;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;

public class GetStudentProgressQueryHandler : IRequestHandler<GetStudentProgressQuery, StudentProgressViewModel>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILearningRepository _learningRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<GetStudentProgressQueryHandler> _logger;

    public GetStudentProgressQueryHandler(
        IStudentRepository studentRepository,
        IEnrollmentRepository enrollmentRepository,
        ILearningRepository learningRepository,
        IMediator mediator,
        ILogger<GetStudentProgressQueryHandler> logger)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
        _learningRepository = learningRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<StudentProgressViewModel> Handle(GetStudentProgressQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting progress for student {StudentId}", request.StudentId);

        // Verificar se o estudante existe
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new NotFoundException($"Student with ID {request.StudentId} not found");
        }

        // Obter todas as matrículas do estudante
        var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.StudentId);
        
        // Se não há matrículas, retornar progresso vazio
        if (!enrollments.Any())
        {
            _logger.LogInformation("No enrollments found for student {StudentId}", request.StudentId);
            return new StudentProgressViewModel
            {
                StudentId = request.StudentId,
                StudentName = student.FullName,
                CourseId = Guid.Empty,
                CourseName = "Nenhum curso matriculado",
                TotalLessons = 0,
                CompletedLessons = 0,
                ProgressPercentage = 0,
                EnrollmentDate = DateTime.MinValue,
                LastActivityDate = null,
                IsCompleted = false,
                CompletionDate = null,
                LessonProgress = new List<LessonProgressDto>()
            };
        }

        // Para simplificar, vamos pegar a primeira matrícula ativa
        // Em uma implementação mais completa, isso poderia retornar uma lista de progressos
        var activeEnrollment = enrollments.FirstOrDefault(e => e.IsActive) ?? enrollments.First();

        // Obter informações do curso usando queries compartilhadas
        var courseInfo = await _mediator.Send(new GetCourseById { CourseId = activeEnrollment.CourseId }, cancellationToken);
        if (courseInfo == null)
        {
            _logger.LogWarning("Course {CourseId} not found for student {StudentId}", activeEnrollment.CourseId, request.StudentId);
            throw new NotFoundException($"Course with ID {activeEnrollment.CourseId} not found");
        }

        // Obter progresso do curso
        var courseProgresses = await _learningRepository.GetCourseProgressesByStudentIdAsync(request.StudentId);
        var courseProgress = courseProgresses.FirstOrDefault(cp => cp.CourseId == activeEnrollment.CourseId);

        // Obter total de lições do curso
        var totalLessons = await GetTotalLessonsForCourse(activeEnrollment.CourseId, cancellationToken);
        
        var completedLessons = courseProgress?.GetCompletedLessonsCount() ?? 0;
        var progressPercentage = totalLessons > 0 ? (int)Math.Round((double)completedLessons / totalLessons * 100) : 0;

        // Obter detalhes das lições (simplificado)
        var lessonProgress = await GetLessonProgressDetails(activeEnrollment.CourseId, courseProgress, cancellationToken);

        var result = new StudentProgressViewModel
        {
            StudentId = request.StudentId,
            StudentName = student.FullName,
            CourseId = activeEnrollment.CourseId,
            CourseName = courseInfo.Name,
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            ProgressPercentage = progressPercentage,
            EnrollmentDate = activeEnrollment.EnrollmentDate,
            LastActivityDate = courseProgress?.LastUpdated,
            IsCompleted = courseProgress?.IsCompleted ?? false,
            CompletionDate = activeEnrollment.CompletionDate,
            LessonProgress = lessonProgress
        };

        _logger.LogInformation("Retrieved progress for student {StudentId}: {CompletedLessons}/{TotalLessons} lessons completed ({ProgressPercentage}%)",
            request.StudentId, completedLessons, totalLessons, progressPercentage);

        return result;
    }

    private Task<int> GetTotalLessonsForCourse(Guid courseId, CancellationToken cancellationToken)
    {
        try
        {
            // Usar query compartilhada para obter informações do curso
            // Por enquanto, vamos retornar um valor padrão
            // Em uma implementação completa, isso seria obtido do contexto de ContentManagement
            return Task.FromResult(10); // Valor padrão para demonstração
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get total lessons for course {CourseId}, using default value", courseId);
            return Task.FromResult(10); // Valor padrão em caso de erro
        }
    }

    private Task<List<LessonProgressDto>> GetLessonProgressDetails(Guid courseId, Domain.CourseProgress? courseProgress, CancellationToken cancellationToken)
    {
        try
        {
            var lessonProgress = new List<LessonProgressDto>();

            if (courseProgress != null)
            {
                var completedLessons = courseProgress.CompletedLessons.ToList();
                
                for (int i = 0; i < completedLessons.Count; i++)
                {
                    var lesson = completedLessons[i];
                    lessonProgress.Add(new LessonProgressDto
                    {
                        LessonId = lesson.LessonId,
                        Title = $"Lição {i + 1}", // Em uma implementação completa, isso viria do ContentManagement
                        Order = i + 1,
                        IsCompleted = true,
                        CompletionDate = lesson.CompletedAt,
                        Score = null // Score não está disponível na entidade CompletedLesson
                    });
                }
            }

            return Task.FromResult(lessonProgress);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get lesson progress details for course {CourseId}", courseId);
            return Task.FromResult(new List<LessonProgressDto>());
        }
    }
} 