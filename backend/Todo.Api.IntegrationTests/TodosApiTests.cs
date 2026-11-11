using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Todo.Api.Models;

namespace Todo.Api.IntegrationTests;

public class TodoApiTests
{
    [Fact]
    public async Task Post_TitleWithSpaces_StoresTrimmedTitle()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/todos", new { title = "   Milk   " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.Equal("Milk", created!.Title);
    }

}