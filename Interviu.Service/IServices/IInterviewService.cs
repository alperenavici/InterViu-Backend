using Interviu.Core.DTOs;
using Interviu.Data.IRepositories;
using Interviu.Entity.Entities;
using Interviu.Entity.Enums;
using Microsoft.AspNetCore.Http;

namespace Interviu.Service.IServices;

public interface IInterviewService
{
    /// <summary>
    /// Yeni bir mülakat başlatır, rastgele sorular seçer ve veritabanına kaydeder.
    /// </summary>
    /// <returns>Başlatılan mülakatın detaylarını içeren DTO.</returns>
    Task<InterviewDto> StartInterviewAsync(StartInterviewDto dto);

    /// <summary>
    /// Bir mülakattaki belirli bir soruya cevap gönderir.
    /// </summary>
    Task SubmitAnswerAsync(SubmitAnswerDto dto);

    /// <summary>
    /// Bir mülakatı tamamlar, genel puan ve geri bildirim ekler.
    /// </summary>
    Task CompleteInterviewAsync(Guid interviewId, float overallScore, string overallFeedback);

    /// <summary>
    /// Belirtilen ID'ye sahip mülakatın tüm detaylarını getirir.
    /// </summary>
    Task<InterviewDto> GetInterviewWithDetailsAsync(Guid interviewId);

    /// <summary>
    /// Belirtilen kullanıcıya ait tüm mülakatların özet listesini getirir.
    /// </summary>
    Task<IEnumerable<InterviewSummaryDto>> GetInterviewsForUserAsync(string userId);
    
    /// <summary>
    /// CV dosyası yükleyerek Gemini AI ile otomatik soru üretimi yaparak mülakat başlatır.
    /// </summary>
    /// <param name="cvFile">Yüklenen CV dosyası</param>
    /// <param name="dto">Mülakat başlatma bilgileri</param>
    /// <returns>Başlatılan mülakatın detaylarını içeren DTO</returns>
    Task<InterviewDto> StartInterviewCvAsync(IFormFile cvFile, StartInterviewDto dto);
}