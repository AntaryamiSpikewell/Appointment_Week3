using AppointmentManagementAPI.DTOs;

namespace AppointmentManagementAPI.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<AppointmentDto?> CreateAppointmentAsync(AppointmentDto appointmentDto);
        Task<bool> UpdateAppointmentAsync(AppointmentDto appointmentDto);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<bool> RescheduleAppointmentAsync(int id, DateTime newDate);
        Task<bool> CompleteAppointmentAsync(int id);
        Task<bool> CancelAppointmentAsync(int id);
        Task<List<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date);
        Task<List<AppointmentDto>> GetAppointmentsByRequestorNameAsync(string name);
        Task<bool> HasExistingAppointmentAsync(int patientId, DateTime date);
    }
}
