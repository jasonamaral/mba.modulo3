using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.SharedKernel.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluencyHub.PaymentProcessing.Infrastructure.Persistence.Repositories
{
    public class EnrollmentRepositoryAdapter : FluencyHub.PaymentProcessing.Application.Common.Interfaces.IEnrollmentRepository
    {
        private readonly IStudentManagementEnrollmentRepository _repository;
        private const string AGUARDANDO_PAGAMENTO = "AguardandoPagamento";

        public EnrollmentRepositoryAdapter(IStudentManagementEnrollmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnrollment?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
        {
            return await _repository.GetByStudentAndCourseAsync(studentId, courseId);
        }

        public async Task<IEnrollment?> GetPendingEnrollmentAsync(Guid studentId, Guid courseId)
        {
            var enrollment = await _repository.GetByStudentAndCourseAsync(studentId, courseId);
            if (enrollment != null && string.Equals(enrollment.Status, AGUARDANDO_PAGAMENTO))
            {
                return enrollment;
            }
            return null;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
} 