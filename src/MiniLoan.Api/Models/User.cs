
namespace MiniLoan.Api.Models;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
}
