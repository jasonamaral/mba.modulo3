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
/// API endpoints for managing courses
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
    /// Get all available courses
    /// </summary>
    /// <returns>A list of all courses</returns>
    /// <response code="200">Returns the list of courses</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all courses",
        Description = "Retrieves all available courses in the system",
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
    /// Get a specific course by ID
    /// </summary>
    /// <param name="id">The unique identifier of the course</param>
    /// <returns>The course details</returns>
    /// <response code="200">Returns the course details</response>
    /// <response code="404">If the course with the given ID is not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get course by ID",
        Description = "Retrieves a specific course by its unique identifier",
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
    /// Create a new course
    /// </summary>
    /// <param name="request">The course creation details</param>
    /// <returns>The ID of the newly created course</returns>
    /// <response code="201">Returns the ID of the newly created course</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized to create courses</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Create a new course",
        Description = "Creates a new course with the provided details. Requires Administrator role.",
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
    /// Update an existing course
    /// </summary>
    /// <param name="id">The unique identifier of the course to update</param>
    /// <param name="request">The updated course details</param>
    /// <returns>Success message with the course ID</returns>
    /// <response code="200">Returns success with the course ID</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized to update courses</response>
    /// <response code="404">If the course with the given ID is not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Update an existing course",
        Description = "Updates an existing course with the provided details. Requires Administrator role.",
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
                return BadRequest("The ID in the URL must be the same as the ID in the request body");
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