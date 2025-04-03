using AppointmentManagementAPI.DTOs;
using AppointmentManagementAPI.Models;
using AppointmentManagementAPI.Repositories;
using AutoMapper;

namespace AppointmentManagementAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        private readonly IMapper _mapper;

        public AppointmentService(IAppointmentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            return _mapper.Map<AppointmentDto>(appointment);
        }



        public async Task<AppointmentDto?> CreateAppointmentAsync(AppointmentDto appointmentDto)
        {
            var appointment = _mapper.Map<Appointment>(appointmentDto);
            var createdAppointment = await _repository.AddAsync(appointment);
            return _mapper.Map<AppointmentDto>(createdAppointment);
        }

        public async Task<bool> UpdateAppointmentAsync(AppointmentDto appointmentDto)
        {
            var existingAppointment = await _repository.GetByIdAsync(appointmentDto.Id);
            if (existingAppointment == null)
                return false; // Ensure entity exists before updating

            _mapper.Map(appointmentDto, existingAppointment); // Update fields instead of creating new object
            return await _repository.UpdateAsync(existingAppointment);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date)
        {
            var appointments = await _repository.GetByDateAsync(date);
            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                RequestorName = a.RequestorName,
                ScheduledDate = a.ScheduledDate,
                Status = a.Status
            }).ToList();
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByRequestorNameAsync(string name)
        {
            var appointments = await _repository.GetByRequestorNameAsync(name);
            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                RequestorName = a.RequestorName,
                ScheduledDate = a.ScheduledDate,
                Status = a.Status
            }).ToList();
        }

        public async Task<bool> RescheduleAppointmentAsync(int id, DateTime newDate)
        {
            try
            {
                var appointment = await _repository.GetByIdAsync(id);
                if (appointment == null)
                    throw new KeyNotFoundException($"Appointment with ID {id} not found.");

                if (appointment.Status is "Completed" or "Cancelled")
                    throw new InvalidOperationException($"Cannot reschedule a {appointment.Status} appointment.");

                // Convert input date to UTC
                DateTime utcDate = newDate.ToUniversalTime();

                // Convert UTC to Pacific Standard Time (PST)
                TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime pstDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, pstZone);

                // Validate time is within allowed hours (9 AM - 7 PM PST)
                if (pstDate.Hour is < 9 or > 19)
                    throw new ArgumentException($"Rescheduling failed: Time {pstDate:hh:mm tt} PST is outside allowed hours (9 AM - 7 PM PST).");

                // Update appointment details
                appointment.ScheduledDate = utcDate;
                appointment.UpdatedAt = DateTime.UtcNow;

                bool updated = await _repository.UpdateAsync(appointment);
                if (!updated)
                    throw new Exception("Database update failed: No rows affected.");

                return true;
            }
            catch (KeyNotFoundException ex) { throw; }
            catch (InvalidOperationException ex) { throw; }
            catch (ArgumentException ex) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Rescheduling failed: {ex.Message}");
            }
        }

        public async Task<bool> CompleteAppointmentAsync(int id)
        {
            return await _repository.UpdateStatusAsync(id, "Completed");
        }

        public async Task<bool> CancelAppointmentAsync(int id)
        {
            return await _repository.UpdateStatusAsync(id, "Cancelled");
        }
        public async Task<bool> HasExistingAppointmentAsync(int patientId, DateTime date)
        {
            return await _repository.HasExistingAppointmentAsync(patientId, date);
        }
    }
}
