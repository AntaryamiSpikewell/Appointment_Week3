using AppointmentManagementAPI.Data;
using AppointmentManagementAPI.Models;
using AppointmentManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppointmentManagementAPI.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentRepository> _logger;

        public AppointmentRepository(ApplicationDbContext context, ILogger<AppointmentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments.ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments.FindAsync(id);
        }

        public async Task<Appointment?> AddAsync(Appointment appointment)
        {
            try
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return appointment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding appointment");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(Appointment appointment)
        {
            try
            {
                var existingAppointment = await _context.Appointments.FindAsync(appointment.Id);
                if (existingAppointment == null)
                    throw new KeyNotFoundException($"Appointment with ID {appointment.Id} not found.");

                existingAppointment.ScheduledDate = appointment.ScheduledDate;
                existingAppointment.UpdatedAt = DateTime.UtcNow;  // Ensure UpdatedAt timestamp is Set

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while updating appointment.");
                throw new InvalidOperationException("Database update failed. Possible constraint violation.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating appointment.");
                throw;
            }
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Appointment>> GetByDateAsync(DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.ScheduledDate.Date == date.Date)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByDateForUserAsync(DateTime date, int userId)
        {
            return await _context.Appointments
                .Where(a => a.ScheduledDate.Date == date.Date && a.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByRequestorNameAsync(string name)
        {
            return await _context.Appointments
                .Where(a => a.User.Username.Contains(name))
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> HasExistingAppointmentAsync(int patientId, DateTime date)
        {
            return await _context.Appointments
                .AnyAsync(a => a.Id == patientId && a.ScheduledDate.Date == date.Date);
        }
    }
}
