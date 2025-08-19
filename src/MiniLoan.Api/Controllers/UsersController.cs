
using Microsoft.AspNetCore.Mvc;
using MiniLoan.Api.DTOs;
using MiniLoan.Api.Models;
using MiniLoan.Api.Services;

namespace MiniLoan.Api.Controllers;

[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly LoanService _service;
    private readonly ILogger<UsersController> _logger;

    public UsersController(LoanService service, ILogger<UsersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("api/users")]
    public ActionResult<User> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var user = _service.CreateUser(dto.Name);
        return CreatedAtAction(nameof(GetLoansForUser), new { userId = user.Id }, user);
    }

    [HttpGet("api/users/{userId:guid}/loans")]
    public ActionResult<IEnumerable<Loan>> GetLoansForUser([FromRoute] Guid userId)
    {
        var loans = _service.GetLoansForUser(userId);
        return Ok(loans);
    }
}
