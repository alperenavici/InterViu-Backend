using Interviu.Entity.Enums;

namespace Interviu.Core.DTOs;

public class InterviewQuestionDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string? AnswerText { get; set; }
    public float? Score { get; set; }
    public string? Feedback { get; set; }
}