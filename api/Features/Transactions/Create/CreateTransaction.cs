using Api.Common;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Transactions.Create;

public record CreateTransactionRequest(
    string MerchantName,
    string? Description,
    decimal Amount,
    TransactionCurrency Currency,
    TransactionType Type,
    string? ReceiptKey,
    DateOnly? Date,
    Guid CategoryId);

public record CreateTransactionResponse(
    Guid Id,
    string MerchantName,
    string? Description,
    decimal Amount,
    decimal AmountInPLN,
    TransactionCurrency Currency,
    TransactionType Type,
    string? ReceiptKey,
    DateOnly Date,
    DateTime CreatedAt,
    Guid CategoryId)
{
    public static CreateTransactionResponse FromEntity(Transaction t) =>
        new(t.Id, t.MerchantName, t.Description, t.Amount, t.AmountInPLN, t.Currency, t.Type, t.ReceiptKey, t.Date, t.CreatedAt, t.CategoryId);
};

public static class CreateTransaction
{
    public static IEndpointRouteBuilder MapCreateTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions", Handle)
            .WithTags("Transactions")
            .WithName("CreateTransaction")
            .AddEndpointFilter<ValidationFilter<CreateTransactionRequest>>();
        return app;
    }

    private static async Task<IResult> Handle(CreateTransactionRequest request, AppDbContext db, ReceiptStorage storage, CurrencyConverter converter, CancellationToken ct)
    {

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);

        if (category is null)
            return Results.BadRequest("Category not found");

        if (category.IsArchived)
            return Results.BadRequest("Cannot add transaction to archived category");

        if (request.ReceiptKey is not null && !await storage.ExistsAsync(request.ReceiptKey, ct))
            return Results.BadRequest("Receipt not found.");

        var amountInPLN = await converter.ToPlnAsync(request.Amount, request.Currency, ct);

        if (amountInPLN is null)
            return Results.Problem("Failed to fetch euro rate from NBP", statusCode: 502);

        var warsaw = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, warsaw));

        var date = request.Date ?? today;


        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            MerchantName = request.MerchantName,
            Description = request.Description,
            Amount = request.Amount,
            AmountInPLN = amountInPLN.Value,
            Currency = request.Currency,
            Type = request.Type,
            ReceiptKey = request.ReceiptKey,
            Date = date,
            CreatedAt = DateTime.UtcNow,
            CategoryId = request.CategoryId,
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/transactions/{transaction.Id}",
            CreateTransactionResponse.FromEntity(transaction));

    }
}
