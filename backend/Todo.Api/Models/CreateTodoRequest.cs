using System.ComponentModel.DataAnnotations;
public sealed record CreateTodoRequest(
    [Required(AllowEmptyStrings = false)]
    [StringLength(TodoRules.MaxTitleLength, MinimumLength = 1)]
    string Title);

public static class TodoRules
{
    public const int MaxTitleLength = 200;
}