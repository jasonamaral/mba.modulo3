using MediatR;

namespace FluencyHub.ContentManagement.Application.Commands.AddLesson;

public class AddLessonCommandHandler : IRequestHandler<AddLessonCommand, Guid>
{
    public async Task<Guid> Handle(AddLessonCommand request, CancellationToken cancellationToken)
    {
        // Simulando uma implementação básica
        await Task.CompletedTask;
        return Guid.NewGuid();
    }
} 