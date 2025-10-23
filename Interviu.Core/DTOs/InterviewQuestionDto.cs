using Interviu.Entity.Enums;

namespace Interviu.Core.DTOs;

public class InterviewQuestionDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; }
    public Difficulty QuestionDifficulty { get; set; }
    public string AnswerText { get; set; }
    public float? Score { get; set; }
    public string? Feedback { get; set; }
}