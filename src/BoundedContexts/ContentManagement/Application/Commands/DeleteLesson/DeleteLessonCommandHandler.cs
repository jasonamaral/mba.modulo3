using MediatR;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FluencyHub.ContentManagement.Application.Commands.DeleteLesson;

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, bool>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILogger<DeleteLessonCommandHandler> _logger;

    public DeleteLessonCommandHandler(
        ILessonRepository lessonRepository,
        ILogger<DeleteLessonCommandHandler> logger)
    {
        _lessonRepository = lessonRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verifica se a lição existe antes de tentar excluir
            var lesson = await _lessonRepository.GetByIdAsync(request.Id, cancellationToken);
            if (lesson == null)
            {
                throw new NotFoundException($"Lição com ID {request.Id} não foi encontrada.");
            }

            await _lessonRepository.DeleteAsync(request.Id, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir lição com ID: {LessonId}", request.Id);
            throw;
        }
    }
} 