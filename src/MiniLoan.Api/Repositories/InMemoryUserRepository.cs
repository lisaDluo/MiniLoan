using MiniLoan.Api.Models;

namespace MiniLoan.Api.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly InMemoryStore _store;
    public InMemoryUserRepository(InMemoryStore store) => _store = store;

    public bool Exists(Guid userId) => _store.Users.ContainsKey(userId);

    public bool TryAdd(User user) => _store.Users.TryAdd(user.Id, user);
}
