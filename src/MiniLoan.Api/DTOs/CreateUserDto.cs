
using System.ComponentModel.DataAnnotations;

namespace MiniLoan.Api.DTOs;

public sealed class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public string Name { get; set; } = string.Empty;
}
