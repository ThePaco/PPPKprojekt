namespace BlazorPatients.Models;

public class Patient
{
    public required int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required bool IsMale { get; set; }
    public required string Oib { get; set; }
    public required DateTime Birthday { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
