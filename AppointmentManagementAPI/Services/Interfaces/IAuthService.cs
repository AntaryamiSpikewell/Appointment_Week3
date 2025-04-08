using AppointmentManagementAPI.DTOs;

namespace AppointmentManagementAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(LoginDto loginDto);
        Task<bool> RegisterUserAsync(UserDto userDto);
    }
}
