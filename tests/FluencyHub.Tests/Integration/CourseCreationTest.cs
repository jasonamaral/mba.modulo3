using FluencyHub.ContentManagement.Application.Commands.CreateCourse;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using FluentAssertions;
using MediatR;
using FluencyHub.SharedKernel.Events;
using Moq;

namespace FluencyHub.Tests.Integration;

public class CourseCreationTest
{
    [Fact]
    public async Task CreateCourse_ShouldNotCauseStackOverflow()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Configurar banco em memória
        services.AddDbContext<ContentDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        
        // Configurar logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Mock do domain event service
        var mockDomainEventService = new Mock<IDomainEventService>();
        services.AddSingleton(mockDomainEventService.Object);
        
        // Registrar repositórios
        services.AddScoped<FluencyHub.ContentManagement.Domain.ICourseRepository, 
            FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories.CourseRepository>();
        services.AddScoped<FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository, 
            FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories.CourseRepository>();
        
        // Registrar MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(CreateCourseCommand).Assembly));
        
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        // Criar comando
        var command = new CreateCourseCommand
        {
            Name = "Curso de Teste",
            Description = "Descrição do curso de teste",
            Syllabus = "Conteúdo programático",
            LearningObjectives = "Objetivo 1, Objetivo 2",
            PreRequisites = "Pré-requisito 1",
            TargetAudience = "Iniciantes",
            Language = "pt-BR",
            Level = "Beginner",
            Price = 99.99m
        };
        
        // Act & Assert - Não deve causar StackOverflowException
        var result = await mediator.Send(command);
        
        // Verificar se o curso foi criado com sucesso
        result.Should().NotBeEmpty();
        
        // Verificar se o evento de domínio foi publicado
        mockDomainEventService.Verify(x => x.PublishAsync(It.IsAny<MediatR.INotification>()), Times.Once);
    }
} 