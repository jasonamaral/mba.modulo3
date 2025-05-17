using System;
using FluencyHub.ContentManagement.Domain;
using Xunit;

namespace FluencyHub.Tests.Domain.ContentManagement
{
    public class CourseContentTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateCourseContent()
        {
            // Arrange
            var syllabus = "Course syllabus";
            var learningObjectives = "Learning objectives";
            var preRequisites = "Prerequisites";
            var targetAudience = "Target audience";
            var language = "Portuguese";
            var level = "Intermediate";

            // Act
            var courseContent = new CourseContent(
                syllabus,
                learningObjectives,
                preRequisites,
                targetAudience,
                language,
                level);

            // Assert
            Assert.Equal(syllabus, courseContent.Syllabus);
            Assert.Equal(learningObjectives, courseContent.LearningObjectives);
            Assert.Equal(preRequisites, courseContent.PreRequisites);
            Assert.Equal(targetAudience, courseContent.TargetAudience);
            Assert.Equal(language, courseContent.Language);
            Assert.Equal(level, courseContent.Level);
        }

        [Fact]
        public void Constructor_WithNullPrerequisites_ShouldSetEmptyString()
        {
            // Arrange
            var syllabus = "Course syllabus";
            var learningObjectives = "Learning objectives";
            string? preRequisites = null;
            var targetAudience = "Target audience";
            var language = "Portuguese";
            var level = "Intermediate";

            // Act
            var courseContent = new CourseContent(
                syllabus,
                learningObjectives,
                preRequisites,
                targetAudience,
                language,
                level);

            // Assert
            Assert.Equal(string.Empty, courseContent.PreRequisites);
        }

        [Theory]
        [InlineData("", "Learning objectives", "Prerequisites", "Target audience", "Portuguese", "Intermediate")]
        [InlineData("Syllabus", "", "Prerequisites", "Target audience", "Portuguese", "Intermediate")]
        [InlineData("Syllabus", "Learning objectives", "Prerequisites", "", "Portuguese", "Intermediate")]
        [InlineData("Syllabus", "Learning objectives", "Prerequisites", "Target audience", "", "Intermediate")]
        [InlineData("Syllabus", "Learning objectives", "Prerequisites", "Target audience", "Portuguese", "")]
        public void Constructor_WithEmptyParameters_ShouldThrowArgumentException(
            string syllabus,
            string learningObjectives,
            string preRequisites,
            string targetAudience,
            string language,
            string level)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CourseContent(
                syllabus,
                learningObjectives,
                preRequisites,
                targetAudience,
                language,
                level));
        }

        [Fact]
        public void Create_ShouldReturnNewCourseContent()
        {
            // Arrange
            var syllabus = "Course syllabus";
            var learningObjectives = "Learning objectives";
            var preRequisites = "Prerequisites";
            var targetAudience = "Target audience";
            var language = "Portuguese";
            var level = "Intermediate";

            // Act
            var courseContent = CourseContent.Create(
                syllabus,
                learningObjectives,
                preRequisites,
                targetAudience,
                language,
                level);

            // Assert
            Assert.Equal(syllabus, courseContent.Syllabus);
            Assert.Equal(learningObjectives, courseContent.LearningObjectives);
            Assert.Equal(preRequisites, courseContent.PreRequisites);
            Assert.Equal(targetAudience, courseContent.TargetAudience);
            Assert.Equal(language, courseContent.Language);
            Assert.Equal(level, courseContent.Level);
        }

        [Fact]
        public void Update_ShouldReturnNewCourseContentWithUpdatedValues()
        {
            // Arrange
            var initial = new CourseContent(
                "Initial syllabus",
                "Initial objectives",
                "Initial prerequisites",
                "Initial audience",
                "English",
                "Beginner");

            var newSyllabus = "Updated syllabus";
            var newObjectives = "Updated objectives";
            var newPrerequisites = "Updated prerequisites";
            var newAudience = "Updated audience";
            var newLanguage = "Portuguese";
            var newLevel = "Advanced";

            // Act
            var updated = initial.Update(
                newSyllabus,
                newObjectives,
                newPrerequisites,
                newAudience,
                newLanguage,
                newLevel);

            // Assert
            Assert.Equal(newSyllabus, updated.Syllabus);
            Assert.Equal(newObjectives, updated.LearningObjectives);
            Assert.Equal(newPrerequisites, updated.PreRequisites);
            Assert.Equal(newAudience, updated.TargetAudience);
            Assert.Equal(newLanguage, updated.Language);
            Assert.Equal(newLevel, updated.Level);
        }
    }
} 