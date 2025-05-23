using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.SharedKernel.Queries;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluencyHub.ContentManagement.Application.Handlers.SharedQueries
{
    public class CourseExistsHandler : IRequestHandler<CourseExists, bool>
    {
        private readonly ICourseRepository _courseRepository;

        public CourseExistsHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        }

        public async Task<bool> Handle(CourseExists request, CancellationToken cancellationToken)
        {
            return await _courseRepository.ExistsAsync(request.CourseId);
        }
    }

    public class GetCourseByIdHandler : IRequestHandler<GetCourseById, CourseDto?>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCourseByIdHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        }

        public async Task<CourseDto?> Handle(GetCourseById request, CancellationToken cancellationToken)
        {
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
    }

    public class GetCourseNameHandler : IRequestHandler<GetCourseName, string>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCourseNameHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        }

        public async Task<string> Handle(GetCourseName request, CancellationToken cancellationToken)
        {
            return await _courseRepository.GetNameAsync(request.CourseId);
        }
    }
} 