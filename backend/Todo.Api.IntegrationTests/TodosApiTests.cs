using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Todo.Api.Models;
using Todo.Api.Repositories;

namespace Todo.Api.IntegrationTests;

public class TodoApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    // Each test gets a fresh app, so the singleton in-memory store starts empty.
    private HttpClient NewClient() => factory.WithWebHostBuilder(_ => { }).CreateClient();

    [Fact]
    public async Task Post_TitleWithSpaces_StoresTrimmedTitle()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/todos", new { title = "   Milk   " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.Equal("Milk", created!.Title);
    }

    [Fact]
    public async Task Get_WhenNothingAdded_ReturnsEmptyArray()
    {
        var client = NewClient();

        var todos = await client.GetFromJsonAsync<List<TodoItem>>("/api/todos");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task Post_ValidTitle_Returns201WithLocationAndItemAppearsInList()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(created);
        Assert.Equal($"/api/todos/{created.Id}", response.Headers.Location?.OriginalString);

        var todos = await client.GetFromJsonAsync<List<TodoItem>>("/api/todos");
        Assert.Equal(created.Id, Assert.Single(todos!).Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_BlankTitle_Returns400(string title)
    {
        var response = await NewClient().PostAsJsonAsync("/api/todos", new { title });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingTitle_Returns400()
    {
        var response = await NewClient().PostAsJsonAsync("/api/todos", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_TitleTooLong_Returns400()
    {
        var title = new string('x', TodoRules.MaxTitleLength + 1);

        var response = await NewClient().PostAsJsonAsync("/api/todos", new { title });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingItem_Returns204AndRemovesIt()
    {
        var client = NewClient();
        var created = await (await client.PostAsJsonAsync("/api/todos", new { title = "temp" }))
            .Content.ReadFromJsonAsync<TodoItem>();

        var response = await client.DeleteAsync($"/api/todos/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<List<TodoItem>>("/api/todos"))!);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var response = await NewClient().DeleteAsync($"/api/todos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_IdThatIsNotAGuid_Returns404()
    {
        var response = await NewClient().DeleteAsync("/api/todos/abc");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnexpectedError_Returns500WithProblemDetails()
    {
        var client = factory
            .WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<ITodoRepository, ThrowingRepository>()))
            .CreateClient();

        var response = await client.GetAsync("/api/todos");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private sealed class ThrowingRepository : ITodoRepository
    {
        public IReadOnlyList<TodoItem> GetAll() => throw new InvalidOperationException("boom");
        public TodoItem Add(string title) => throw new InvalidOperationException("boom");
        public bool Delete(Guid id) => throw new InvalidOperationException("boom");
    }

}