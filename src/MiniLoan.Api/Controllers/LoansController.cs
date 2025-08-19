
using Microsoft.AspNetCore.Mvc;
using MiniLoan.Api.DTOs;
using MiniLoan.Api.Models;
using MiniLoan.Api.Services;

namespace MiniLoan.Api.Controllers;

[ApiController]
public sealed class LoansController : ControllerBase
{
    private readonly LoanService _service;
    private readonly ILogger<LoansController> _logger;

    public LoansController(LoanService service, ILogger<LoansController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("api/users/{userId:guid}/loans")]
    public ActionResult<Loan> CreateLoan([FromRoute] Guid userId, [FromBody] CreateLoanDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var loan = _service.CreateLoan(userId, dto);
            return CreatedAtAction(nameof(GetSchedule), new { loanId = loan.Id }, loan);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    [HttpGet("api/loans/{loanId:guid}/schedule")]
    public ActionResult<IEnumerable<LoanScheduleRow>> GetSchedule([FromRoute] Guid loanId)
    {
        try
        {
            var schedule = _service.GetSchedule(loanId);
            return Ok(schedule);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Loan not found" });
        }
    }

    [HttpGet("api/loans/{loanId:guid}/summary")]
    public ActionResult<LoanSummaryDto> GetSummary([FromRoute] Guid loanId, [FromQuery] int month)
    {
        if (month <= 0) return BadRequest(new { message = "Month must be >= 1" });
        try
        {
            var summary = _service.GetSummary(loanId, month);
            return Ok(summary);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Loan not found" });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
