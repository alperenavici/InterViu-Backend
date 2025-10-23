using Interviu.Entity.Enums;

namespace Interviu.Core.DTOs;

public class InterviewSummaryDto
{
    public Guid Id { get; set; }
    public string Position { get; set; }
    public InterviewStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public float? OverallScore { get; set; }
}