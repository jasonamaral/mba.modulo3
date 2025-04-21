using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.PaymentProcessing.Queries.GetPaymentById;
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
    /// Process a payment for an enrollment
    /// </summary>
    /// <param name="request">Payment details including card information</param>
    /// <returns>The processed payment information</returns>
    /// <response code="201">Returns the newly created payment</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the enrollment is not found</response>
    /// <response code="422">If the payment cannot be processed</response>
    [HttpPost]
    [Authorize(Roles = "Student,Administrator")]
    [SwaggerOperation(
        Summary = "Process a payment for an enrollment",
        Description = "Creates a new payment for a specific enrollment using the provided card details",
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
    /// Get payment details by ID
    /// </summary>
    /// <param name="id">The unique identifier of the payment</param>
    /// <returns>The payment details</returns>
    /// <response code="200">Returns the payment details</response>
    /// <response code="404">If the payment is not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get payment by ID",
        Description = "Retrieves a specific payment by its unique identifier",
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
    /// Process a refund for a payment
    /// </summary>
    /// <param name="id">The unique identifier of the payment to refund</param>
    /// <param name="request">The refund request data</param>
    /// <returns>The updated payment details</returns>
    /// <response code="200">Returns the updated payment details</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the payment is not found</response>
    /// <response code="422">If the refund cannot be processed</response>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Refund a payment",
        Description = "Processes a refund for a specific payment. Requires Administrator role.",
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