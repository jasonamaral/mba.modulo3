using FluencyHub.SharedKernel.Events.StudentManagement;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using FluencyHub.PaymentProcessing.Domain;

namespace FluencyHub.PaymentProcessing.Application.Handlers;

public class StudentEnrolledEventHandler : INotificationHandler<StudentEnrolledEvent>
{
    private readonly FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository _paymentRepository;
    private readonly ILogger<StudentEnrolledEventHandler> _logger;
    
    public StudentEnrolledEventHandler(
        FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository paymentRepository,
        ILogger<StudentEnrolledEventHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }
    
    public async Task Handle(StudentEnrolledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling StudentEnrolledEvent for StudentId: {StudentId}, CourseId: {CourseId}, EnrollmentId: {EnrollmentId}", 
            notification.StudentId, notification.CourseId, notification.EnrollmentId);
        
        try
        {
            // Criar um registro de pagamento pendente para a matrícula
            // Como não temos os detalhes do cartão neste momento, usamos valores temporários
            var cardDetails = new CardDetails(
                "Pending Payment",
                "4111111111111111", // Número de cartão de teste válido
                "12",
                "2030"
            );
            
            var payment = new Payment(
                notification.StudentId,
                notification.EnrollmentId,
                100.0m, // Valor padrão, será atualizado posteriormente
                cardDetails
            );
            
            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created pending payment record for enrollment: {EnrollmentId}, PaymentId: {PaymentId}",
                notification.EnrollmentId, payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment record for enrollment: {EnrollmentId}", notification.EnrollmentId);
            throw;
        }
    }
} 