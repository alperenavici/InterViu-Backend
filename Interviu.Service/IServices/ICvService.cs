using Interviu.Core.DTOs;

namespace Interviu.Service.IServices;

public interface ICvService
{
    Task<IEnumerable<CvDto>> GetAllAsync();
    Task<IEnumerable<CvDto>> GetCvsForUserAsync(string userId);
    Task<CvDto>UploadAndCreateAsync(CvDto cvDto);
    
    
}