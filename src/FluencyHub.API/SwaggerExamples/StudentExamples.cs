using FluencyHub.Application.StudentManagement.Commands.ActivateStudent;
using FluencyHub.Application.StudentManagement.Commands.CompleteCourseForStudent;
using FluencyHub.Application.StudentManagement.Commands.CompleteLessonForStudent;
using FluencyHub.Application.StudentManagement.Commands.CreateStudent;
using FluencyHub.Application.StudentManagement.Commands.DeactivateStudent;
using FluencyHub.Application.StudentManagement.Commands.UpdateStudent;
using FluencyHub.Application.StudentManagement.Queries.GetAllStudents;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentProgress;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using GetStudentByIdDto = FluencyHub.Application.StudentManagement.Queries.GetStudentById.StudentDto;
using GetAllStudentsDto = FluencyHub.Application.StudentManagement.Queries.GetAllStudents.StudentDto;

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
            DateOfBirth = new DateTime(1995, 5, 15),
            Password = "Senha123!"
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
            CreatedAt = DateTime.Now.AddDays(-60),
            UpdatedAt = DateTime.Now.AddDays(-30)
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
                UserId = "user-id-1",
                FirstName = "Maria",
                LastName = "Silva",
                Email = "maria.silva@example.com",
                Phone = "+5511987654321",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-60),
                UpdatedAt = DateTime.Now.AddDays(-30)
            },
            new GetAllStudentsDto
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = "user-id-2",
                FirstName = "João",
                LastName = "Santos",
                Email = "joao.santos@example.com",
                Phone = "+5511912345678",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-90),
                UpdatedAt = DateTime.Now.AddDays(-15)
            }
        };
    }
}

public class ActivateStudentCommandExample : IExamplesProvider<ActivateStudentCommand>
{
    public ActivateStudentCommand GetExamples()
    {
        return new ActivateStudentCommand(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    }
}

public class DeactivateStudentCommandExample : IExamplesProvider<DeactivateStudentCommand>
{
    public DeactivateStudentCommand GetExamples()
    {
        return new DeactivateStudentCommand(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    }
}

public class StudentProgressViewModelExample : IExamplesProvider<StudentProgressViewModel>
{
    public StudentProgressViewModel GetExamples()
    {
        return new StudentProgressViewModel
        {
            Progress = new Dictionary<Guid, CourseProgressDto>
            {
                {
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    new CourseProgressDto
                    {
                        CompletedLessons = 3,
                        TotalLessons = 5,
                        IsCompleted = false,
                        LastUpdated = DateTime.Now.AddDays(-2)
                    }
                },
                {
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    new CourseProgressDto
                    {
                        CompletedLessons = 8,
                        TotalLessons = 8,
                        IsCompleted = true,
                        LastUpdated = DateTime.Now.AddDays(-5)
                    }
                }
            }
        };
    }
}

public class CompleteLessonForStudentCommandExample : IExamplesProvider<CompleteLessonForStudentCommand>
{
    public CompleteLessonForStudentCommand GetExamples()
    {
        return new CompleteLessonForStudentCommand(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("44444444-4444-4444-4444-444444444444")
        );
    }
}

public class CompleteLessonForStudentResultExample : IExamplesProvider<CompleteLessonForStudentResult>
{
    public CompleteLessonForStudentResult GetExamples()
    {
        return new CompleteLessonForStudentResult
        {
            StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            LessonId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Message = "Lesson completed successfully",
            Success = true
        };
    }
}

public class CompleteCourseForStudentCommandExample : IExamplesProvider<CompleteCourseForStudentCommand>
{
    public CompleteCourseForStudentCommand GetExamples()
    {
        return new CompleteCourseForStudentCommand(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Guid.Parse("11111111-1111-1111-1111-111111111111")
        );
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
            Message = "Course completed successfully",
            Success = true,
            CompletedLessons = 8,
            TotalLessons = 8
        };
    }
} 