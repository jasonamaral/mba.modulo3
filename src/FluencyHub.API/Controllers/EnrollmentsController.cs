using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Commands.CompleteEnrollment;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentEnrollments;
using FluencyHub.Domain.StudentManagement;
using FluencyHub.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net.Mime;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly FluencyHubDbContext _dbContext;

    public EnrollmentsController(IMediator mediator, FluencyHubDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Enroll a student in a course
    /// </summary>
    /// <param name="request">Enrollment request data</param>
    /// <returns>The newly created enrollment details</returns>
    /// <response code="201">Returns the newly created enrollment</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the student or course is not found</response>
    [HttpPost]
    [Authorize(Roles = "Student,Administrator")]
    [SwaggerOperation(
        Summary = "Enroll a student in a course",
        Description = "Creates a new enrollment for a student in a specific course",
        OperationId = "EnrollStudent",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(EnrollmentCreateRequest), typeof(EnrollmentCreateRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(EnrollmentDtoExample))]
    public async Task<IActionResult> EnrollStudent([FromBody] EnrollmentCreateRequest request)
    {
        try
        {
            var command = request.ToCommand();
            var enrollmentId = await _mediator.Send(command);

            var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(enrollmentId));
            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollmentId }, enrollment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get enrollment details by ID
    /// </summary>
    /// <param name="id">The unique identifier of the enrollment</param>
    /// <returns>The enrollment details</returns>
    /// <response code="200">Returns the enrollment details</response>
    /// <response code="404">If the enrollment is not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get enrollment by ID",
        Description = "Retrieves a specific enrollment by its unique identifier",
        OperationId = "GetEnrollment",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnrollmentDtoExample))]
    public async Task<IActionResult> GetEnrollment(Guid id)
    {
        try
        {
            var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(id));
            return Ok(enrollment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get all enrollments for a specific student
    /// </summary>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <returns>A list of enrollments for the student</returns>
    /// <response code="200">Returns the list of enrollments</response>
    /// <response code="404">If the student is not found</response>
    [HttpGet("student/{studentId}")]
    [SwaggerOperation(
        Summary = "Get student enrollments",
        Description = "Retrieves all enrollments associated with a specific student",
        OperationId = "GetStudentEnrollments",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnrollmentDtoExample))]
    public async Task<IActionResult> GetStudentEnrollments(Guid studentId)
    {
        try
        {
            var enrollments = await _mediator.Send(new GetStudentEnrollmentsQuery(studentId));
            return Ok(enrollments);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Mark a lesson as completed for an enrollment
    /// </summary>
    /// <param name="enrollmentId">The unique identifier of the enrollment</param>
    /// <param name="lessonId">The unique identifier of the lesson</param>
    /// <param name="request">The completion request data</param>
    /// <returns>Success message</returns>
    /// <response code="200">If the lesson was marked as completed successfully</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the enrollment or lesson is not found</response>
    [HttpPost("{enrollmentId}/lessons/{lessonId}/complete")]
    [SwaggerOperation(
        Summary = "Complete a lesson",
        Description = "Marks a specific lesson as completed for a given enrollment",
        OperationId = "CompleteLesson",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(LessonCompleteRequest), typeof(LessonCompleteRequestExample))]
    public async Task<IActionResult> CompleteLesson(Guid enrollmentId, Guid lessonId, [FromBody] LessonCompleteRequest request)
    {
        try
        {
            var command = new FluencyHub.Application.StudentManagement.Commands.CompleteLesson.CompleteLessonCommand
            {
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                Completed = request.Completed
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Mark a course as completed for an enrollment
    /// </summary>
    /// <param name="id">The unique identifier of the enrollment</param>
    /// <returns>Success message</returns>
    /// <response code="200">If the course was marked as completed successfully</response>
    /// <response code="400">If the request is invalid or not all lessons are completed</response>
    /// <response code="404">If the enrollment or course is not found</response>
    [HttpPost("{id}/complete")]
    [SwaggerOperation(
        Summary = "Complete a course",
        Description = "Marks an enrollment as completed. All lessons must be completed first.",
        OperationId = "CompleteCourse",
        Tags = new[] { "Enrollments" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCourse(Guid id)
    {
        try
        {
            var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(id));
            if (enrollment == null)
                return NotFound("Enrollment not found");

            if (enrollment.Status != EnrollmentStatus.Active.ToString())
                return BadRequest("Only active enrollments can be completed.");

            var learningHistory = await _dbContext.LearningHistories
                .Include(lh => lh.CourseProgress)
                .FirstOrDefaultAsync(lh => lh.Id == enrollment.StudentId);

            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(enrollment.StudentId);
                _dbContext.LearningHistories.Add(learningHistory);
                await _dbContext.SaveChangesAsync();

                learningHistory = await _dbContext.LearningHistories
                    .FirstOrDefaultAsync(lh => lh.Id == enrollment.StudentId);

                if (learningHistory == null)
                {
                    return BadRequest("Failed to create learning history record");
                }
            }

            var courseProgress = await _dbContext.CourseProgresses
                .Include(cp => cp.CompletedLessons)
                .FirstOrDefaultAsync(cp => cp.CourseId == enrollment.CourseId && cp.LearningHistoryId == learningHistory.Id);

            if (courseProgress == null)
            {
                courseProgress = new CourseProgress(enrollment.CourseId)
                {
                    LearningHistoryId = learningHistory.Id
                };
                _dbContext.CourseProgresses.Add(courseProgress);
                await _dbContext.SaveChangesAsync();
            }

            var course = await _mediator.Send(new GetCourseByIdQuery(enrollment.CourseId));
            if (course == null)
                return NotFound("Course not found");

            var allLessons = course.Lessons.ToList();

            var allLessonIds = allLessons.Select(l => l.Id).ToList();

            var completedLessonIds = courseProgress.CompletedLessons.Select(cl => cl.LessonId).ToList();
            int completedLessonsCount = completedLessonIds.Count;
            int totalLessonsCount = allLessonIds.Count;

            var notCompletedLessonIds = allLessonIds.Except(completedLessonIds).ToList();

            if (notCompletedLessonIds.Count != 0)
            {
                return BadRequest($"All classes must be completed before completing the course. Completed classes: {completedLessonsCount}/{totalLessonsCount}. Missing: {notCompletedLessonIds.Count} lessons.");
            }

            courseProgress.CompleteCourse();
            await _dbContext.SaveChangesAsync();

            try
            {
                await _mediator.Send(new CompleteEnrollmentCommand(id));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Course completed successfully");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}