using MediatR;
using System;

namespace FluencyHub.SharedKernel.Queries
{
    public class CourseExists : IRequest<bool>
    {
        public Guid CourseId { get; set; }
    }

    public class GetCourseById : IRequest<CourseDto?>
    {
        public Guid CourseId { get; set; }
    }

    public class GetCourseName : IRequest<string>
    {
        public Guid CourseId { get; set; }
    }

    public class CourseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
} 