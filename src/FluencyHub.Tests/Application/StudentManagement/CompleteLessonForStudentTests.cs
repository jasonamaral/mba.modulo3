using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Application.ContentManagement.Queries.GetLessonById;
using FluencyHub.Application.StudentManagement.Commands.CompleteLessonForStudent;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.Domain.StudentManagement;
using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluencyHub.Tests.Application.StudentManagement;

public class CompleteLessonForStudentTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILearningRepository> _learningRepositoryMock;
    private readonly Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository> _courseRepositoryMock;
    private readonly CompleteLessonForStudentCommandHandler _handler;

    public CompleteLessonForStudentTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _learningRepositoryMock = new Mock<ILearningRepository>();
        _courseRepositoryMock = new Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository>();
        
        _handler = new CompleteLessonForStudentCommandHandler(
            _mediatorMock.Object,
            _learningRepositoryMock.Object,
            _courseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_MarksLessonAsCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);

        var studentDto = new StudentDto
        {
            Id = command.StudentId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var courseDto = new CourseDto
        {
            Id = command.CourseId,
            Name = "English Course",
            Description = "Learn English"
        };

        var lessonDto = new FluencyHub.Application.ContentManagement.Queries.GetLessonById.LessonDto
        {
            Id = command.LessonId,
            CourseId = command.CourseId,
            Title = "Lesson 1",
            Content = "Lesson content",
            Order = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var learningHistory = new LearningHistory(command.StudentId);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentByIdQuery>(q => q.Id == command.StudentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == command.CourseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetLessonByIdQuery>(q => q.LessonId == command.LessonId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessonDto);

        _learningRepositoryMock
            .Setup(r => r.GetByStudentIdAsync(command.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningHistory);

        // Configurando para o mock para retornar false para HasCompletedLesson
        _learningRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<LearningHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _learningRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(command.StudentId, result.StudentId);
        Assert.Equal(command.CourseId, result.CourseId);
        Assert.Contains("completed", result.Message.ToLower());
        
        // Verificamos que o repositório foi chamado para salvar as alterações
        _learningRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentStudent_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentByIdQuery>(q => q.Id == command.StudentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentDto)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentCourse_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);

        var studentDto = new StudentDto
        {
            Id = command.StudentId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentByIdQuery>(q => q.Id == command.StudentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == command.CourseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseDto)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentLesson_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);

        var studentDto = new StudentDto
        {
            Id = command.StudentId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var courseDto = new CourseDto
        {
            Id = command.CourseId,
            Name = "English Course",
            Description = "Learn English"
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentByIdQuery>(q => q.Id == command.StudentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == command.CourseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetLessonByIdQuery>(q => q.LessonId == command.LessonId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FluencyHub.Application.ContentManagement.Queries.GetLessonById.LessonDto)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithLessonFromDifferentCourse_ThrowsBadRequestException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);

        var studentDto = new StudentDto
        {
            Id = command.StudentId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var courseDto = new CourseDto
        {
            Id = command.CourseId,
            Name = "English Course",
            Description = "Learn English"
        };

        var differentCourseId = Guid.NewGuid();
        var lessonDto = new FluencyHub.Application.ContentManagement.Queries.GetLessonById.LessonDto
        {
            Id = command.LessonId,
            CourseId = differentCourseId, // Different course ID
            Title = "Lesson 1",
            Content = "Lesson content",
            Order = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentByIdQuery>(q => q.Id == command.StudentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == command.CourseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetLessonByIdQuery>(q => q.LessonId == command.LessonId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessonDto);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAlreadyCompletedLesson_ReturnsAppropriateMessage()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);

        var studentDto = new StudentDto
        {
            Id = command.StudentId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var courseDto = new CourseDto
        {
            Id = command.CourseId,
            Name = "English Course",
            Description = "Learn English"
        };

        var lessonDto = new FluencyHub.Application.ContentManagement.Queries.GetLessonById.LessonDto
        {
            Id = command.LessonId,
            CourseId = command.CourseId,
            Title = "Lesson 1",
            Content = "Lesson content",
            Order = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var learningHistory = new LearningHistory(command.StudentId);
        // Configurando a história de aprendizado para mostrar que a lição já foi concluída
        learningHistory.AddProgress(courseId, lessonId);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentByIdQuery>(q => q.Id == command.StudentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == command.CourseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetLessonByIdQuery>(q => q.LessonId == command.LessonId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessonDto);

        _learningRepositoryMock
            .Setup(r => r.GetByStudentIdAsync(command.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningHistory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(command.StudentId, result.StudentId);
        Assert.Equal(command.CourseId, result.CourseId);
        Assert.Contains("already", result.Message.ToLower());
        
        // Verificamos que o método SaveChangesAsync não foi chamado
        _learningRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAllLessonsCompleted_IndicatesInResult()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var command = new CompleteLessonForStudentCommand(studentId, courseId, lessonId);

        var studentDto = new StudentDto
        {
            Id = command.StudentId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var courseDto = new CourseDto
        {
            Id = command.CourseId,
            Name = "English Course",
            Description = "Learn English"
        };

        var lessonDto = new FluencyHub.Application.ContentManagement.Queries.GetLessonById.LessonDto
        {
            Id = command.LessonId,
            CourseId = command.CourseId,
            Title = "Lesson 1",
            Content = "Lesson content",
            Order = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var learningHistory = new LearningHistory(command.StudentId);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentByIdQuery>(q => q.Id == command.StudentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == command.CourseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetLessonByIdQuery>(q => q.LessonId == command.LessonId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessonDto);

        _learningRepositoryMock
            .Setup(r => r.GetByStudentIdAsync(command.StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningHistory);

        _learningRepositoryMock
            .Setup(r => r.HasCompletedLessonAsync(command.StudentId, command.LessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // This is the final lesson to complete
        _courseRepositoryMock
            .Setup(r => r.GetLessonsCountByCourseId(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _learningRepositoryMock
            .Setup(r => r.GetCompletedLessonsCountAsync(command.StudentId, command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4); // Already completed 4 out of 5

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(command.StudentId, result.StudentId);
        Assert.Equal(command.CourseId, result.CourseId);
        Assert.Contains("completed", result.Message.ToLower());
    }
} 