using Todo.Api.Repositories;

namespace Todo.Api.Tests;

public class InMemoryTodoRepositoryTests
{
    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();

        // Act
        var result = repo.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Add_StoresItemAndAssignsIdAndTitle()
    {
        var repo = new InMemoryTodoRepository();

        var item = repo.Add("Write tests");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("Write tests", item.Title);
        Assert.Equal(item, Assert.Single(repo.GetAll()));
    }

    [Fact]
    public void GetAll_ReturnsItemsInInsertionOrder()
    {
        var repo = new InMemoryTodoRepository();
        repo.Add("first");
        repo.Add("second");

        var titles = repo.GetAll().Select(i => i.Title);

        Assert.Equal(["first", "second"], titles);
    }

    [Fact]
    public void Delete_ExistingItem_RemovesItAndReturnsTrue()
    {
        var repo = new InMemoryTodoRepository();
        var keep = repo.Add("keep");
        var remove = repo.Add("remove");

        var deleted = repo.Delete(remove.Id);

        Assert.True(deleted);
        Assert.Equal(keep, Assert.Single(repo.GetAll()));
    }

    [Fact]
    public void Delete_UnknownId_ReturnsFalse()
    {
        var repo = new InMemoryTodoRepository();

        var deleted = repo.Delete(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public void Add_FromManyThreadsAtOnce_KeepsEveryItem()
    {
        var repo = new InMemoryTodoRepository();

        Parallel.For(0, 10_000, i => repo.Add($"item {i}"));

        Assert.Equal(10_000, repo.GetAll().Count);
    }

    [Fact]
    public void Delete_FromManyThreadsAtOnce_RemovesEveryItem()
    {
        var repo = new InMemoryTodoRepository();
        var ids = Enumerable.Range(0, 10_000).Select(i => repo.Add($"item {i}").Id).ToArray();

        var results = new bool[ids.Length];
        Parallel.For(0, ids.Length, i => results[i] = repo.Delete(ids[i]));

        Assert.All(results, Assert.True);
        Assert.Empty(repo.GetAll());
    }

}