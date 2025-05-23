using System;
using System.Threading.Tasks;
using FluencyHub.PaymentProcessing.Domain.Events;

namespace FluencyHub.PaymentProcessing.Domain
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGateway _paymentGateway;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPaymentGateway paymentGateway)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        }

        public async Task<PaymentResult> ProcessPaymentAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var gatewayResult = await _paymentGateway.ProcessPaymentAsync(
                payment.Id.ToString(),
                payment.Amount,
                payment.CardDetails);

            var result = new PaymentResult
            {
                IsSuccessful = gatewayResult.IsSuccessful,
                TransactionId = gatewayResult.TransactionId,
                Message = gatewayResult.ErrorMessage
            };

            if (result.IsSuccessful)
            {
                payment.MarkAsSuccess(result.TransactionId!);
                await _paymentRepository.UpdateAsync(payment);
            }

            return result;
        }

        public async Task<PaymentResult> RefundPaymentAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            if (!payment.IsSuccessful)
                throw new InvalidOperationException("Não é possível reembolsar um pagamento que não foi bem-sucedido.");

            if (payment.IsRefunded)
                throw new InvalidOperationException("Este pagamento já foi reembolsado.");

            if (string.IsNullOrEmpty(payment.TransactionId))
                throw new InvalidOperationException("ID da transação não encontrado para este pagamento.");

            var refundResult = await _paymentGateway.ProcessRefundAsync(
                payment.TransactionId,
                payment.Amount,
                "Solicitação de reembolso");

            var result = new PaymentResult
            {
                IsSuccessful = refundResult.IsSuccessful,
                TransactionId = refundResult.TransactionId,
                Message = refundResult.ErrorMessage
            };

            if (result.IsSuccessful)
            {
                payment.MarkAsRefunded("Reembolso processado com sucesso");
                await _paymentRepository.UpdateAsync(payment);
            }

            return result;
        }
    }
} 