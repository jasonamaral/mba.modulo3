using FluencyHub.StudentManagement.Domain.Common;
using FluencyHub.StudentManagement.Domain.Enums;
using FluencyHub.StudentManagement.Domain.Events;
using FluencyHub.StudentManagement.Domain.Models;
using FluencyHub.SharedKernel.Contracts;
using System.Text.Json.Serialization;

namespace FluencyHub.StudentManagement.Domain;

public class Student : BaseEntity
{
    [JsonIgnore]
    private readonly List<Enrollment> _enrollments = [];

    [JsonIgnore]
    private readonly List<Certificate> _certificates = [];

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }

    [JsonIgnore]
    public required LearningHistory LearningHistory { get; set; }

    [JsonIgnore]
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<Certificate> Certificates => _certificates.AsReadOnly();

    public new bool IsActive { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    private Student()
    { }

    public Student(
        string firstName,
        string lastName,
        string email,
        DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("O nome não pode estar vazio", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("O sobrenome não pode estar vazio", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O email não pode estar vazio", nameof(email));

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DateOfBirth = dateOfBirth;
        IsActive = true;
        LearningHistory = new LearningHistory(Id);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string? address,
        string? city,
        string? state,
        string? country)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("O nome não pode estar vazio", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("O sobrenome não pode estar vazio", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        State = state;
        Country = country;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StudentDeactivatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StudentActivatedEvent(Id));
    }

    public void EnrollInCourse(CourseReference course)
    {
        if (!IsActive)
            throw new InvalidOperationException("Estudante inativo não pode se matricular em cursos");

        if (_enrollments.Any(e => e.CourseId == course.Id && e.Status == StatusMatricula.Ativa))
            throw new InvalidOperationException("Estudante já está matriculado neste curso");

        var enrollment = new Enrollment(Id, course.Id, course.Price)
        {
            Student = this,
            Course = (ICourse)course
        };
        _enrollments.Add(enrollment);

        AddDomainEvent(new StudentEnrolledEvent(Id, course.Id, enrollment.Id));
    }

    public void CompleteCourse(CourseReference course)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == course.Id) ?? throw new InvalidOperationException("O estudante não está matriculado neste curso");
        if (enrollment.Status == StatusMatricula.Concluida)
            throw new InvalidOperationException("Curso já foi concluído");

        enrollment.CompleteEnrollment();
        var certificate = new Certificate(Id, course.Id, $"Certificado do curso {course.Name}")
        {
            Student = this,
            Course = (ICourse)course,
            Title = $"Certificado do curso {course.Name}",
            CertificateNumber = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
        };
        _certificates.Add(certificate);

        AddDomainEvent(new CertificateIssuedEvent(Id, course.Id, certificate.Id));
    }

    public void RecordProgress(Guid courseId, Guid lessonId)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId && e.Status == StatusMatricula.Ativa);
        if (enrollment == null)
            throw new InvalidOperationException("O estudante não está ativamente matriculado neste curso");

        LearningHistory.AddProgress(courseId, lessonId);
        UpdatedAt = DateTime.UtcNow;
    }
} 