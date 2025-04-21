using FluencyHub.API.Models;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using Swashbuckle.AspNetCore.Filters;

namespace FluencyHub.API.SwaggerExamples;

public class CourseCreateRequestExample : IExamplesProvider<CourseCreateRequest>
{
    public CourseCreateRequest GetExamples()
    {
        return new CourseCreateRequest
        {
            Name = "English for Beginners",
            Description = "A comprehensive course for beginners to learn English",
            Language = "English",
            Level = "Beginner",
            Price = 99.99m,
            Syllabus = "Complete syllabus for beginners to learn English",
            LearningObjectives = "By the end of this course, students will be able to communicate basic needs and understand simple conversations",
            PreRequisites = "No prerequisites required",
            TargetAudience = "Beginners with no prior knowledge of English"
        };
    }
}

public class CourseUpdateRequestExample : IExamplesProvider<CourseUpdateRequest>
{
    public CourseUpdateRequest GetExamples()
    {
        return new CourseUpdateRequest
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Advanced English",
            Description = "A comprehensive course for advanced English learners",
            Language = "English",
            Level = "Advanced",
            Price = 149.99m,
            Syllabus = "Complete syllabus for advanced English learners",
            LearningObjectives = "By the end of this course, students will be able to communicate fluently in business settings",
            PreRequisites = "Intermediate English proficiency",
            TargetAudience = "Students with intermediate level of English"
        };
    }
}

public class CourseDtoExample : IExamplesProvider<CourseDto>
{
    public CourseDto GetExamples()
    {
        return new CourseDto
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "English for Beginners",
            Description = "A comprehensive course for beginners to learn English",
            Content = new CourseContentDto
            {
                Language = "English",
                Level = "Beginner",
                Syllabus = "Complete syllabus for beginners",
                LearningObjectives = "Learn basic English skills",
                PreRequisites = "No prerequisites",
                TargetAudience = "Beginners with no prior knowledge"
            },
            Price = 99.99m,
            IsActive = true,
            CreatedAt = DateTime.Now.AddDays(-30),
            UpdatedAt = DateTime.Now,
            Lessons = new List<LessonDto>
            {
                new LessonDto
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Title = "Introduction to English",
                    Content = "Basic introduction to English language",
                    Order = 1,
                    MaterialUrl = "https://example.com/materials/intro",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now
                }
            }
        };
    }
} 