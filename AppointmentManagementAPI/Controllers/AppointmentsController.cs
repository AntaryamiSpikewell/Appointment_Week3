using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentManagementAPI.DTOs;
using AppointmentManagementAPI.Services.Interfaces;
using System.Security.Claims;
using AppointmentManagementAPI.Models;
using System.Globalization;

namespace AppointmentManagementAPI.Controllers
{
    //[ApiExplorerSettings(GroupName = "2. Appointments")]
    [Authorize]
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

        // GET /api/Appointments - Accessible to Admin only
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Extract user role from the JWT token
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (userRole != "Admin")
                {
                    return StatusCode(403, new { message = "You don't have access to view the other details." });
                }
                var appointments = await _service.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong on our end. Please try again later.", error = ex.Message });
            }
        }


        // POST /api/Appointments - Accessible to both User and Admin
        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create([FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null)
                return BadRequest(new { message = "The appointment data you provided is empty or invalid. Please check and try again." });

            var (isValid, errorMessage) = IsValidAppointmentDateTime(appointmentDto.ScheduledDate);
            if (!isValid)
                return BadRequest(new { message = errorMessage });

            //"Scheduled".Equals(appointmentDto.Status, StringComparison.OrdinalIgnoreCase)
            if (!(appointmentDto.Status == "Scheduled")){
                return BadRequest(new { message = "You can only create appointments with status 'Scheduled'" });
            }

            try
            {
                // Set UserId to current user if not admin
                if (!User.IsInRole("Admin"))
                {
                    appointmentDto.UserId = int.Parse(GetCurrentUserId());
                }

                var newAppointment = await _service.CreateAppointmentAsync(appointmentDto);
                if (newAppointment == null || newAppointment.Id <= 0)
                    return StatusCode(500, new { message = "We couldn’t create your appointment due to a system error Or User Not Exist." });

                return CreatedAtAction(nameof(Get), new { id = newAppointment.Id }, newAppointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong while creating your appointment. Please try again later.", error = ex.Message });
            }
        }

        // GET /api/Appointments/{id} - User (own only), Admin (all)
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var appointment = await _service.GetAppointmentByIdAsync(id);
                if (appointment == null)
                    return NotFound(new { message = $"No appointment was found with ID {id}. It may have been deleted or never existed." });

                if (!User.IsInRole("Admin") && !IsUserOwner(appointment))
                    return StatusCode(403, new { message = "You don’t have permission to view this appointment. It belongs to another user." });

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "We ran into an issue while fetching the appointment. Please try again later.", error = ex.Message });
            }
        }

        // PUT /api/Appointments/{id} - User (own only), Admin (all)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null || id != appointmentDto.Id)
                return BadRequest(new { message = "The appointment data is invalid or the ID in the URL doesn’t match the data. Please check and try again." });

            var existingAppointment = await _service.GetAppointmentByIdAsync(id);
            if (existingAppointment == null)
                return NotFound(new { message = $"We couldn’t find an appointment with ID {id}. It may have been deleted." });

            if (!User.IsInRole("Admin") && !IsUserOwner(existingAppointment))
                return StatusCode(403, new { message = "You can’t update this appointment because it belongs to another user." });

            if (existingAppointment.Status == "Completed" || existingAppointment.Status == "Cancelled")
                return BadRequest(new { message = "This appointment can’t be updated because it’s already completed or cancelled." });

            var (isValid, errorMessage) = IsValidAppointmentDateTime(appointmentDto.ScheduledDate);
            if (!isValid)
                return BadRequest(new { message = errorMessage });

            var updated = await _service.UpdateAppointmentAsync(appointmentDto);
            if (updated)
                return Ok(new { message = "Your appointment was updated successfully." });
            else
                return NotFound(new { message = "We couldn’t update the appointment. It may no longer exist." });
        }


        // GET /api/Appointments/by-date/{date} - Accessible to both User and Admin
        [HttpGet("by-date/{date}")]
        [Authorize]
        public async Task<IActionResult> GetByDate(DateTime date)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (userRole == "Admin")
                {
                    // An Admin Can, fetch all appointments for the given date
                    var allAppointments = await _service.GetAppointmentsByDateAsync(date);
                    return Ok(allAppointments);
                }
                else
                {
                    // User can fetch only their own appointments
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized(new { message = "User identity could not be verified." });

                    var userAppointments = await _service.GetAppointmentsByDateForUserAsync(date, int.Parse(userId));

                    if (!userAppointments.Any())
                        return NotFound(new { message = "No appointments found for the given date." });

                    return Ok(userAppointments);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong while retrieving the appointments.", error = ex.Message });
            }
        }

        // GET /api/Appointments/by-requestor/{name} - User (own only), Admin (all)
        [HttpGet("by-requestor/{name}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetByRequestor(string name)
        {
            var appointments = await _service.GetAppointmentsByRequestorNameAsync(name);
            if (!appointments.Any())
                return NotFound(new { message = $"No appointments were found for the requestor '{name}'." });

            if (!User.IsInRole("Admin"))
            {
                var currentUserId = int.Parse(GetCurrentUserId());
                appointments = appointments.Where(a => a.UserId == currentUserId).ToList();
                if (!appointments.Any())
                    return StatusCode(403, new { message = "You don’t have permission to view appointments for this requestor." });
            }

            return Ok(appointments);
        }

        // PUT /api/Appointments/{id}/reschedule - User (own only), Admin (all)
        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleRequestDto request)
        {
            if (string.IsNullOrEmpty(request.NewDate))
                return BadRequest(new { message = "You must provide a new date to reschedule the appointment." });

            if (!DateTime.TryParse(request.NewDate, null, DateTimeStyles.AdjustToUniversal, out DateTime parsedDate))
                return BadRequest(new { message = "The date format is incorrect. Please use a valid date (e.g., '2025-04-10T14:30:00Z')." });

            var (isValid, errorMessage) = IsValidAppointmentDateTime(parsedDate);
            if (!isValid)
                return BadRequest(new { message = errorMessage });

            try
            {
                 
                var appointment = await _service.GetAppointmentByIdAsync(id);
                if (appointment == null)
                    return NotFound(new { message = $"We couldn’t find an appointment with ID {id} to reschedule." });

                if (!User.IsInRole("Admin") && !IsUserOwner(appointment))
                    return StatusCode(403, new { message = "You can’t reschedule this appointment because it belongs to another user." });

                if (appointment.Status == "Canceled")
                    return BadRequest(new { message = "This appointment was canceled and cannot be rescheduled." });

                if (appointment.Status == "Completed")
                    return BadRequest(new { message = "This appointment is already completed and cannot be rescheduled." });

                bool updated = await _service.RescheduleAppointmentAsync(id, parsedDate);
                if (!updated)
                    return NotFound(new { message = "The appointment couldn’t be rescheduled. It may no longer exist or is in a state that prevents rescheduling." });

                return Ok(new { message = "Your appointment was rescheduled successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "We encountered an issue while rescheduling your appointment. Please try again later.", error = ex.Message });
            }
        }

        // PUT /api/Appointments/{id}/complete - Admin only
        [HttpPut("{id}/complete")]
        [Authorize]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (userRole != "Admin"){
                    return StatusCode(403, new { message = "You don't have access to complete appointments." });
                }

                var appointment = await _service.GetAppointmentByIdAsync(id);
                if (appointment == null){
                    return NotFound(new { message = $"No appointment with ID {id} was found." });
                }

                if (appointment.Status == "Canceled"){
                    return BadRequest(new { message = "This appointment has been canceled and cannot be marked as completed." });
                }

                var updated = await _service.CompleteAppointmentAsync(id);
                if (!updated)
                    return NotFound(new { message = $"No appointment with ID {id} was found, or it’s already marked as completed." });


                return Ok(new { message = "The appointment was marked as completed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong while trying to complete the appointment.", error = ex.Message });
            }
        }


        // PUT /api/Appointments/{id}/cancel - User (own only), Admin (all)
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var appointment = await _service.GetAppointmentByIdAsync(id);

                if (appointment == null)
                    return NotFound(new { message = $"We couldn’t find an appointment with ID {id} to cancel." });

                if (!User.IsInRole("Admin") && !IsUserOwner(appointment))
                    return StatusCode(403, new { message = "You can’t cancel this appointment because it belongs to another user." });
               

                if (appointment.Status == "Completed")
                    return BadRequest(new { message = "This appointment has already been completed and cannot be canceled." });


                var updated = await _service.CancelAppointmentAsync(id);
                if (!updated)
                    return NotFound(new { message = "The appointment couldn’t be cancelled. It may already be cancelled or no longer exists." });

                return Ok(new { message = "Your appointment was cancelled successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "We ran into a problem while cancelling your appointment. Please try again later.", error = ex.Message });
            }
        }

        // DELETE /api/Appointments/{id} - Admin only
        [HttpDelete("{id:int}")]
        [Authorize] 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (userRole != "Admin")
                {
                    return StatusCode(403, new { message = "You don't have access to delete appointments." });
                }

                var deleted = await _service.DeleteAppointmentAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"No appointment with ID {id} was found to delete." });

                return Ok(new { message = $"Successfully deleted appointment with ID {id}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong.", error = ex.Message });
            }
        }


        private (bool, string) IsValidAppointmentDateTime(DateTime utcDateTime)
        {
            try
            {
                if (utcDateTime < DateTime.UtcNow)
                {
                    return (false, "You can’t schedule an appointment in the past. Please choose a future date and time.");
                }

                TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById(PacificTimeZone);
                DateTime scheduledPST = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, pstZone);

                int hour = scheduledPST.Hour;
                if (hour < 9 || hour > 19)
                {
                    return (false, $"Appointments must be scheduled between 9 AM and 7 PM Pacific Time. Your selected time ({scheduledPST:hh:mm tt} PST) is outside these hours.");
                }

                return (true, string.Empty);
            }
            catch (Exception)
            {
                return (false, "There was an issue with the date format you provided. Please use a valid UTC date (e.g., '2025-04-10T23:30:00').");
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("User ID not found in token.");
        }

        private bool IsUserOwner(AppointmentDto appointment)
        {
            var currentUserId = int.Parse(GetCurrentUserId());
            return appointment.UserId == currentUserId;
        }
    }
}