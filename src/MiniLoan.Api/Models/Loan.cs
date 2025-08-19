
using System.Text.Json.Serialization;

namespace MiniLoan.Api.Models;

public sealed class Loan
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public required decimal Amount { get; init; }
    /// <summary>Annual percentage rate as percent, e.g., 7.5 = 7.5%</summary>
    public required decimal AnnualInterestRate { get; init; }
    /// <summary>Loan term in years</summary>
    public required int LoanTermYears { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PaymentFrequency PaymentFrequency { get; init; }
    public DateOnly StartDate { get; init; } = DateOnly.FromDateTime(DateTime.Today);
}
