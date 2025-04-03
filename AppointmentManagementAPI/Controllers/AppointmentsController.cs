using Microsoft.AspNetCore.Mvc;
using AppointmentManagementAPI.Services;
using AppointmentManagementAPI.DTOs;
using System;
using System.Threading.Tasks;
using TimeZoneConverter;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AppointmentManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;
        private readonly string PacificTimeZone = "Pacific Standard Time";

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var appointments = await _service.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var appointment = await _service.GetAppointmentByIdAsync(id);
                if (appointment == null)
                    return NotFound(new { message = $"Appointment with ID {id} not found." });

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null)
                return BadRequest(new { message = "Invalid appointment data." });

            var (isValid, errorMessage) = IsValidAppointmentDateTime(appointmentDto.ScheduledDate);
            if (!isValid)
                return BadRequest(new { message = errorMessage });

            try
            {
                var newAppointment = await _service.CreateAppointmentAsync(appointmentDto);
                Console.WriteLine(newAppointment);
                if (newAppointment == null || newAppointment.Id <= 0)
                    return StatusCode(500, new { message = "Failed to create the appointment." });

                return CreatedAtAction(nameof(Get), new { id = newAppointment.Id }, newAppointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null || id != appointmentDto.Id)
                return BadRequest(new { message = "Invalid data or ID mismatch." });

            Console.WriteLine($"Updating appointment ID: {id}");

            var existingAppointment = await _service.GetAppointmentByIdAsync(id);

            if (existingAppointment == null)
            {
                Console.WriteLine($"Appointment ID {id} not found in database.");
                return NotFound(new { message = $"Appointment with ID {id} not found." });
            }

            if (existingAppointment.Status == "Completed" || existingAppointment.Status == "Cancelled")
                return BadRequest(new { message = "Cannot modify a completed or cancelled appointment." });

            var (isValid, errorMessage) = IsValidAppointmentDateTime(appointmentDto.ScheduledDate);
            if (!isValid)
                return BadRequest(new { message = errorMessage });

            var updated = await _service.UpdateAppointmentAsync(appointmentDto);

            //In ASP.NET Core, when performing a PUT operation, the typical response for a successful update is 204 No Content, which doesn't include a message in the response body
            //return updated ? NoContent() : NotFound();
            if (updated)
                return Ok(new { message = "Appointment updated successfully." }); // Returns success message with 200 OK
            else
                return NotFound(new { message = "Failed to update appointment." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAppointmentAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Appointment with ID {id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("by-date/{date}")]
        public async Task<IActionResult> GetByDate(DateTime date)
        {
            var appointments = await _service.GetAppointmentsByDateAsync(date);
            if (!appointments.Any())
                return NotFound(new { message = "No appointments found for this date." });

            return Ok(appointments);
        }

        [HttpGet("by-requestor/{name}")]
        public async Task<IActionResult> GetByRequestor(string name)
        {
            var appointments = await _service.GetAppointmentsByRequestorNameAsync(name);
            if (!appointments.Any())
                return NotFound(new { message = "No appointments found for this requestor." });

            return Ok(appointments);
        }

        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleRequestDto request)
        {
            if (string.IsNullOrEmpty(request.NewDate))
                return BadRequest(new { message = "New date is required." });

            if (!DateTime.TryParse(request.NewDate, out DateTime parsedDate))
                return BadRequest(new { message = "Invalid date format. Use ISO 8601 format." });

            var (isValid, errorMessage) = IsValidAppointmentDateTime(parsedDate);
            if (!isValid)
                return BadRequest(new { message = errorMessage });

            try
            {
                bool updated = await _service.RescheduleAppointmentAsync(id, parsedDate);
                if (!updated)
                    return NotFound(new { message = "Appointment not found or cannot be rescheduled." });

                return Ok(new { message = "Appointment rescheduled successfully." }); // Success message
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (SqlException ex) 
            {
                return StatusCode(500, new { message = "Database error occurred.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var updated = await _service.CompleteAppointmentAsync(id);
            if (!updated)
                return NotFound(new { message = "Appointment not found or already completed." });

            return Ok(new { message = "Appointment marked as completed successfully." }); // Success message
        }


        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var updated = await _service.CancelAppointmentAsync(id);
            if (!updated)
                return NotFound(new { message = "Appointment not found or already cancelled." });

            return Ok(new { message = "Appointment cancelled successfully." }); // Success message
        }


        private (bool, string) IsValidAppointmentDateTime(DateTime utcDateTime)
        {
            try
            {
                if (utcDateTime < DateTime.UtcNow)
                {
                    return (false, "Appointment must be scheduled for a future date.");
                }

                // Convert UTC to PST
                TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime scheduledPST = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, pstZone);

                int hour = scheduledPST.Hour;

                if (hour < 9 || hour > 19)
                {
                    return (false, $"Appointment must be scheduled between 9 AM - 7 PM PST. Provided time: {scheduledPST:hh:mm tt} PST.");
                }

                return (true, string.Empty);
            }
            catch (Exception)
            {
                return (false, "Invalid date format or conversion error.");
            }
        }
    }
}
