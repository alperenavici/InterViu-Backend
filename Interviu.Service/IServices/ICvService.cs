using Interviu.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace Interviu.Service.IServices;

public interface ICvService
{
    Task<IEnumerable<CvDto>> GetAllAsync();
    Task<IEnumerable<CvDto>> GetCvsForUserAsync(string userId);
    Task<CvDto> UploadAndCreateCvAsync(IFormFile file, string userId);
    
    /// <summary>
    /// Verilen bir IFormFile nesnesinden ham metni asenkron olarak çıkarır.
    /// </summary>
    /// <param name="file">Controller'dan gelen dosya.</param>
    /// <returns>Dosyanın içindeki ham metin.</returns>
    /// <exception cref="ValidationException">Desteklenmeyen dosya tipi veya boş dosya durumunda fırlatılır.</exception>
    Task<string> ExtractTextAsync(IFormFile file);
    
    /// <summary>
    /// Verilen bir Stream nesnesinden ve dosya adından ham metni asenkron olarak çıkarır.
    /// Bu, IFormFile'a bağımlılığı azaltır ve servisi daha genel hale getirir.
    /// </summary>
    /// <param name="fileStream">Dosyanın içerik akışı.</param>
    /// <param name="fileName">Dosyanın orijinal adı (uzantıyı belirlemek için).</param>
    /// <returns>Dosyanın içindeki ham metin.</returns>
    Task<string> ExtractTextAsync(Stream fileStream, string fileName);

}



 

    