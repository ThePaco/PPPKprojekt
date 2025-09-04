namespace BlazorPatients.Models;

public class Prescription
{
    public required int Id { get; set; }
    public required int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public required string MedicationName { get; set; }
    public required DateTime DatePrescribed { get; set; }
    public DateTime? DateEnding { get; set; }
}
