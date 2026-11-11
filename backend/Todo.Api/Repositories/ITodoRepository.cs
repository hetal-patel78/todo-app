using Todo.Api.Models;

namespace Todo.Api.Repositories;

public interface ITodoRepository
{
    IReadOnlyList<TodoItem> GetAll();
    TodoItem Add(string title);

    /// <returns><c>true</c> if an item was removed; <c>false</c> if it did not exist.</returns>
    bool Delete(Guid id);
}
