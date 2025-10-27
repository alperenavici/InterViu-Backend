using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Interviu.Entity.Enums;

namespace Interviu.Entity.Entities;

public class Question
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [Column(TypeName = "text")]
    public string Text { get; set; }
    
    // Alias for compatibility - maps to Text
    [NotMapped]
    public string QuestionText 
    { 
        get => Text; 
        set => Text = value; 
    }
    
    [Required]
    public string Category { get; set; }

    public Difficulty Difficulty { get; set; } = Difficulty.MEDIUM;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<InterviewQuestion> InterviewQuestions { get; set; } = new List<InterviewQuestion>();



}