
using System.ComponentModel.DataAnnotations;
using MiniLoan.Api.Models;

namespace MiniLoan.Api.DTOs;

public sealed class CreateLoanDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>Annual percentage rate as percent, e.g., 7.5 = 7.5%</summary>
    [Range(0.0, 100.0)]
    public decimal AnnualInterestRate { get; set; }

    [Range(1, 100)]
    public int LoanTermYears { get; set; }

    [Required]
    public PaymentFrequency PaymentFrequency { get; set; }
}
