using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class LearningRepository : ILearningRepository
{
    private readonly StudentDbContext _context;

    public LearningRepository(StudentDbContext context)
    {
        _context = context;
    }

    public async Task<LearningHistory?> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.LearningHistories
            .Include(lh => lh.CourseProgresses)
            .ThenInclude(cp => cp.CompletedLessons)
            .Include(lh => lh.Records)
            .FirstOrDefaultAsync(lh => lh.Id == studentId);
    }

    public async Task<LearningHistory?> GetByStudentIdFreshAsync(Guid studentId)
    {
        return await _context.LearningHistories
            .AsNoTracking()
            .Include(lh => lh.CourseProgresses)
            .ThenInclude(cp => cp.CompletedLessons)
            .Include(lh => lh.Records)
            .FirstOrDefaultAsync(lh => lh.Id == studentId);
    }

    public async Task<CourseProgress> GetCourseProgressAsync(Guid courseId, Guid learningHistoryId)
    {
        var progress = await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.CourseId == courseId && cp.LearningHistoryId == learningHistoryId);
            
        if (progress == null)
            throw new InvalidOperationException($"Progresso do curso não encontrado para courseId {courseId} e learningHistoryId {learningHistoryId}");
            
        return progress;
    }

    public async Task<IEnumerable<CourseProgress>> GetAllCourseProgressAsync(Guid studentId)
    {
        var learningHistory = await GetByStudentIdAsync(studentId);
        if (learningHistory == null)
            return new List<CourseProgress>();

        return learningHistory.CourseProgresses;
    }

    public async Task AddLearningHistoryAsync(LearningHistory learningHistory)
    {
        await _context.LearningHistories.AddAsync(learningHistory);
    }

    public async Task AddCourseProgressAsync(CourseProgress courseProgress)
    {
        await _context.CourseProgresses.AddAsync(courseProgress);
    }

    public async Task AddCompletedLessonAsync(CompletedLesson completedLesson)
    {
        await _context.CompletedLessons.AddAsync(completedLesson);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // SOLUÇÃO SIMPLIFICADA: Forçar detecção de mudanças antes de salvar
        _context.ChangeTracker.DetectChanges();
        
        // Adicionar qualquer entidade detached que possa ter sido criada via domínio
        var learningHistories = _context.ChangeTracker.Entries<LearningHistory>()
            .Select(e => e.Entity)
            .ToList();

        foreach (var learningHistory in learningHistories)
        {
            foreach (var courseProgress in learningHistory.CourseProgresses)
            {
                if (_context.Entry(courseProgress).State == EntityState.Detached)
                {
                    _context.CourseProgresses.Add(courseProgress);
                }

                foreach (var completedLesson in courseProgress.CompletedLessons)
                {
                    if (_context.Entry(completedLesson).State == EntityState.Detached)
                    {
                        _context.CompletedLessons.Add(completedLesson);
                    }
                }
            }

            foreach (var record in learningHistory.Records)
            {
                if (_context.Entry(record).State == EntityState.Detached)
                {
                    _context.LearningRecords.Add(record);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasCompletedLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var learningHistory = await GetByStudentIdAsync(studentId);
        if (learningHistory == null)
            return false;
            
        return learningHistory.CourseProgresses
            .SelectMany(cp => cp.CompletedLessons)
            .Any(cl => cl.LessonId == lessonId);
    }

    public async Task<int> GetCompletedLessonsCountAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        // Buscar courseProgress de forma segura, sem lançar exceção se não existir
        var courseProgress = await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.CourseId == courseId && cp.LearningHistoryId == studentId, cancellationToken);
            
        return courseProgress?.CompletedLessons.Count ?? 0;
    }

    public async Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        // Buscar courseProgress de forma segura, sem lançar exceção se não existir
        var courseProgress = await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.CourseId == courseId && cp.LearningHistoryId == studentId, cancellationToken);
            
        return courseProgress?.CompletedLessons.Select(cl => cl.LessonId).ToList() ?? new List<Guid>();
    }

    public async Task CompleteLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        // SOLUÇÃO ROBUSTA: Usar transação explícita e verificação de existência
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Verificar se a lição já foi completada (evitar duplicatas)
            var existingCompletion = await _context.CompletedLessons
                .AnyAsync(cl => cl.LessonId == lessonId && 
                    cl.CourseProgress.LearningHistoryId == studentId &&
                    cl.CourseProgress.CourseId == courseId, cancellationToken);
                    
            if (existingCompletion)
            {
                // Lição já completada, não fazer nada
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            // 2. Obter ou criar LearningHistory
            var learningHistory = await _context.LearningHistories
                .Include(lh => lh.CourseProgresses)
                .ThenInclude(cp => cp.CompletedLessons)
                .FirstOrDefaultAsync(lh => lh.Id == studentId, cancellationToken);
            
            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(studentId);
                await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 3. Obter ou criar CourseProgress
            var courseProgress = learningHistory.CourseProgresses
                .FirstOrDefault(cp => cp.CourseId == courseId);

            if (courseProgress == null)
            {
                courseProgress = new CourseProgress(courseId)
                {
                    LearningHistoryId = learningHistory.Id
                };
                
                // Adicionar explicitamente ao contexto
                await _context.CourseProgresses.AddAsync(courseProgress, cancellationToken);
                learningHistory.AddCourseProgress(courseProgress);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 4. Criar e adicionar CompletedLesson diretamente
            var completedLesson = new CompletedLesson
            {
                Id = Guid.NewGuid(),
                LessonId = lessonId,
                CompletedAt = DateTime.UtcNow,
                CourseProgressId = courseProgress.Id
            };

            await _context.CompletedLessons.AddAsync(completedLesson, cancellationToken);
            
            // 5. Atualizar CourseProgress via domínio
            courseProgress.CompleteLesson(lessonId);
            
            // 6. Salvar mudanças
            await _context.SaveChangesAsync(cancellationToken);
            
            // 7. Confirmar transação
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UncompleteLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var learningHistory = await GetByStudentIdAsync(studentId);
        if (learningHistory == null)
            return;
            
        var completedLesson = await _context.CompletedLessons
            .FirstOrDefaultAsync(cl => cl.LessonId == lessonId && 
                cl.CourseProgress.LearningHistoryId == learningHistory.Id,
                cancellationToken);
            
        if (completedLesson != null)
        {
            _context.CompletedLessons.Remove(completedLesson);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<LearningHistory> GetLearningHistoryByStudentIdAsync(Guid studentId)
    {
        return await _context.LearningHistories
            .Include(lh => lh.CourseProgresses)
            .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.Id == studentId);
    }

    public async Task<CourseProgress> GetCourseProgressByIdAsync(Guid courseProgressId)
    {
        return await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.Id == courseProgressId);
    }

    public async Task<IEnumerable<CourseProgress>> GetCourseProgressesByStudentIdAsync(Guid studentId)
    {
        // Forçar recarregamento dos dados do banco para evitar problemas de cache
        var learningHistory = await _context.LearningHistories
            .AsNoTracking() // Não rastrear para forçar dados frescos
            .Include(lh => lh.CourseProgresses)
            .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.Id == studentId);

        return learningHistory?.CourseProgresses ?? new List<CourseProgress>();
    }

    public async Task<IEnumerable<CompletedLesson>> GetCompletedLessonsByCourseProgressIdAsync(Guid courseProgressId)
    {
        var courseProgress = await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.Id == courseProgressId);

        return courseProgress?.CompletedLessons ?? new List<CompletedLesson>();
    }
} 