using FluencyHub.Application.ContentManagement.Commands.AddLesson;
using FluencyHub.Application.ContentManagement.Commands.UpdateLesson;
using FluencyHub.Application.ContentManagement.Queries.GetLessonsByCourse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public LessonsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LessonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLesson(Guid courseId, AddLessonCommand command)
    {
        if (courseId != command.CourseId)
        {
            return BadRequest("Course ID in the route must match the course ID in the command");
        }
        
        try
        {
            var lessonId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetLessonsByCourse), new { courseId }, null);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPut("{lessonId}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLesson(Guid courseId, Guid lessonId, UpdateLessonCommand command)
    {
        if (courseId != command.CourseId || lessonId != command.LessonId)
        {
            return BadRequest("IDs in the route must match the IDs in the command");
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
} 