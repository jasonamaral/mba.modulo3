using FluencyHub.API.SwaggerExamples;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;
using FluencyHub.Application.StudentManagement.Queries.GetCertificateById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentCertificates;
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
public class CertificatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Generate a certificate for a student who completed a course
    /// </summary>
    /// <param name="command">Certificate generation details</param>
    /// <returns>ID of the newly generated certificate</returns>
    /// <response code="201">Returns the ID of the newly generated certificate</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the student or course is not found</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Generate a certificate",
        Description = "Creates a certificate for a student who has successfully completed a course",
        OperationId = "GenerateCertificate",
        Tags = new[] { "Certificates" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(GenerateCertificateCommand), typeof(GenerateCertificateCommandExample))]
    public async Task<IActionResult> GenerateCertificate(GenerateCertificateCommand command)
    {
        try
        {
            var certificateId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCertificate), new { id = certificateId }, new { Id = certificateId });
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

    /// <summary>
    /// Get a certificate by ID
    /// </summary>
    /// <param name="id">ID of the certificate to retrieve</param>
    /// <returns>Certificate details</returns>
    /// <response code="200">Returns the certificate details</response>
    /// <response code="404">If the certificate is not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get certificate by ID",
        Description = "Retrieves a specific certificate by its unique identifier",
        OperationId = "GetCertificate",
        Tags = new[] { "Certificates" }
    )]
    [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CertificateDtoExample))]
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

    /// <summary>
    /// Get all certificates for a student
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <returns>List of certificates for the student</returns>
    /// <response code="200">Returns the list of certificates</response>
    /// <response code="404">If the student is not found</response>
    [HttpGet("student/{studentId}")]
    [SwaggerOperation(
        Summary = "Get all certificates for a student",
        Description = "Retrieves all certificates issued to a specific student",
        OperationId = "GetStudentCertificates",
        Tags = new[] { "Certificates" }
    )]
    [ProducesResponseType(typeof(IEnumerable<CertificateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CertificateListDtoExample))]
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