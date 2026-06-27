using Api.Domain;

namespace Api.Infrastructure;

public class CurrencyConverter(NbpClient nbpClient)
{
    public async Task<decimal?> ToPlnAsync(
        decimal amount, TransactionCurrency currency, CancellationToken ct)
    {
        decimal rate = 1m;

        if (currency == TransactionCurrency.EUR)
        {
            var euroRate = await nbpClient.GetEuroRateAsync(ct);
            if (euroRate is null)
                return null;
            rate = euroRate.Value;
        }

        return Math.Round(amount * rate, 2);
    }

}
