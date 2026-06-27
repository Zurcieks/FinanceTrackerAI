namespace Api.Infrastructure;

public class NbpClient(HttpClient http)
{
    public async Task<decimal?> GetEuroRateAsync(CancellationToken ct)
    {
        var response = await http.GetAsync(
            "exchangerates/rates/a/eur/?format=json", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var data = await response.Content.ReadFromJsonAsync<NbpRateResponse>(cancellationToken: ct);

        return data?.Rates?.FirstOrDefault()?.Mid;

    }
}

internal record NbpRateResponse(string Table, string Currency, string Code, List<NbpRate> Rates);
internal record NbpRate(string No, DateTime EffectiveDate, decimal Mid);
