using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.CreateStudent;
using FluencyHub.Application.StudentManagement.Commands.UpdateStudent;
using FluencyHub.Application.StudentManagement.Commands.ActivateStudent;
using FluencyHub.Application.StudentManagement.Commands.DeactivateStudent;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        // Only allow administrators or the student themselves to access their profile
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");
        
        if (!isAdmin && userId != id.ToString())
        {
            return Forbid();
        }
        
        try
        {
            var student = await _mediator.Send(new GetStudentByIdQuery(id));
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStudent(Guid id, UpdateStudentCommand command)
    {
        // Only allow administrators or the student themselves to update their profile
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");
        
        if (!isAdmin && userId != id.ToString())
        {
            return Forbid();
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
    
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateStudent(Guid id)
    {
        try
        {
            await _mediator.Send(new DeactivateStudentCommand(id));
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        try
        {
            await _mediator.Send(new ActivateStudentCommand(id));
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
} 