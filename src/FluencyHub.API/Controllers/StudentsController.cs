using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.CreateStudent;
using FluencyHub.Application.StudentManagement.Commands.UpdateStudent;
using FluencyHub.Application.StudentManagement.Commands.ActivateStudent;
using FluencyHub.Application.StudentManagement.Commands.DeactivateStudent;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.Application.StudentManagement.Queries.GetAllStudents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FluencyHub.Application.StudentManagement.Queries.GetStudentByEmail;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluencyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using FluencyHub.Domain.StudentManagement;
using Microsoft.Data.Sqlite;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly FluencyHubDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public StudentsController(IMediator mediator, FluencyHubDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _userManager = userManager;
    }
    
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<Application.StudentManagement.Queries.GetAllStudents.StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            Console.WriteLine("Email não encontrado no token");
            return BadRequest(new { error = "Email não encontrado no token" });
        }
        
        try
        {
            Console.WriteLine($"Buscando estudante com email: {userEmail}");
            var student = await _mediator.Send(new GetStudentByEmailQuery(userEmail));
            Console.WriteLine($"Estudante encontrado: {student.Id}");
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Estudante não encontrado: {ex.Message}");
            return NotFound(ex.Message);
        }
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStudent(Guid id, UpdateStudentCommand command)
    {
        // Only allow administrators or the student themselves to update their profile
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");
        
        if (!isAdmin)
        {
            // Buscar o usuário para verificar se ele está tentando atualizar seu próprio perfil de estudante
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpGet("{studentId}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentProgress(Guid studentId)
    {
        try
        {
            // Verificar se o estudante existe
            var student = await _mediator.Send(new GetStudentByIdQuery(studentId));
            
            // Buscar todos os CourseProgresses do estudante com suas lições concluídas
            var courseProgresses = await _dbContext.CourseProgresses
                .AsNoTracking()
                .Where(cp => cp.LearningHistoryId == studentId)
                .Select(cp => new
                {
                    cp.CourseId,
                    cp.IsCompleted,
                    cp.LastUpdated,
                    CompletedLessonsCount = cp.CompletedLessons.Count
                })
                .ToListAsync();
                
            if (courseProgresses == null || !courseProgresses.Any())
            {
                return Ok(new { progress = new Dictionary<Guid, object>() });
            }
            
            // Montar o dicionário de progresso
            var progress = courseProgresses.ToDictionary(
                cp => cp.CourseId,
                cp => new
                {
                    completedLessons = cp.CompletedLessonsCount,
                    isCompleted = cp.IsCompleted,
                    lastUpdated = cp.LastUpdated
                }
            );
            
            return Ok(new { progress });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter progresso: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("{studentId}/courses/{courseId}/lessons/{lessonId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkLessonAsCompleted(Guid studentId, Guid courseId, Guid lessonId)
    {
        try
        {
            // Verificar se o estudante existe
            var student = await _mediator.Send(new GetStudentByIdQuery(studentId));
            
            // Verificar se a lição existe
            var course = await _dbContext.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);
                
            if (course == null)
            {
                return NotFound($"Curso não encontrado: {courseId}");
            }
            
            var lesson = course.Lessons.FirstOrDefault(l => l.Id == lessonId);
            if (lesson == null)
            {
                return NotFound($"Lição não encontrada: {lessonId}");
            }
            
            // Obter ou criar o histórico de aprendizado
            var learningHistory = await _dbContext.LearningHistories
                .Include(lh => lh.CourseProgress)
                .FirstOrDefaultAsync(lh => lh.Id == studentId);
                
            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(studentId);
                _dbContext.LearningHistories.Add(learningHistory);
                await _dbContext.SaveChangesAsync();
                
                // Recarregar o histórico
                learningHistory = await _dbContext.LearningHistories
                    .Include(lh => lh.CourseProgress)
                    .FirstOrDefaultAsync(lh => lh.Id == studentId);
                    
                if (learningHistory == null)
                {
                    return BadRequest("Erro ao criar histórico de aprendizado");
                }
            }
            
            // Adicionar o progresso
            learningHistory.AddProgress(courseId, lessonId);
            await _dbContext.SaveChangesAsync();
            
            return Ok(new { 
                message = "Lição marcada como concluída com sucesso",
                studentId,
                courseId,
                lessonId 
            });
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
    
    [HttpPost("{studentId}/courses/{courseId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCourse(Guid studentId, Guid courseId)
    {
        try
        {
            // Verificar se o estudante existe
            var student = await _mediator.Send(new GetStudentByIdQuery(studentId));
            
            // Verificar se o curso existe
            var course = await _dbContext.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);
                
            if (course == null)
            {
                return NotFound($"Curso não encontrado: {courseId}");
            }
            
            // Obter ou criar o histórico de aprendizado
            var learningHistory = await _dbContext.LearningHistories
                .Include(lh => lh.CourseProgress)
                .FirstOrDefaultAsync(lh => lh.Id == studentId);
                
            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(studentId);
                _dbContext.LearningHistories.Add(learningHistory);
                await _dbContext.SaveChangesAsync();
                
                // Recarregar o histórico
                learningHistory = await _dbContext.LearningHistories
                    .Include(lh => lh.CourseProgress)
                    .FirstOrDefaultAsync(lh => lh.Id == studentId);
                    
                if (learningHistory == null)
                {
                    return BadRequest("Erro ao criar histórico de aprendizado");
                }
            }
            
            // Verificar se todas as lições foram concluídas
            var courseProgress = learningHistory.CourseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
            if (courseProgress != null)
            {
                var allLessonIds = course.Lessons.Select(l => l.Id).ToList();
                var completedLessonIds = courseProgress.CompletedLessons.Select(cl => cl.LessonId).ToList();
                int completedLessonsCount = completedLessonIds.Count;
                int totalLessonsCount = allLessonIds.Count;
                
                // Verificar quais lições ainda não foram concluídas
                var notCompletedLessonIds = allLessonIds.Where(id => !completedLessonIds.Contains(id)).ToList();
                
                if (notCompletedLessonIds.Any())
                {
                    return BadRequest($"All classes must be completed before completing the course. Completed classes: {completedLessonsCount}/{totalLessonsCount}. Missing: {notCompletedLessonIds.Count} lessons.");
                }
            }
            else
            {
                return BadRequest("Nenhuma lição do curso foi concluída.");
            }
            
            // Marcar o curso como concluído
            learningHistory.CompleteCourse(courseId);
            await _dbContext.SaveChangesAsync();
            
            return Ok(new { 
                message = "Curso marcado como concluído com sucesso",
                studentId,
                courseId
            });
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

    [HttpGet("{studentId}/debug-progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDebugProgress(Guid studentId)
    {
        try
        {
            // Obter os CourseProgresses diretamente do banco de dados
            var debugInfo = new List<object>();
            
            // 1. Buscar os CourseProgresses
            var courseProgresses = await _dbContext.CourseProgresses
                .AsNoTracking()
                .Where(cp => cp.LearningHistoryId == studentId)
                .Select(cp => new { cp.Id, cp.CourseId, cp.IsCompleted, cp.LastUpdated })
                .ToListAsync();
                
            foreach (var cp in courseProgresses)
            {
                // 2. Para cada CourseProgress, buscar o campo CompletedLessons diretamente do SQLite
                var rawData = await _dbContext.Database
                    .ExecuteSqlRawAsync("SELECT CompletedLessons FROM CourseProgresses WHERE Id = @p0", cp.Id);
                    
                // 3. Obter o valor do campo CompletedLessons como string
                var jsonData = await _dbContext.Database
                    .SqlQueryRaw<string>("SELECT CompletedLessons AS Value FROM CourseProgresses WHERE Id = @id", 
                        new SqliteParameter("@id", cp.Id))
                    .FirstOrDefaultAsync();
                
                debugInfo.Add(new {
                    courseProgressId = cp.Id,
                    courseId = cp.CourseId,
                    isCompleted = cp.IsCompleted,
                    lastUpdated = cp.LastUpdated,
                    completedLessonsRaw = jsonData
                });
            }
            
            return Ok(new { debugInfo });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
} 