using AppointmentManagementAPI.Data;
using AppointmentManagementAPI.Models;
using AppointmentManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AppointmentManagementAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.AsNoTracking()  // Improves performance for read-only queries
                                       .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
