using FluencyHub.API.Controllers;
using FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;
using FluencyHub.Application.StudentManagement.Queries.GetCertificateById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentCertificates;
using FluencyHub.Application.Common.Models;
using FluencyHub.Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluencyHub.Tests.API.Controllers;

public class CertificatesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CertificatesController _controller;

    public CertificatesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CertificatesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetCertificate_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var certificateId = Guid.NewGuid();
        var certificateDto = new CertificateDto
        {
            Id = certificateId,
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            CourseName = "English Course",
            StudentName = "John Doe",
            IssueDate = DateTime.UtcNow,
            Title = "Certificate of Completion"
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCertificateByIdQuery>(q => q.Id == certificateId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(certificateDto);

        // Act
        var result = await _controller.GetCertificate(certificateId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<CertificateDto>(okResult.Value);
        Assert.Equal(certificateDto.Id, returnValue.Id);
        Assert.Equal(certificateDto.CourseName, returnValue.CourseName);
        Assert.Equal(certificateDto.StudentName, returnValue.StudentName);
    }

    [Fact]
    public async Task GetCertificate_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var certificateId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCertificateByIdQuery>(q => q.Id == certificateId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Certificate", certificateId));

        // Act
        var result = await _controller.GetCertificate(certificateId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetStudentCertificates_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var certificates = new List<CertificateDto>
        {
            new CertificateDto
            {
                Id = Guid.NewGuid(),
                CourseName = "English Course",
                IssueDate = DateTime.UtcNow,
                Title = "Certificate of Completion"
            },
            new CertificateDto
            {
                Id = Guid.NewGuid(),
                CourseName = "Spanish Course",
                IssueDate = DateTime.UtcNow,
                Title = "Certificate of Completion"
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentCertificatesQuery>(q => q.StudentId == studentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(certificates);

        // Act
        var result = await _controller.GetStudentCertificates(studentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<CertificateDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count());
    }

    [Fact]
    public async Task GenerateCertificate_WithValidCommand_ReturnsCreatedResult()
    {
        // Arrange
        var command = new GenerateCertificateCommand
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid()
        };

        var certificateId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(certificateId);

        // Act
        var result = await _controller.GenerateCertificate(command);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetCertificate", createdAtResult.ActionName);
        Assert.Equal(certificateId, createdAtResult.RouteValues["id"]);
    }

    [Fact]
    public async Task GenerateCertificate_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new GenerateCertificateCommand
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid()
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Student has not completed the course"));

        // Act
        var result = await _controller.GenerateCertificate(command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Student has not completed the course", badRequestResult.Value);
    }
} 