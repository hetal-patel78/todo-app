using Todo.Api.Models;
namespace Todo.Api.Repositories;

public interface ITodoRepository
{
    IReadOnlyList<TodoItem> GetAll();
}


