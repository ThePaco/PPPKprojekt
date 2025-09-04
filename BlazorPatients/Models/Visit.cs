using BlazorPatients.Models.Enum;

namespace BlazorPatients.Models;

public class Visit
{
    public required int Id { get; set; }
    public required int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public required VisitType Type { get; set; }
    public required DateTime Date { get; set; }
    public required string DoctorsNotes { get; set; }
    public ICollection<Image> Images { get; set; } = new List<Image>();
}
