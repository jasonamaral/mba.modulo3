using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Infrastructure.Persistence.Repositories;

public class LearningRepository : ILearningRepository
{
    private readonly FluencyHubDbContext _context;

    public LearningRepository(FluencyHubDbContext context)
    {
        _context = context;
    }

    public async Task<LearningHistory?> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.LearningHistories
            .Include(lh => lh.CourseProgress)
                .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.Id == studentId, cancellationToken);
    }

    public async Task<CourseProgress?> GetCourseProgressAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.LearningHistoryId == studentId && cp.CourseId == courseId, cancellationToken);
    }

    public async Task<bool> HasCompletedLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress != null && courseProgress.CompletedLessons.Any(cl => cl.LessonId == lessonId);
    }

    public async Task<int> GetCompletedLessonsCountAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress?.CompletedLessons.Count ?? 0;
    }

    public async Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress?.CompletedLessons.Select(cl => cl.LessonId).ToList() ?? new List<Guid>();
    }

    public async Task AddAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        // Abordagem mais direta para garantir que a entidade seja rastreada corretamente
        // e para evitar problemas de concorrência
        try
        {
            // Desativa o rastreamento automático para evitar conflitos
            _context.ChangeTracker.Clear();
            
            // Busca ou cria a entidade LearningHistory
            var existingHistory = await _context.LearningHistories
                .FirstOrDefaultAsync(lh => lh.Id == learningHistory.Id, cancellationToken);

            if (existingHistory == null)
            {
                // Se não existir, adiciona como nova - isso nunca deve acontecer neste ponto,
                // mas é uma medida defensiva
                await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }
            
            // Atualiza a data de modificação
            existingHistory.UpdatedAt = DateTime.UtcNow;
            
            // Percorre os CourseProgresses para adicionar/atualizar
            foreach (var progress in learningHistory.CourseProgress)
            {
                // Busca ou cria o CourseProgress
                var existingProgress = await _context.CourseProgresses
                    .FirstOrDefaultAsync(cp => cp.CourseId == progress.CourseId && 
                                               cp.LearningHistoryId == learningHistory.Id, 
                                         cancellationToken);
                
                if (existingProgress == null)
                {
                    // Se não existir, cria um novo
                    existingProgress = new CourseProgress(progress.CourseId)
                    {
                        LearningHistoryId = learningHistory.Id
                    };
                    await _context.CourseProgresses.AddAsync(existingProgress, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                
                // Marca como concluído se necessário
                if (progress.IsCompleted && !existingProgress.IsCompleted)
                {
                    existingProgress.CompleteCourse();
                }
                
                // Adiciona as lições concluídas
                foreach (var lessonToAdd in progress.CompletedLessons)
                {
                    // Verifica se a lição já foi concluída
                    bool lessonAlreadyCompleted = await _context.CompletedLessons
                        .AnyAsync(cl => cl.CourseProgressId == existingProgress.Id && 
                                       cl.LessonId == lessonToAdd.LessonId,
                                 cancellationToken);
                    
                    if (!lessonAlreadyCompleted)
                    {
                        // Adiciona diretamente na tabela CompletedLessons
                        var completedLesson = new CompletedLesson
                        {
                            LessonId = lessonToAdd.LessonId,
                            CourseProgressId = existingProgress.Id,
                            CompletedAt = DateTime.UtcNow
                        };
                        
                        await _context.CompletedLessons.AddAsync(completedLesson, cancellationToken);
                    }
                }
            }
            
            // Salva todas as mudanças
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Se ocorrer um erro de concorrência, tentamos novamente com uma abordagem direta
            // Adicionamos cada lição concluída individualmente
            foreach (var progress in learningHistory.CourseProgress)
            {
                foreach (var lesson in progress.CompletedLessons)
                {
                    try
                    {
                        // Verificar se a lição já existe
                        bool exists = await _context.CompletedLessons
                            .AnyAsync(cl => cl.LessonId == lesson.LessonId &&
                                         cl.CourseProgress.CourseId == progress.CourseId &&
                                         cl.CourseProgress.LearningHistoryId == learningHistory.Id,
                                  cancellationToken);
                        
                        if (!exists)
                        {
                            // Encontrar o CourseProgress correto
                            var courseProgress = await _context.CourseProgresses
                                .FirstOrDefaultAsync(cp => cp.CourseId == progress.CourseId &&
                                                      cp.LearningHistoryId == learningHistory.Id,
                                               cancellationToken);
                            
                            if (courseProgress != null)
                            {
                                // Inserir diretamente na tabela
                                await _context.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO CompletedLessons (Id, LessonId, CourseProgressId, CompletedAt) VALUES ({0}, {1}, {2}, {3})",
                                    Guid.NewGuid(), lesson.LessonId, courseProgress.Id, DateTime.UtcNow);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignora erros individuais para tentar processar o máximo possível
                        continue;
                    }
                }
            }
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
} 