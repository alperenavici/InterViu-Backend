using System.ComponentModel.DataAnnotations;
using Interviu.Entity.Enums;

namespace Interviu.Core.DTOs;

public class CreateQuestionDto
{
    [Required(ErrorMessage = "Soru alanı boş olamaz")]
    [MinLength(10, ErrorMessage = "Minimum 10 karakter olmalı")]
    public string Text { get; set; }
    [Required(ErrorMessage = "Kategori alanı bos olamaz")]
    public string Category { get; set; }
    
    public Difficulty Difficulty { get; set; }=Difficulty.MEDIUM;

}