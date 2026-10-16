using Moq;
using Xunit;

namespace PaymentCacheFiltering.Tests;

/// <summary>
/// Additional behavior-focused tests using the existing seeded cache builder.
/// Expected results are computed from the fixture, not hard-coded random values.
/// </summary>
public sealed class PaymentFilterCoverageTests
{
    private static (PaymentQueryService Service, IReadOnlyList<Payment> Source) CreateSut(
        int count = 100, int seed = 42)
    {
        var payments = PaymentCacheEntryBuilder.Build(count, seed);
        var cache = new Mock<IPaymentCacheManager>(MockBehavior.Strict);
        cache.Setup(x => x.GetPayments()).Returns(payments);
        return (new PaymentQueryService(cache.Object), payments);
    }

    private static FilterStuckPaymentDto Filter(
        string[]? instruments = null,
        string[]? localInstruments = null,
        PaymentStatus[]? statuses = null,
        string[]? desks = null,
        StatusCategory[]? categories = null,
        string[]? agents = null,
        string[]? countries = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        decimal? amountFrom = null,
        decimal? amountTo = null,
        bool? showAll = false,
        int page = 1,
        int size = 200,
        List<SortCriteria>? sort = null) => new()
        {
            Instruments = instruments,
            LocalInstruments = localInstruments,
            Statuses = statuses,
            Desks = desks,
            Categories = categories,
            Agents = agents,
            Countries = countries,
            DateFrom = dateFrom,
            DateTo = dateTo,
            AmountFrom = amountFrom,
            AmountTo = amountTo,
            ShowAllPayments = showAll,
            CurrentPage = page,
            PageSize = size,
            Sort = sort ?? []
        };

    private static void AssertMatching(
        PagedPayments actual, IEnumerable<Payment> expected)
    {
        var ids = expected.Select(p => p.Id).OrderBy(id => id).ToArray();
        Assert.Equal(ids.Length, actual.TotalCount);
        Assert.Equal(ids, actual.Payments.Select(p => p.Id).OrderBy(id => id));
    }

