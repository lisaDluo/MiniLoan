
namespace MiniLoan.Api.DTOs;

public sealed class LoanSummaryDto
{
    public int Month { get; init; }
    public decimal CurrentPrincipalBalance { get; init; }
    public decimal TotalPrincipalPaid { get; init; }
    public decimal TotalInterestPaid { get; init; }
}
