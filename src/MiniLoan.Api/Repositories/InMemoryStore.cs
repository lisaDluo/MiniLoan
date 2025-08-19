
using MiniLoan.Api.Models;
using System.Collections.Concurrent;

namespace MiniLoan.Api.Repositories;

public sealed class InMemoryStore
{
    public ConcurrentDictionary<Guid, User> Users { get; } = new();
    public ConcurrentDictionary<Guid, Loan> Loans { get; } = new();

    public IEnumerable<Loan> GetLoansByUser(Guid userId) =>
        Loans.Values.Where(l => l.UserId == userId);
}
