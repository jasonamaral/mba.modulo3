using MediatR;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FluencyHub.ContentManagement.Application.Commands.UpdateLesson;

public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, bool>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILogger<UpdateLessonCommandHandler> _logger;

    public UpdateLessonCommandHandler(
        ILessonRepository lessonRepository,
        ILogger<UpdateLessonCommandHandler> logger)
    {
        _lessonRepository = lessonRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Atualizando lição com ID: {LessonId}", request.Id);

            var lesson = await _lessonRepository.GetByIdAsync(request.Id, cancellationToken);
            if (lesson == null)
            {
                _logger.LogWarning("Lição com ID {LessonId} não encontrada", request.Id);
                throw new NotFoundException($"Lição com ID {request.Id} não foi encontrada.");
            }

            // Atualiza a lição usando o método Update da entidade
            lesson.Update(
                title: request.Title,
                description: request.Description,
                content: request.Content,
                materialUrl: request.VideoUrl,
                durationMinutes: request.DurationMinutes
            );

            // Atualiza a ordem se necessário
            if (lesson.Order != request.Order)
            {
                lesson.UpdateOrder(request.Order);
            }

            await _lessonRepository.UpdateAsync(lesson, cancellationToken);
            await _lessonRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Lição com ID {LessonId} atualizada com sucesso", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar lição com ID: {LessonId}", request.Id);
            throw;
        }
    }
} 