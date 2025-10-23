using Interviu.Entity.Entities;
using Interviu.Entity.Enums;

namespace Interviu.Core.DTOs;

public class InterviewDto
{
    public Guid Id { get; set; }
    public string Position { get; set; }
    public InterviewStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public float? OverallScore { get; set; }
    public string? OverallFeedback { get; set; }

    // İlişkili veriler için DTO'lar
    public UserDto User { get; set; }
    public CvDto? Cv { get; set; }
    public List<InterviewQuestionDto> Questions { get; set; } = new List<InterviewQuestionDto>();
    
}