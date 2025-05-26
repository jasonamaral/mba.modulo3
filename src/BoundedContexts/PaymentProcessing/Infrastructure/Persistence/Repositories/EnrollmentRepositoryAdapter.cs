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
        private readonly IServiceProvider _serviceProvider;
        private const string AGUARDANDO_PAGAMENTO = "AguardandoPagamento";

        public EnrollmentRepositoryAdapter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private IStudentManagementEnrollmentRepository GetRepository()
        {
            // Obter o serviço do StudentManagement de forma lazy para evitar dependências circulares
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IStudentManagementEnrollmentRepository>();
        }

        public async Task<IEnrollment?> GetByIdAsync(Guid id)
        {
            return await GetRepository().GetByIdAsync(id);
        }

        public async Task<IEnrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
        {
            return await GetRepository().GetByStudentAndCourseAsync(studentId, courseId);
        }

        public async Task<IEnrollment?> GetPendingEnrollmentAsync(Guid studentId, Guid courseId)
        {
            var enrollment = await GetRepository().GetByStudentAndCourseAsync(studentId, courseId);
            if (enrollment != null && string.Equals(enrollment.Status, AGUARDANDO_PAGAMENTO))
            {
                return enrollment;
            }
            return null;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await GetRepository().SaveChangesAsync(cancellationToken);
        }
    }
} 