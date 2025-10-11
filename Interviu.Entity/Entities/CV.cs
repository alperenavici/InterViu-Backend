using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Interviu.Entity.Entities;

public class CV
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public string FileName { get; set; }
    [Required]
    public string FilePath { get; set; }
    
    [Column(TypeName="text")]
    public string ExtractedText { get; set; }
    
    public DateTime UploadDate { get; set; }=DateTime.UtcNow;
    
    [Required]
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }

    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

}