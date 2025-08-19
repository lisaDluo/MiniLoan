using MiniLoan.Api.Models;
namespace MiniLoan.Api.Repositories;

public interface IUserRepository
{
    bool Exists(Guid userId);
    bool TryAdd(User user);
}

public interface ILoanRepository
{
    bool TryAdd(Loan loan);
    bool TryGet(Guid loanId, out Loan? loan);
    IEnumerable<Loan> GetByUser(Guid userId);
}