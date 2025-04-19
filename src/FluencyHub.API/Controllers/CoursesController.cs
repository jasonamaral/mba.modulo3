using FluencyHub.API.Models;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Queries.GetAllCourses;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await _mediator.Send(new GetAllCoursesQuery());
        return Ok(courses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(Guid id)
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

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse(CourseCreateRequest request)
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

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(Guid id, CourseUpdateRequest request)
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