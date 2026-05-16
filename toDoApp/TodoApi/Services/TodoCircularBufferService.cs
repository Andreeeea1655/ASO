using TodoApi.Data;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Services;

/// <summary>
/// Circular Buffer pur pentru TODO-uri — comportament FIFO.
/// Capacitate maximă: MaxCapacity items.
/// Când buffer-ul e plin și se adaugă un item nou,
/// cel mai VECHI todo (primul intrat) este șters automat,
/// iar cel nou este adăugat la final.
/// </summary>
public class TodoCircularBufferService
{
    private readonly TodoDbContext _db;
    private readonly ILogger<TodoCircularBufferService> _logger;

    public const int MaxCapacity = 10;

    public TodoCircularBufferService(TodoDbContext db, ILogger<TodoCircularBufferService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<TodoItem> AddTodoAsync(CreateTodoDto dto)
    {
        var count = await _db.Todos.CountAsync();

        if (count >= MaxCapacity)
        {
            // CIRCULAR BUFFER: scoatem primul intrat (cel mai vechi după CreatedAt)
            var oldest = await _db.Todos
                .OrderBy(t => t.CreatedAt)
                .FirstAsync();

            _logger.LogInformation(
                "Circular buffer plin ({Count}/{Max}). Se elimină todo ID={Id} ('{Title}') — primul intrat.",
                count, MaxCapacity, oldest.Id, oldest.Title);

            _db.Todos.Remove(oldest);
            await _db.SaveChangesAsync();
        }

        var todo = new TodoItem
        {
            Title    = dto.Title,
            Description = dto.Description,
            CreatedAt   = DateTime.UtcNow
        };

        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();
        return todo;
    }

    public async Task<List<TodoItem>> GetAllAsync()
        => await _db.Todos
            .OrderBy(t => t.CreatedAt)   // afișăm în ordinea în care au intrat
            .ToListAsync();

    public async Task<TodoItem?> GetByIdAsync(int id)
        => await _db.Todos.FindAsync(id);

    public async Task<TodoItem?> UpdateAsync(int id, UpdateTodoDto dto)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo == null) return null;

        if (dto.Title != null)       todo.Title       = dto.Title;
        if (dto.Description != null) todo.Description = dto.Description;
        if (dto.IsCompleted.HasValue)
        {
            todo.IsCompleted  = dto.IsCompleted.Value;
            todo.CompletedAt  = dto.IsCompleted.Value ? DateTime.UtcNow : null;
        }

        await _db.SaveChangesAsync();
        return todo;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo == null) return false;
        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<BufferStats> GetStatsAsync()
    {
        var todos = await _db.Todos.ToListAsync();
        return new BufferStats
        {
            CurrentCount   = todos.Count,
            MaxCapacity    = MaxCapacity,
            CompletedCount = todos.Count(t => t.IsCompleted),
            PendingCount   = todos.Count(t => !t.IsCompleted),
            IsAtCapacity   = todos.Count >= MaxCapacity
        };
    }
}

public class BufferStats
{
    public int    CurrentCount   { get; set; }
    public int    MaxCapacity    { get; set; }
    public int    CompletedCount { get; set; }
    public int    PendingCount   { get; set; }
    public bool   IsAtCapacity   { get; set; }
    public double FillPercent    => MaxCapacity > 0 ? (double)CurrentCount / MaxCapacity * 100 : 0;
}

