using Interviu.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace Interviu.Service.IServices;

public interface ICvService
{
    Task<IEnumerable<CvDto>> GetAllAsync();
    Task<IEnumerable<CvDto>> GetCvsForUserAsync(string userId);
    Task<CvDto> UploadAndCreateCvAsync(IFormFile file, string userId);


}



 

    