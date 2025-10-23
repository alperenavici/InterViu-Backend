using System.ComponentModel.DataAnnotations;

namespace Interviu.Core.DTOs;

public class StartInterviewDto
{
    
        [Required]
        public string Position { get; set; }

        [Required]
        public string UserId { get; set; }
    
        public Guid? CvId { get; set; }

        // Mülakatı özelleştirmek için opsiyonel parametreler
        public int QuestionCount { get; set; } = 10; // Varsayılan 10 soru
        public string? Category { get; set; }
}