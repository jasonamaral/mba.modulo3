using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Events;
using FluencyHub.Tests.Helpers;
using Moq;

namespace FluencyHub.Tests.Unit.StudentManagement.Infrastructure;

public class StudentRepositoryTests : IDisposable
{
    private readonly StudentDbContext _context;
    private readonly Mock<IDomainEventService> _mockEventService;
    private readonly StudentRepository _repository;

    public StudentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StudentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StudentDbContext(options);
        _mockEventService = new Mock<IDomainEventService>();
        _repository = new StudentRepository(_context, _mockEventService.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnStudent_WhenStudentExists()
    {
        // Arrange
        var student = new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(student.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(student.Id);
        result.FirstName.Should().Be("João");
        result.LastName.Should().Be("Silva");
        result.Email.Should().Be("joao@email.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenStudentDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnStudent_WhenEmailExists()
    {
        // Arrange
        var student = new Student("Maria", "Santos", "maria@email.com", new DateTime(1985, 5, 15));
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("maria@email.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("maria@email.com");
        result.FirstName.Should().Be("Maria");
        result.LastName.Should().Be("Santos");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        var nonExistentEmail = "naoexiste@email.com";

        // Act
        var result = await _repository.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveStudents_WhenIncludeInactiveIsFalse()
    {
        // Arrange
        var activeStudent = new Student("Pedro", "Costa", "pedro@email.com", new DateTime(1992, 3, 10));
        var inactiveStudent = new Student("Ana", "Lima", "ana@email.com", new DateTime(1988, 7, 20));
        
        await _context.Students.AddRangeAsync(activeStudent, inactiveStudent);
        await _context.SaveChangesAsync();

        // Desativar o estudante usando o método público
        inactiveStudent.Deactivate();
        _context.Students.Update(inactiveStudent);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(includeInactive: false);

        // Assert
        var students = result.ToList();
        students.Should().HaveCount(1);
        students.First().Email.Should().Be("pedro@email.com");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllStudents_WhenIncludeInactiveIsTrue()
    {
        // Arrange
        var student1 = new Student("Carlos", "Oliveira", "carlos@email.com", new DateTime(1991, 12, 5));
        var student2 = new Student("Lucia", "Ferreira", "lucia@email.com", new DateTime(1987, 9, 25));
        
        await _context.Students.AddRangeAsync(student1, student2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(includeInactive: true);

        // Assert
        var students = result.ToList();
        students.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveStudentsAsync_ShouldReturnOnlyActiveStudents()
    {
        // Arrange
        var activeStudent = new Student("Roberto", "Alves", "roberto@email.com", new DateTime(1993, 4, 18));
        var inactiveStudent = new Student("Fernanda", "Rocha", "fernanda@email.com", new DateTime(1989, 11, 8));
        
        await _context.Students.AddRangeAsync(activeStudent, inactiveStudent);
        await _context.SaveChangesAsync();

        // Desativar o estudante usando o método público
        inactiveStudent.Deactivate();
        _context.Students.Update(inactiveStudent);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveStudentsAsync();

        // Assert
        var students = result.ToList();
        students.Should().HaveCount(1);
        students.First().Email.Should().Be("roberto@email.com");
    }

    [Fact]
    public async Task AddAsync_ShouldAddStudentToDatabase()
    {
        // Arrange
        var student = new Student("Ricardo", "Mendes", "ricardo@email.com", new DateTime(1994, 6, 12));

        // Act
        await _repository.AddAsync(student);
        await _repository.SaveChangesAsync();

        // Assert
        var savedStudent = await _context.Students.FindAsync(student.Id);
        savedStudent.Should().NotBeNull();
        savedStudent!.FirstName.Should().Be("Ricardo");
        savedStudent.LastName.Should().Be("Mendes");
        savedStudent.Email.Should().Be("ricardo@email.com");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStudent_WhenValidStudent()
    {
        // Arrange
        var student = new Student("Alexandre", "Souza", "alexandre@email.com", new DateTime(1991, 8, 15));
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Act - Atualizar o estudante usando métodos públicos
        student.Update("Alexandre", "Silva", "alexandre.silva@email.com", "Rua Nova, 123", "+5511999999999", null, null, null);
        _context.Students.Update(student);
        await _repository.SaveChangesAsync();

        // Assert
        var updatedStudent = await _context.Students.FindAsync(student.Id);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.LastName.Should().Be("Silva");
        updatedStudent.Email.Should().Be("alexandre.silva@email.com");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenStudentExistsWithoutRelatedEntities()
    {
        // Arrange
        var student = new Student("Marcos", "Barbosa", "marcos@email.com", new DateTime(1986, 2, 28));
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Primeiro, precisamos deletar o LearningHistory que é automaticamente criado
        await _repository.DeleteLearningHistoryAsync(student.Id);

        // Act
        var result = await _repository.DeleteAsync(student.Id);

        // Assert
        result.Should().BeTrue();
        var deletedStudent = await _context.Students.FindAsync(student.Id);
        deletedStudent.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenStudentDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenStudentHasRelatedEntities()
    {
        // Arrange
        var student = new Student("Marcos", "Barbosa", "marcos.unique@email.com", new DateTime(1986, 2, 28));
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
        
        // Limpar o tracker para evitar conflitos
        _context.ChangeTracker.Clear();
        
        // Criar uma matrícula para simular entidade relacionada
        var courseReference = TestDataBuilder.CreateValidCourseReference();
        student.EnrollInCourse(courseReference);
        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.DeleteAsync(student.Id));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 