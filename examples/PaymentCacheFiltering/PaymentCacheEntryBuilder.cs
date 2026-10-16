namespace PaymentCacheFiltering;

/// <summary>Seeded pseudo-random fixture data: varied but reproducible.</summary>
public static class PaymentCacheEntryBuilder
{
    public static IReadOnlyList<Payment> Build(int count = 100, int seed = 42)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        var random = new Random(seed);
        string[] legal = ["Bond", "Equity", "Fund"];
        string[] instruments = ["Wire", "Swift", "Sepa"];
        string[] desks = ["Frankfurt", "Paris", "London"];
        string[] agents = ["Alice", "Bob", "Charlie"];
        string[] countries = ["FR", "DE", "GB"];
        return Enumerable.Range(1, count).Select(id => new Payment(
            id,
            legal[random.Next(legal.Length)],
            instruments[random.Next(instruments.Length)],
            (PaymentStatus)random.Next(Enum.GetValues<PaymentStatus>().Length),
            (StatusCategory)random.Next(Enum.GetValues<StatusCategory>().Length),
            desks[random.Next(desks.Length)],
            agents[random.Next(agents.Length)],
            countries[random.Next(countries.Length)],
            new DateTime(2020, 1, 1).AddDays(random.Next(365)),
            random.Next(1, 10001) / 10m)).ToArray();
    }
}
