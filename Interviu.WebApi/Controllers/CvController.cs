using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using Interviu.Core.DTOs;
using Interviu.Entity.Entities;
using Interviu.Service.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interviu.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CvController:ControllerBase
{
    private readonly ICvService _cvService;
    private readonly ILogger<CvController> _logger;
    private readonly IConfiguration _configuration;

    public CvController(ICvService cvService, ILogger<CvController> logger, IConfiguration configuration)
    {
        _cvService = cvService;
        _logger = logger;
        _configuration = configuration;
    }
    /// <summary>
    /// Tüm CV'leri getirir (Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CvDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CvDto>>> GetAll()
    {
        try
        {
            var cvs = await _cvService.GetAllAsync();
            return Ok(cvs);
        }
        catch (EntryPointNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bir hata oluştu", error = ex.Message });
        }
    }

    /// <summary>
    /// Giriş yapmış kullanıcının CV'lerini getirir
    /// </summary>
    [HttpGet("my-cvs")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<CvDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CvDto>>> GetMyCvs()
    {
        try
        {
            var userId=User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new{message="Kullanıcı Kimligi Bulunamadı"});
                
            }

            var cvs = await _cvService.GetCvsForUserAsync(userId);
            return Ok(cvs);
            
        }catch (EntryPointNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bir hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CvDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CvDto>> Upload([FromForm] IFormFile file)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
            }

            var result = await _cvService.UploadAndCreateCvAsync(file, userId);
            return CreatedAtAction(nameof(GetMyCvs), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500,new{message="Dosya yüklenirken hata oluştu",error=ex.Message });
            
        }
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CvDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CvDto>>> GetCvsByUserId(string userId)
    {
        try
        {
            var cvs = await _cvService.GetCvsForUserAsync(userId);
            return Ok(cvs);
        }
        catch (EntryPointNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
        
}