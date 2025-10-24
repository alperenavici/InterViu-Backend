using System.ComponentModel.DataAnnotations;
using Interviu.Data.IRepositories;
using Interviu.Service.IServices;
using AutoMapper;
using Interviu.Core.DTOs;
using Microsoft.AspNetCore.Hosting;
using Interviu.Entity.Entities;
using Microsoft.AspNetCore.Http;

namespace Interviu.Service.Services;

public class CvService:ICvService
{
    private readonly   IMapper _mapper;
    private readonly ICVRepository  _cvRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;     
    public CvService(IMapper mapper, ICVRepository cvRepository,IWebHostEnvironment webHostEnvironment)
    {
        _mapper = mapper;
        _cvRepository = cvRepository;
        _webHostEnvironment = webHostEnvironment;
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
        await _cvRepository.SaveChangesAsync();

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