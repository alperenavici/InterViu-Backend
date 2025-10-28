using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Interviu.Core.DTOs;
using Interviu.Service.Exceptions;
using Interviu.Service.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interviu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewsController:ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly ILogger<InterviewsController> _logger;

    public InterviewsController(IInterviewService interviewService, ILogger<InterviewsController> logger)
    {
        _interviewService = interviewService;
        _logger = logger;
    }

    /// <summary>
    /// Yeni bir mülakat başlatır.
    /// </summary>
    /// <remarks>
    /// Admin veya mülakat başlatma yetkisi olan bir kullanıcı tarafından çağrılmalıdır.
    /// </remarks>
    [HttpPost("start")]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> StartInterview([FromBody] StartInterviewDto dto)
    {
        try
        {
            var createdInterview = await _interviewService.StartInterviewAsync(dto);
            return CreatedAtAction(nameof(GetInterviewDetails), new { interviewId = createdInterview.Id }, createdInterview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mülakat başlatılırken bir hata oluştu.");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// CV dosyası yükleyerek Gemini AI ile otomatik soru üretimi yaparak mülakat başlatır.
    /// </summary>
    /// <remarks>
    /// CV dosyası (PDF/DOCX) yüklenir ve Gemini AI pozisyona uygun sorular üretir.
    /// UserId otomatik olarak giriş yapmış kullanıcıdan (Admin) alınır.
    /// </remarks>
    [HttpPost("start-with-cv")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StartInterviewWithCv([FromForm] IFormFile cvFile, [FromForm] string position, [FromForm] int questionCount = 10)
    {
        try
        {
            // Giriş yapmış kullanıcının ID'sini token'dan al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği doğrulanamadı" });
            }

            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest(new { Message = "CV dosyası gereklidir" });
            }

            var dto = new StartInterviewDto
            {
                Position = position,
                UserId = userId,
                QuestionCount = questionCount
            };

            _logger.LogInformation("Admin {AdminId} tarafından {Position} pozisyonu için CV ile mülakat başlatılıyor", userId, position);

            var createdInterview = await _interviewService.StartInterviewCvAsync(cvFile, dto);
            return CreatedAtAction(nameof(GetInterviewDetails), new { interviewId = createdInterview.Id }, createdInterview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CV ile mülakat başlatılırken bir hata oluştu.");
            return BadRequest(new { Message = ex.Message });
        }
    }
    /// <summary>
    /// Bir mülakattaki belirli bir soruya cevap gönderir.
    /// </summary>
    [HttpPost("submit-answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDto dto)
    {
        try
        {
            await _interviewService.SubmitAnswerAsync(dto);
            // Başarılı olursa, bir içerik döndürmeye gerek yok. 204 No Content idealdir.
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cevap gönderilirken bir hata oluştu: InterviewId {InterviewId}", dto.InterviewId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Bir mülakatı tamamlar ve genel değerlendirmeyi kaydeder.
    /// </summary>
    /// <remarks>
    /// Sadece Admin veya değerlendirici rolündeki kullanıcılar tarafından çağrılmalıdır.
    /// </remarks>
    [HttpPost("{interviewId:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CompleteInterview(Guid interviewId, [FromBody] CompleteInterviewDto dto)
    {
        try
        {
            await _interviewService.CompleteInterviewAsync(interviewId, dto.OverallScore, dto.OverallFeedback);
            return Ok(new { Message = "Mülakat başarıyla tamamlandı." });
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mülakat tamamlanırken bir hata oluştu: InterviewId {InterviewId}", interviewId);
            return StatusCode(500, "Mülakat tamamlanırken sunucu hatası oluştu.");
        }
    }

    /// <summary>
    /// Belirtilen ID'ye sahip mülakatın tüm detaylarını getirir.
    /// </summary>
    [HttpGet("{interviewId:guid}")]
    public async Task<IActionResult> GetInterviewDetails(Guid interviewId)
    {
        try
        {
            var interviewDto = await _interviewService.GetInterviewWithDetailsAsync(interviewId);
            return Ok(interviewDto);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Mevcut giriş yapmış kullanıcının tüm mülakatlarını listeler.
    /// </summary>
    [HttpGet("my-interviews")]
    public async Task<IActionResult> GetMyInterviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var interviews = await _interviewService.GetInterviewsForUserAsync(userId);
        return Ok(interviews);
    }
    [HttpPost("{interviewId:guid}/questions/{questionId:guid}/submit-audio")]
    public async Task<IActionResult> SubmitAudioAnswer(Guid interviewId, Guid questionId, IFormFile audioFile)
    {
    if (audioFile == null || audioFile.Length == 0)
    {
        return BadRequest(new { Message = "Lütfen bir ses dosyası yükleyin." });
    }

    try
    {
        var transcribedText = await _interviewService.SubmitAudioAnswerAsync(interviewId, questionId, audioFile);
        return Ok(new { message = "Cevap başarıyla kaydedildi.", transcribedText = transcribedText });
    }
    catch (EntityNotFoundException ex)
    {
        _logger.LogWarning(ex, "Sesli cevap gönderilirken kaynak bulunamadı: {Message}", ex.Message);
        // 404 Not Found: İstenen kaynak (mülakat/soru) mevcut değil.
        return NotFound(new { Message = ex.Message });
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning(ex, "Sesli cevap gönderilirken doğrulama hatası: {Message}", ex.Message);
        // 400 Bad Request: Kullanıcının isteği geçersiz veya mantıksız.
        return BadRequest(new { Message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Sesli cevap gönderilirken beklenmedik bir hata oluştu. Mülakat ID: {InterviewId}", interviewId);
        // 500 Internal Server Error: Sunucu tarafında beklenmedik bir sorun oluştu.
        return StatusCode(500, new { Message = "Sesli cevap işlenirken sunucuda beklenmedik bir hata oluştu." });
    }
}

[HttpPost("{interviewId:guid}/complete-and-analyze")]
public async Task<IActionResult> CompleteAndAnalyzeInterview(Guid interviewId)
{
    try
    {
        var analysisResult = await _interviewService.AnalyzeAndCompleteInterviewAsync(interviewId);
        return Ok(analysisResult);
    }
    catch (EntityNotFoundException ex)
    {
        _logger.LogWarning(ex, "Analiz için mülakat bulunamadı: {Message}", ex.Message);
        return NotFound(new { Message = ex.Message });
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning(ex, "Mülakat analizi sırasında doğrulama hatası: {Message}", ex.Message);
        // 400 Bad Request: İstek geçersiz.
        return BadRequest(new { Message = ex.Message });
    }
    catch (HttpRequestException ex)
    {
         _logger.LogError(ex, "Mülakat analizi sırasında Gemini API'sine ulaşılamadı. Mülakat ID: {InterviewId}", interviewId);
         // 503 Service Unavailable: Harici bir servis yanıt vermiyor.
         return StatusCode(503, new { Message = "Yapay zeka analiz servisine şu anda ulaşılamıyor. Lütfen daha sonra tekrar deneyin." });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Mülakat analizi sırasında beklenmedik bir hata oluştu. Mülakat ID: {InterviewId}", interviewId);
        // 500 Internal Server Error: Sunucuda bir şeyler ters gitti.
        return StatusCode(500, new { Message = "Mülakat analizi sırasında sunucuda beklenmedik bir hata oluştu." });
    }
    }
}


