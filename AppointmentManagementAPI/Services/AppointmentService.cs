﻿using AppointmentManagementAPI.DTOs;
using AppointmentManagementAPI.Models;
using AppointmentManagementAPI.Repositories.Interfaces;
using AppointmentManagementAPI.Services.Interfaces;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                return false;

            _mapper.Map(appointmentDto, existingAppointment);
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
                UserId = a.UserId,
                ScheduledDate = a.ScheduledDate,
                Status = a.Status
            }).ToList();
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByDateForUserAsync(DateTime date, int userId)
        {
            var appointments = await _repository.GetByDateForUserAsync(date, userId);
            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                UserId = a.UserId,
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
                UserId = a.UserId,
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
                    throw new Exception($"Appointment with ID {id} not found.");

                if (appointment.Status is "Completed" or "Cancelled")
                    throw new Exception($"Cannot reschedule a {appointment.Status} appointment.");

                // Convert input date to UTC
                DateTime utcDate = newDate.ToUniversalTime();

                // Convert UTC to Pacific Standard Time (PST)
                TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime pstDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, pstZone);

                // Validate time is within allowed hours (9 AM - 7 PM PST)
                if (pstDate.Hour is < 9 or > 19)
                    throw new Exception($"Rescheduling failed: Time {pstDate:hh:mm tt} PST is outside allowed hours (9 AM - 7 PM PST).");

                // Update appointment details
                appointment.ScheduledDate = utcDate;
                appointment.Status = "Rescheduled";
                appointment.UpdatedAt = DateTime.UtcNow;

                bool updated = await _repository.UpdateAsync(appointment);
                if (!updated)
                    throw new Exception("Database update failed: No rows affected.");

                return true;
            }
            catch (Exception e)
            {
                throw new Exception($"Rescheduling failed: {e.Message}");
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
