using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Interviu.Core.DTOs;
using Interviu.Data.IRepositories;
using Interviu.Entity.Entities;
using Interviu.Service.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Interviu.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class QuestionController:ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionController> _logger;

    public QuestionController(IQuestionService questionService, IMapper mapper,
        ILogger<QuestionController> logger)
    {
        _questionService = questionService;
        _mapper = mapper;
        _logger = logger;
        
    }
    
    /// <summary>
    /// Tüm soruları getirir
    /// </summary>
    /// <returns>Soru listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetAllQuestions()
    {
        try
        {
            var questions = await _questionService.GetAllQuestionsAsync();
            return Ok(questions);
        }
        catch (EntryPointNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sorular bulunamadı");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sorular getirilirken hata oluştu");
            return StatusCode(500, new { message = "Sorular getirilirken bir hata oluştu", error = ex.Message });
        }
    }

    /// <summary>
    /// ID'ye göre soru getirir
    /// </summary>
    /// <param name="id">Soru ID'si</param>
    /// <returns>Soru detayı</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionDto>> GetQuestionById(Guid id)
    {
        try
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            return Ok(question);
        }
        catch (EntryPointNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sorular bulunamad:{QuestionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Soru bulunamadı:{QuestionId}", id);
            return NotFound(new { message = ex.Message });
        }
       
    }

    /// <summary>
    /// Yeni soru oluşturur (Admin)
    /// </summary>
    /// <param name="dto">Soru bilgileri</param>
    /// <returns>Oluşturulan soru</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionDto>> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        try
        {
            var createdQuestion = await _questionService.CreateQuestionAsync(dto);

            return CreatedAtAction(
                nameof(GetQuestionById),
                new { id = createdQuestion.Id },
                createdQuestion
            );
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Soru oluşturma validasyon hatası");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Soru oluşturulurken hata oluştu");
            return StatusCode(500, new { message = "Soru oluşturulurken bir hata oluştu", error = ex.Message });
        }
    }
    
    
    /// <summary>
    /// Soruyu günceller (Admin)
    /// </summary>
    /// <param name="id">Soru ID'si</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Başarı durumu</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionDto dto)
    {
        try
        {
            await _questionService.UpdateQuestionAsync(id, dto);
            return NoContent();
        }
        catch (EntryPointNotFoundException ex)
        {
            _logger.LogWarning(ex, "Güncellenecek soru bulunamadı: {QuestionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Soru güncellenirken hata oluştu: {QuestionId}", id);
            return StatusCode(500, new { message = "Soru güncellenirken bir hata oluştu", error = ex.Message });
        }
    }

    /// <summary>
    /// Soruyu siler (Admin)
    /// </summary>
    /// <param name="id">Soru ID'si</param>
    /// <returns>Başarı durumu</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        try
        {
            await _questionService.DeleteQuestionAsync(id);
            return NoContent();
        }
        catch (EntryPointNotFoundException ex)
        {
            _logger.LogWarning(ex, "Silinecek soru bulunamadı: {QuestionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Soru silinirken hata oluştu: {QuestionId}", id);
            return StatusCode(500, new { message = "Soru silinirken bir hata oluştu", error = ex.Message });
        }
    }

    /// <summary>
    /// Rastgele sorular getirir
    /// </summary>
    /// <param name="count">Soru sayısı</param>
    /// <param name="category">Kategori filtresi (opsiyonel)</param>
    /// <returns>Rastgele soru listesi</returns>
    [HttpGet("random")]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetRandomQuestions(
        [FromQuery] int count = 5, 
        [FromQuery] string? category = null)
    {
        try
        {
            if (count <= 0 || count > 50)
            {
                return BadRequest(new { message = "Soru sayısı 1 ile 50 arasında olmalıdır" });
            }

            var questions = await _questionService.GetRandomQuestionsAsync(count, category);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rastgele sorular getirilirken hata oluştu");
            return StatusCode(500, new { message = "Rastgele sorular getirilirken bir hata oluştu", error = ex.Message });
        }
    }

    /// <summary>
    /// Tüm kategorileri getirir
    /// </summary>
    /// <returns>Kategori listesi</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        try
        {
            var categories = await _questionService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategoriler getirilirken hata oluştu");
            return StatusCode(500, new { message = "Kategoriler getirilirken bir hata oluştu", error = ex.Message });
        }
    }

    /// <summary>
    /// Kategoriye göre soruları getirir
    /// </summary>
    /// <param name="category">Kategori adı</param>
    /// <returns>Kategorideki sorular</returns>
    [HttpGet("by-category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestionsByCategory(string category)
    {
        try
        {
            var questions = await _questionService.GetRandomQuestionsAsync(100, category); // Tüm soruları al
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori soruları getirilirken hata oluştu: {Category}", category);
            return StatusCode(500, new { message = "Sorular getirilirken bir hata oluştu", error = ex.Message });
        }
    }
    
}
