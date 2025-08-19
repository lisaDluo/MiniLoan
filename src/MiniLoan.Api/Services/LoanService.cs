using MiniLoan.Api.DTOs;
using MiniLoan.Api.Models;
using MiniLoan.Api.Repositories;

namespace MiniLoan.Api.Services
{
    public sealed class LoanService
    {
        private readonly IUserRepository _users;
        private readonly ILoanRepository _loans;
        private readonly ILogger<LoanService> _logger;

        public LoanService(IUserRepository users, ILoanRepository loans, ILogger<LoanService> logger)
        {
            _users = users;
            _loans = loans;
            _logger = logger;
        }

        public User CreateUser(string name)
        {
            var user = new User { Name = name };
            if (!_users.TryAdd(user))
                throw new InvalidOperationException("Failed to create user.");
            _logger.LogInformation("Created user {UserId} - {Name}", user.Id, name);
            return user;
        }

        public Loan CreateLoan(Guid userId, CreateLoanDto dto)
        {
            if (!_users.Exists(userId))
                throw new KeyNotFoundException("User not found.");

            var loan = new Loan
            {
                UserId = userId,
                Amount = Round2(dto.Amount),
                AnnualInterestRate = dto.AnnualInterestRate,
                LoanTermYears = dto.LoanTermYears,
                PaymentFrequency = dto.PaymentFrequency
            };

            if (!_loans.TryAdd(loan))
                throw new InvalidOperationException("Failed to create loan.");

            _logger.LogInformation("Created loan {LoanId} for user {UserId}", loan.Id, userId);
            return loan;
        }

        public IEnumerable<Loan> GetLoansForUser(Guid userId) => _loans.GetByUser(userId);

        public IReadOnlyList<LoanScheduleRow> GetSchedule(Guid loanId)
        {
            if (!_loans.TryGet(loanId, out var loan) || loan is null)
                throw new KeyNotFoundException("Loan not found.");
            return ComputeSchedule(loan);
        }

        public LoanSummaryDto GetSummary(Guid loanId, int month)
        {
            var schedule = GetSchedule(loanId);
            if (month < 1 || month > schedule.Count)
                throw new ArgumentOutOfRangeException(nameof(month), "Month out of range for loan term.");

            if (!_loans.TryGet(loanId, out var loan) || loan is null)
                throw new KeyNotFoundException("Loan not found.");

            var monthlyRate = loan.AnnualInterestRate / 100m / 12m;
            var monthsPerPayment = (int)loan.PaymentFrequency;
            if (monthsPerPayment <= 0)
                throw new ArgumentOutOfRangeException(nameof(loan.PaymentFrequency), "Payment frequency must be >= 1 month.");
            var totalMonths = loan.LoanTermYears * 12;

            decimal balance = loan.Amount;
            decimal accrued = 0m;
            decimal totalInterestPaid = 0m;
            decimal totalPrincipalPaid = 0m;
            decimal paymentPerPayPeriod = ComputePaymentPerPeriod(loan);

            for (int m = 1; m <= month; m++)
            {
                var interest = Round2(balance * monthlyRate);
                accrued += interest;

                if (m % monthsPerPayment == 0 || m == totalMonths)
                {
                    var planned = Round2(paymentPerPayPeriod);
                    var due = Round2(accrued);
                    var actualPayment = Math.Min(planned, Round2(balance + accrued));
                    var principalPayment = Round2(actualPayment - due);
                    if (principalPayment < 0) principalPayment = 0m;

                    balance = Round2(balance - principalPayment);
                    totalInterestPaid = Round2(totalInterestPaid + due);
                    totalPrincipalPaid = Round2(totalPrincipalPaid + principalPayment);
                    accrued = 0m;
                }
            }

            var currentPrincipalBalance = Round2(balance + accrued);

            return new LoanSummaryDto
            {
                Month = month,
                CurrentPrincipalBalance = currentPrincipalBalance,
                TotalInterestPaid = totalInterestPaid,
                TotalPrincipalPaid = totalPrincipalPaid
            };
        }

        // --------- helpers ----------

        private static decimal ComputePaymentPerPeriod(Loan loan)
        {
            var monthsPerPayment = (int)loan.PaymentFrequency;
            var monthlyRate = loan.AnnualInterestRate / 100m / 12m;
            var totalMonths = loan.LoanTermYears * 12;
            var numberOfPayments = (int)Math.Ceiling(totalMonths / (decimal)monthsPerPayment);

            // Effective interest per payment period (compounding monthly)
            var iEff = (decimal)Math.Pow((double)(1m + monthlyRate), monthsPerPayment) - 1m;

            if (iEff == 0m) // zero-interest loan: even principal payments
                return Round2(loan.Amount / numberOfPayments);

            var numerator = loan.Amount * iEff;
            var denom = 1m - (decimal)Math.Pow((double)(1m + iEff), -numberOfPayments);
            var payment = numerator / denom;
            return Round2(payment);
        }

        private static List<LoanScheduleRow> ComputeSchedule(Loan loan)
        {
            var monthsPerPayment = (int)loan.PaymentFrequency;
            var monthlyRate = loan.AnnualInterestRate / 100m / 12m;
            var totalMonths = loan.LoanTermYears * 12;

            decimal balance = loan.Amount;
            decimal accrued = 0m;
            var schedule = new List<LoanScheduleRow>(capacity: totalMonths);
            var paymentPerPayPeriod = ComputePaymentPerPeriod(loan);

            for (int m = 1; m <= totalMonths; m++)
            {
                var interest = Round2(balance * monthlyRate);
                accrued += interest;

                decimal paymentThisMonth = 0m;
                if (m % monthsPerPayment == 0 || m == totalMonths)
                {
                    var planned = Round2(paymentPerPayPeriod);
                    var due = Round2(accrued);
                    var actualPayment = Math.Min(planned, Round2(balance + accrued));
                    var principalPayment = Round2(actualPayment - due);
                    if (principalPayment < 0) principalPayment = 0m;

                    balance = Round2(balance - principalPayment);
                    paymentThisMonth = Round2(actualPayment);
                    accrued = 0m;
                }

                var remaining = Round2(balance + accrued);
                schedule.Add(new LoanScheduleRow
                {
                    Month = m,
                    MonthlyPayment = paymentThisMonth,
                    RemainingBalance = remaining
                });
            }

            return schedule;
        }

        private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
