using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;
using FluencyHub.Application.StudentManagement.Queries.GetCertificateById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentCertificates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateCertificate(GenerateCertificateCommand command)
    {
        try
        {
            var certificateId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCertificate), new { id = certificateId }, null);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCertificate(Guid id)
    {
        try
        {
            var certificate = await _mediator.Send(new GetCertificateByIdQuery(id));
            return Ok(certificate);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("student/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentCertificates(Guid studentId)
    {
        try
        {
            var certificates = await _mediator.Send(new GetStudentCertificatesQuery(studentId));
            return Ok(certificates);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}