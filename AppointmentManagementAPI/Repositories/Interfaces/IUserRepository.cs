using AppointmentManagementAPI.Models;

namespace AppointmentManagementAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task AddUserAsync(User user);
        Task<bool> SaveChangesAsync();
    }
}
