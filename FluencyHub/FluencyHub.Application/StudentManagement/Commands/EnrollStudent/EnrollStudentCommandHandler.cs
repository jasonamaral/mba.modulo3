using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.EnrollStudent;

public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Guid>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;

    public EnrollStudentCommandHandler(
        FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository,
        FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Guid> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
            throw new NotFoundException("Student", request.StudentId);

        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
            throw new NotFoundException("Course", request.CourseId);

        var enrollment = student.EnrollInCourse(course.Id, course.Price);
        
        await _studentRepository.SaveChangesAsync(cancellationToken);
        
        return enrollment.Id;
    }
} 