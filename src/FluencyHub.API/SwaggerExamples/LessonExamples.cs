using FluencyHub.API.Models;
using FluencyHub.Application.ContentManagement.Commands.UpdateLesson;
using FluencyHub.Application.ContentManagement.Queries.GetLessonsByCourse;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace FluencyHub.API.SwaggerExamples;

public class LessonCreateRequestExample : IExamplesProvider<LessonCreateRequest>
{
    public LessonCreateRequest GetExamples()
    {
        return new LessonCreateRequest
        {
            Title = "Introduction to Basic Grammar",
            Content = "In this lesson, we will learn the basics of grammar including nouns, verbs, and adjectives.",
            Order = 1,
            MaterialUrl = "https://example.com/materials/basic-grammar"
        };
    }
}

public class UpdateLessonCommandExample : IExamplesProvider<UpdateLessonCommand>
{
    public UpdateLessonCommand GetExamples()
    {
        return new UpdateLessonCommand
        {
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            LessonId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Advanced Grammar Concepts",
            Content = "This updated lesson covers advanced grammar concepts including complex sentence structures.",
            MaterialUrl = "https://example.com/materials/advanced-grammar"
        };
    }
}

public class LessonDtoExample : IExamplesProvider<LessonDto>
{
    public LessonDto GetExamples()
    {
        return new LessonDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Introduction to Basic Grammar",
            Content = "In this lesson, we will learn the basics of grammar including nouns, verbs, and adjectives.",
            Order = 1,
            MaterialUrl = "https://example.com/materials/basic-grammar",
            IsActive = true,
            CreatedAt = DateTime.Now.AddDays(-30),
            UpdatedAt = DateTime.Now
        };
    }
}

public class LessonDtoListExample : IExamplesProvider<List<LessonDto>>
{
    public List<LessonDto> GetExamples()
    {
        return new List<LessonDto>
        {
            new LessonDto
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Introduction to Basic Grammar",
                Content = "In this lesson, we will learn the basics of grammar including nouns, verbs, and adjectives.",
                Order = 1,
                MaterialUrl = "https://example.com/materials/basic-grammar",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30),
                UpdatedAt = DateTime.Now
            },
            new LessonDto
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Vocabulary Building",
                Content = "This lesson focuses on building a strong vocabulary foundation.",
                Order = 2,
                MaterialUrl = "https://example.com/materials/vocabulary",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-25),
                UpdatedAt = DateTime.Now
            }
        };
    }
} 