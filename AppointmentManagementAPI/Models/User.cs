using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AppointmentManagementAPI.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Username { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        public required string Password { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "User";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
