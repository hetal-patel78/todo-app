using Microsoft.AspNetCore.Mvc;
using Todo.Api.Models;
using Todo.Api.Repositories;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/todos")]
public sealed class TodosController(ITodoRepository todoRepository) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<TodoItem>> GetAll()
    {
        return Ok(todoRepository.GetAll());
    }

    [HttpPost]
    public ActionResult<TodoItem> Create(CreateTodoRequest request)
    {
        var item = todoRepository.Add(request.Title.Trim());
        return Created($"/api/todos/{item.Id}", item);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var deleted = todoRepository.Delete(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}