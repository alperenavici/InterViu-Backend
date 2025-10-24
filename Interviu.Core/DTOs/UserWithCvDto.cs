namespace Interviu.Core.DTOs;

public class UserWithCvDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public ICollection<CvDto> Cvs { get; set; } = new List<CvDto>();
}

