using System.Net;
using System.Net.Http.Json;


namespace Tests.Integration;

public class CategoriesTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/categories");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsBadRequest()
    {
        var request = new { name = "", hexColor = "#FF5733", icon = "tag.fill" };

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    }

    [Fact]
    public async Task CreateCategory_ReturnsCreated_WhenRequestIsValid()
    {
        var name = $"test-{Guid.NewGuid()}";
        var request = new { name, hexColor = "#FF5733", icon = "tag.fill" };

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_PersistCategory_WhenRequestIsValid()
    {
        var name = $"test-{Guid.NewGuid()}";

        var request = new { name, hexColor = "#FF5733", icon = "tag.fill" };

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");

        Assert.Contains(categories!, c => c.Name == name);

    }

    [Fact]
    public async Task CreateCategory_ReturnsConflict_WhenNameAlreadyExists()
    {
        var name = $"test-{Guid.NewGuid()}";
        var request = new { name, hexColor = "#FF5733", icon = "tag.fill" };

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var conflict = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task ArchiveCategory_RemovesFromList_WhenArchived()
    {
        var name = $"test-{Guid.NewGuid()}";
        var request = new { name, hexColor = "#FF5733", icon = "tag.fill" };

        var response = await _client.PostAsJsonAsync("/api/categories", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();

        var update = await _client.PatchAsync($"/api/categories/{created!.Id}/archive", null);

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");
        Assert.DoesNotContain(categories!, c => c.Name == name);

    }
    private record CategoryDto(Guid Id, string Name, string HexColor, string Icon, bool IsDefault, bool IsArchived);
}


