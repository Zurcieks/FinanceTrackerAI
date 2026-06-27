using Api.Infrastructure;

namespace Api.Features.Transactions.Delete;


public static class DeleteTransaction
{
    public static IEndpointRouteBuilder MapDeleteTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/transactions/{id}", Handle)
            .WithTags("Transactions")
            .WithName("DeleteTransaction");

        return app;
    }

    private static async Task<IResult> Handle(
        Guid id,
        AppDbContext context,
        ReceiptStorage storage,
        CancellationToken ct)
    {
        var transaction = await context.Transactions.FindAsync([id], ct);

        if (transaction is null)
            return Results.NotFound("Transaction not found");

        if (transaction.ReceiptKey is not null)
            await storage.DeleteAsync(transaction.ReceiptKey, ct);


        context.Transactions.Remove(transaction);

        await context.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
