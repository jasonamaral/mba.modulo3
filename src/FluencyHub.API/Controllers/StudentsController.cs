using FluencyHub.API.SwaggerExamples;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.ActivateStudent;
using FluencyHub.Application.StudentManagement.Commands.CompleteCourseForStudent;
using FluencyHub.Application.StudentManagement.Commands.CompleteLessonForStudent;
using FluencyHub.Application.StudentManagement.Commands.CreateStudent;
using FluencyHub.Application.StudentManagement.Commands.DeactivateStudent;
using FluencyHub.Application.StudentManagement.Commands.UpdateStudent;
using FluencyHub.Application.StudentManagement.Queries.GetAllStudents;
using FluencyHub.Application.StudentManagement.Queries.GetStudentByEmail;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentProgress;
using FluencyHub.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net.Mime;
using System.Security.Claims;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentsController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    /// <summary>
    /// Get all students
    /// </summary>
    /// <returns>List of all students</returns>
    /// <response code="200">Returns the list of students</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Get all students",
        Description = "Retrieves a list of all students in the system. Requires Administrator role.",
        OperationId = "GetAllStudents",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(typeof(IEnumerable<FluencyHub.Application.StudentManagement.Queries.GetAllStudents.StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentListDtoExample))]
    public async Task<IActionResult> GetAllStudents()
    {
        try
        {
            var students = await _mediator.Send(new GetAllStudentsQuery());
            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get the currently authenticated student
    /// </summary>
    /// <returns>Current student details</returns>
    /// <response code="200">Returns the current student details</response>
    /// <response code="404">If the student is not found</response>
    /// <response code="400">If the email is not found in token</response>
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Get current student",
        Description = "Retrieves the details of the currently authenticated student based on the JWT token",
        OperationId = "GetCurrentStudent",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(typeof(FluencyHub.Application.StudentManagement.Queries.GetStudentById.StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentDtoExample))]
    public async Task<IActionResult> GetCurrentStudent()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        Console.WriteLine($"Student/me endpoint called. Email from token: {userEmail}");

        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
        }

        if (string.IsNullOrEmpty(userEmail))
        {
            Console.WriteLine("Email not found in token");
            return BadRequest(new { error = "Email not found in token" });
        }

        try
        {
            Console.WriteLine($"Looking for student with email: {userEmail}");
            var student = await _mediator.Send(new GetStudentByEmailQuery(userEmail));
            Console.WriteLine($"Student found: {student.Id}");
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Student not found: {ex.Message}");
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get a student by ID
    /// </summary>
    /// <param name="id">ID of the student to retrieve</param>
    /// <returns>Student details</returns>
    /// <response code="200">Returns the student details</response>
    /// <response code="404">If the student is not found</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Get student by ID",
        Description = "Retrieves a specific student by their unique identifier. Requires Administrator role.",
        OperationId = "GetStudentById",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(typeof(FluencyHub.Application.StudentManagement.Queries.GetStudentById.StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentDtoExample))]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        try
        {
            var student = await _mediator.Send(new GetStudentByIdQuery(id));
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    /// <param name="command">Student details</param>
    /// <returns>ID of the newly created student</returns>
    /// <response code="201">Returns the ID of the newly created student</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Create a new student",
        Description = "Registers a new student in the system with the provided details",
        OperationId = "CreateStudent",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(CreateStudentCommand), typeof(CreateStudentCommandExample))]
    public async Task<IActionResult> CreateStudent(CreateStudentCommand command)
    {
        try
        {
            var studentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetStudentById), new { id = studentId }, null);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    /// <param name="id">ID of the student to update</param>
    /// <param name="command">Updated student details</param>
    /// <returns>Success message</returns>
    /// <response code="200">If the student was updated successfully</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the student is not found</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update a student",
        Description = "Updates an existing student's details. The student can only update their own profile, unless they are an Administrator.",
        OperationId = "UpdateStudent",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(UpdateStudentCommand), typeof(UpdateStudentCommandExample))]
    public async Task<IActionResult> UpdateStudent(Guid id, UpdateStudentCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");

        if (!isAdmin)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.StudentId != id)
            {
                return Forbid();
            }
        }

        if (id != command.Id)
        {
            return BadRequest("Student ID in the route must match the student ID in the command");
        }

        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Deactivate a student",
        Description = "Deactivates a student account, preventing them from accessing the system. Requires Administrator role.",
        OperationId = "DeactivateStudent",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(DeactivateStudentCommand), typeof(DeactivateStudentCommandExample))]
    public async Task<IActionResult> DeactivateStudent(Guid id)
    {
        try
        {
            await _mediator.Send(new DeactivateStudentCommand(id));
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Activate a student",
        Description = "Activates a previously deactivated student account, allowing them to access the system again. Requires Administrator role.",
        OperationId = "ActivateStudent",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(ActivateStudentCommand), typeof(ActivateStudentCommandExample))]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        try
        {
            await _mediator.Send(new ActivateStudentCommand(id));
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get the learning progress for a specific student
    /// </summary>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <returns>The student's progress across all courses</returns>
    /// <response code="200">Returns the student's progress</response>
    /// <response code="404">If the student is not found</response>
    [HttpGet("{studentId}/progress")]
    [SwaggerOperation(
        Summary = "Get student progress",
        Description = "Retrieves the learning progress of a student across all courses",
        OperationId = "GetStudentProgress",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(typeof(StudentProgressViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentProgressViewModelExample))]
    public async Task<IActionResult> GetStudentProgress(Guid studentId)
    {
        try
        {
            var progress = await _mediator.Send(new GetStudentProgressQuery(studentId));
            return Ok(progress);
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
    /// Mark a lesson as completed for a student
    /// </summary>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="courseId">The unique identifier of the course</param>
    /// <param name="lessonId">The unique identifier of the lesson to mark as completed</param>
    /// <returns>Success message and completion details</returns>
    /// <response code="200">If the lesson was marked as completed successfully</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the student, course, or lesson is not found</response>
    [HttpPost("{studentId}/courses/{courseId}/lessons/{lessonId}/complete")]
    [SwaggerOperation(
        Summary = "Complete a lesson for a student",
        Description = "Marks a specific lesson as completed for a student in a particular course",
        OperationId = "CompleteLessonForStudent",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(typeof(CompleteLessonForStudentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CompleteLessonForStudentResultExample))]
    public async Task<IActionResult> MarkLessonAsCompleted(Guid studentId, Guid courseId, Guid lessonId)
    {
        try
        {
            var result = await _mediator.Send(new CompleteLessonForStudentCommand(studentId, courseId, lessonId));
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark a course as completed for a student
    /// </summary>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="courseId">The unique identifier of the course to mark as completed</param>
    /// <returns>Success message and completion details</returns>
    /// <response code="200">If the course was marked as completed successfully</response>
    /// <response code="400">If the request is invalid or not all lessons are completed</response>
    /// <response code="404">If the student or course is not found</response>
    [HttpPost("{studentId}/courses/{courseId}/complete")]
    [SwaggerOperation(
        Summary = "Complete a course for a student",
        Description = "Marks a course as completed for a student. All lessons must be completed first.",
        OperationId = "CompleteCourseForStudent",
        Tags = new[] { "Students" }
    )]
    [ProducesResponseType(typeof(CompleteCourseForStudentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CompleteCourseForStudentResultExample))]
    public async Task<IActionResult> CompleteCourse(Guid studentId, Guid courseId)
    {
        try
        {
            var result = await _mediator.Send(new CompleteCourseForStudentCommand(studentId, courseId));

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
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
}