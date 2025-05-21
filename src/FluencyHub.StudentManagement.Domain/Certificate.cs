using FluencyHub.StudentManagement.Domain.Common;
using FluencyHub.ContentManagement.Domain;
using System.Text.Json.Serialization;

namespace FluencyHub.StudentManagement.Domain;

public class Certificate : BaseEntity
{
    public Guid StudentId { get; private set; }
    public Guid CourseId { get; private set; }
    public string Title { get; private set; }
    public DateTime IssueDate { get; private set; }
    public string CertificateNumber { get; private set; }
    public int? Score { get; private set; }
    public string? Feedback { get; private set; }

    [JsonIgnore]
    public Student Student { get; private set; }

    [JsonIgnore]
    public Course Course { get; private set; }

    private Certificate()
    { }

    public Certificate(Guid studentId, Guid courseId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título do certificado não pode estar vazio", nameof(title));

        StudentId = studentId;
        CourseId = courseId;
        Title = title;
        IssueDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        CertificateNumber = GenerateRandomCertificateNumber();
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título do certificado não pode estar vazio", nameof(title));

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetScore(int score)
    {
        if (score < 0 || score > 100)
            throw new ArgumentException("A nota deve estar entre 0 e 100", nameof(score));

        Score = score;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeedback(string feedback)
    {
        Feedback = feedback;
        UpdatedAt = DateTime.UtcNow;
    }

    private string GenerateRandomCertificateNumber()
    {
        return $"CERT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
} 