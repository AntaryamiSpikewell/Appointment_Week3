using Microsoft.EntityFrameworkCore;
using AppointmentManagementAPI.Models;

namespace AppointmentManagementAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>().Property(a => a.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Appointment>().Property(a => a.UpdatedAt).HasDefaultValueSql("GETDATE()");
        }
    }
}
