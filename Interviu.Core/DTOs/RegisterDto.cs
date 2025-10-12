using System.ComponentModel.DataAnnotations;

namespace Interviu.Core.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    [MinLength(8, ErrorMessage = "Şifreniz 8 Karakterden Uzun Olmalı")]
    public string Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}