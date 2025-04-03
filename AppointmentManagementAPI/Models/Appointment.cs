using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentManagementAPI.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RequestorName { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
