using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Application.Queries.GetPaymentById;
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
public class PaymentsController : ControllerBase
{
    private readonly IPaymentApplicationService _paymentService;

    public PaymentsController(IPaymentApplicationService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Processar um pagamento para uma matrícula
    /// </summary>
    /// <param name="request">Detalhes do pagamento incluindo informações do cartão</param>
    /// <returns>As informações do pagamento processado</returns>
    /// <response code="201">Retorna o pagamento recém-criado</response>
    /// <response code="400">Se a requisição for inválida</response>
    /// <response code="404">Se a matrícula não for encontrada</response>
    /// <response code="422">Se o pagamento não puder ser processado</response>
    [HttpPost]
    [Authorize(Roles = "Student,Administrator")]
    [SwaggerOperation(
        Summary = "Processar um pagamento para uma matrícula",
        Description = "Cria um novo pagamento para uma matrícula específica usando os detalhes do cartão fornecidos",
        OperationId = "ProcessPayment",
        Tags = new[] { "Payments" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [SwaggerRequestExample(typeof(PaymentProcessRequest), typeof(PaymentProcessRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(PaymentDtoExample))]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentProcessRequest request)
    {
        try
        {
            var paymentId = await _paymentService.ProcessPaymentAsync(
                request.EnrollmentId,
                request.CardDetails.CardholderName,
                request.CardDetails.CardNumber,
                request.CardDetails.ExpiryMonth.ToString(),
                request.CardDetails.ExpiryYear.ToString());

            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            return CreatedAtAction(nameof(GetPayment), new { id = paymentId }, payment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obter detalhes do pagamento por ID
    /// </summary>
    /// <param name="id">O identificador único do pagamento</param>
    /// <returns>Os detalhes do pagamento</returns>
    /// <response code="200">Retorna os detalhes do pagamento</response>
    /// <response code="404">Se o pagamento não for encontrado</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter pagamento por ID",
        Description = "Recupera um pagamento específico pelo seu identificador único",
        OperationId = "GetPayment",
        Tags = new[] { "Payments" }
    )]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PaymentDtoExample))]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            return Ok(payment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Processar um reembolso para um pagamento
    /// </summary>
    /// <param name="id">O identificador único do pagamento a ser reembolsado</param>
    /// <param name="request">Os dados da requisição de reembolso</param>
    /// <returns>Os detalhes atualizados do pagamento</returns>
    /// <response code="200">Retorna os detalhes atualizados do pagamento</response>
    /// <response code="400">Se a requisição for inválida</response>
    /// <response code="404">Se o pagamento não for encontrado</response>
    /// <response code="422">Se o reembolso não puder ser processado</response>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Reembolsar um pagamento",
        Description = "Processa um reembolso para um pagamento específico. Requer perfil de Administrador.",
        OperationId = "RefundPayment",
        Tags = new[] { "Payments" }
    )]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [SwaggerRequestExample(typeof(RefundProcessRequest), typeof(RefundProcessRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PaymentDtoExample))]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundProcessRequest request)
    {
        try
        {
            await _paymentService.RefundPaymentAsync(id, request.Reason);
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            return Ok(payment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}