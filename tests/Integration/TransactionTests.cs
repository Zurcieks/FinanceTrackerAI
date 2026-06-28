using System.Net;
using System.Net.Http.Json;
namespace Tests.Integration;

public class TransactionTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateCategoryAsync()
    {
        var category = new { name = $"test-{Guid.NewGuid()}", hexColor = "#FF5733", icon = "tag.fill" };
        var response = await _client.PostAsJsonAsync("/api/categories", category);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        return created!.Id;
    }

    private async Task<HttpResponseMessage> PostTransactionAsync(Guid categoryId, string merchant, decimal amount)
    {
        var transaction = new
        {
            MerchantName = merchant,
            Amount = amount,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId
        };
        return await _client.PostAsJsonAsync("/api/transactions", transaction);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_PersistsWithCorrectPln_WhenCurrencyIsPln()
    {
        var categoryId = await CreateCategoryAsync();

        var response = await PostTransactionAsync(categoryId, "Żabka", 100m);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TransactionDto>();
        Assert.Equal(100m, created!.AmountInPLN);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenCategoryIsArchived()
    {
        var categoryId = await CreateCategoryAsync();

        var archive = await _client.PatchAsync($"/api/categories/{categoryId}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);

        var response = await PostTransactionAsync(categoryId, "Żabka", 100m);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_PersistsChanges_WhenRequestIsValid()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await PostTransactionAsync(categoryId, "Żabka", 100m);
        var created = await createResponse.Content.ReadFromJsonAsync<TransactionDto>();

        var update = new
        {
            MerchantName = "Biedronka",
            Amount = 250m,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId
        };
        var response = await _client.PatchAsJsonAsync($"/api/transactions/{created!.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<TransactionDto>();
        Assert.Equal("Biedronka", updated!.MerchantName);
        Assert.Equal(250m, updated.Amount);
        Assert.Equal(250m, updated.AmountInPLN);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsNotFound_WhenIdDoesNotExist()
    {
        var update = new
        {
            MerchantName = "Żabka",
            Amount = 100m,
            Currency = "PLN",
            Type = "Expense",
            Date = (string?)null,
            CategoryId = Guid.NewGuid()
        };
        var response = await _client.PatchAsJsonAsync($"/api/transactions/{Guid.NewGuid()}", update);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record TransactionDto(
        Guid Id,
        string MerchantName,
        string? Description,
        decimal Amount,
        decimal AmountInPLN,
        string Currency,
        string Type,
        string? ReceiptKey,
        DateOnly? Date,
        Guid CategoryId);

    private record CategoryDto(
        Guid Id,
        string Name,
        string HexColor,
        string Icon,
        bool IsDefault,
        bool IsArchived);
}
