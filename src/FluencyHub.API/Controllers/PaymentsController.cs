using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.PaymentProcessing.Commands.ProcessPayment;
using FluencyHub.Application.PaymentProcessing.Commands.RefundPayment;
using FluencyHub.Application.PaymentProcessing.Queries.GetPaymentById;
using FluencyHub.API.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentProcessRequest request)
    {
        try
        {
            var command = request.ToCommand();
            var paymentId = await _mediator.Send(command);
            
            var payment = await _mediator.Send(new GetPaymentByIdQuery(paymentId));
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
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        try
        {
            var payment = await _mediator.Send(new GetPaymentByIdQuery(id));
            return Ok(payment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundProcessRequest request)
    {
        try
        {
            var command = request.ToCommand(id);
            await _mediator.Send(command);
            
            var payment = await _mediator.Send(new GetPaymentByIdQuery(id));
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