using Interviu.Core.DTOs; // Kendi DTO namespace'iniz
using Interviu.Data.IRepositories;
using Interviu.Service.IServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // IEnumerable için
using System.Linq;
using System.Threading.Tasks;
using Interviu.Entity.Entities;
using Interviu.Service.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Interviu.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var userEntity = await _userRepository.GetUserByIdAsync(id);
            if (userEntity == null)
            {
                throw new EntityNotFoundException("ApplicationUser", id);
            }
            return MapToUserDto(userEntity);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var userEntities = await _userRepository.GetAllUsersAsync();
            return userEntities.Select(MapToUserDto);
        }

        

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var userEntity = await _userRepository.GetUserByEmailAsync(email);
            if (userEntity == null)
            {
                return null;
            }
            return MapToUserDto(userEntity);
        }

        public async Task<UserDto> RegisterUserAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ValidationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            var newUserEntity = new ApplicationUser()
            {
                Email = dto.Email,
                UserName = dto.UserName,
                FirstName = dto.FirstName, 
                LastName = dto.LastName,   
            };

            var identityResult = await _userRepository.CreateAsync(newUserEntity, dto.Password);

            if (!identityResult.Succeeded)
            {
                var hatalar = identityResult.Errors.Select(e => e.Description);
                var hataMesaji = string.Join(", ", hatalar);
    
                throw new ValidationException(hataMesaji);
            }
            
            await _userRepository.AddToRoleAsync(newUserEntity, "User");
            
            return MapToUserDto(newUserEntity);
        }
        
        public async Task AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new EntityNotFoundException("ApplicationUser", userId);
            }
            
            var identityResult = await _userRepository.AddToRoleAsync(user, roleName);
            
            if (!identityResult.Succeeded)
            {
                var hatalar = identityResult.Errors.Select(e => e.Description);
                var hataMesaji = string.Join(", ", hatalar);
    
                throw new ValidationException(hataMesaji);
            }
        }
        
        public async Task<(bool Succeeded, UserDto? User)> ValidateCredentialsAsync(LoginDto dto)
        {
            var userEntity = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (userEntity == null) return (false, null);

            var passwordIsValid = await _userRepository.CheckPasswordAsync(userEntity, dto.Password);
            if (!passwordIsValid) return (false, null);
            
            return (true, MapToUserDto(userEntity));
        }
        
        public async Task<IEnumerable<UserWithCvDto>> GetUsersWithCvAsync()
        {
            var userEntities = await _userRepository.GetUsersWithCVsAsync();
            return userEntities.Select(user => new UserWithCvDto()
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Cvs = user.Cvs?.Select(cv => new CvDto
                {
                    Id = cv.Id,
                    FileName = cv.FileName,
                    UploadDate = cv.UploadDate
                }).ToList() ?? new List<CvDto>()
            });
        }
        
        
        private UserDto MapToUserDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }
    }
}