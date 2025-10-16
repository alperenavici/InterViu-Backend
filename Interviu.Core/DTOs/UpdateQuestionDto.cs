using Interviu.Entity.Enums;
namespace Interviu.Core.DTOs;

public class UpdateQuestionDto
{
    public string? Text { get; set; }
    public string? Category { get; set; }
    public Difficulty? Difficulty { get; set; }
}