using Todo.Api.Models;

namespace Todo.Api.Repositories;

public sealed class InMemoryTodoRepository(TimeProvider clock) : ITodoRepository
{

    private readonly Lock _gate = new();
    private readonly List<TodoItem> _items = [];

    public TodoItem Add(string title)
    {
        var item = new TodoItem(Guid.NewGuid(), title, clock.GetUtcNow());
        lock (_gate)
        {
            _items.Add(item);
        }
        return item;
    }

    public bool Delete(Guid id)
    {
        lock (_gate)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item is null)
            {
                return false;
            }

            return _items.Remove(item);
        }
    }

    public IReadOnlyList<TodoItem> GetAll()
    {
        lock (_gate)
        {
            return _items.ToArray();
        }

    }
}