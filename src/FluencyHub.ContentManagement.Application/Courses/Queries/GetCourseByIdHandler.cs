using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.SharedKernel.Queries;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluencyHub.ContentManagement.Application.Courses.Queries
{
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