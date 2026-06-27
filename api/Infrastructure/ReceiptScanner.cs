using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
namespace Api.Infrastructure;

public record ScannedReceipt(
    string? MerchantName, decimal? Amount, string? Currency,
    DateOnly? Date, string? Type, Guid? SuggestedCategoryId);


public class ReceiptScanner
{
    private readonly ChatClient _client;
    private readonly ILogger<ReceiptScanner> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ReceiptScanner(IConfiguration config, ILogger<ReceiptScanner> logger)
    {
        var apiKey = config["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");

        var model = config["OpenAI:Model"] ?? "gpt-5.4-mini";

        _client = new ChatClient(model, apiKey);
        _logger = logger;
    }

    public async Task<ScannedReceipt?> ScanAsync(
        Stream image, string contentType, string categoriesContext, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms, ct);
        var imageBytes = BinaryData.FromBytes(ms.ToArray());

        var prompt = $$"""
            Jesteś asystentem wyciągającym dane z paragonów.
            Przeanalizuj obraz paragonu i zwróć WYŁĄCZNIE JSON o strukturze:
            {
              "merchantName": string | null,
              "amount": number | null,
              "currency": "PLN" | "EUR" | null,
              "date": "YYYY-MM-DD" | null,
              "type": "Expense" | "Income" | null,
              "suggestedCategoryId": string | null
            }
            Jeśli jakiegoś pola nie da się odczytać, ustaw null.
            Dla suggestedCategoryId wybierz najlepiej pasującą kategorię
            z poniższej listy (zwróć jej Id) albo null;
            {{categoriesContext}}
            Nie dodawaj żadnego tekstu poza JSON.
        """;

        var messages = new List<ChatMessage>
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(prompt),
                ChatMessageContentPart.CreateImagePart(imageBytes, contentType)
            )
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        try
        {
            var completion = await _client.CompleteChatAsync(messages, options, ct);

            if (completion.Value.Content.Count == 0)
            {
                _logger.LogWarning("OpenAI returned empty content for receipt scan");
                return null;
            }

            var json = completion.Value.Content[0].Text;
            return JsonSerializer.Deserialize<ScannedReceipt>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to scan receipt via OpenAI");
            return null;
        }
    }

}

