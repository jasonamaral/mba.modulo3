using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.ContentManagement.Application.Commands.DeleteLesson;
using FluencyHub.ContentManagement.Application.Commands.UpdateLesson;
using FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourse;
using FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;
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
    /// Get all lessons for a course
    /// </summary>
    /// <param name="courseId">ID of the course</param>
    /// <returns>List of lessons for the course</returns>
    /// <response code="200">Returns the list of lessons</response>
    /// <response code="404">If the course is not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all lessons for a course",
        Description = "Retrieves all lessons that belong to the specified course",
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
            var lessons = await _mediator.Send(new GetLessonsByCourseQuery(courseId));
            return Ok(lessons);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Add a new lesson to a course
    /// </summary>
    /// <param name="courseId">ID of the course</param>
    /// <param name="request">Lesson details</param>
    /// <returns>ID of the newly created lesson</returns>
    /// <response code="201">Returns the ID of the newly created lesson</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the course is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Add a new lesson to a course",
        Description = "Creates a new lesson within the specified course. Requires Administrator role.",
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
    /// Update an existing lesson
    /// </summary>
    /// <param name="courseId">ID of the course</param>
    /// <param name="lessonId">ID of the lesson to update</param>
    /// <param name="command">Updated lesson details</param>
    /// <returns>Success message</returns>
    /// <response code="200">If the lesson was updated successfully</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the course or lesson is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPut("{lessonId}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Update an existing lesson",
        Description = "Updates an existing lesson with the provided details. Requires Administrator role.",
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
        if (courseId != command.CourseId || lessonId != command.LessonId)
        {
            return BadRequest("Course ID and Lesson ID in the route must match the IDs in the command");
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
    /// Delete a lesson from a course
    /// </summary>
    /// <param name="courseId">ID of the course</param>
    /// <param name="lessonId">ID of the lesson to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">If the lesson was deleted successfully</response>
    /// <response code="404">If the course or lesson is not found</response>
    /// <response code="400">If there was an error during deletion</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpDelete("{lessonId}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Delete a lesson from a course",
        Description = "Removes the specified lesson from the course. Requires Administrator role.",
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
            var command = new DeleteLessonCommand(courseId, lessonId);
            await _mediator.Send(command);
            return Ok();
        }
        catch (FluencyHub.Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark a lesson as completed for the current student
    /// </summary>
    /// <param name="courseId">ID of the course</param>
    /// <param name="lessonId">ID of the lesson to mark as completed</param>
    /// <param name="request">Completion request</param>
    /// <returns>Result of the completion operation</returns>
    /// <response code="200">Returns the completion result</response>
    /// <response code="400">If the request is invalid or the student is not enrolled</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPost("{lessonId}/complete")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Mark a lesson as completed",
        Description = "Marks the specified lesson as completed for the current student.",
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

            var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}