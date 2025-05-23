using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands;

public record CreateLearningHistoryCommand(LearningHistory LearningHistory) : IRequest<Unit>;

public class CreateLearningHistoryCommandHandler : IRequestHandler<CreateLearningHistoryCommand, Unit>
{
    private readonly ILearningRepository _learningRepository;

    public CreateLearningHistoryCommandHandler(ILearningRepository learningRepository)
    {
        _learningRepository = learningRepository;
    }

    public async Task<Unit> Handle(CreateLearningHistoryCommand request, CancellationToken cancellationToken)
    {
        await _learningRepository.AddLearningHistoryAsync(request.LearningHistory);
        await _learningRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
} 