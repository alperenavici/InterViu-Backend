using Interviu.Core.DTOs; // Kendi DTO namespace'iniz
using Interviu.Data.IRepositories;
using Interviu.Service.IServices;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Interviu.Data.UnitOfWork;
using Interviu.Entity.Entities;
using Interviu.Service.Exceptions;

namespace Interviu.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var userEntity = await _userRepository.GetUserByIdAsync(id);
            if (userEntity == null)
            {
                throw new EntityNotFoundException("ApplicationUser", id);
            }
            return _mapper.Map<UserDto>(userEntity);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var userEntities = await _userRepository.GetAllUsersAsync();
            return _mapper.Map<IEnumerable<UserDto>>(userEntities);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var userEntity = await _userRepository.GetUserByEmailAsync(email);
            if (userEntity == null)
            {
                return null;
            }
            return _mapper.Map<UserDto>(userEntity);
        }

        public async Task<UserDto> RegisterUserAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ValidationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            var newUserEntity = _mapper.Map<ApplicationUser>(dto);

            var identityResult = await _userRepository.CreateAsync(newUserEntity, dto.Password);

            if (!identityResult.Succeeded)
            {
                var hatalar = identityResult.Errors.Select(e => e.Description);
                var hataMesaji = string.Join(", ", hatalar);
    
                throw new ValidationException(hataMesaji);
            }
            
            await _userRepository.AddToRoleAsync(newUserEntity, "User");
            
            return _mapper.Map<UserDto>(newUserEntity);
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
            
            return (true, _mapper.Map<UserDto>(userEntity));
        }
        
        public async Task<IEnumerable<UserWithCvDto>> GetUsersWithCvAsync()
        {
            var userEntities = await _userRepository.GetUsersWithCVsAsync();
            return _mapper.Map<IEnumerable<UserWithCvDto>>(userEntities);
        }
    }
}