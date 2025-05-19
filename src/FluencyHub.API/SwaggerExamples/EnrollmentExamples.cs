using FluencyHub.API.Models;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.StudentManagement.Domain;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace FluencyHub.API.SwaggerExamples;

public class EnrollmentCreateRequestExample : IExamplesProvider<EnrollmentCreateRequest>
{
    public EnrollmentCreateRequest GetExamples()
    {
        return new EnrollmentCreateRequest
        {
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222")
        };
    }
}

public class EnrollmentDtoExample : IExamplesProvider<EnrollmentDto>
{
    public EnrollmentDto GetExamples()
    {
        return new EnrollmentDto
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CourseName = "English for Beginners",
            Price = 99.99m,
            Status = StatusMatricula.AguardandoPagamento.ToString(),
            EnrollmentDate = DateTime.Now.AddDays(-7),
            ActivationDate = null,
            CompletionDate = null
        };
    }
}

public class LessonCompleteRequestExample : IExamplesProvider<LessonCompleteRequest>
{
    public LessonCompleteRequest GetExamples()
    {
        return new LessonCompleteRequest
        {
            Completed = true
        };
    }
} 