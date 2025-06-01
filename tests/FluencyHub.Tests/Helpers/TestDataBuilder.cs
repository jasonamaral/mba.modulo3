using FluencyHub.ContentManagement.Domain;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.StudentManagement.Domain;
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
        decimal price = 299.99m)
    {
        return CreateValidEnrollmentWithNavigation(studentId, courseId, price);
    }

    public static Enrollment CreateValidEnrollmentWithoutNavigation(
        Guid studentId,
        Guid courseId,
        decimal price = 299.99m)
    {
        // Criar objetos dummy para satisfazer propriedades obrigatórias do EF
        var dummyStudent = CreateDummyStudent();
        var dummyCourse = CreateDummyCourse();
        
        // Para testes unitários, criar enrollment básico sem navegação
        var enrollment = new Enrollment(studentId, courseId, price)
        {
            Student = dummyStudent,
            Course = dummyCourse
        };
        
        // Definir um ID único para a matrícula usando reflection
        SetEntityId(enrollment, Guid.NewGuid());
        
        return enrollment;
    }

    public static Enrollment CreateValidEnrollmentWithNavigation(
        Guid studentId,
        Guid courseId,
        decimal price = 299.99m)
    {
        // Criar student com o ID específico do teste
        var student = CreateValidStudent($"Student{studentId:N}"[..8], $"Test{studentId:N}"[..8], $"test{studentId:N}@email.com");
        SetEntityId(student, studentId);
        
        var course = CreateValidCourseReference(courseId, $"Course{courseId:N}"[..10], "Test course description", price);
        
        var enrollment = new Enrollment(studentId, courseId, price)
        {
            Student = student,
            Course = course
        };
        
        // Definir um ID único para a matrícula
        SetEntityId(enrollment, Guid.NewGuid());
        
        return enrollment;
    }

    public static Payment CreateValidPayment(
        Guid studentId,
        Guid enrollmentId,
        decimal amount = 299.99m,
        CardDetails? cardDetails = null)
    {
        cardDetails ??= CreateValidCardDetails();
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        // Garantir que o pagamento tenha um ID único
        SetEntityId(payment, Guid.NewGuid());
        return payment;
    }

    public static CardDetails CreateValidCardDetails(
        string cardholderName = "João Silva",
        string cardNumber = "4532015112830366",
        string expiryMonth = "12",
        string expiryYear = "2025")
    {
        return new CardDetails(cardholderName, cardNumber, expiryMonth, expiryYear);
    }

    public static FluencyHub.PaymentProcessing.Application.Common.Models.CardDetails CreateValidApplicationCardDetails(
        string cardholderName = "João Silva",
        string cardNumber = "4532015112830366",
        string expiryMonth = "12",
        string expiryYear = "2025",
        string cvv = "123")
    {
        return new FluencyHub.PaymentProcessing.Application.Common.Models.CardDetails
        {
            CardHolderName = cardholderName,
            CardNumber = cardNumber,
            MaskedCardNumber = $"****-****-****-{cardNumber.Substring(cardNumber.Length - 4)}",
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            Cvv = cvv
        };
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
        return CreateValidCertificateWithoutNavigation(studentId, courseId, courseName);
    }

    public static Certificate CreateValidCertificateWithoutNavigation(
        Guid studentId,
        Guid courseId,
        string courseName = "Curso de Inglês")
    {
        // Para testes de integração, criar certificate com student e course dummy para satisfazer propriedades obrigatórias
        var dummyStudent = CreateDummyStudent();
        var dummyCourse = CreateDummyCourse();
        
        var certificate = new Certificate(studentId, courseId, courseName)
        {
            Title = courseName,
            CertificateNumber = $"CERT-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString().Substring(0, 8)}",
            Student = dummyStudent,
            Course = dummyCourse
        };
        
        // Definir um ID único para o certificado
        SetEntityId(certificate, Guid.NewGuid());
        return certificate;
    }

    public static Certificate CreateValidCertificateWithNavigation(
        Guid studentId,
        Guid courseId,
        string courseName = "Curso de Inglês")
    {
        // Para testes de integração, criar navigation properties completas
        var student = CreateValidStudent($"Student{studentId:N}"[..8], $"Test{studentId:N}"[..8], $"test{studentId:N}@email.com");
        SetEntityId(student, studentId);
        
        var course = CreateValidCourseReference(courseId, courseName, "Test course description", 299.99m);
        
        var certificate = new Certificate(studentId, courseId, courseName)
        {
            Title = courseName,
            CertificateNumber = $"CERT-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString().Substring(0, 8)}",
            Student = student,
            Course = course
        };
        
        // Definir um ID único para o certificado
        SetEntityId(certificate, Guid.NewGuid());
        return certificate;
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

    private static Student CreateDummyStudent()
    {
        var student = CreateValidStudent("Dummy", "Student", $"dummy{Guid.NewGuid():N}@email.com");
        SetEntityId(student, Guid.NewGuid());
        return student;
    }

    private static CourseReference CreateDummyCourse()
    {
        return CreateValidCourseReference(
            id: Guid.NewGuid(),
            name: "Dummy Course",
            description: "Dummy course for testing",
            price: 299.99m,
            isActive: true);
    }
} 