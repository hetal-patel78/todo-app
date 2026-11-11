using Microsoft.Extensions.Time.Testing;
using Todo.Api.Repositories;

namespace Todo.Api.Tests;

public class InMemoryTodoRepositoryTests
{
    private static InMemoryTodoRepository Create() => new(TimeProvider.System);
    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        var repo = Create();

        // Act
        var result = repo.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Add_StoresItemAndAssignsIdAndTitle()
    {
        var repo = Create();

        var item = repo.Add("Write tests");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("Write tests", item.Title);
        Assert.Equal(item, Assert.Single(repo.GetAll()));
    }

    [Fact]
    public void GetAll_ReturnsItemsInInsertionOrder()
    {
        var repo = Create();
        repo.Add("first");
        repo.Add("second");

        var titles = repo.GetAll().Select(i => i.Title);

        Assert.Equal(["first", "second"], titles);
    }

    [Fact]
    public void Delete_ExistingItem_RemovesItAndReturnsTrue()
    {
        var repo = Create();
        var keep = repo.Add("keep");
        var remove = repo.Add("remove");

        var deleted = repo.Delete(remove.Id);

        Assert.True(deleted);
        Assert.Equal(keep, Assert.Single(repo.GetAll()));
    }

    [Fact]
    public void Delete_UnknownId_ReturnsFalse()
    {
        var repo = Create();

        var deleted = repo.Delete(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public void Add_FromManyThreadsAtOnce_KeepsEveryItem()
    {
        var repo = Create();

        Parallel.For(0, 10_000, i => repo.Add($"item {i}"));

        Assert.Equal(10_000, repo.GetAll().Count);
    }

    [Fact]
    public void Delete_FromManyThreadsAtOnce_RemovesEveryItem()
    {
        var repo = Create();
        var ids = Enumerable.Range(0, 10_000).Select(i => repo.Add($"item {i}").Id).ToArray();

        var results = new bool[ids.Length];
        Parallel.For(0, ids.Length, i => results[i] = repo.Delete(ids[i]));

        Assert.All(results, Assert.True);
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void GetAll_ReturnsSnapshot_NotAffectedByLaterAdds()
    {
        var repo = Create();
        repo.Add("first");

        var snapshot = repo.GetAll();
        repo.Add("second");
        Assert.Single(snapshot);
    }

    [Fact]
    public void Add_SetsCreatedAtFromTheClock()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
        var repo = new InMemoryTodoRepository(clock);

        var item = repo.Add("Milk");

        Assert.Equal(clock.GetUtcNow(), item.CreatedAt);
    }
}