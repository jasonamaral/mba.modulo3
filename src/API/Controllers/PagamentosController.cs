using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.PaymentProcessing.Application.Common.Exceptions;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Application.Queries.GetPaymentById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class PagamentosController : ControllerBase
{
    private readonly IPaymentApplicationService _paymentService;

    public PagamentosController(IPaymentApplicationService paymentService)
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
        OperationId = "ProcessPayment"
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
                request.CardDetails.ExpiryMonth.ToString("D2"),
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
            return UnprocessableEntity(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
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
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Obter pagamento por ID",
        Description = "Recupera um pagamento específico pelo seu identificador único",
        OperationId = "GetPayment"
    )]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PaymentDtoExample))]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        Console.WriteLine($"=== GetPayment INICIADO ===");
        Console.WriteLine($"ID recebido: {id}");
        Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
        
        if (User.Claims.Any())
        {
            Console.WriteLine("Claims do usuário:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }
        }
        else
        {
            Console.WriteLine("Nenhuma claim encontrada");
        }
        
        try
        {
            Console.WriteLine("Chamando _paymentService.GetPaymentByIdAsync...");
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            Console.WriteLine($"Pagamento encontrado: {payment?.Id}");
            return Ok(payment);
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"NotFoundException: {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception geral: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
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
    [HttpPost("{id}/reembolso")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Reembolsar um pagamento",
        Description = "Processa um reembolso para um pagamento específico. Requer perfil de Administrador.",
        OperationId = "RefundPayment"
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
            return UnprocessableEntity(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}