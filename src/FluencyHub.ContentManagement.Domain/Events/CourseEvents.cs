using FluencyHub.ContentManagement.Domain.Common;
using FluencyHub.SharedKernel.Events;

namespace FluencyHub.ContentManagement.Domain.Events
{
    /// <summary>
    /// Evento de domínio disparado quando um curso é criado
    /// </summary>
    public class CourseCreatedDomainEvent : DomainEvent
    {
        public Course Course { get; }

        public CourseCreatedDomainEvent(Course course)
        {
            Course = course;
        }
    }

    /// <summary>
    /// Evento de domínio disparado quando um curso é atualizado
    /// </summary>
    public class CourseUpdatedDomainEvent : DomainEvent
    {
        public Course Course { get; }

        public CourseUpdatedDomainEvent(Course course)
        {
            Course = course;
        }
    }

    /// <summary>
    /// Evento de domínio disparado quando um curso é excluído
    /// </summary>
    public class CourseDeletedDomainEvent : DomainEvent
    {
        public Guid CourseId { get; }

        public CourseDeletedDomainEvent(Guid courseId)
        {
            CourseId = courseId;
        }
    }
} 