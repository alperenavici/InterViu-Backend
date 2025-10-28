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
    
    /// <summary>
    /// Gemini AI'ya genel bir prompt göndererek içerik üretir.
    /// </summary>
    /// <param name="prompt">Gemini AI'ya gönderilecek prompt</param>
    /// <returns>Gemini AI'dan dönen yanıt metni</returns>
    Task<string> GenerateContentAsync(string prompt);
}

