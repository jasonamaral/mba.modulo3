using FluencyHub.ContentManagement.Domain;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Contracts;
using FluencyHub.StudentManagement.Domain.Models;

namespace FluencyHub.Tests.Helpers;

public static class TestDataBuilder
{
    public static Student CreateValidStudent(
        string firstName = "João",
        string lastName = "Silva",
        string email = "joao.silva@email.com",
        DateTime? dateOfBirth = null)
    {
        return new Student(
            firstName,
            lastName,
            email,
            dateOfBirth ?? DateTime.Now.AddYears(-25));
    }

    public static Student CreateStudentWithId(Guid id)
    {
        var student = new Student(
            firstName: "Maria",
            lastName: "Santos",
            email: "maria.santos@email.com",
            dateOfBirth: new DateTime(1985, 8, 20)
        );
        
        // Usar reflection para definir o ID
        var idProperty = typeof(Student).GetProperty("Id");
        idProperty?.SetValue(student, id);
        
        return student;
    }

    public static Course CreateValidCourse(
        string name = "Curso de Inglês",
        string description = "Curso completo de inglês",
        decimal price = 299.99m)
    {
        var content = CreateValidCourseContent();
        return new Course(name, description, content, price)
        {
            Name = name,
            Description = description,
            Content = content
        };
    }

    public static Course CreateCourseWithId(Guid id)
    {
        var content = new CourseContent(
            syllabus: "Conversação avançada, business english",
            learningObjectives: "Dominar inglês avançado",
            preRequisites: "Inglês básico",
            targetAudience: "Estudantes avançados",
            language: "Inglês",
            level: "Avançado"
        );

        var course = new Course(
            name: "Inglês Avançado",
            description: "Curso de inglês para estudantes avançados",
            content: content,
            price: 499.99m
        )
        {
            Name = "Inglês Avançado",
            Description = "Curso de inglês para estudantes avançados",
            Content = content
        };
        
        // Usar reflection para definir o ID
        var idProperty = typeof(Course).GetProperty("Id");
        idProperty?.SetValue(course, id);
        
        return course;
    }

    public static CourseContent CreateValidCourseContent(
        string syllabus = "Módulo 1: Introdução",
        string learningObjectives = "Aprender vocabulário básico",
        string preRequisites = "Nenhum",
        string targetAudience = "Iniciantes",
        string language = "Português",
        string level = "Básico")
    {
        return new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);
    }

    public static Lesson CreateValidLesson(
        Course course,
        string title = "Lição 1",
        string content = "Conteúdo da lição",
        string description = "Descrição da lição",
        int order = 1,
        int durationMinutes = 30)
    {
        return new Lesson(title, content, description, course, order, durationMinutes);
    }

    public static Enrollment CreateValidEnrollment(
        Guid studentId,
        Guid courseId,
        decimal amount = 299.99m)
    {
        var student = CreateStudentWithId(studentId);
        var course = CreateCourseWithId(courseId);
        
        return new Enrollment(studentId, courseId, amount)
        {
            Student = student,
            Course = (ICourse)course
        };
    }

    public static Payment CreateValidPayment(
        Guid studentId,
        Guid enrollmentId,
        decimal amount = 299.99m,
        CardDetails? cardDetails = null)
    {
        cardDetails ??= CreateValidCardDetails();
        return new Payment(studentId, enrollmentId, amount, cardDetails);
    }

    public static CardDetails CreateValidCardDetails(
        string cardholderName = "João Silva",
        string cardNumber = "4532015112830366",
        string expiryMonth = "12",
        string expiryYear = "2025")
    {
        return new CardDetails(cardholderName, cardNumber, expiryMonth, expiryYear);
    }

    public static LearningHistory CreateValidLearningHistory(
        Guid studentId)
    {
        return new LearningHistory(studentId);
    }

    public static Certificate CreateValidCertificate(
        Guid studentId,
        Guid courseId,
        string courseName = "Curso de Inglês")
    {
        var student = CreateStudentWithId(studentId);
        var course = CreateCourseWithId(courseId);
        
        return new Certificate(studentId, courseId, courseName)
        {
            Title = courseName,
            CertificateNumber = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
            Student = student,
            Course = (ICourse)course
        };
    }

    public static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var property = typeof(T).GetProperty("Id");
        property?.SetValue(entity, id);
    }

    public static T WithId<T>(this T entity, Guid id) where T : class
    {
        SetEntityId(entity, id);
        return entity;
    }

    public static CourseReference CreateValidCourseReference(
        Guid? id = null,
        string name = "Inglês Básico",
        string description = "Curso de inglês para iniciantes",
        decimal price = 299.99m,
        bool isActive = true)
    {
        return new CourseReference(
            id ?? Guid.NewGuid(),
            name,
            description,
            price,
            isActive);
    }

    public static Course CreateCourseWithLessons()
    {
        var course = CreateValidCourse();
        course.AddLesson("Introdução", "Conteúdo da introdução", "Primeira lição", 1, 30);
        course.AddLesson("Gramática Básica", "Conteúdo de gramática", "Segunda lição", 2, 45);
        return course;
    }
} 