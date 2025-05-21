using FluencyHub.SharedKernel.Events.ContentManagement;
using FluencyHub.SharedKernel.Queries;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly ILogger<CourseEventConsumerService> _logger;
        private const string CacheKeyPrefix = "Course_";

        public CourseEventConsumerService(
            IMemoryCache cache,
            IMediator mediator,
            ILogger<CourseEventConsumerService> logger)
        {
            _cache = cache;
            _mediator = mediator;
            _logger = logger;
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
        public Task Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recebido evento CourseCreatedEvent para o curso {CourseId}", notification.CourseId);
            
            var courseInfo = new CourseInfo
            {
                Id = notification.CourseId,
                Name = notification.Title,
                Description = notification.Description,
                Price = notification.Price
            };
            
            _cache.Set($"{CacheKeyPrefix}{notification.CourseId}", courseInfo, TimeSpan.FromMinutes(30));
            
            return Task.CompletedTask;
        }

        public Task Handle(CourseUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recebido evento CourseUpdatedEvent para o curso {CourseId}", notification.CourseId);
            
            var courseInfo = new CourseInfo
            {
                Id = notification.CourseId,
                Name = notification.Title,
                Description = notification.Description,
                Price = notification.Price
            };
            
            _cache.Set($"{CacheKeyPrefix}{notification.CourseId}", courseInfo, TimeSpan.FromMinutes(30));
            
            return Task.CompletedTask;
        }

        public Task Handle(CourseDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recebido evento CourseDeletedEvent para o curso {CourseId}", notification.CourseId);
            
            _cache.Remove($"{CacheKeyPrefix}{notification.CourseId}");
            
            return Task.CompletedTask;
        }
    }
} 