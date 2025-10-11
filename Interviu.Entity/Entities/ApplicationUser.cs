using Microsoft.AspNetCore.Identity;

namespace Interviu.Entity.Entities;

public class ApplicationUser:IdentityUser
{
    public string? firstName {get; set;}
    public string? lastName {get; set;}
    
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}

    public virtual ICollection<CV> Cvs { get; set; } = new List<CV>();
    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

}