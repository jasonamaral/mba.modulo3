using FluencyHub.SharedKernel.Events.ContentManagement;
using FluencyHub.SharedKernel.Queries;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace FluencyHub.StudentManagement.Infrastructure.Services
{
    /// <summary>
    /// Serviço que consome eventos de cursos e mantém um cache local para consultas
    /// </summary>
    public class CourseEventConsumerService : 
        ICourseRepository,
        INotificationHandler<CourseCreatedEvent>,
        INotificationHandler<CourseUpdatedEvent>,
        INotificationHandler<CourseDeletedEvent>
    {
        private readonly IMemoryCache _cache;
        private readonly IMediator _mediator;
        private const string CacheKeyPrefix = "Course_";

        public CourseEventConsumerService(
            IMemoryCache cache,
            IMediator mediator)
        {
            _cache = cache;
            _mediator = mediator;
        }

        // Implementação de ICourseRepository
        public async Task<CourseInfo?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            
            if (_cache.TryGetValue(cacheKey, out CourseInfo? courseInfo))
            {
                return courseInfo;
            }

            // Se não estiver em cache, busca via mediator
            var query = new GetCourseById { CourseId = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
                return null;
                
            courseInfo = new CourseInfo
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                Price = result.Price
            };

            // Armazena em cache
            _cache.Set(cacheKey, courseInfo, TimeSpan.FromMinutes(30));
            
            return courseInfo;
        }

        public async Task<bool> ExistsAsync(Guid courseId)
        {
            var cacheKey = $"{CacheKeyPrefix}{courseId}";
            
            if (_cache.TryGetValue(cacheKey, out CourseInfo? _))
            {
                return true;
            }

            // Se não estiver em cache, verifica via mediator
            var query = new CourseExists { CourseId = courseId };
            return await _mediator.Send(query);
        }

        public async Task<string> GetNameAsync(Guid courseId)
        {
            var cacheKey = $"{CacheKeyPrefix}{courseId}";
            
            if (_cache.TryGetValue(cacheKey, out CourseInfo? courseInfo))
            {
                return courseInfo?.Name ?? string.Empty;
            }

            // Se não estiver em cache, busca via mediator
            var query = new GetCourseName { CourseId = courseId };
            return await _mediator.Send(query);
        }

        // Handlers de eventos
        public async Task Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
        {
            // Lógica para processar o evento de curso criado
            // Por exemplo, atualizar cache local ou notificar outros serviços
            await Task.CompletedTask;
        }

        public async Task Handle(CourseUpdatedEvent notification, CancellationToken cancellationToken)
        {
            // Lógica para processar o evento de curso atualizado
            // Por exemplo, invalidar cache ou atualizar dados locais
            await Task.CompletedTask;
        }

        public async Task Handle(CourseDeletedEvent notification, CancellationToken cancellationToken)
        {
            // Lógica para processar o evento de curso excluído
            // Por exemplo, limpar dados relacionados ou notificar usuários
            await Task.CompletedTask;
        }
    }
} 