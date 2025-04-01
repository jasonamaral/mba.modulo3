using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.EnrollStudent;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentEnrollments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public EnrollmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrollStudent(EnrollStudentCommand command)
    {
        try
        {
            var enrollmentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollmentId }, null);
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
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollment(Guid id)
    {
        try
        {
            var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(id));
            return Ok(enrollment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    }
} 