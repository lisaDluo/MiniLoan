using MiniLoan.Api.Models;

namespace MiniLoan.Api.Repositories;

public sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly InMemoryStore _store;
    public InMemoryLoanRepository(InMemoryStore store) => _store = store;

    public bool TryAdd(Loan loan) => _store.Loans.TryAdd(loan.Id, loan);

    public bool TryGet(Guid loanId, out Loan? loan)
    {
        var ok = _store.Loans.TryGetValue(loanId, out var value);
        loan = value;
        return ok;
    }

    public IEnumerable<Loan> GetByUser(Guid userId) => _store.GetLoansByUser(userId);
}
