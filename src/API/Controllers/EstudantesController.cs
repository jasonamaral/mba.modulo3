using FluencyHub.API.SwaggerExamples;
using FluencyHub.StudentManagement.Application.Commands.ActivateStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;
using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluencyHub.StudentManagement.Application.Commands.DeactivateStudent;
using FluencyHub.StudentManagement.Application.Commands.UpdateStudent;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Models;
using FluencyHub.StudentManagement.Application.Queries.GetAllStudents;
using FluencyHub.StudentManagement.Application.Queries.GetStudentByEmail;
using FluencyHub.StudentManagement.Application.Queries.GetStudentById;
using FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;
using FluencyHub.StudentManagement.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;
using FluencyHub.SharedKernel.Common.Exceptions;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EstudantesController : Controller
{

    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentRepository _studentRepository;

    // Construtor simplificado para teste
    public EstudantesController(IMediator mediator, UserManager<ApplicationUser> userManager, IStudentRepository studentRepository)
    {
        _mediator = mediator;
        _userManager = userManager;
        _studentRepository = studentRepository; // Testando esta dependência
    }

    /// <summary>
    /// Obter todos os estudantes
    /// </summary>
    /// <returns>Lista de todos os estudantes</returns>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Obter todos os estudantes",
        Description = "Recupera uma lista de todos os estudantes no sistema. Requer função de Administrador.",
        OperationId = "GetAllStudents"
    )]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentListDtoExample))]
    public async Task<IActionResult> GetAllStudents()
    {
        try
        {
            var query = new GetAllStudentsQuery();
            var students = await _mediator.Send(query);
            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Obter o estudante atualmente autenticado
    /// </summary>
    /// <returns>Detalhes do estudante atual</returns>
    /// <response code="200">Retorna os detalhes do estudante atual</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="400">Se o e-mail não for encontrado no token</response>
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Obter estudante atual",
        Description = "Recupera os detalhes do estudante atualmente autenticado com base no token JWT",
        OperationId = "GetCurrentStudent"
    )]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentDtoExample))]
    public async Task<IActionResult> GetCurrentStudent()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);


        if (string.IsNullOrEmpty(userEmail))
        {
            return BadRequest(new { error = "E-mail não encontrado no token" });
        }

        try
        {
            var student = await _mediator.Send(new GetStudentByEmailQuery(userEmail));
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Student not found: {ex.Message}");
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Obter um estudante pelo ID
    /// </summary>
    /// <param name="id">ID do estudante a ser recuperado</param>
    /// <returns>Detalhes do estudante</returns>
    /// <response code="200">Retorna os detalhes do estudante</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Obter estudante por ID",
        Description = "Recupera um estudante específico pelo seu identificador único. Requer função de Administrador.",
        OperationId = "GetStudentById"
    )]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StudentDtoExample))]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        try
        {
            var query = new GetStudentByIdQuery { StudentId = id };
            var student = await _mediator.Send(query);
            return Ok(student);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Criar um novo estudante
    /// </summary>
    /// <param name="command">Detalhes do estudante</param>
    /// <returns>ID do estudante recém-criado</returns>
    /// <response code="201">Retorna o ID do estudante recém-criado</response>
    /// <response code="400">Se a requisição for inválida</response>
    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Criar um novo estudante",
        Description = "Registra um novo estudante no sistema com os detalhes fornecidos",
        OperationId = "CreateStudent"
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
    /// Atualizar um estudante existente
    /// </summary>
    /// <param name="id">ID do estudante a ser atualizado</param>
    /// <param name="command">Detalhes atualizados do estudante</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se o estudante foi atualizado com sucesso</response>
    /// <response code="400">Se a requisição for inválida</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualizar um estudante",
        Description = "Atualiza os detalhes de um estudante existente. O estudante só pode atualizar seu próprio perfil, a menos que seja um Administrador.",
        OperationId = "UpdateStudent"
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
            return BadRequest("O ID do estudante na rota deve corresponder ao ID do estudante no comando");
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

    /// <summary>
    /// Desativar uma conta de estudante
    /// </summary>
    /// <param name="id">ID do estudante a ser desativado</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se o estudante foi desativado com sucesso</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpPut("{id}/desativar")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Desativar um estudante",
        Description = "Desativa uma conta de estudante, impedindo-o de acessar o sistema. Requer função de Administrador.",
        OperationId = "DeactivateStudent"
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(DeactivateStudentCommand), typeof(DeactivateStudentCommandExample))]
    public async Task<IActionResult> DeactivateStudent(Guid id)
    {
        try
        {
            var command = new DeactivateStudentCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(new { message = "Estudante desativado com sucesso", result });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Ativar uma conta de estudante
    /// </summary>
    /// <param name="id">ID do estudante a ser ativado</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Se o estudante foi ativado com sucesso</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    /// <response code="403">Se o usuário não estiver autorizado</response>
    [HttpPut("{id}/ativar")]
    [Authorize(Roles = "Administrator")]
    [SwaggerOperation(
        Summary = "Ativar um estudante",
        Description = "Ativa uma conta de estudante previamente desativada, permitindo que ele acesse o sistema novamente. Requer função de Administrador.",
        OperationId = "ActivateStudent"
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerRequestExample(typeof(ActivateStudentCommand), typeof(ActivateStudentCommandExample))]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        try
        {
            var command = new ActivateStudentCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(new { message = "Estudante ativado com sucesso", result });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Obter o progresso de aprendizado de um estudante
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <returns>O progresso do estudante em todos os cursos</returns>
    /// <response code="200">Retorna o progresso do estudante</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    [HttpGet("{studentId}/progresso")]
    [SwaggerOperation(
        Summary = "Obter progresso do estudante",
        Description = "Recupera o progresso de aprendizado de um estudante em todos os cursos",
        OperationId = "GetStudentProgress"
    )]
    [ProducesResponseType(typeof(StudentProgressViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentProgress(Guid studentId)
    {
        try
        {
            var query = new GetStudentProgressQuery { StudentId = studentId };
            var progress = await _mediator.Send(query);
            return Ok(progress);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Marcar uma lição como concluída para um estudante
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <param name="courseId">ID do curso</param>
    /// <param name="lessonId">ID da lição</param>
    /// <returns>Resultado da conclusão</returns>
    /// <response code="200">Se a lição foi marcada como concluída com sucesso</response>
    /// <response code="400">Se houve um problema ao concluir a lição</response>
    /// <response code="404">Se o estudante, curso ou lição não for encontrado</response>
    [HttpPost("{studentId}/curso/{courseId}/licao/{lessonId}/completa")]
    [SwaggerOperation(
        Summary = "Completar uma lição para um estudante",
        Description = "Marca uma lição específica como concluída para um estudante em um curso específico",
        OperationId = "CompleteLessonForStudent"
    )]
    [ProducesResponseType(typeof(CompleteLessonForStudentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CompleteLessonForStudentResultExample))]
    public async Task<IActionResult> MarkLessonAsCompleted(Guid studentId, Guid courseId, Guid lessonId)
    {
        try
        {
            var command = new CompleteLessonForStudentCommand
            {
                StudentId = studentId,
                LessonId = lessonId,
                CompletionDate = DateTime.UtcNow
            };
            var result = await _mediator.Send(command);
            return Ok(result);
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

    /// <summary>
    /// Marcar um curso como concluído para um estudante
    /// </summary>
    /// <param name="studentId">ID do estudante</param>
    /// <param name="courseId">ID do curso</param>
    /// <returns>Resultado da conclusão</returns>
    /// <response code="200">Se o curso foi marcado como concluído com sucesso</response>
    /// <response code="400">Se houve um problema ao concluir o curso</response>
    /// <response code="404">Se o estudante ou curso não for encontrado</response>
    [HttpPost("{studentId}/curso/{courseId}/completo")]
    [SwaggerOperation(
        Summary = "Completar um curso para um estudante",
        Description = "Marca um curso como concluído para um estudante. Todas as lições devem ser concluídas primeiro.",
        OperationId = "CompleteCourseForStudent"
    )]
    [ProducesResponseType(typeof(CompleteCourseForStudentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CompleteCourseForStudentResultExample))]
    public async Task<IActionResult> CompleteCourse(Guid studentId, Guid courseId)
    {
        try
        {
            var command = new CompleteCourseForStudentCommand
            {
                StudentId = studentId,
                CourseId = courseId,
                CompletionDate = DateTime.UtcNow
            };
            var result = await _mediator.Send(command);
            return Ok(result);
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

}

