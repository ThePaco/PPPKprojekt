using Microsoft.EntityFrameworkCore;

namespace BlazorPatients.Data;

public class PatientsContext : DbContext
{
    public PatientsContext(DbContextOptions<PatientsContext> options) : base(options)
    {
    }
    public DbSet<Models.Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.UseIdentityByDefaultColumns();
        modelBuilder.Entity<Models.Patient>().ToTable("Patients");

    }
}
