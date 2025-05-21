using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Domain.Events;
using FluencyHub.SharedKernel.Events;
using FluencyHub.SharedKernel.Events.ContentManagement;
using FluencyHub.SharedKernel.Queries;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluencyHub.ContentManagement.Application.Courses.EventHandlers
{
    /// <summary>
    /// Handler para o evento de curso criado no domínio
    /// </summary>
    public class CourseCreatedDomainEventHandler : INotificationHandler<CourseCreatedDomainEvent>
    {
        private readonly FluencyHub.SharedKernel.Events.IDomainEventService _domainEventService;
        private readonly ILogger<CourseCreatedDomainEventHandler> _logger;

        public CourseCreatedDomainEventHandler(
            FluencyHub.SharedKernel.Events.IDomainEventService domainEventService,
            ILogger<CourseCreatedDomainEventHandler> logger)
        {
            _domainEventService = domainEventService;
            _logger = logger;
        }

        public async Task Handle(CourseCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Curso criado: {CourseId}", notification.Course.Id);

            // Publicar evento de integração
            var integrationEvent = new CourseCreatedEvent(
                notification.Course.Id,
                notification.Course.Name,
                notification.Course.Description ?? string.Empty,
                notification.Course.Lessons.Count,
                notification.Course.Price);

            await _domainEventService.PublishEventAsync(integrationEvent);
        }
    }

    /// <summary>
    /// Handler para o evento de curso atualizado no domínio
    /// </summary>
    public class CourseUpdatedDomainEventHandler : INotificationHandler<CourseUpdatedDomainEvent>
    {
        private readonly FluencyHub.SharedKernel.Events.IDomainEventService _domainEventService;
        private readonly ILogger<CourseUpdatedDomainEventHandler> _logger;

        public CourseUpdatedDomainEventHandler(
            FluencyHub.SharedKernel.Events.IDomainEventService domainEventService,
            ILogger<CourseUpdatedDomainEventHandler> logger)
        {
            _domainEventService = domainEventService;
            _logger = logger;
        }

        public async Task Handle(CourseUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Curso atualizado: {CourseId}", notification.Course.Id);

            // Publicar evento de integração
            var integrationEvent = new CourseUpdatedEvent(
                notification.Course.Id,
                notification.Course.Name,
                notification.Course.Description ?? string.Empty,
                notification.Course.Lessons.Count,
                notification.Course.Price,
                notification.Course.IsActive);

            await _domainEventService.PublishEventAsync(integrationEvent);
        }
    }

    /// <summary>
    /// Handler para o evento de curso excluído no domínio
    /// </summary>
    public class CourseDeletedDomainEventHandler : INotificationHandler<CourseDeletedDomainEvent>
    {
        private readonly FluencyHub.SharedKernel.Events.IDomainEventService _domainEventService;
        private readonly ILogger<CourseDeletedDomainEventHandler> _logger;

        public CourseDeletedDomainEventHandler(
            FluencyHub.SharedKernel.Events.IDomainEventService domainEventService,
            ILogger<CourseDeletedDomainEventHandler> logger)
        {
            _domainEventService = domainEventService;
            _logger = logger;
        }

        public async Task Handle(CourseDeletedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Curso excluído: {CourseId}", notification.CourseId);

            // Publicar evento de integração
            var integrationEvent = new CourseDeletedEvent(notification.CourseId);

            await _domainEventService.PublishEventAsync(integrationEvent);
        }
    }

    /// <summary>
    /// Handler para a consulta GetCourseById
    /// </summary>
    public class GetCourseByIdHandler : IRequestHandler<GetCourseById, CourseDto?>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<GetCourseByIdHandler> _logger;

        public GetCourseByIdHandler(
            ICourseRepository courseRepository,
            ILogger<GetCourseByIdHandler> logger)
        {
            _courseRepository = courseRepository;
            _logger = logger;
        }

        public async Task<CourseDto?> Handle(GetCourseById request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consultando curso por ID: {CourseId}", request.CourseId);

            try
            {
                var exists = await _courseRepository.ExistsAsync(request.CourseId);
                
                if (!exists)
                {
                    _logger.LogWarning("Curso não encontrado: {CourseId}", request.CourseId);
                    return null;
                }
                
                var course = await _courseRepository.GetByIdAsync(request.CourseId);
                
                if (course == null)
                    return null;

                return new CourseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Description = course.Description,
                    Price = course.Price
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar curso {CourseId}", request.CourseId);
                return null;
            }
        }
    }
} 