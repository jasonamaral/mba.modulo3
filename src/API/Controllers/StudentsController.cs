using FluencyHub.API.SwaggerExamples;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Application.Commands.ActivateStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;
using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluencyHub.StudentManagement.Application.Commands.DeactivateStudent;
using FluencyHub.StudentManagement.Application.Commands.UpdateStudent;
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
using System.Net.Mime;
using System.Security.Claims;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentsController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    /// <summary>
    /// Obter todos os estudantes
    /// </summary>
    /// <returns>Lista de todos os estudantes</returns>
    /// <response code="200">Retorna a lista de estudantes</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Obter todos os estudantes",
        Description = "Recupera uma lista de todos os estudantes no sistema. Requer função de Administrador.",
        OperationId = "GetAllStudents",
        Tags = new[] { "Students" }
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
        OperationId = "GetCurrentStudent",
        Tags = new[] { "Students" }
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
        OperationId = "GetStudentById",
        Tags = new[] { "Students" }
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
        OperationId = "CreateStudent",
        Tags = new[] { "Students" }
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
        OperationId = "UpdateStudent",
        Tags = new[] { "Students" }
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
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Desativar um estudante",
        Description = "Desativa uma conta de estudante, impedindo-o de acessar o sistema. Requer função de Administrador.",
        OperationId = "DeactivateStudent",
        Tags = new[] { "Students" }
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
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Ativar um estudante",
        Description = "Ativa uma conta de estudante previamente desativada, permitindo que ele acesse o sistema novamente. Requer função de Administrador.",
        OperationId = "ActivateStudent",
        Tags = new[] { "Students" }
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
    [HttpGet("{studentId}/progress")]
    [SwaggerOperation(
        Summary = "Obter progresso do estudante",
        Description = "Recupera o progresso de aprendizado de um estudante em todos os cursos",
        OperationId = "GetStudentProgress",
        Tags = new[] { "Students" }
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
    [HttpPost("{studentId}/courses/{courseId}/lessons/{lessonId}/complete")]
    [SwaggerOperation(
        Summary = "Completar uma lição para um estudante",
        Description = "Marca uma lição específica como concluída para um estudante em um curso específico",
        OperationId = "CompleteLessonForStudent",
        Tags = new[] { "Students" }
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
    [HttpPost("{studentId}/courses/{courseId}/complete")]
    [SwaggerOperation(
        Summary = "Completar um curso para um estudante",
        Description = "Marca um curso como concluído para um estudante. Todas as lições devem ser concluídas primeiro.",
        OperationId = "CompleteCourseForStudent",
        Tags = new[] { "Students" }
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
}