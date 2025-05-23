using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Domain.Events;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
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
            _logger.LogInformation("Curso criado: {CourseId}", notification.CourseId);

            // Publicar evento de integração
            var integrationEvent = new CourseCreatedEvent(
                notification.CourseId,
                notification.Name,
                notification.Description,
                notification.Content.Syllabus,
                notification.Content.LearningObjectives,
                notification.Content.PreRequisites,
                notification.Content.TargetAudience,
                notification.Content.Language,
                notification.Content.Level,
                notification.Price,
                notification.IsActive,
                notification.Status);

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
            _logger.LogInformation("Curso atualizado: {CourseId}", notification.CourseId);

            // Publicar evento de integração
            var integrationEvent = new CourseUpdatedEvent(
                notification.CourseId,
                notification.Name,
                notification.Description,
                notification.Content.Syllabus,
                notification.Content.LearningObjectives,
                notification.Content.PreRequisites,
                notification.Content.TargetAudience,
                notification.Content.Language,
                notification.Content.Level,
                notification.Price,
                notification.IsActive,
                notification.Status);

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
        private readonly FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository _courseRepository;
        private readonly ILogger<GetCourseByIdHandler> _logger;

        public GetCourseByIdHandler(
            FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository courseRepository,
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

    public class CourseEventHandlers :
        INotificationHandler<CourseCreatedDomainEvent>,
        INotificationHandler<CourseUpdatedDomainEvent>,
        INotificationHandler<CourseDeletedDomainEvent>
    {
        private readonly FluencyHub.ContentManagement.Domain.ICourseRepository _courseRepository;
        private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;
        private readonly ILogger<CourseEventHandlers> _logger;

        public CourseEventHandlers(
            FluencyHub.ContentManagement.Domain.ICourseRepository courseRepository,
            FluencyHub.SharedKernel.Events.IDomainEventService eventService,
            ILogger<CourseEventHandlers> logger)
        {
            _courseRepository = courseRepository;
            _eventService = eventService;
            _logger = logger;
        }

        public async Task Handle(CourseCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CourseCreatedDomainEvent for course {CourseId}", notification.CourseId);

            var courseContent = new CourseContent(
                notification.Content.Syllabus,
                notification.Content.LearningObjectives,
                notification.Content.PreRequisites,
                notification.Content.TargetAudience,
                notification.Content.Language,
                notification.Content.Level);

            var course = new Course(
                notification.Name,
                notification.Description,
                courseContent,
                notification.Price)
            {
                Name = notification.Name,
                Description = notification.Description,
                Content = courseContent
            };

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Course {CourseId} created successfully", course.Id);
        }

        public async Task Handle(CourseUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CourseUpdatedDomainEvent for course {CourseId}", notification.CourseId);

            var course = await _courseRepository.GetByIdAsync(notification.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found", notification.CourseId);
                return;
            }

            var courseContent = new CourseContent(
                notification.Content.Syllabus,
                notification.Content.LearningObjectives,
                notification.Content.PreRequisites,
                notification.Content.TargetAudience,
                notification.Content.Language,
                notification.Content.Level);

            course.UpdateDetails(
                notification.Name,
                notification.Description,
                courseContent,
                notification.Price);

            await _courseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Course {CourseId} updated successfully", course.Id);
        }

        public async Task Handle(CourseDeletedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CourseDeletedDomainEvent for course {CourseId}", notification.CourseId);

            var course = await _courseRepository.GetByIdAsync(notification.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found", notification.CourseId);
                return;
            }

            await _courseRepository.DeleteAsync(course.Id);
            await _courseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Course {CourseId} deleted successfully", course.Id);
        }
    }
} 