
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Interviu.Entity.Entities;
public class InterviewQuestion //ara tablo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } 

    [Column(TypeName = "text")]
    public string? AnswerText { get; set; }

    public float? Score { get; set; }

    [Column(TypeName = "text")]
    public string? Feedback { get; set; }

    [Required]
    public Guid InterviewId { get; set; }
    [ForeignKey("InterviewId")]
    public virtual Interview Interview { get; set; }

    [Required]
    public Guid QuestionId { get; set; }
    [ForeignKey("QuestionId")]
    public virtual Question Question { get; set; }
}