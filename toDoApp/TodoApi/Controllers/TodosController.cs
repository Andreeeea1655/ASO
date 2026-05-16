using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoCircularBufferService _service;

    public TodosController(TodoCircularBufferService service)
    {
        _service = service;
    }

    // GET api/todos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var todos = await _service.GetAllAsync();
        return Ok(todos);
    }

    // GET api/todos/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _service.GetStatsAsync();
        return Ok(stats);
    }

    // GET api/todos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todo = await _service.GetByIdAsync(id);
        if (todo == null) return NotFound(new { message = $"Todo cu ID={id} nu există." });
        return Ok(todo);
    }

    // POST api/todos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Titlul este obligatoriu." });

        var todo = await _service.AddTodoAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    // PUT api/todos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoDto dto)
    {
        var todo = await _service.UpdateAsync(id, dto);
        if (todo == null) return NotFound(new { message = $"Todo cu ID={id} nu există." });
        return Ok(todo);
    }

    // DELETE api/todos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = $"Todo cu ID={id} nu există." });
        return NoContent();
    }
}
