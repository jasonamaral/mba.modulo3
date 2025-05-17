using System;
using FluencyHub.ContentManagement.Domain;
using Xunit;

namespace FluencyHub.Tests.Domain.ContentManagement
{
    public class LessonTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateLesson()
        {
            // Arrange
            var title = "Introduction to C#";
            var content = "C# is a programming language developed by Microsoft";
            var materialUrl = "https://example.com/csharp-intro";
            var order = 1;

            // Act
            var lesson = new Lesson(title, content, materialUrl, order);

            // Assert
            Assert.Equal(title, lesson.Title);
            Assert.Equal(content, lesson.Content);
            Assert.Equal(materialUrl, lesson.MaterialUrl);
            Assert.Equal(order, lesson.Order);
            Assert.True(DateTime.UtcNow.Subtract(lesson.CreatedAt).TotalSeconds < 1);
        }

        [Fact]
        public void Constructor_WithNullMaterialUrl_ShouldCreateLesson()
        {
            // Arrange
            var title = "Introduction to C#";
            var content = "C# is a programming language developed by Microsoft";
            string? materialUrl = null;
            var order = 1;

            // Act
            var lesson = new Lesson(title, content, materialUrl, order);

            // Assert
            Assert.Equal(title, lesson.Title);
            Assert.Equal(content, lesson.Content);
            Assert.Null(lesson.MaterialUrl);
            Assert.Equal(order, lesson.Order);
        }

        [Theory]
        [InlineData("", "Content", "https://example.com", 1)]
        [InlineData("Title", "", "https://example.com", 1)]
        [InlineData("Title", "Content", "https://example.com", 0)]
        [InlineData("Title", "Content", "https://example.com", -1)]
        public void Constructor_WithInvalidParameters_ShouldThrowArgumentException(
            string title,
            string content,
            string materialUrl,
            int order)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Lesson(title, content, materialUrl, order));
        }

        [Fact]
        public void Update_WithValidParameters_ShouldUpdateLesson()
        {
            // Arrange
            var lesson = new Lesson("Old Title", "Old Content", "https://old-url.com", 1);
            var newTitle = "New Title";
            var newContent = "New Content";
            var newMaterialUrl = "https://new-url.com";

            // Act
            lesson.Update(newTitle, newContent, newMaterialUrl);

            // Assert
            Assert.Equal(newTitle, lesson.Title);
            Assert.Equal(newContent, lesson.Content);
            Assert.Equal(newMaterialUrl, lesson.MaterialUrl);
            Assert.NotNull(lesson.UpdatedAt);
            Assert.True(DateTime.UtcNow.Subtract(lesson.UpdatedAt.Value).TotalSeconds < 1);
        }

        [Theory]
        [InlineData("", "Content")]
        [InlineData("Title", "")]
        public void Update_WithInvalidParameters_ShouldThrowArgumentException(
            string title,
            string content)
        {
            // Arrange
            var lesson = new Lesson("Old Title", "Old Content", "https://old-url.com", 1);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => lesson.Update(title, content, "https://new-url.com"));
        }

        [Fact]
        public void UpdateOrder_WithValidOrder_ShouldUpdateOrder()
        {
            // Arrange
            var lesson = new Lesson("Title", "Content", "https://example.com", 1);
            var newOrder = 2;

            // Act
            lesson.UpdateOrder(newOrder);

            // Assert
            Assert.Equal(newOrder, lesson.Order);
            Assert.NotNull(lesson.UpdatedAt);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateOrder_WithInvalidOrder_ShouldThrowArgumentException(int newOrder)
        {
            // Arrange
            var lesson = new Lesson("Title", "Content", "https://example.com", 1);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => lesson.UpdateOrder(newOrder));
        }

        [Fact]
        public void UpdateMaterial_ShouldUpdateMaterialUrl()
        {
            // Arrange
            var lesson = new Lesson("Title", "Content", "https://old-url.com", 1);
            var newMaterialUrl = "https://new-url.com";

            // Act
            lesson.UpdateMaterial(newMaterialUrl);

            // Assert
            Assert.Equal(newMaterialUrl, lesson.MaterialUrl);
            Assert.NotNull(lesson.UpdatedAt);
        }
    }
} 