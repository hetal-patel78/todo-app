using Microsoft.AspNetCore.Mvc;
using Todo.Api.Models;
using Todo.Api.Repositories;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/todos")]
public sealed class TodosController(ITodoRepository todoRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TodoItem>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<TodoItem>> GetAll()
    {
        return Ok(todoRepository.GetAll());
    }

    [HttpPost]
    [ProducesResponseType<TodoItem>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<TodoItem> Create(CreateTodoRequest request)
    {
        var item = todoRepository.Add(request.Title.Trim());
        return Created($"/api/todos/{item.Id}", item);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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