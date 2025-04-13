using System;
using System.Linq;
using FluentAssertions;
using FluencyHub.Domain.StudentManagement;
using Xunit;

namespace FluencyHub.Tests.Domain.StudentManagement
{
    public class LearningHistoryTests
    {
        [Fact]
        public void AddProgress_ShouldAddLessonToCourseProgress()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var lessonId = Guid.NewGuid();
            var learningHistory = new LearningHistory(studentId);

            // Act
            learningHistory.AddProgress(courseId, lessonId);

            // Assert
            learningHistory.HasCompletedLesson(courseId, lessonId).Should().BeTrue();
        }

        [Fact]
        public void CompleteCourse_ShouldMarkCourseAsCompleted()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var learningHistory = new LearningHistory(studentId);

            // Act
            learningHistory.CompleteCourse(courseId);

            // Assert
            learningHistory.HasCompletedCourse(courseId).Should().BeTrue();
        }

        [Fact]
        public void GetCompletedLessonsCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var lessonId1 = Guid.NewGuid();
            var lessonId2 = Guid.NewGuid();
            var learningHistory = new LearningHistory(studentId);

            // Act
            learningHistory.AddProgress(courseId, lessonId1);
            learningHistory.AddProgress(courseId, lessonId2);

            // Assert
            learningHistory.GetCompletedLessonsCount(courseId).Should().Be(2);
        }
    }
} 