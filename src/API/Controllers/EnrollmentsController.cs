using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Application.Commands.CompleteEnrollment;
using FluencyHub.ContentManagement.Application.Queries.GetCourseById;
using FluencyHub.StudentManagement.Application.Commands.EnrollStudent;
using FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById;
using FluencyHub.StudentManagement.Application.Queries.GetStudentEnrollments;
using FluencyHub.StudentManagement.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net.Mime;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EnrollmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Matricular um aluno em um curso
    /// </summary>
    /// <param name="request">Dados da requisição de matrícula</param>
    /// <returns>A matrícula recém-criada</returns>
    /// <response code="201">Retorna a matrícula recém-criada</response>
    /// <response code="400">Se a matrícula for inválida</response>
    /// <response code="404">Se o aluno ou curso não for encontrado</response>
    [HttpPost]
    [Authorize(Roles = "Student,Administrator")]
    [SwaggerOperation(
        Summary = "Matricular um aluno em um curso",
        Description = "Cria uma nova matrícula para um aluno em um curso específico",
        OperationId = "EnrollStudent",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(EnrollmentCreateRequest), typeof(EnrollmentCreateRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(EnrollmentDtoExample))]
    public async Task<IActionResult> EnrollStudent([FromBody] EnrollmentCreateRequest request)
    {
        try
        {
            var command = request.ToCommand();
            var enrollmentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollmentId }, null);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obter uma matrícula específica pelo ID
    /// </summary>
    /// <param name="id">O identificador único da matrícula</param>
    /// <returns>Os detalhes da matrícula</returns>
    /// <response code="200">Retorna a matrícula</response>
    /// <response code="404">Se a matrícula não for encontrada</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter matrícula por ID",
        Description = "Recupera uma matrícula específica pelo seu identificador único",
        OperationId = "GetEnrollment",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnrollmentDtoExample))]
    public async Task<IActionResult> GetEnrollment(Guid id)
    {
        try
        {
            var query = new GetEnrollmentByIdQuery { EnrollmentId = id };
            var enrollment = await _mediator.Send(query);
            return Ok(enrollment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Obter todas as matrículas para um aluno específico
    /// </summary>
    /// <param name="studentId">O identificador único do aluno</param>
    /// <returns>Uma lista de matrículas do aluno</returns>
    /// <response code="200">Retorna a lista de matrículas</response>
    /// <response code="404">Se o aluno não for encontrado</response>
    [HttpGet("student/{studentId}")]
    [SwaggerOperation(
        Summary = "Obter matrículas do aluno",
        Description = "Recupera todas as matrículas associadas a um aluno específico",
        OperationId = "GetStudentEnrollments",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnrollmentDtoExample))]
    public async Task<IActionResult> GetStudentEnrollments(Guid studentId)
    {
        try
        {
            var enrollments = await _mediator.Send(new GetStudentEnrollmentsQuery(studentId));
            return Ok(enrollments);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marcar uma lição como concluída para uma matrícula
    /// </summary>
    /// <param name="enrollmentId">O identificador único da matrícula</param>
    /// <param name="lessonId">O identificador único da lição</param>
    /// <param name="request">Os dados de requisição de conclusão</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se a lição foi marcada como concluída com sucesso</response>
    /// <response code="400">Se a requisição é inválida</response>
    /// <response code="404">Se a matrícula ou lição não for encontrada</response>
    [HttpPost("{enrollmentId}/lessons/{lessonId}/complete")]
    [SwaggerOperation(
        Summary = "Concluir uma lição",
        Description = "Marca uma lição específica como concluída para uma determinada matrícula",
        OperationId = "CompleteLesson",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(LessonCompleteRequest), typeof(LessonCompleteRequestExample))]
    public async Task<IActionResult> CompleteLesson(Guid enrollmentId, Guid lessonId, [FromBody] LessonCompleteRequest request)
    {
        try
        {
            // TODO: Implementar quando o comando CompleteLesson estiver disponível
            return Ok(new { message = "Lição concluída com sucesso" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marcar um curso como concluído para uma matrícula
    /// </summary>
    /// <param name="id">O identificador único da matrícula</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se o curso foi marcado como concluído com sucesso</response>
    /// <response code="400">Se a requisição é inválida ou nem todas as lições foram concluídas</response>
    /// <response code="404">Se a matrícula ou curso não for encontrado</response>
    [HttpPost("{id}/complete")]
    [SwaggerOperation(
        Summary = "Concluir um curso",
        Description = "Marca uma matrícula como concluída. Todas as lições devem ser concluídas primeiro.",
        OperationId = "CompleteCourse",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCourse(Guid id)
    {
        try
        {
            // TODO: Implementar quando os comandos necessários estiverem disponíveis
            return Ok(new { message = "Curso concluído com sucesso!" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}