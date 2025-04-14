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
            if (request == null || !request.Completed)
            {
                return BadRequest("Request must contain 'completed: true' value");
            }
            
            var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(enrollmentId));
            
            if (enrollment == null)
                return NotFound("Enrollment not found");

            if (enrollment.Status != EnrollmentStatus.Active.ToString())
                return BadRequest("Only active enrollments can be completed.");
            
            var lesson = await _mediator.Send(new GetLessonByIdQuery(lessonId));
            
            if (lesson == null)
                return NotFound("Lesson not found");
                
            if (lesson.CourseId != enrollment.CourseId)
            {
                return BadRequest(new { error = "The class does not belong to the course of enrollment" });
            }

            // Desativa o rastreamento automático de entidades para evitar conflitos
            _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Verifica se o LearningHistory já existe
            var learningHistoryId = enrollment.StudentId;
            var learningHistory = await _dbContext.LearningHistories.AsNoTracking()
                .FirstOrDefaultAsync(lh => lh.Id == learningHistoryId);

            if (learningHistory == null)
            {
                // Cria um novo com SQL direto para garantir
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO LearningHistories (Id, CreatedAt, UpdatedAt) VALUES ({0}, {1}, {2})",
                    learningHistoryId, DateTime.UtcNow, DateTime.UtcNow);
            }
            else
            {
                // Atualiza a data de atualização
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE LearningHistories SET UpdatedAt = {0} WHERE Id = {1}",
                    DateTime.UtcNow, learningHistoryId);
            }

            // Verifica se o CourseProgress já existe
            var courseProgress = await _dbContext.CourseProgresses.AsNoTracking()
                .FirstOrDefaultAsync(cp => cp.CourseId == enrollment.CourseId && cp.LearningHistoryId == learningHistoryId);

            Guid courseProgressId;
            
            if (courseProgress == null)
            {
                // Cria um novo CourseProgress
                courseProgressId = Guid.NewGuid();
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO CourseProgresses (Id, CourseId, LearningHistoryId, IsCompleted, LastUpdated) VALUES ({0}, {1}, {2}, {3}, {4})",
                    courseProgressId, enrollment.CourseId, learningHistoryId, false, DateTime.UtcNow);
            }
            else
            {
                courseProgressId = courseProgress.Id;
                
                // Atualiza a data de atualização
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE CourseProgresses SET LastUpdated = {0} WHERE Id = {1}",
                    DateTime.UtcNow, courseProgressId);
            }

            // Verifica se a lição já foi marcada como concluída
            var completedLessonExists = await _dbContext.CompletedLessons.AsNoTracking()
                .AnyAsync(cl => cl.LessonId == lessonId && cl.CourseProgressId == courseProgressId);

            if (completedLessonExists)
            {
                return Ok(new { 
                    enrollmentId, 
                    lessonId, 
                    completed = request.Completed,
                    message = "Class was already completed"
                });
            }

            // Adiciona a lição concluída diretamente com SQL para evitar problemas de rastreamento
            var completedLessonId = Guid.NewGuid();
            var completedAt = DateTime.UtcNow;
            
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO CompletedLessons (Id, LessonId, CourseProgressId, CompletedAt) VALUES ({0}, {1}, {2}, {3})",
                    completedLessonId, lessonId, courseProgressId, completedAt);
            }
            catch (DbUpdateException)
            {
                // Verifica se a inserção falhou por causa de um índice único (a lição já foi concluída)
                var checkAgain = await _dbContext.CompletedLessons.AsNoTracking()
                    .AnyAsync(cl => cl.LessonId == lessonId && cl.CourseProgressId == courseProgressId);
                
                if (checkAgain)
                {
                    return Ok(new { 
                        enrollmentId, 
                        lessonId, 
                        completed = request.Completed,
                        message = "Class was already completed (concurrent operation detected)"
                    });
                }
                
                throw;
            }
            
            // Verifica se todas as lições do curso foram concluídas
            var course = await _mediator.Send(new GetCourseByIdQuery(enrollment.CourseId));
            if (course != null)
            {
                // Obtém todas as lições do curso
                var allLessons = course.Lessons.ToList();
                var allLessonIds = allLessons.Select(l => l.Id).ToList();
                
                // Obtém todas as lições concluídas deste curso
                var completedLessons = await _dbContext.CompletedLessons
                    .Where(cl => cl.CourseProgressId == courseProgressId)
                    .Select(cl => cl.LessonId)
                    .ToListAsync();
                
                // Verifica se todas as lições foram concluídas
                var notCompletedLessonIds = allLessonIds.Except(completedLessons).ToList();
                
                // Se todas as lições do curso foram concluídas, chama o método de conclusão do curso
                if (!notCompletedLessonIds.Any())
                {
                    // Completa o curso automaticamente
                    await CompleteCourse(enrollmentId);
                    
                    return Ok(new { 
                        enrollmentId, 
                        lessonId, 
                        completed = request.Completed,
                        message = "Class completed successfully and course has been completed automatically"
                    });
                }
            }
            
            return Ok(new { 
                enrollmentId, 
                lessonId, 
                completed = request.Completed,
                message = "Class completed successfully"
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
        try
        {
            var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(id));
            if (enrollment == null)
                return NotFound("Enrollment not found");

            if (enrollment.Status != EnrollmentStatus.Active.ToString())
                return BadRequest("Only active enrollments can be completed.");

            // Obter o histórico de aprendizado
            var learningHistory = await _dbContext.LearningHistories
                .Include(lh => lh.CourseProgress)
                .FirstOrDefaultAsync(lh => lh.Id == enrollment.StudentId);

            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(enrollment.StudentId);
                _dbContext.LearningHistories.Add(learningHistory);
                await _dbContext.SaveChangesAsync();
                
                // Recarrega o histórico recém-criado
                learningHistory = await _dbContext.LearningHistories
                    .FirstOrDefaultAsync(lh => lh.Id == enrollment.StudentId);
                    
                if (learningHistory == null)
                {
                    return BadRequest("Failed to create learning history record");
                }
            }

            // Verifica se o CourseProgress já existe
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
                
            // Obtém todas as aulas do curso
            var allLessons = course.Lessons.ToList();
            
            // Obtendo as lições do curso
            var allLessonIds = allLessons.Select(l => l.Id).ToList();
            
            // Verifica se todas as aulas foram completadas
            var completedLessonIds = courseProgress.CompletedLessons.Select(cl => cl.LessonId).ToList();
            int completedLessonsCount = completedLessonIds.Count;
            int totalLessonsCount = allLessonIds.Count;
            
            // Verificar quais lições ainda não foram concluídas
            var notCompletedLessonIds = allLessonIds.Except(completedLessonIds).ToList();
            
            if (notCompletedLessonIds.Any())
            {
                return BadRequest($"All classes must be completed before completing the course. Completed classes: {completedLessonsCount}/{totalLessonsCount}. Missing: {notCompletedLessonIds.Count} lessons.");
            }
            
            // Marca o CourseProgress como concluído
            courseProgress.CompleteCourse();
            await _dbContext.SaveChangesAsync();

            // Implementa tratamento de concorrência para a conclusão da matrícula
            int maxRetries = 3;
            int currentRetry = 0;
            bool operationComplete = false;
            
            while (!operationComplete && currentRetry < maxRetries)
            {
                try
                {
                    await _mediator.Send(new CompleteEnrollmentCommand(id));
                    operationComplete = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    currentRetry++;
                    
                    if (currentRetry >= maxRetries)
                        throw;
                        
                    // Aguarda um pouco antes de tentar novamente
                    await Task.Delay(100);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
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