using System.ComponentModel.DataAnnotations;

namespace Interviu.Core.DTOs;

public class RoleAssingDto
{
    [Required]
    public string RoleName { get; set; }
}