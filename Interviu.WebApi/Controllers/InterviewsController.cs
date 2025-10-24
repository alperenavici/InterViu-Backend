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
}


