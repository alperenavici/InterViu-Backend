using Interviu.Core.DTOs;
using System.Threading.Tasks;
using Interviu.Core.DTOs;
namespace Interviu.Service.IServices;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> RegisterUserAsync(RegisterDto dto);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<UserWithCvDto>> GetUsersWithCvAsync();
    Task<(bool Succeeded, UserDto? User)> ValidateCredentialsAsync(LoginDto dto);
}