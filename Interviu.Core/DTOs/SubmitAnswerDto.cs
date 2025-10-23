using System.ComponentModel.DataAnnotations;

namespace Interviu.Core.DTOs;

public class SubmitAnswerDto
{
    [Required]
    public Guid InterviewId { get; set; }

    [Required]
    public Guid QuestionId { get; set; }

    [Required]
    public string AnswerText { get; set; }
}