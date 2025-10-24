using System;

namespace Interviu.Core.DTOs;

public class CvDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ExtractedText { get; set; }
    public DateTime UploadDate { get; set; }
    public string UserId { get; set; }
}