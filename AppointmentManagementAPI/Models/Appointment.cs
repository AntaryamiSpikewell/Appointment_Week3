using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentManagementAPI.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign key to Users table

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [EnumDataType(typeof(AppointmentStatus))]
        public string Status { get; set; } = AppointmentStatus.Scheduled.ToString();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public required User User { get; set; }
    }

    public enum AppointmentStatus
    {
        Scheduled,
        Rescheduled,
        Completed,
        Cancelled
    }
}
