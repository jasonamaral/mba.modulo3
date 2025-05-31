using FluencyHub.API.Common.Exceptions;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.StudentManagement.Application.Commands.ActivateStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;
using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluencyHub.StudentManagement.Application.Commands.DeactivateStudent;
using FluencyHub.StudentManagement.Application.Commands.UpdateStudent;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Queries.GetAllStudents;
using FluencyHub.StudentManagement.Application.Queries.GetStudentByEmail;
using FluencyHub.StudentManagement.Application.Queries.GetStudentById;
using FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;
using FluencyHub.StudentManagement.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EstudantesController : Controller
{

    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentRepository _studentRepository;

    // Construtor simplificado para teste
    public EstudantesController(IMediator mediator, UserManager<ApplicationUser> userManager, IStudentRepository studentRepository)
    {
        _mediator = mediator;
        _userManager = userManager;
        _studentRepository = studentRepository; // Testando esta dependência
    }

    /// <summary>
    /// Obter todos os estudantes
    /// </summary>
    /// <returns>Lista de todos os estudantes</returns>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Obter todos os estudantes",
        Description = "Recupera uma lista de todos os estudantes no sistema. Requer função de Administrador.",
        OperationId = "GetAllStudents"
    )]
    [ProducesResponseType(typeof(IEnumerable<FluencyHub.StudentManagement.Application.Queries.GetAllStudents.StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentListDtoExample))]
    public async Task<IActionResult> GetAllStudents()
    {
        try
        {
            var query = new GetAllStudentsQuery();
            var students = await _mediator.Send(query);
            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Obter o estudante atualmente autenticado
    /// </summary>
    /// <returns>Detalhes do estudante atual</returns>
    /// <response code="200">Retorna os detalhes do estudante atual</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="400">Se o e-mail não for encontrado no token</response>
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Obter estudante atual",
        Description = "Recupera os detalhes do estudante atualmente autenticado com base no token JWT",
        OperationId = "GetCurrentStudent"
    )]
    [ProducesResponseType(typeof(FluencyHub.StudentManagement.Application.Queries.GetStudentById.StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentDtoExample))]
    public async Task<IActionResult> GetCurrentStudent()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        Console.WriteLine($"Student/me endpoint called. Email from token: {userEmail}");

        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
        }

        if (string.IsNullOrEmpty(userEmail))
        {
            Console.WriteLine("Email not found in token");
            return BadRequest(new { error = "E-mail não encontrado no token" });
        }

        try
        {
            Console.WriteLine($"Looking for student with email: {userEmail}");
            var student = await _mediator.Send(new GetStudentByEmailQuery(userEmail));
            Console.WriteLine($"Student found: {student.Id}");
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Student not found: {ex.Message}");
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Obter um estudante pelo ID
    /// </summary>
    /// <param name="id">ID do estudante a ser recuperado</param>
    /// <returns>Detalhes do estudante</returns>
    /// <response code="200">Retorna os detalhes do estudante</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Obter estudante por ID",
        Description = "Recupera um estudante específico pelo seu identificador único. Requer função de Administrador.",
        OperationId = "GetStudentById"
    )]
    [ProducesResponseType(typeof(FluencyHub.StudentManagement.Application.Queries.GetStudentById.StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentDtoExample))]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        try
        {
            var query = new GetStudentByIdQuery { StudentId = id };
            var student = await _mediator.Send(query);
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Criar um novo estudante
    /// </summary>
    /// <param name="command">Detalhes do estudante</param>
    /// <returns>ID do estudante recém-criado</returns>
    /// <response code="201">Retorna o ID do estudante recém-criado</response>
    /// <response code="400">Se a requisição for inválida</response>
    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Criar um novo estudante",
        Description = "Registra um novo estudante no sistema com os detalhes fornecidos",
        OperationId = "CreateStudent"
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(CreateStudentCommand), typeof(CreateStudentCommandExample))]
    public async Task<IActionResult> CreateStudent(CreateStudentCommand command)
    {
        try
        {
            var studentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetStudentById), new { id = studentId }, null);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Atualizar um estudante existente
    /// </summary>
    /// <param name="id">ID do estudante a ser atualizado</param>
    /// <param name="command">Detalhes atualizados do estudante</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se o estudante foi atualizado com sucesso</response>
    /// <response code="400">Se a requisição for inválida</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualizar um estudante",
        Description = "Atualiza os detalhes de um estudante existente. O estudante só pode atualizar seu próprio perfil, a menos que seja um Administrador.",
        OperationId = "UpdateStudent"
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(UpdateStudentCommand), typeof(UpdateStudentCommandExample))]
    public async Task<IActionResult> UpdateStudent(Guid id, UpdateStudentCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");

        if (!isAdmin)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.StudentId != id)
            {
                return Forbid();
            }
        }

        if (id != command.Id)
        {
            return BadRequest("Student ID in the route must match the student ID in the command");
        }

        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deactivate a student account
    /// </summary>
    /// <param name="id">ID of the student to deactivate</param>
    /// <returns>Success message</returns>
    /// <response code="200">If the student was deactivated successfully</response>
    /// <response code="404">If the student is not found</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPut("{id}/desativar")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Desativar um estudante",
        Description = "Desativa uma conta de estudante, impedindo-o de acessar o sistema. Requer função de Administrador.",
        OperationId = "DeactivateStudent"
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(DeactivateStudentCommand), typeof(DeactivateStudentCommandExample))]
    public async Task<IActionResult> DeactivateStudent(Guid id)
    {
        try
        {
            var command = new DeactivateStudentCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(new { message = "Student deactivated successfully", result });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Activate a student account
    /// </summary>
    /// <param name="id">ID of the student to activate</param>
    /// <returns>Success message</returns>
    /// <response code="200">If the student was activated successfully</response>
    /// <response code="404">If the student is not found</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPut("{id}/ativar")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Ativar um estudante",
        Description = "Ativa uma conta de estudante previamente desativada, permitindo que ele acesse o sistema novamente. Requer função de Administrador.",
        OperationId = "ActivateStudent"
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(ActivateStudentCommand), typeof(ActivateStudentCommandExample))]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        try
        {
            var command = new ActivateStudentCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(new { message = "Student activated successfully", result });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get a student's learning progress
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <returns>The student's progress across all courses</returns>
    /// <response code="200">Returns the student's progress</response>
    /// <response code="404">If the student is not found</response>
    [HttpGet("{studentId}/progresso")]
    [SwaggerOperation(
        Summary = "Obter progresso do estudante",
        Description = "Recupera o progresso de aprendizado de um estudante em todos os cursos",
        OperationId = "GetStudentProgress"
    )]
    [ProducesResponseType(typeof(StudentProgressViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentProgress(Guid studentId)
    {
        try
        {
            var query = new GetStudentProgressQuery { StudentId = studentId };
            var progress = await _mediator.Send(query);
            return Ok(progress);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Mark a lesson as completed for a student
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <param name="courseId">ID of the course</param>
    /// <param name="lessonId">ID of the lesson</param>
    /// <returns>Result of the completion</returns>
    /// <response code="200">If the lesson was marked as completed successfully</response>
    /// <response code="400">If there was a problem completing the lesson</response>
    /// <response code="404">If the student, course or lesson is not found</response>
    [HttpPost("{studentId}/curso/{courseId}/licao/{lessonId}/completa")]
    [SwaggerOperation(
        Summary = "Completar uma lição para um estudante",
        Description = "Marca uma lição específica como concluída para um estudante em um curso específico",
        OperationId = "CompleteLessonForStudent"
    )]
    [ProducesResponseType(typeof(CompleteLessonForStudentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CompleteLessonForStudentResultExample))]
    public async Task<IActionResult> MarkLessonAsCompleted(Guid studentId, Guid courseId, Guid lessonId)
    {
        try
        {
            var command = new CompleteLessonForStudentCommand
            {
                StudentId = studentId,
                LessonId = lessonId,
                CompletionDate = DateTime.UtcNow
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark a course as completed for a student
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <param name="courseId">ID of the course</param>
    /// <returns>Result of the completion</returns>
    /// <response code="200">If the course was marked as completed successfully</response>
    /// <response code="400">If there was a problem completing the course</response>
    /// <response code="404">If the student or course is not found</response>
    [HttpPost("{studentId}/curso/{courseId}/completo")]
    [SwaggerOperation(
        Summary = "Completar um curso para um estudante",
        Description = "Marca um curso como concluído para um estudante. Todas as lições devem ser concluídas primeiro.",
        OperationId = "CompleteCourseForStudent"
    )]
    [ProducesResponseType(typeof(CompleteCourseForStudentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CompleteCourseForStudentResultExample))]
    public async Task<IActionResult> CompleteCourse(Guid studentId, Guid courseId)
    {
        try
        {
            var command = new CompleteCourseForStudentCommand
            {
                StudentId = studentId,
                CourseId = courseId,
                CompletionDate = DateTime.UtcNow
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// [DEBUG] Verificar lições completadas de um estudante
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <returns>Lista de lições completadas</returns>
    [HttpGet("{studentId}/debug/completed-lessons")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "[DEBUG] Verificar lições completadas",
        Description = "Endpoint temporário para debug - verificar lições completadas de um estudante",
        OperationId = "DebugCompletedLessons"
    )]
    public async Task<IActionResult> DebugCompletedLessons(Guid studentId)
    {
        try
        {
            var learningHistory = await _studentRepository.GetLearningHistoryByStudentIdAsync(studentId);
            if (learningHistory == null)
            {
                return Ok(new { message = "Nenhum histórico de aprendizado encontrado", completedLessons = new List<object>() });
            }

            var result = new
            {
                studentId = studentId,
                learningHistoryId = learningHistory.Id,
                courseProgresses = learningHistory.CourseProgresses.Select(cp => new
                {
                    courseId = cp.CourseId,
                    isCompleted = cp.IsCompleted,
                    lastUpdated = cp.LastUpdated,
                    completedLessonsCount = cp.CompletedLessons.Count,
                    completedLessons = cp.CompletedLessons.Select(cl => new
                    {
                        id = cl.Id,
                        lessonId = cl.LessonId,
                        completedAt = cl.CompletedAt,
                        courseProgressId = cl.CourseProgressId
                    }).ToList()
                }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// [DEBUG] Limpar dados de progresso de um estudante
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <returns>Resultado da limpeza</returns>
    [HttpDelete("{studentId}/debug/clear-progress")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "[DEBUG] Limpar progresso do estudante",
        Description = "Endpoint temporário para debug - limpar todo o progresso de aprendizado de um estudante",
        OperationId = "ClearStudentProgress"
    )]
    public async Task<IActionResult> ClearStudentProgress(Guid studentId)
    {
        try
        {
            var learningHistory = await _studentRepository.GetLearningHistoryByStudentIdAsync(studentId);
            
            if (learningHistory == null)
            {
                return Ok(new { message = "Nenhum histórico de aprendizado encontrado para limpar", studentId });
            }

            // Contar registros antes da limpeza
            var totalCourseProgresses = learningHistory.CourseProgresses.Count;
            var totalCompletedLessons = learningHistory.CourseProgresses.SelectMany(cp => cp.CompletedLessons).Count();
            var totalLearningRecords = learningHistory.Records.Count;

            // Remover o histórico completo - cascade delete removerá as dependências
            await _studentRepository.DeleteLearningHistoryAsync(studentId);

            var result = new
            {
                message = "Progresso do estudante limpo com sucesso",
                studentId = studentId,
                deletedData = new
                {
                    courseProgresses = totalCourseProgresses,
                    completedLessons = totalCompletedLessons,
                    learningRecords = totalLearningRecords
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// [DEBUG] Verificar tabela CompletedLessons diretamente
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <returns>Dados diretos da tabela CompletedLessons</returns>
    [HttpGet("{studentId}/debug/completed-lessons-raw")]
    [SwaggerOperation(
        Summary = "[DEBUG] Verificar tabela CompletedLessons diretamente",
        Description = "Endpoint temporário para debug - verificar dados diretos da tabela CompletedLessons",
        OperationId = "DebugCompletedLessonsRaw"
    )]
    public async Task<IActionResult> DebugCompletedLessonsRaw(Guid studentId)
    {
        try
        {
            // Como não temos acesso direto ao DbContext aqui, vamos usar o repositório
            var learningHistory = await _studentRepository.GetLearningHistoryByStudentIdAsync(studentId);
            
            if (learningHistory == null)
            {
                return Ok(new 
                { 
                    studentId = studentId,
                    learningHistoryExists = false,
                    learningHistoryId = (Guid?)null,
                    totalCourseProgresses = 0,
                    allCompletedLessons = new List<object>()
                });
            }

            var completedLessons = learningHistory.CourseProgresses
                .SelectMany(cp => cp.CompletedLessons.Select(cl => new
                {
                    completedLessonId = cl.Id,
                    lessonId = cl.LessonId,
                    completedAt = cl.CompletedAt,
                    courseProgressId = cl.CourseProgressId,
                    courseId = cp.CourseId
                }))
                .OrderByDescending(x => x.completedAt)
                .ToList();

            var result = new
            {
                studentId = studentId,
                learningHistoryExists = true,
                learningHistoryId = learningHistory.Id,
                totalCourseProgresses = learningHistory.CourseProgresses.Count,
                allCompletedLessons = completedLessons
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// [DEBUG] Teste direto de persistência CompletedLesson
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <param name="courseId">ID do curso</param>
    /// <param name="lessonId">ID da lição</param>
    /// <returns>Resultado do teste</returns>
    [HttpPost("{studentId}/debug/force-complete-lesson/{courseId}/{lessonId}")]
    [SwaggerOperation(
        Summary = "[DEBUG] Teste direto de persistência",
        Description = "Endpoint temporário para debug - força criação direta de CompletedLesson no contexto EF",
        OperationId = "DebugForceCompleteLesson"
    )]
    public async Task<IActionResult> DebugForceCompleteLesson(Guid studentId, Guid courseId, Guid lessonId)
    {
        try
        {
            // Primeiro, vamos obter ou criar o histórico de aprendizado
            var learningHistory = await _studentRepository.GetLearningHistoryByStudentIdAsync(studentId);
            
            if (learningHistory == null)
            {
                return BadRequest(new { error = "LearningHistory não encontrado", studentId });
            }

            // Obter ou criar o CourseProgress
            var courseProgress = learningHistory.CourseProgresses.FirstOrDefault(cp => cp.CourseId == courseId);
            
            if (courseProgress == null)
            {
                return BadRequest(new { error = "CourseProgress não encontrado", courseId });
            }

            // Teste: adicionar CompletedLesson através do domínio
            var countBefore = courseProgress.CompletedLessons.Count;
            
            courseProgress.AddCompletedLesson(lessonId);
            
            var countAfter = courseProgress.CompletedLessons.Count;

            // Salvar explicitamente
            await _studentRepository.SaveChangesAsync();

            // Verificar se foi salvo
            var reloadedHistory = await _studentRepository.GetLearningHistoryByStudentIdAsync(studentId);
            var reloadedCourseProgress = reloadedHistory?.CourseProgresses.FirstOrDefault(cp => cp.CourseId == courseId);
            var persistedCount = reloadedCourseProgress?.CompletedLessons.Count ?? 0;

            var result = new
            {
                message = "Teste de persistência CompletedLesson",
                studentId = studentId,
                courseId = courseId,
                lessonId = lessonId,
                countBefore = countBefore,
                countAfterDomainCall = countAfter,
                countAfterSave = persistedCount,
                wasAddedInMemory = countAfter > countBefore,
                wasPersisted = persistedCount > countBefore,
                completedLessons = reloadedCourseProgress?.CompletedLessons.Select(cl => new
                {
                    id = cl.Id,
                    lessonId = cl.LessonId,
                    completedAt = cl.CompletedAt,
                    courseProgressId = cl.CourseProgressId
                }).Cast<object>().ToList() ?? new List<object>()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// [DEBUG] Criar matrícula para teste
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <param name="courseId">ID do curso</param>
    /// <returns>Resultado da criação</returns>
    [HttpPost("{studentId}/debug/create-enrollment/{courseId}")]
    [SwaggerOperation(
        Summary = "[DEBUG] Criar matrícula para teste",
        Description = "Endpoint temporário para debug - criar matrícula necessária para teste",
        OperationId = "DebugCreateEnrollment"
    )]
    public async Task<IActionResult> DebugCreateEnrollment(Guid studentId, Guid courseId)
    {
        try
        {
            return Ok(new { message = "Método temporariamente desabilitado - use a API de matrículas" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}

