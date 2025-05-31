using MediatR;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Enums;
using FluencyHub.SharedKernel.Queries;
using FluencyHub.SharedKernel.Contracts;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IEnrollmentRepository;

namespace FluencyHub.StudentManagement.Application.Commands.EnrollStudent;

public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Guid>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMediator _mediator;

    public EnrollStudentCommandHandler(
        IStudentRepository studentRepository,
        IEnrollmentRepository enrollmentRepository,
        IMediator mediator)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        

        // Verificar se o estudante existe
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new NotFoundException($"Estudante com ID {request.StudentId} não encontrado");
        }

        if (!student.IsActive)
        {
            throw new InvalidOperationException("Não é possível matricular um estudante inativo");
        }

        // Verificar se o curso existe usando query compartilhada
        var courseExists = await _mediator.Send(new CourseExists { CourseId = request.CourseId }, cancellationToken);
        if (!courseExists)
        {
            throw new NotFoundException($"Curso com ID {request.CourseId} não encontrado");
        }

        // Obter informações do curso
        var courseInfo = await _mediator.Send(new GetCourseById { CourseId = request.CourseId }, cancellationToken);
        if (courseInfo == null)
        {
            throw new NotFoundException($"Curso com ID {request.CourseId} não encontrado");
        }

        // Verificar se já existe uma matrícula ativa para este estudante e curso
        var existingEnrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(
            request.StudentId, request.CourseId);

        if (existingEnrollment != null)
        {
            if (existingEnrollment.Status == StatusMatricula.Ativa || 
                existingEnrollment.Status == StatusMatricula.AguardandoPagamento)
            {
                throw new InvalidOperationException(
                    $"Student is already enrolled in course {request.CourseId} with status {existingEnrollment.Status}");
            }
        }

        // Calcular o preço final com desconto se aplicável
        var finalPrice = courseInfo.Price;
        if (request.DiscountPercentage.HasValue && request.DiscountPercentage.Value > 0)
        {
            var discountAmount = finalPrice * (request.DiscountPercentage.Value / 100);
            finalPrice -= discountAmount;
            

        }

        // Criar a matrícula
        var enrollment = new Enrollment(request.StudentId, request.CourseId, finalPrice)
        {
            Student = student,
            Course = new CourseReference(courseInfo.Id, courseInfo.Name, courseInfo.Description, courseInfo.Price)
        };

        await _enrollmentRepository.AddAsync(enrollment);
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);



        return enrollment.Id;
    }
}

// Classe auxiliar para representar informações do curso
public class CourseReference : ICourse
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Language { get; }
    public string Level { get; }
    public decimal Price { get; }
    public bool IsActive { get; }

    public CourseReference(Guid id, string name, string description, decimal price, 
        string language = "", string level = "", bool isActive = true)
    {
        Id = id;
        Name = name;
        Description = description;
        Language = language;
        Level = level;
        Price = price;
        IsActive = isActive;
    }
} 