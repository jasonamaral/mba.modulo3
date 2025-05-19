using FluencyHub.StudentManagement.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.StudentManagement.Domain;

public class Student : BaseEntity
{
    [JsonIgnore]
    private readonly List<Enrollment> _enrollments = [];

    [JsonIgnore]
    private readonly List<Certificate> _certificates = [];

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTime DateOfBirth { get; private set; }

    [JsonIgnore]
    public LearningHistory LearningHistory { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<Certificate> Certificates => _certificates.AsReadOnly();

    public bool IsActive { get; private set; }

    private Student()
    { }

    public Student(string firstName, string lastName, string email, DateTime dateOfBirth, string? phoneNumber = null)
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
        PhoneNumber = phoneNumber;
        LearningHistory = new LearningHistory(Id);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdateDetails(string firstName, string lastName, DateTime dateOfBirth, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("O nome não pode estar vazio", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("O sobrenome não pode estar vazio", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public Enrollment EnrollInCourse(Guid courseId, decimal price)
    {
        if (!IsActive)
            throw new InvalidOperationException("Não é possível matricular um estudante inativo");

        if (_enrollments.Any(e => e.CourseId == courseId && e.Status != StatusMatricula.Cancelada))
            throw new InvalidOperationException("O estudante já está matriculado neste curso");

        var enrollment = new Enrollment(this.Id, courseId, price);
        _enrollments.Add(enrollment);
        UpdatedAt = DateTime.UtcNow;
        return enrollment;
    }

    public void AddCertificate(Guid courseId, string title)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId && e.Status == StatusMatricula.Concluida) ?? throw new InvalidOperationException("O estudante não concluiu este curso");

        if (_certificates.Any(c => c.CourseId == courseId))
            throw new InvalidOperationException("O certificado para este curso já existe");

        var certificate = new Certificate(this.Id, courseId, title);
        _certificates.Add(certificate);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordProgress(Guid courseId, Guid lessonId)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId && e.Status == StatusMatricula.Ativa);
        if (enrollment == null)
            throw new InvalidOperationException("O estudante não está ativamente matriculado neste curso");

        LearningHistory.AddProgress(courseId, lessonId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteCourse(Guid courseId)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId && e.Status == StatusMatricula.Ativa) ?? throw new InvalidOperationException("O estudante não está ativamente matriculado neste curso");
        enrollment.CompleteEnrollment();
        LearningHistory.CompleteCourse(courseId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";
} 