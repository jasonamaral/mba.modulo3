using FluencyHub.StudentManagement.Application.Commands.GenerateCertificate;
using FluencyHub.StudentManagement.Application.Queries.GetCertificateById;
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
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            IssueDate = DateTime.Now
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
            CertificateNumber = "CERT-2023-00001",
            IssueDate = DateTime.Now.AddDays(-5),
            Score = 95,
            Feedback = "Excellent performance"
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
                CertificateNumber = "CERT-2023-00001",
                IssueDate = DateTime.Now.AddDays(-5),
                Score = 95,
                Feedback = "Excellent performance"
            },
            new CertificateDto
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                StudentName = "Maria Silva",
                CourseId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                CourseName = "Intermediate Spanish",
                CertificateNumber = "CERT-2023-00002",
                IssueDate = DateTime.Now.AddDays(-20),
                Score = 85,
                Feedback = "Very good performance"
            }
        };
    }
} 