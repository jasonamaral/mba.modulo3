using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries;

public record GetLearningHistoryQuery(Guid StudentId) : IRequest<LearningHistory>;

public class GetLearningHistoryQueryHandler : IRequestHandler<GetLearningHistoryQuery, LearningHistory>
{
    private readonly ILearningRepository _learningRepository;

    public GetLearningHistoryQueryHandler(ILearningRepository learningRepository)
    {
        _learningRepository = learningRepository;
    }

    public async Task<LearningHistory> Handle(GetLearningHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _learningRepository.GetLearningHistoryByStudentIdAsync(request.StudentId);
    }
} 