using AppointmentManagementAPI.DTOs;
using AppointmentManagementAPI.Models;
using AppointmentManagementAPI.Repositories.Interfaces;
using AppointmentManagementAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AppointmentManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<string?> AuthenticateAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username);
            if (user == null || !VerifyPassword(loginDto.Password, user.Password))
                return null;

            return _jwtService.GenerateToken(user);
        }

        public async Task<bool> RegisterUserAsync(UserDto userDto)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(userDto.Username);
            if (existingUser != null) return false;

            var newUser = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = HashPassword(userDto.Password),
                Role = "User"
            };

            await _userRepository.AddUserAsync(newUser);
            return await _userRepository.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
