using AppointmentManagementAPI.Models;

namespace AppointmentManagementAPI.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task<Appointment?> AddAsync(Appointment appointment);
        Task<bool> UpdateAsync(Appointment appointment);
        Task<bool> DeleteAsync(int id);
        Task<List<Appointment>> GetByDateAsync(DateTime date);
        Task<List<Appointment>> GetByDateForUserAsync(DateTime date, int userId);
        Task<List<Appointment>> GetByRequestorNameAsync(string name);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> HasExistingAppointmentAsync(int patientId, DateTime date);
    }
}