    [Theory]
    [InlineData("Frankfurt")]
    [InlineData("Paris")]
    [InlineData("London")]
    public void Desk_Filter_Returns_Only_Matching_Payments(string desk)
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(desks: [desk])),
            source.Where(p => p.Desk == desk));
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Approved)]
    [InlineData(PaymentStatus.Rejected)]
    public void Status_Filter_Returns_Only_Matching_Payments(PaymentStatus status)
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(statuses: [status])),
            source.Where(p => p.ComplianceStatus == status));
    }

    [Theory]
    [InlineData(StatusCategory.Initial)]
    [InlineData(StatusCategory.Final)]
    [InlineData(StatusCategory.Failed)]
    public void Category_Filter_Returns_Only_Matching_Payments(StatusCategory category)
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(categories: [category])),
            source.Where(p => p.StatusCategory == category));
    }

    [Theory]
    [InlineData("Wire")]
    [InlineData("Swift")]
    [InlineData("Sepa")]
    public void Instrument_Filter_Returns_Only_Matching_Payments(string instrument)
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(instruments: [instrument])),
            source.Where(p => p.PaymentInstrument == instrument));
    }

    [Theory]
    [InlineData("Bond")]
    [InlineData("Equity")]
    [InlineData("Fund")]
    public void LocalInstrument_Filter_Returns_Only_Matching_Payments(string instrument)
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(localInstruments: [instrument])),
            source.Where(p => p.LegalInstrument == instrument));
    }

    [Theory]
    [InlineData("Alice")]
    [InlineData("Bob")]
    [InlineData("Charlie")]
    public void Agent_Filter_Returns_Only_Matching_Payments(string agent)
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(agents: [agent])),
            source.Where(p => p.Agent == agent));
    }

    [Theory]
    [InlineData("FR")]
    [InlineData("DE")]
    [InlineData("GB")]
    public void Country_Filter_Returns_Only_Matching_Payments(string country)
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(countries: [country])),
            source.Where(p => p.Country == country));
    }

    [Fact]
    public void Multiple_Selected_Values_Are_Combined_With_Or_Within_A_Field()
    {
        var (sut, source) = CreateSut();
        AssertMatching(sut.Search(Filter(desks: ["Paris", "London"])),
            source.Where(p => p.Desk is "Paris" or "London"));
    }

    [Fact]
    public void Multiple_Fields_Are_Combined_With_And()
    {
        var (sut, source) = CreateSut();
        var selected = source[0];
        AssertMatching(sut.Search(Filter(
                desks: [selected.Desk],
                statuses: [selected.ComplianceStatus],
                categories: [selected.StatusCategory],
                countries: [selected.Country])),
            source.Where(p => p.Desk == selected.Desk &&
                              p.ComplianceStatus == selected.ComplianceStatus &&
                              p.StatusCategory == selected.StatusCategory &&
                              p.Country == selected.Country));
    }

    [Fact]
    public void Date_Range_Is_Inclusive()
    {
        var (sut, source) = CreateSut();
        var date = source[0].ExecutionDate;
        AssertMatching(sut.Search(Filter(dateFrom: date, dateTo: date)),
            source.Where(p => p.ExecutionDate == date));
    }

    [Fact]
    public void Amount_Range_Is_Inclusive()
    {
        var (sut, source) = CreateSut();
        var amount = source[0].Amount;
        AssertMatching(sut.Search(Filter(amountFrom: amount, amountTo: amount)),
            source.Where(p => p.Amount == amount));
    }

    [Fact]
    public void Impossible_Date_And_Amount_Ranges_Return_Empty()
    {
        var (sut, _) = CreateSut();
        var result = sut.Search(Filter(
            dateFrom: new DateTime(2099, 1, 1),
            amountFrom: 999999m));
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Payments);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public void ShowAllPayments_Bypasses_Every_Filter_But_Not_Pagination(bool? showAll)
    {
        var (sut, source) = CreateSut();
        var result = sut.Search(Filter(
            desks: ["DoesNotExist"],
            statuses: [PaymentStatus.Pending],
            dateFrom: new DateTime(2099, 1, 1),
            amountFrom: 999999m,
            showAll: showAll,
            page: 2,
            size: 10));
        Assert.Equal(source.Count, result.TotalCount);
        Assert.Equal(10, result.Payments.Count);
        Assert.Equal(Enumerable.Range(11, 10), result.Payments.Select(p => p.Id));
    }

    [Fact]
    public void Empty_Filter_Collections_Do_Not_Restrict_Results()
    {
        var (sut, source) = CreateSut();
        var result = sut.Search(Filter(
            desks: [],
            instruments: [],
            statuses: [],
            categories: []));
        Assert.Equal(source.Count, result.TotalCount);
        Assert.Equal(source.Count, result.Payments.Count);
    }

    [Fact]
    public void MultiColumn_Sort_Uses_ThenBy_For_Second_Field()
    {
        var (sut, source) = CreateSut();
        var result = sut.Search(Filter(sort:
            [new("Desk", SortDirection.Asc), new("Amount", SortDirection.Desc)]));
        var expected = source.OrderBy(p => p.Desk)
            .ThenByDescending(p => p.Amount)
            .Select(p => p.Id);
        Assert.Equal(expected, result.Payments.Select(p => p.Id));
    }

    [Fact]
    public void Sorting_Is_Still_Applied_When_ShowAllPayments_Is_True()
    {
        var (sut, source) = CreateSut();
        var result = sut.Search(Filter(
            showAll: true,
            desks: ["DoesNotExist"],
            size: 5,
            sort: [new("Amount", SortDirection.Desc)]));
        Assert.Equal(source.OrderByDescending(p => p.Amount).Take(5).Select(p => p.Id),
            result.Payments.Select(p => p.Id));
    }

    [Fact]
    public void Pagination_Uses_Stable_Ordering_And_Keeps_TotalCount()
    {
        var (sut, source) = CreateSut();
        var page1 = sut.Search(Filter(page: 1, size: 10));
        var page2 = sut.Search(Filter(page: 2, size: 10));
        Assert.Equal(source.Count, page1.TotalCount);
        Assert.Equal(source.Count, page2.TotalCount);
        Assert.Equal(Enumerable.Range(1, 10), page1.Payments.Select(p => p.Id));
        Assert.Equal(Enumerable.Range(11, 10), page2.Payments.Select(p => p.Id));
    }

    [Fact]
    public void Page_Beyond_End_Is_Empty_But_TotalCount_Remains()
    {
        var (sut, source) = CreateSut();
        var result = sut.Search(Filter(page: 999, size: 10));
        Assert.Equal(source.Count, result.TotalCount);
        Assert.Empty(result.Payments);
    }

    [Fact]
    public void Filtering_And_Sorting_Do_Not_Mutate_The_Cache()
    {
        var (sut, source) = CreateSut();
        var snapshot = source.ToArray();
        sut.Search(Filter(desks: ["Paris"],
            sort: [new("Amount", SortDirection.Desc)]));
        Assert.Equal(snapshot, source);
    }
}
