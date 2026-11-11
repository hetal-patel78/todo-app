namespace Todo.Api.Models;

public sealed record TodoItem(Guid Id, string Title, DateTimeOffset CreatedAt);
