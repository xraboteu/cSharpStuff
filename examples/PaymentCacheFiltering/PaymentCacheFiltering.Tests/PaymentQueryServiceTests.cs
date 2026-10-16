using Moq;
using Xunit;

namespace PaymentCacheFiltering.Tests;

public sealed class PaymentQueryServiceTests
{
    private static (PaymentQueryService Service, IReadOnlyList<Payment> Source) Setup(int count = 100)
    {
        var source = PaymentCacheEntryBuilder.Build(count);
        var cache = new Mock<IPaymentCacheManager>();
        cache.Setup(x => x.GetPayments()).Returns(source);
        return (new PaymentQueryService(cache.Object), source);
    }

    [Fact]
    public void Builder_Produces_Deterministic_Varied_Data()
    {
        var first = PaymentCacheEntryBuilder.Build(100, 123);
        Assert.Equal(first, PaymentCacheEntryBuilder.Build(100, 123));
        Assert.True(first.Select(x => x.Desk).Distinct().Count() > 1);
        Assert.True(first.Select(x => x.ComplianceStatus).Distinct().Count() > 1);
        Assert.True(first.Select(x => x.Amount).Distinct().Count() > 1);
        Assert.NotEqual(first, PaymentCacheEntryBuilder.Build(100, 456));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public void ShowAllPayments_Ignores_Every_Filter(bool? showAll)
    {
        var (service, _) = Setup();
        var result = service.Search(new FilterStuckPaymentDto
        {
            ShowAllPayments = showAll,
            Desks = ["Nonexistent"],
            AmountFrom = 999999m,
            DateFrom = new DateTime(2099, 1, 1)
        });
        Assert.Equal(100, result.TotalCount);
        Assert.Equal(10, result.Payments.Count);
    }

    [Fact]
    public void False_Applies_All_Filters()
    {
        var (service, source) = Setup();
        var sample = source[0];
        var filter = new FilterStuckPaymentDto
        {
            ShowAllPayments = false,
            Instruments = [sample.PaymentInstrument],
            LocalInstruments = [sample.LegalInstrument],
            Statuses = [sample.ComplianceStatus],
            Categories = [sample.StatusCategory],
            Desks = [sample.Desk],
            Agents = [sample.Agent],
            Countries = [sample.Country],
            DateFrom = sample.ExecutionDate,
            DateTo = sample.ExecutionDate,
            AmountFrom = sample.Amount,
            AmountTo = sample.Amount,
            PageSize = 200
        };
        var expected = source.Where(p =>
            p.PaymentInstrument == sample.PaymentInstrument &&
            p.LegalInstrument == sample.LegalInstrument &&
            p.ComplianceStatus == sample.ComplianceStatus &&
            p.StatusCategory == sample.StatusCategory &&
            p.Desk == sample.Desk && p.Agent == sample.Agent &&
            p.Country == sample.Country && p.ExecutionDate == sample.ExecutionDate &&
            p.Amount == sample.Amount).Select(p => p.Id).ToArray();
        var result = service.Search(filter);
        Assert.Equal(expected.Length, result.TotalCount);
        Assert.Equal(expected, result.Payments.Select(p => p.Id));
    }

    [Fact]
    public void No_Match_Returns_Empty()
    {
        var (service, _) = Setup();
        var result = service.Search(new FilterStuckPaymentDto
            { ShowAllPayments = false, Desks = ["Unknown"] });
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Payments);
    }

    [Fact]
    public void Pagination_Preserves_Total_Count()
    {
        var (service, _) = Setup();
        var first = service.Search(new FilterStuckPaymentDto { PageSize = 12, CurrentPage = 1 });
        var second = service.Search(new FilterStuckPaymentDto { PageSize = 12, CurrentPage = 2 });
        Assert.Equal(100, second.TotalCount);
        Assert.Equal(12, second.Payments.Count);
        Assert.Empty(first.Payments.Select(x => x.Id).Intersect(second.Payments.Select(x => x.Id)));
    }

    [Fact]
    public void Multi_Column_Sort_Is_Applied()
    {
        var (service, source) = Setup();
        var result = service.Search(new FilterStuckPaymentDto
        {
            PageSize = 200,
            Sort = [new("Desk", SortDirection.Asc), new("Amount", SortDirection.Desc)]
        });
        var expected = source.OrderBy(x => x.Desk).ThenByDescending(x => x.Amount).Select(x => x.Id);
        Assert.Equal(expected, result.Payments.Select(x => x.Id));
    }

    [Fact]
    public void Queries_Do_Not_Modify_Cached_Collection()
    {
        var (service, source) = Setup();
        var before = source.ToArray();
        service.Search(new FilterStuckPaymentDto
        {
            ShowAllPayments = false,
            Statuses = [PaymentStatus.Pending],
            Sort = [new("Amount", SortDirection.Desc)]
        });
        Assert.Equal(before, source);
    }
}
