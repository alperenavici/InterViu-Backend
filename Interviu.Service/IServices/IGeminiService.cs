using Interviu.Core.DTOs;

namespace Interviu.Service.IServices;

public interface IGeminiService
{
    /// <summary>
    /// CV metni ve pozisyona göre Gemini AI ile mülakat soruları oluşturur.
    /// </summary>
    /// <param name="cvText">CV'nin ham metni</param>
    /// <param name="position">Başvurulan pozisyon</param>
    /// <param name="questionCount">Oluşturulacak soru sayısı</param>
    /// <returns>Oluşturulan soruların listesi</returns>
    Task<List<GeneratedQuestionDto>> GenerateInterviewQuestionsAsync(string cvText, string position, int questionCount = 10);
}

