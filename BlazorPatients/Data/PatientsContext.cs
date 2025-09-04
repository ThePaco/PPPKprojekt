using BlazorPatients.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorPatients.Data;

public class PatientsContext : DbContext
{
    public PatientsContext(DbContextOptions<PatientsContext> options) : base(options) { }
    public DbSet<Models.Patient> Patients { get; set; }
    public DbSet<Models.Prescription> Prescriptions { get; set; }
    public DbSet<Models.Visit> Visits { get; set; }
    public DbSet<Models.Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Patient>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasKey(p => p.Id);
            
            entity.HasIndex(p => p.Oib)
                  .IsUnique()
                  .HasDatabaseName("IX_Patients_OIB_Unique");
            
            entity.Property(p => p.Oib)
                  .HasMaxLength(11);
            
            entity.Property(p => p.FirstName)
                  .HasMaxLength(100);
                  
            entity.Property(p => p.LastName)
                  .HasMaxLength(100);
        });

        modelBuilder.Entity<Models.Prescription>(entity =>
        {
            entity.ToTable("Prescriptions");
            entity.HasKey(p => p.Id);
            
            entity.HasOne(p => p.Patient)
                  .WithMany(pt => pt.Prescriptions)
                  .HasForeignKey(p => p.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(p => p.MedicationName)
                  .HasMaxLength(200);
        });

        modelBuilder.Entity<Models.Visit>(entity =>
        {
            entity.ToTable("Visits");
            entity.HasKey(v => v.Id);
            
            entity.HasOne(v => v.Patient)
                  .WithMany(p => p.Visits)
                  .HasForeignKey(v => v.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Models.Image>(entity =>
        {
            entity.ToTable("Images");
            entity.HasKey(i => i.Id);
            
            entity.HasOne(v => v.Visit)
                  .WithMany(i => i.Images)
                  .HasForeignKey(i => i.VisitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
