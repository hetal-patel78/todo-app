using Todo.Api.Models;

namespace Todo.Api.Repositories;

public sealed class InMemoryTodoRepository : ITodoRepository
{

    private readonly List<TodoItem> _items = [];
    public TodoItem Add(string title)
    {
        var item = new TodoItem(Guid.NewGuid(), title, DateTimeOffset.UtcNow);
        _items.Add(item);
        return item;
    }

    public bool Delete(Guid id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null)
        {
            return false;
        }

        return _items.Remove(item);
    }

    public IReadOnlyList<TodoItem> GetAll()
    {

        return _items;
    }
}