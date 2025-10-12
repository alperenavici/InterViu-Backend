using Interviu.Entity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Interviu.Data.IRepositories; 
public interface IUserRepository
{ 
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByEmailAsync(string email); 
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<IEnumerable<ApplicationUser>> GetUsersWithCVsAsync(); 
}
    
