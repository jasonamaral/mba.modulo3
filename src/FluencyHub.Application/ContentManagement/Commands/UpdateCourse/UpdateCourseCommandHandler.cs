using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.ContentManagement.Commands.UpdateCourse;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
{
    private readonly Common.Interfaces.ICourseRepository _courseRepository;
    private readonly ILogger<UpdateCourseCommandHandler> _logger;
    
    public UpdateCourseCommandHandler(
        Common.Interfaces.ICourseRepository courseRepository,
        ILogger<UpdateCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }
    
    public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Atualizando curso: {CourseId}", request.Id);
            
            var course = await _courseRepository.GetByIdAsync(request.Id);
            
            if (course == null)
            {
                throw new NotFoundException($"Curso com ID {request.Id} não encontrado");
            }
            
            // Criar o CourseContent atualizado
            var updatedContent = new CourseContent(
                request.Syllabus,
                request.LearningObjectives,
                request.PreRequisites,
                request.TargetAudience,
                request.Language,
                request.Level);
                
            // Atualizar os detalhes do curso
            course.UpdateDetails(
                request.Name,
                request.Description,
                updatedContent,
                request.Price);
                
            // Salvar no repositório
            await _courseRepository.UpdateAsync(course);
            await _courseRepository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Curso atualizado com sucesso: {CourseId}", course.Id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar curso {CourseId}", request.Id);
            throw;
        }
    }
} 