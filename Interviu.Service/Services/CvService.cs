using System.ComponentModel.DataAnnotations;
using System.Text;
using Interviu.Data.IRepositories;
using Interviu.Service.IServices;
using AutoMapper;
using Interviu.Core.DTOs;
using Interviu.Data.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Interviu.Entity.Entities;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Xceed.Words.NET;

namespace Interviu.Service.Services;

public class CvService:ICvService
{
    private readonly   IMapper _mapper;
    private readonly ICVRepository  _cvRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;  
    private readonly IUnitOfWork _unitOfWork;
    public CvService(IMapper mapper,IUnitOfWork unitOfWork, ICVRepository cvRepository,IWebHostEnvironment webHostEnvironment)
    {
        _mapper = mapper;
        _cvRepository = cvRepository;
        _webHostEnvironment = webHostEnvironment;
        _unitOfWork = unitOfWork;
    }
    public async Task<string> ExtractTextAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ValidationException("Lütfen işlenecek bir dosya sağlayın.");
        }

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0; 
            return await ExtractTextAsync(memoryStream, file.FileName);
        }
    }
    
    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        string rawText;
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        switch (fileExtension)
        {
            case ".pdf":
                try
                {
                    using (PdfDocument document = PdfDocument.Open(fileStream))
                    {
                        var textBuilder = new StringBuilder();
                        foreach (Page page in document.GetPages())
                        {
                            textBuilder.Append(page.Text);
                            textBuilder.Append(" "); 
                        }
                        rawText = textBuilder.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("PDF dosyası işlenirken bir hata oluştu.", ex);
                }
                break;
                    
            case ".docx":
                try
                {
                    var doc = DocX.Load(fileStream);
                    rawText = doc.Text;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("DOCX dosyası işlenirken bir hata oluştu.", ex);
                }
                break;
                    
            default:
                throw new ValidationException($"Desteklenmeyen dosya tipi: '{fileExtension}'. Lütfen .pdf veya .docx formatında bir dosya yükleyin.");
        }

        if (string.IsNullOrWhiteSpace(rawText))
        {
            throw new ValidationException("Dosyanın içeriği okunamadı veya dosya boş.");
        }

        return rawText;
    }

    public async Task<IEnumerable<CvDto>> GetAllAsync()
    {
        var cvEntity=await _cvRepository.GetAllAsync();
        if (cvEntity == null)
        {
            throw new EntryPointNotFoundException("CvEntity not found");
        }
        return _mapper.Map<IEnumerable<CvDto>>(cvEntity);
        
    }

    public  async Task<IEnumerable<CvDto>> GetCvsForUserAsync(string userId)
    {
        var cvEntity = await _cvRepository.GetByCvsByUserIdAsync(userId);
        if (cvEntity == null || !cvEntity.Any())
        {
            throw new EntryPointNotFoundException("No CVs found for this user");
        }
        return _mapper.Map<IEnumerable<CvDto>>(cvEntity);
    }

    public async Task<CvDto> UploadAndCreateCvAsync(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ValidationException("Lütfen yüklenecek bir dosya seçin.");
        }


        var uploadsFolderPath = Path.Combine("PrivateFiles", "uploads", "cvs");

        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }


        var cvEntity = new CV
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName, 
            FilePath = Path.Combine("PrivateFiles", "cvs", uniqueFileName),
            UploadDate = DateTime.UtcNow,
            UserId = userId,
            ExtractedText = "Metin çıkarma işlemi burada yapılmalı" 
        };

        await _cvRepository.AddAsync(cvEntity);
        await _unitOfWork.SaveChangesAsync();

        return new CvDto
        {
            Id = cvEntity.Id,
            FileName = cvEntity.FileName,
            FilePath = cvEntity.FilePath,
            ExtractedText = cvEntity.ExtractedText,
            UploadDate = cvEntity.UploadDate,
            UserId = cvEntity.UserId
        };
    }
}