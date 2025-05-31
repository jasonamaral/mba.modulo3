using FluencyHub.API.SwaggerExamples;
using FluencyHub.StudentManagement.Application.Commands.GenerateCertificate;
using FluencyHub.StudentManagement.Application.Queries.GetCertificateById;
using FluencyHub.StudentManagement.Application.Queries.GetStudentCertificates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using FluencyHub.SharedKernel.Common.Exceptions;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificadosController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificadosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gerar um certificado para um aluno que concluiu um curso
    /// </summary>
    /// <param name="command">Detalhes da geração do certificado</param>
    /// <returns>ID do certificado recém-gerado</returns>
    /// <response code="201">Retorna o ID do certificado recém-gerado</response>
    /// <response code="400">Se a solicitação for inválida</response>
    /// <response code="404">Se o aluno ou curso não for encontrado</response>
    [HttpPost]
    [Authorize(Roles = "Student,Administrator")]
    [SwaggerOperation(
        Summary = "Gerar um certificado",
        Description = "Cria um certificado para um aluno que concluiu um curso com sucesso",
        OperationId = "GerarCertificado"
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
    /// Obter um certificado por ID
    /// </summary>
    /// <param name="id">ID do certificado a ser recuperado</param>
    /// <returns>Detalhes do certificado</returns>
    /// <response code="200">Retorna os detalhes do certificado</response>
    /// <response code="404">Se o certificado não for encontrado</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Student,Administrator")]
    [SwaggerOperation(
        Summary = "Obter certificado por ID",
        Description = "Recupera um certificado específico por seu identificador exclusivo",
        OperationId = "ObterCertificado"
    )]
    [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CertificateDtoExample))]
    public async Task<IActionResult> GetCertificate(Guid id)
    {
        try
        {
            var query = new GetCertificateByIdQuery { CertificateId = id };
            var certificate = await _mediator.Send(query);
            return Ok(certificate);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Obter todos os certificados de um aluno
    /// </summary>
    /// <param name="studentId">ID do aluno</param>
    /// <returns>Lista de certificados do aluno</returns>
    /// <response code="200">Retorna a lista de certificados</response>
    /// <response code="404">Se o aluno não for encontrado</response>
    [HttpGet("estudente/{studentId}")]
    [Authorize(Roles = "Student,Administrator")]
    [SwaggerOperation(
        Summary = "Obtenha todos os certificados de um aluno",
        Description = "Recupera todos os certificados emitidos para um aluno específico",
        OperationId = "ObterCertificadosEstudante"
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