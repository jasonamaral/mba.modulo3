using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.ContentManagement.Application.Commands.DeleteLesson;
using FluencyHub.ContentManagement.Application.Commands.UpdateLesson;
using FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourseId;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;
using FluencyHub.ContentManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LessonsController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Obter todas as lições de um curso
    /// </summary>
    /// <param name="courseId">ID do curso</param>
    /// <returns>Lista de lições do curso</returns>
    /// <response code="200">Retorna a lista de lições</response>
    /// <response code="404">Se o curso não for encontrado</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obter todas as lições de um curso",
        Description = "Recupera todas as lições que pertencem ao curso especificado",
        OperationId = "GetLessonsByCourse",
        Tags = new[] { "Lessons" }
    )]
    [ProducesResponseType(typeof(IEnumerable<LessonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LessonDtoListExample))]
    public async Task<IActionResult> GetLessonsByCourse(Guid courseId)
    {
        try
        {
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery { CourseId = courseId });
            return Ok(lessons);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Adicionar uma nova lição a um curso
    /// </summary>
    /// <param name="courseId">ID do curso</param>
    /// <param name="request">Detalhes da lição</param>
    /// <returns>ID da lição recém-criada</returns>
    /// <response code="201">Retorna o ID da lição recém-criada</response>
    /// <response code="400">Se a requisição for inválida</response>
    /// <response code="404">Se o curso não for encontrado</response>
    /// <response code="401">Se o usuário não estiver autenticado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Adicionar uma nova lição a um curso",
        Description = "Cria uma nova lição dentro do curso especificado. Requer função de Administrador.",
        OperationId = "AddLesson",
        Tags = new[] { "Lessons" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(LessonCreateRequest), typeof(LessonCreateRequestExample))]
    public async Task<IActionResult> AddLesson(Guid courseId, [FromBody] LessonCreateRequest request)
    {
        try
        {
            var command = request.ToCommand(courseId);
            var lessonId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetLessonsByCourse), new { courseId }, new { id = lessonId });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Atualizar uma lição existente
    /// </summary>
    /// <param name="courseId">ID do curso</param>
    /// <param name="lessonId">ID da lição a ser atualizada</param>
    /// <param name="command">Detalhes atualizados da lição</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se a lição foi atualizada com sucesso</response>
    /// <response code="400">Se a requisição for inválida</response>
    /// <response code="404">Se o curso ou a lição não for encontrada</response>
    /// <response code="401">Se o usuário não estiver autenticado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpPut("{lessonId}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Atualizar uma lição existente",
        Description = "Atualiza uma lição existente com os detalhes fornecidos. Requer função de Administrador.",
        OperationId = "UpdateLesson",
        Tags = new[] { "Lessons" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(UpdateLessonCommand), typeof(UpdateLessonCommandExample))]
    public async Task<IActionResult> UpdateLesson(Guid courseId, Guid lessonId, [FromBody] UpdateLessonCommand command)
    {
        if (command.Id != lessonId)
        {
            return BadRequest("Lesson ID in the route must match the ID in the command");
        }

        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Excluir uma lição de um curso
    /// </summary>
    /// <param name="courseId">ID do curso</param>
    /// <param name="lessonId">ID da lição a ser excluída</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se a lição foi excluída com sucesso</response>
    /// <response code="404">Se o curso ou a lição não for encontrada</response>
    /// <response code="400">Se houver um erro durante a exclusão</response>
    /// <response code="401">Se o usuário não estiver autenticado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpDelete("{lessonId}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Excluir uma lição de um curso",
        Description = "Remove a lição especificada do curso. Requer função de Administrador.",
        OperationId = "DeleteLesson",
        Tags = new[] { "Lessons" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteLesson(Guid courseId, Guid lessonId)
    {
        try
        {
            var command = new DeleteLessonCommand { Id = lessonId };
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
    /// Marcar uma lição como concluída para o estudante atual
    /// </summary>
    /// <param name="courseId">ID do curso</param>
    /// <param name="lessonId">ID da lição a ser marcada como concluída</param>
    /// <param name="request">Requisição de conclusão</param>
    /// <returns>Resultado da operação de conclusão</returns>
    /// <response code="200">Retorna o resultado da conclusão</response>
    /// <response code="400">Se a requisição for inválida ou o estudante não estiver inscrito</response>
    /// <response code="401">Se o usuário não estiver autenticado</response>
    [HttpPost("{lessonId}/complete")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Marcar uma lição como concluída",
        Description = "Marca a lição especificada como concluída para o estudante atual.",
        OperationId = "CompleteLesson",
        Tags = new[] { "Lessons" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteLesson(Guid courseId, Guid lessonId, [FromBody] LessonCompleteRequest request)
    {
        if (!request.Completed)
        {
            return BadRequest("The 'Completed' field must be true to mark a lesson as completed.");
        }

        try
        {
            var studentIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (studentIdClaim == null || !Guid.TryParse(studentIdClaim.Value, out var studentId))
            {
                return BadRequest("Student ID could not be determined from the authentication token.");
            }

            var command = new CompleteLessonForStudentCommand 
            {
                StudentId = studentId,
                LessonId = lessonId,
                CompletionDate = DateTime.UtcNow,
                Score = request.Score,
                Feedback = request.Feedback
            };
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}