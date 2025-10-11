using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Interviu.Entity.Enums;
namespace Interviu.Entity.Entities;
public class Interview
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } 

    [Required]
    public string Position { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.ONGOING;

    public float? OverallScore { get; set; }

    [Column(TypeName = "text")]
    public string? OverallFeedback { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // İlişkiler: Mülakatın sahibi olan kullanıcı.
    [Required]
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }

    // İlişkiler: Mülakatta kullanılan CV (isteğe bağlı).
    public Guid? CvId { get; set; }
    [ForeignKey("CvId")]
    public virtual CV? Cv { get; set; }

    
    public virtual ICollection<InterviewQuestion> InterviewQuestions { get; set; } = new List<InterviewQuestion>();
}