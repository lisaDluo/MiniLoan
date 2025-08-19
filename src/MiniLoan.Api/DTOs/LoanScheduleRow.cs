
namespace MiniLoan.Api.DTOs;

public sealed class LoanScheduleRow
{
    public int Month { get; init; }
    public decimal MonthlyPayment { get; init; }
    public decimal RemainingBalance { get; init; }
}
