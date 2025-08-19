using System;
using System.Linq;
using Moq;
using Microsoft.Extensions.Logging;
using MiniLoan.Api.DTOs;
using MiniLoan.Api.Models;
using MiniLoan.Api.Repositories;
using MiniLoan.Api.Services;
using Xunit;
using System.Collections.Generic;

namespace MiniLoan.Tests;

public class LoanServiceTests
{
    private static LoanService CreateService(
        out Mock<IUserRepository> users,
        out Mock<ILoanRepository> loans,
        out Mock<ILogger<LoanService>> logger)
    {
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        loans = new Mock<ILoanRepository>(MockBehavior.Strict);
        logger = new Mock<ILogger<LoanService>>();

        return new LoanService(users.Object, loans.Object, logger.Object);
    }

    [Fact]
    public void CreateUser_Should_Add_And_Log()
    {
        // Arrange
        var svc = CreateService(out var users, out var loans, out var logger);
        users.Setup(u => u.TryAdd(It.IsAny<User>())).Returns(true);
        // no loan calls here

        // Act
        var user = svc.CreateUser("Alice");

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alice", user.Name);
        users.Verify(u => u.TryAdd(It.Is<User>(x => x.Name == "Alice")), Times.Once);
        logger.VerifyLogInformationContains("Created user", Times.Once());
        loans.VerifyNoOtherCalls();
    }

    [Fact]
    public void CreateLoan_Should_Throw_When_User_Not_Found()
    {
        var svc = CreateService(out var users, out var loans, out var logger);

        users.Setup(u => u.Exists(It.IsAny<Guid>())).Returns(false);

        var userId = Guid.NewGuid();
        var dto = new CreateLoanDto
        {
            Amount = 10000m, AnnualInterestRate = 5m, LoanTermYears = 1, PaymentFrequency = PaymentFrequency.Monthly
        };

        Assert.Throws<KeyNotFoundException>(() => svc.CreateLoan(userId, dto));
        users.Verify(u => u.Exists(userId), Times.Once);
        loans.VerifyNoOtherCalls();
    }

    [Fact]
    public void CreateLoan_Should_Add_And_Log()
    {
        var svc = CreateService(out var users, out var loans, out var logger);
        var userId = Guid.NewGuid();

        users.Setup(u => u.Exists(userId)).Returns(true);
        loans.Setup(l => l.TryAdd(It.IsAny<Loan>())).Returns(true);

        var dto = new CreateLoanDto
        {
            Amount = 10000m, AnnualInterestRate = 6m, LoanTermYears = 1, PaymentFrequency = PaymentFrequency.Monthly
        };

        var loan = svc.CreateLoan(userId, dto);

        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Equal(userId, loan.UserId);
        Assert.Equal(10000m, loan.Amount);

        users.Verify(u => u.Exists(userId), Times.Once);
        loans.Verify(l => l.TryAdd(It.Is<Loan>(x => x.UserId == userId && x.Amount == 10000m)), Times.Once);
        logger.VerifyLogInformationContains("Created loan", Times.Once());
    }

    [Fact]
    public void GetSchedule_Should_Throw_When_Loan_Not_Found()
    {
        var svc = CreateService(out var users, out var loans, out var logger);

        loans.Setup(l => l.TryGet(It.IsAny<Guid>(), out It.Ref<Loan?>.IsAny))
             .Returns(false);

        Assert.Throws<KeyNotFoundException>(() => svc.GetSchedule(Guid.NewGuid()));
    }

    [Fact]
    public void GetSummary_Should_Throw_When_Month_Out_Of_Range()
    {
        var svc = CreateService(out var users, out var loans, out var logger);

        var loan = new Loan
        {
            UserId = Guid.NewGuid(),
            Amount = 5000m,
            AnnualInterestRate = 5m,
            LoanTermYears = 1,
            PaymentFrequency = PaymentFrequency.Monthly
        };

        loans.Setup(l => l.TryGet(loan.Id, out loan)).Returns(true);

        // Month 0 and 13 invalid
        Assert.Throws<ArgumentOutOfRangeException>(() => svc.GetSummary(loan.Id, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => svc.GetSummary(loan.Id, 13));
    }

    [Fact]
    public void ZeroInterest_Should_Pay_Principal_Evenly()
    {
        var svc = CreateService(out var users, out var loans, out var logger);

        var loan = new Loan
        {
            UserId = Guid.NewGuid(),
            Amount = 12000m,
            AnnualInterestRate = 0m,
            LoanTermYears = 1,
            PaymentFrequency = PaymentFrequency.Monthly
        };

        loans.Setup(l => l.TryGet(loan.Id, out loan)).Returns(true);

        var schedule = svc.GetSchedule(loan.Id);
        Assert.Equal(12, schedule.Count);
        foreach (var row in schedule.Take(11))
            Assert.Equal(1000m, row.MonthlyPayment);

        Assert.True(Math.Abs(schedule.Last().RemainingBalance) <= 0.01m);
    }

    [Fact]
    public void GetLoansForUser_Should_Return_From_Repo()
    {
        var svc = CreateService(out var users, out var loans, out var logger);
        var userId = Guid.NewGuid();

        var list = new[]
        {
            new Loan { UserId = userId, Amount = 1, AnnualInterestRate = 1, LoanTermYears = 1, PaymentFrequency = PaymentFrequency.Monthly },
            new Loan { UserId = userId, Amount = 2, AnnualInterestRate = 2, LoanTermYears = 2, PaymentFrequency = PaymentFrequency.Monthly }
        };

        loans.Setup(l => l.GetByUser(userId)).Returns(list);

        var result = svc.GetLoansForUser(userId).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Amount);
        loans.Verify(l => l.GetByUser(userId), Times.Once);
    }
}

// ------------- helpers to verify ILogger<T>.LogInformation -----------------
internal static class LoggerMoqExtensions
{
    public static void VerifyLogInformationContains<T>(
        this Mock<ILogger<T>> logger,
        string contains,
        Times times)
    {
        logger.Verify(l => l.Log(
            It.Is<LogLevel>(lvl => lvl == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) =>
                state != null &&
                state.ToString() != null &&
                state.ToString()!.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}

