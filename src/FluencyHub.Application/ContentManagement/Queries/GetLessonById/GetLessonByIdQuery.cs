using MediatR;

namespace FluencyHub.Application.ContentManagement.Queries.GetLessonById;

public record GetLessonByIdQuery(Guid LessonId) : IRequest<LessonDto>; 