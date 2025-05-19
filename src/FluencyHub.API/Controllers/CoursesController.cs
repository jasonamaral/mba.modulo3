using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Queries.GetAllCourses;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using ValidationException = FluencyHub.Application.Common.Exceptions.ValidationException;

namespace FluencyHub.API.Controllers;

/// <summary>
/// Endpoints da API para gerenciamento de cursos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obter todos os cursos disponíveis
    /// </summary>
    /// <returns>Uma lista de todos os cursos</returns>
    /// <response code="200">Retorna a lista de cursos</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obter todos os cursos",
        Description = "Recupera todos os cursos disponíveis no sistema",
        OperationId = "GetAllCourses",
        Tags = new[] { "Courses" }
    )]
    [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CourseDtoExample))]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await _mediator.Send(new GetAllCoursesQuery());
        return Ok(courses);
    }

    /// <summary>
    /// Obter um curso específico por ID
    /// </summary>
    /// <param name="id">O identificador único do curso</param>
    /// <returns>Os detalhes do curso</returns>
    /// <response code="200">Retorna os detalhes do curso</response>
    /// <response code="404">Se o curso com o ID fornecido não for encontrado</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter curso por ID",
        Description = "Recupera um curso específico pelo seu identificador único",
        OperationId = "GetCourseById",
        Tags = new[] { "Courses" }
    )]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CourseDtoExample))]
    public async Task<IActionResult> GetCourseById([Required] Guid id)
    {
        try
        {
            var course = await _mediator.Send(new GetCourseByIdQuery(id));
            return Ok(course);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Criar um novo curso
    /// </summary>
    /// <param name="request">Os detalhes para criação do curso</param>
    /// <returns>O ID do curso recém-criado</returns>
    /// <response code="201">Retorna o ID do curso recém-criado</response>
    /// <response code="400">Se os dados da requisição forem inválidos</response>
    /// <response code="401">Se o usuário não estiver autenticado</response>
    /// <response code="403">Se o usuário não estiver autorizado a criar cursos</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Criar um novo curso",
        Description = "Cria um novo curso com os detalhes fornecidos. Requer perfil de Administrador.",
        OperationId = "CreateCourse",
        Tags = new[] { "Courses" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerRequestExample(typeof(CourseCreateRequest), typeof(CourseCreateRequestExample))]
    public async Task<IActionResult> CreateCourse([FromBody] CourseCreateRequest request)
    {
        try
        {
            var command = request.ToCommand();
            var courseId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCourseById), new { id = courseId }, new { id = courseId });
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Atualizar um curso existente
    /// </summary>
    /// <param name="id">O identificador único do curso a ser atualizado</param>
    /// <param name="request">Os detalhes atualizados do curso</param>
    /// <returns>Mensagem de sucesso com o ID do curso</returns>
    /// <response code="200">Retorna sucesso com o ID do curso</response>
    /// <response code="400">Se os dados da requisição forem inválidos</response>
    /// <response code="401">Se o usuário não estiver autenticado</response>
    /// <response code="403">Se o usuário não estiver autorizado a atualizar cursos</response>
    /// <response code="404">Se o curso com o ID fornecido não for encontrado</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Atualizar um curso existente",
        Description = "Atualiza um curso existente com os detalhes fornecidos. Requer perfil de Administrador.",
        OperationId = "UpdateCourse",
        Tags = new[] { "Courses" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(CourseUpdateRequest), typeof(CourseUpdateRequestExample))]
    public async Task<IActionResult> UpdateCourse([Required] Guid id, [FromBody] CourseUpdateRequest request)
    {
        try
        {
            if (id != request.Id)
            {
                return BadRequest("O ID na URL deve ser o mesmo que o ID no corpo da requisição");
            }

            var command = request.ToCommand();
            await _mediator.Send(command);
            return Ok(new { id });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}