using AppointmentManagementAPI.Models;

namespace AppointmentManagementAPI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
