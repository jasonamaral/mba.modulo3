using FluencyHub.StudentManagement.Application.Commands.ActivateStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;
using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluencyHub.StudentManagement.Application.Commands.DeactivateStudent;
using FluencyHub.StudentManagement.Application.Commands.UpdateStudent;
using FluencyHub.StudentManagement.Application.Queries.GetAllStudents;
using FluencyHub.StudentManagement.Application.Queries.GetStudentById;
using FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using GetStudentByIdDto = FluencyHub.StudentManagement.Application.Queries.GetStudentById.StudentDto;
using GetAllStudentsDto = FluencyHub.StudentManagement.Application.Queries.GetAllStudents.StudentDto;

namespace FluencyHub.API.SwaggerExamples;

public class CreateStudentCommandExample : IExamplesProvider<CreateStudentCommand>
{
    public CreateStudentCommand GetExamples()
    {
        return new CreateStudentCommand
        {
            FirstName = "Maria",
            LastName = "Silva",
            Email = "maria.silva@example.com",
            PhoneNumber = "+5511987654321",
            DateOfBirth = new DateTime(1995, 5, 15)
        };
    }
}

public class UpdateStudentCommandExample : IExamplesProvider<UpdateStudentCommand>
{
    public UpdateStudentCommand GetExamples()
    {
        return new UpdateStudentCommand
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FirstName = "Maria",
            LastName = "Santos",
            Email = "maria.santos@example.com",
            PhoneNumber = "+5511987654321",
            DateOfBirth = new DateTime(1995, 5, 15)
        };
    }
}

public class StudentDtoExample : IExamplesProvider<GetStudentByIdDto>
{
    public GetStudentByIdDto GetExamples()
    {
        return new GetStudentByIdDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FirstName = "Maria",
            LastName = "Silva",
            FullName = "Maria Silva",
            Email = "maria.silva@example.com",
            PhoneNumber = "+5511987654321",
            DateOfBirth = new DateTime(1995, 5, 15),
            IsActive = true,
            EnrollmentsCount = 2,
            CertificatesCount = 1,
            CreatedAt = DateTime.Now.AddDays(-60)
        };
    }
}

public class StudentListDtoExample : IExamplesProvider<List<GetAllStudentsDto>>
{
    public List<GetAllStudentsDto> GetExamples()
    {
        return new List<GetAllStudentsDto>
        {
            new GetAllStudentsDto
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Maria",
                LastName = "Silva",
                Email = "maria.silva@example.com",
                PhoneNumber = "+5511987654321",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-60)
            },
            new GetAllStudentsDto
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FirstName = "João",
                LastName = "Santos",
                Email = "joao.santos@example.com",
                PhoneNumber = "+5511912345678",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-90)
            }
        };
    }
}

public class ActivateStudentCommandExample : IExamplesProvider<ActivateStudentCommand>
{
    public ActivateStudentCommand GetExamples()
    {
        return new ActivateStudentCommand
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222")
        };
    }
}

public class DeactivateStudentCommandExample : IExamplesProvider<DeactivateStudentCommand>
{
    public DeactivateStudentCommand GetExamples()
    {
        return new DeactivateStudentCommand
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222")
        };
    }
}

// Comentado temporariamente pois a classe relacionada não foi definida
/*
public class StudentProgressViewModelExample : IExamplesProvider<StudentProgressViewModel>
{
    public StudentProgressViewModel GetExamples()
    {
        return new StudentProgressViewModel
        {
            StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Courses = new List<CourseProgressDto>
            {
                new CourseProgressDto
                {
                    CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CompletedLessonsCount = 3,
                    TotalLessonsCount = 5,
                    CompletionPercentage = 60
                },
                new CourseProgressDto
                {
                    CourseId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    CompletedLessonsCount = 8,
                    TotalLessonsCount = 8,
                    CompletionPercentage = 100
                }
            }
        };
    }
}
*/

public class CompleteLessonForStudentCommandExample : IExamplesProvider<CompleteLessonForStudentCommand>
{
    public CompleteLessonForStudentCommand GetExamples()
    {
        return new CompleteLessonForStudentCommand
        {
            StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            LessonId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            CompletionDate = DateTime.Now,
            Score = 85
        };
    }
}

public class CompleteLessonForStudentResultExample : IExamplesProvider<CompleteLessonForStudentResult>
{
    public CompleteLessonForStudentResult GetExamples()
    {
        return new CompleteLessonForStudentResult
        {
            StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            LessonId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            IsSuccessful = true,
            Score = 85,
            CourseProgress = 60,
            CourseCompleted = false
        };
    }
}

public class CompleteCourseForStudentCommandExample : IExamplesProvider<CompleteCourseForStudentCommand>
{
    public CompleteCourseForStudentCommand GetExamples()
    {
        return new CompleteCourseForStudentCommand
        {
            StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CompletionDate = DateTime.Now,
            FinalScore = 92
        };
    }
}

public class CompleteCourseForStudentResultExample : IExamplesProvider<CompleteCourseForStudentResult>
{
    public CompleteCourseForStudentResult GetExamples()
    {
        return new CompleteCourseForStudentResult
        {
            StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            IsSuccessful = true,
            FinalScore = 92,
            CertificateGenerated = true,
            CertificateId = Guid.Parse("55555555-5555-5555-5555-555555555555")
        };
    }
} 