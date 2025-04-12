using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.EnrollStudent;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentEnrollments;
using FluencyHub.Application.ContentManagement.Queries.GetLessonById;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.API.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluencyHub.Domain.StudentManagement;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FluencyHub.Application.ContentManagement.Commands.CompleteEnrollment;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly FluencyHubDbContext _dbContext;
    
    public EnrollmentsController(IMediator mediator, FluencyHubDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpPost("{enrollmentId}/lessons/{lessonId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteLesson(Guid enrollmentId, Guid lessonId, [FromBody] LessonCompleteRequest request)
    {
        try
        {
            var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(enrollmentId));
            
            if (enrollment == null)
                return NotFound("Matrícula não encontrada");

            if (enrollment.Status != EnrollmentStatus.Active.ToString())
                return BadRequest("Apenas matrículas ativas podem ser concluídas");
            
            var lesson = await _mediator.Send(new GetLessonByIdQuery(lessonId));
            
            if (lesson.CourseId != enrollment.CourseId)
            {
                return BadRequest(new { error = "A aula não pertence ao curso da matrícula" });
            }
            
            // Obter ou criar o histórico de aprendizado do aluno
            var learningHistory = await _dbContext.LearningHistories
                .FirstOrDefaultAsync(lh => lh.Id == enrollment.StudentId);
                
            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(enrollment.StudentId);
                _dbContext.LearningHistories.Add(learningHistory);
            }
            
            // Adicionar o progresso da aula
            learningHistory.AddProgress(enrollment.CourseId, lessonId);
            
            await _dbContext.SaveChangesAsync();
            
            return Ok(new { 
                enrollmentId, 
                lessonId, 
                completed = request.Completed,
                message = "Aula concluída com sucesso"
            });
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

    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCourse(Guid id)
    {
        var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(id));
        if (enrollment == null)
            return NotFound("Matrícula não encontrada");

        if (enrollment.Status != "Active")
            return BadRequest("Apenas matrículas ativas podem ser concluídas");

        var learningHistory = await _dbContext.LearningHistories
            .FirstOrDefaultAsync(lh => lh.Id == enrollment.StudentId);

        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(enrollment.StudentId);
            _dbContext.LearningHistories.Add(learningHistory);
            await _dbContext.SaveChangesAsync();
        }

        var completedLessons = learningHistory.GetCompletedLessonsCount(enrollment.CourseId);
        var course = await _mediator.Send(new GetCourseByIdQuery(enrollment.CourseId));
        var totalLessons = course.Lessons.Count();

        if (completedLessons < totalLessons)
            return BadRequest($"Todas as aulas precisam ser concluídas antes de completar o curso. Aulas concluídas: {completedLessons}, Total de aulas: {totalLessons}");

        try
        {
            await _mediator.Send(new CompleteEnrollmentCommand(id));
            return Ok("Curso concluído com sucesso");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 