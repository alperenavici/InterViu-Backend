using System.ComponentModel.DataAnnotations;

namespace Interviu.Core.DTOs;

public class CompleteInterviewDto
{
    [Range(0, 10)]
    public float OverallScore { get; set; }

    [Required]
    public string OverallFeedback { get; set; }
}
