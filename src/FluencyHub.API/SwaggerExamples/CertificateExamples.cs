using FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;
using FluencyHub.Application.StudentManagement.Queries.GetCertificateById;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace FluencyHub.API.SwaggerExamples;

public class GenerateCertificateCommandExample : IExamplesProvider<GenerateCertificateCommand>
{
    public GenerateCertificateCommand GetExamples()
    {
        return new GenerateCertificateCommand
        {
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };
    }
}

public class CertificateDtoExample : IExamplesProvider<CertificateDto>
{
    public CertificateDto GetExamples()
    {
        return new CertificateDto
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            StudentName = "Maria Silva",
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CourseName = "English for Beginners",
            Title = "Certificate of Completion - English for Beginners",
            IssueDate = DateTime.Now.AddDays(-5)
        };
    }
}

public class CertificateListDtoExample : IExamplesProvider<List<CertificateDto>>
{
    public List<CertificateDto> GetExamples()
    {
        return new List<CertificateDto>
        {
            new CertificateDto
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                StudentName = "Maria Silva",
                CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CourseName = "English for Beginners",
                Title = "Certificate of Completion - English for Beginners",
                IssueDate = DateTime.Now.AddDays(-5)
            },
            new CertificateDto
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                StudentName = "Maria Silva",
                CourseId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                CourseName = "Intermediate Spanish",
                Title = "Certificate of Completion - Intermediate Spanish",
                IssueDate = DateTime.Now.AddDays(-20)
            }
        };
    }
} 